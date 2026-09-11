using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zarządza pojedynczą falą: spawn, timer, warunki zakończenia (M4/M7).
/// </summary>
[DisallowMultipleComponent]
public class WaveManager : MonoBehaviour
{
    [SerializeField] private WaveDefinition waveDefinition;
    [SerializeField] private MapGreyboxBuilder mapBuilder;
    [SerializeField] private bool autoStartOnPlay;
    [SerializeField] private float laneSpawnSpread = 1.2f;

    private readonly List<EnemyController> _spawnedEnemies = new();
    private readonly List<Health> _spawnedBossHealth = new();
    private WaveState _state = WaveState.Idle;
    private float _timeRemaining;
    private int _totalSpawned;
    private bool _allGroupsSpawned;
    private Coroutine _waveRoutine;

    public WaveDefinition CurrentWave => waveDefinition;
    public WaveState State => _state;
    public float TimeRemaining => _timeRemaining;
    public int AliveEnemyCount { get; private set; }
    public int TotalSpawned => _totalSpawned;

    public event Action<WaveDefinition> WaveStarted;
    public event Action<WaveDefinition, bool> WaveCompleted;

    private void Start()
    {
        if (mapBuilder == null)
            mapBuilder = FindAnyObjectByType<MapGreyboxBuilder>();

        if (autoStartOnPlay && waveDefinition != null)
            StartWave(waveDefinition);
    }

    public void StartWave(WaveDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogError("[WaveManager] Brak WaveDefinition.");
            return;
        }

        if (_waveRoutine != null)
            StopCoroutine(_waveRoutine);

        waveDefinition = definition;
        ResetWaveState();
        _state = WaveState.Active;
        _timeRemaining = definition.DurationSeconds;
        _waveRoutine = StartCoroutine(RunWave(definition));
        WaveStarted?.Invoke(definition);
        Debug.Log($"[WaveManager] Fala {definition.WaveNumber} rozpoczęta ({definition.DurationSeconds:0}s).");
    }

    public void RegisterMidWaveEnemy(EnemyController enemy)
    {
        if (enemy == null) return;
        _spawnedEnemies.Add(enemy);
        _totalSpawned++;
        RefreshAliveCount();
    }

    private void ResetWaveState()
    {
        DespawnAllWaveActors();
        _totalSpawned = 0;
        _allGroupsSpawned = false;
        AliveEnemyCount = 0;
    }

    private IEnumerator RunWave(WaveDefinition definition)
    {
        if (definition.IsBossWave && definition.WardenDefinition != null)
        {
            var bossPos = mapBuilder != null
                ? mapBuilder.GetLaneSpawnPosition(AttackLineId.Center, 0, laneSpawnSpread)
                : new Vector3(0f, 1f, 26f);
            var warden = BossSpawner.SpawnWarden(definition.WardenDefinition, bossPos);
            if (warden != null)
            {
                var health = warden.GetComponent<Health>();
                if (health != null)
                {
                    _spawnedBossHealth.Add(health);
                    _totalSpawned = 1;
                }
            }

            _allGroupsSpawned = true;
        }
        else if (definition.IsBossWave && definition.BossDefinition != null)
        {
            var bossPos = mapBuilder != null
                ? mapBuilder.GetLaneSpawnPosition(AttackLineId.Center, 0, laneSpawnSpread)
                : new Vector3(0f, 1f, 26f);
            var boss = BossSpawner.Spawn(definition.BossDefinition, bossPos);
            if (boss != null)
            {
                var health = boss.GetComponent<Health>();
                if (health != null)
                {
                    _spawnedBossHealth.Add(health);
                    _totalSpawned = 1;
                }
            }

            _allGroupsSpawned = true;
        }
        else
        {
            var entries = definition.SpawnEntries;
            if (entries == null || entries.Length == 0)
            {
                Debug.LogWarning("[WaveManager] Fala bez spawn entries — kończy się po czasie.");
                _allGroupsSpawned = true;
            }
            else
            {
                foreach (var entry in entries)
                {
                    if (entry == null) continue;

                    if (entry.StartDelaySeconds > 0f)
                        yield return new WaitForSeconds(entry.StartDelaySeconds);

                    for (var i = 0; i < entry.Count; i++)
                    {
                        if (_state != WaveState.Active) yield break;

                        var lanePos = GetLaneSpawnPosition(entry.Lane, i);
                        var enemy = EnemySpawner.Spawn(
                            entry.EnemyDefinition,
                            lanePos,
                            _totalSpawned,
                            entry.Lane,
                            entry.EliteModifier);
                        if (enemy != null)
                        {
                            _spawnedEnemies.Add(enemy);
                            _totalSpawned++;
                        }

                        if (i < entry.Count - 1 && entry.DelayBetweenSpawns > 0f)
                            yield return new WaitForSeconds(entry.DelayBetweenSpawns);
                    }
                }

                _allGroupsSpawned = true;
            }
        }

        RefreshAliveCount();

        while (_state == WaveState.Active)
        {
            _timeRemaining -= Time.deltaTime;
            RefreshAliveCount();

            if (WaveDefinition.ShouldComplete(_timeRemaining, AliveEnemyCount, _totalSpawned, _allGroupsSpawned))
            {
                CompleteWave(_timeRemaining <= 0f);
                yield break;
            }

            yield return null;
        }
    }

    private void RefreshAliveCount()
    {
        var alive = 0;

        for (var i = _spawnedEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = _spawnedEnemies[i];
            if (enemy == null)
            {
                _spawnedEnemies.RemoveAt(i);
                continue;
            }

            var health = enemy.GetComponent<Health>();
            if (health != null && health.IsAlive)
                alive++;
        }

        for (var i = _spawnedBossHealth.Count - 1; i >= 0; i--)
        {
            var bossHealth = _spawnedBossHealth[i];
            if (bossHealth == null)
            {
                _spawnedBossHealth.RemoveAt(i);
                continue;
            }

            if (bossHealth.IsAlive)
                alive++;
        }

        AliveEnemyCount = alive;
    }

    private void CompleteWave(bool endedByTimer)
    {
        var flow = FindAnyObjectByType<GameFlowManager>();
        if (flow != null &&
            (flow.State == GameFlowState.RunFailed || flow.State == GameFlowState.RunWon))
            return;

        DespawnAllWaveActors();
        _state = WaveState.Completed;
        _timeRemaining = Mathf.Max(0f, _timeRemaining);
        AliveEnemyCount = 0;
        WaveCompleted?.Invoke(waveDefinition, endedByTimer);
        Debug.Log($"[WaveManager] Fala {waveDefinition.WaveNumber} zakończona " +
                  $"({(endedByTimer ? "czas" : "wyczyść")}).");
    }

    private void DespawnAllWaveActors()
    {
        foreach (var enemy in _spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        foreach (var bossHealth in _spawnedBossHealth)
        {
            if (bossHealth != null)
                Destroy(bossHealth.gameObject);
        }

        _spawnedEnemies.Clear();
        _spawnedBossHealth.Clear();
    }

    private Vector3 GetLaneSpawnPosition(AttackLineId lane, int offsetIndex)
    {
        if (mapBuilder != null)
            return mapBuilder.GetLaneSpawnPosition(lane, offsetIndex, laneSpawnSpread);

        return lane switch
        {
            AttackLineId.Left => new Vector3(-16f + offsetIndex * laneSpawnSpread, 1f, 26f),
            AttackLineId.Right => new Vector3(16f - offsetIndex * laneSpawnSpread, 1f, 26f),
            _ => new Vector3(offsetIndex * laneSpawnSpread, 1f, 26f)
        };
    }
}
