using UnityEngine;

/// <summary>
/// Definicja fali — czas trwania i grupy spawnu (data-driven, M4).
/// </summary>
[CreateAssetMenu(fileName = "WaveDefinition", menuName = "Game/Waves/Wave Definition")]
public class WaveDefinition : ScriptableObject
{
    [SerializeField] private int waveNumber = 1;
    [SerializeField] private float durationSeconds = 120f;
    [SerializeField] private bool isBossWave;
    [SerializeField] private BossDefinition bossDefinition;
    [SerializeField] private WardenDefinition wardenDefinition;
    [SerializeField] private WaveSpawnEntry[] spawnEntries;

    public int WaveNumber => waveNumber;
    public float DurationSeconds => durationSeconds;
    public bool IsBossWave => isBossWave;
    public BossDefinition BossDefinition => bossDefinition;
    public WardenDefinition WardenDefinition => wardenDefinition;
    public WaveSpawnEntry[] SpawnEntries => spawnEntries;

    /// <summary>
    /// FROZEN: fala kończy się po czasie albo po wybiciu wszystkich wrogów.
    /// </summary>
    public static bool ShouldComplete(float timeRemaining, int aliveEnemies, int totalSpawned, bool allGroupsSpawned)
    {
        if (timeRemaining <= 0f)
            return true;

        if (allGroupsSpawned && totalSpawned > 0 && aliveEnemies <= 0)
            return true;

        return false;
    }
}
