using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SiegeWaveDirector : MonoBehaviour
{
    private SiegeWaveSequenceDefinition _sequence;
    private Func<AttackLineId, int, Vector3> _spawnPosition;
    private readonly List<Health> _actors = new();
    private SiegeWaveSchedule _schedule;
    private int _players = 1;
    private bool _paused;
    public bool IsActive { get; private set; }
    public int CurrentWaveIndex { get; private set; } = -1;
    public SiegeWaveDefinition CurrentWave => _sequence != null && CurrentWaveIndex >= 0
        ? _sequence.Waves[CurrentWaveIndex] : null;
    public int AliveEnemyCount { get; private set; }
    public int TotalSpawned => _schedule?.Spawned ?? 0;
    public int TotalBudget => _schedule?.Budget ?? 0;
    public bool AllGroupsSpawned => _schedule != null && _schedule.AllSpawned;
    public int ActiveCap => _schedule?.ActiveCap ?? 0;
    public float ElapsedSeconds => _schedule?.Elapsed ?? 0f;
    public bool IsSpawningPaused => _paused;
    public event Action<SiegeWaveDefinition> WaveCompleted;
    public event Action<AttackLineId, string, float> GroupTelegraphed;
    public event Action<EnemyController> EnemySpawned;

    public void Configure(SiegeWaveSequenceDefinition sequence,
        Func<AttackLineId, int, Vector3> spawnPosition, int playerCount = 1)
    {
        if (IsActive) throw new InvalidOperationException("Najpierw zatrzymaj aktywną falę.");
        _sequence = sequence;
        _spawnPosition = spawnPosition;
        _players = Mathf.Clamp(playerCount, 1, 4);
    }

    public void StartWave(int zeroBasedIndex)
    {
        if (_sequence == null || zeroBasedIndex < 0 || zeroBasedIndex >= _sequence.Waves.Length)
            throw new ArgumentOutOfRangeException(nameof(zeroBasedIndex));
        StopAndClear();
        CurrentWaveIndex = zeroBasedIndex;
        _schedule = new SiegeWaveSchedule(CurrentWave, _players);
        _paused = false;
        IsActive = true;
    }

    public void SetSpawningPaused(bool paused) => _paused = paused;

    private void Update() => Tick(Time.deltaTime);

    /// <summary>Advances the director; exposed for deterministic editor validation.</summary>
    public void Tick(float deltaTime)
    {
        if (!IsActive) return;
        RefreshAlive();
        _schedule.Tick(deltaTime, AliveEnemyCount, _paused,
            group => GroupTelegraphed?.Invoke(group.Lane, group.Label, group.TelegraphSeconds), Spawn);
        RefreshAlive();
        if (!_paused && _schedule.IsComplete(AliveEnemyCount))
        {
            IsActive = false;
            WaveCompleted?.Invoke(CurrentWave);
        }
    }

    private bool Spawn(SiegeWaveGroup group, int index)
    {
        var position = _spawnPosition != null ? _spawnPosition(group.Lane, index)
            : new Vector3(((int)group.Lane - 1) * 8f + index % 5 - 2f, 1f, 14f);
        var enemy = EnemySpawner.Spawn(group.Enemy, position, index, group.Lane, group.Elite);
        if (enemy == null) return false;
        _actors.Add(enemy.GetComponent<Health>());
        EnemySpawned?.Invoke(enemy);
        return true;
    }

    private void RefreshAlive()
    {
        AliveEnemyCount = 0;
        for (var i = _actors.Count - 1; i >= 0; i--)
        {
            if (_actors[i] == null) { _actors.RemoveAt(i); continue; }
            if (_actors[i].IsAlive) AliveEnemyCount++;
        }
    }

    public void StopAndClear()
    {
        IsActive = false;
        foreach (var actor in _actors)
            if (actor != null) Destroy(actor.gameObject);
        _actors.Clear();
        AliveEnemyCount = 0;
    }

    private void OnDestroy() => StopAndClear();
}
