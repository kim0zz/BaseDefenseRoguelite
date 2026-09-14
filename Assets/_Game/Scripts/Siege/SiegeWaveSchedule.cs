using System;
using UnityEngine;

/// <summary>Skończony harmonogram: pełny limit zatrzymuje emisję, ale nie usuwa budżetu.</summary>
public sealed class SiegeWaveSchedule
{
    private readonly SiegeWaveDefinition _wave;
    private readonly int[] _remaining;
    private readonly float[] _nextSpawn;
    private readonly bool[] _announced;
    public float Elapsed { get; private set; }
    public int Spawned { get; private set; }
    public int Budget { get; }
    public bool AllSpawned => Spawned >= Budget;
    public int ActiveCap { get; }

    public SiegeWaveSchedule(SiegeWaveDefinition wave, int players)
    {
        _wave = wave != null ? wave : throw new ArgumentNullException(nameof(wave));
        _remaining = new int[wave.Groups.Length];
        _nextSpawn = new float[wave.Groups.Length];
        _announced = new bool[wave.Groups.Length];
        ActiveCap = Mathf.Clamp(Mathf.RoundToInt(wave.ActiveCap * (1f + .12f * (Mathf.Clamp(players, 1, 4) - 1))), 1, 80);
        for (var i = 0; i < wave.Groups.Length; i++)
        {
            var group = wave.Groups[i];
            if (group == null || group.Enemy == null || group.Count <= 0)
                throw new ArgumentException("Niepoprawna grupa w fali Siege.");
            _remaining[i] = ScaledCount(group.Count, players);
            Budget += _remaining[i];
            // Telegraph is presentation only. The first unit is eligible at the
            // group's start time; delaying it by the telegraph made zero-start
            // groups silently miss the first tick.
            _nextSpawn[i] = Mathf.Max(0f, group.StartSeconds);
        }
    }

    public static int ScaledCount(int count, int players) => count <= 0 ? 0 :
        Mathf.Max(1, Mathf.RoundToInt(count * (1f + .3f * (Mathf.Clamp(players, 1, 4) - 1))));

    public void Tick(float deltaTime, int alive, bool paused, Action<SiegeWaveGroup> announce,
        Func<SiegeWaveGroup, int, bool> spawn)
    {
        if (paused || AllSpawned) return;
        Elapsed += Mathf.Max(0f, deltaTime);
        for (var i = 0; i < _remaining.Length; i++)
        {
            var group = _wave.Groups[i];
            if (!_announced[i] && Elapsed >= _nextSpawn[i] - Mathf.Max(0f, group.TelegraphSeconds))
            {
                _announced[i] = true;
                announce?.Invoke(group);
            }
            if (_remaining[i] <= 0 || Elapsed < _nextSpawn[i] || alive >= ActiveCap) continue;
            // Catch up due emissions while there is room, but never consume
            // pending budget while the active cap is full.
            while (_remaining[i] > 0 && Elapsed >= _nextSpawn[i] && alive < ActiveCap)
            {
                if (spawn == null || !spawn(group, Spawned)) break;
                _remaining[i]--;
                Spawned++;
                alive++;
                _nextSpawn[i] += Mathf.Max(.01f, group.SpawnInterval);
            }
        }
    }

    public bool IsComplete(int alive) => AllSpawned && alive == 0;
}
