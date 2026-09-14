using System;
using UnityEngine;

[Serializable]
public sealed class SiegeWaveGroup
{
    public string Label;
    public AttackLineId Lane;
    public EnemyDefinition Enemy;
    [Min(1)] public int Count = 1;
    [Min(0f)] public float StartSeconds;
    [Min(0f)] public float TelegraphSeconds = 2.5f;
    [Min(0.01f)] public float SpawnInterval = 0.3f;
    public EliteModifier Elite;
}

[CreateAssetMenu(fileName = "SiegeWave", menuName = "Game/Siege/Fala")]
public sealed class SiegeWaveDefinition : ScriptableObject
{
    public string Title;
    public string Description;
    [Min(1)] public int ActiveCap = 30;
    public SiegeWaveGroup[] Groups = Array.Empty<SiegeWaveGroup>();

    public int GetBudget(int players)
    {
        var total = 0;
        foreach (var group in Groups)
            if (group != null) total += SiegeWaveSchedule.ScaledCount(group.Count, players);
        return total;
    }
}
