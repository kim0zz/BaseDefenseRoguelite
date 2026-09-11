using System;
using UnityEngine;

/// <summary>
/// Pojedyncza grupa spawnu w definicji fali (M4).
/// </summary>
[Serializable]
public class WaveSpawnEntry
{
    [SerializeField] private EnemyDefinition enemyDefinition;
    [SerializeField] private AttackLineId lane = AttackLineId.Center;
    [SerializeField] private int count = 1;
    [SerializeField] private float startDelaySeconds;
    [SerializeField] private float delayBetweenSpawns = 0.75f;
    [SerializeField] private EliteModifier eliteModifier = EliteModifier.None;

    public EnemyDefinition EnemyDefinition => enemyDefinition;
    public AttackLineId Lane => lane;
    public int Count => count;
    public float StartDelaySeconds => startDelaySeconds;
    public float DelayBetweenSpawns => delayBetweenSpawns;
    public EliteModifier EliteModifier => eliteModifier;
}
