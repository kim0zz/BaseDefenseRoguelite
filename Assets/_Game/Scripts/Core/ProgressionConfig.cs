using System;
using UnityEngine;

/// <summary>
/// Progi EXP, ekonomia bazy i nagrody — wartości konfiguracyjne (M5).
/// </summary>
[CreateAssetMenu(fileName = "ProgressionConfig", menuName = "Game/Progression/Progression Config")]
public class ProgressionConfig : ScriptableObject
{
    [Header("Wspólny EXP — łączne progi awansu (poziom 2–5)")]
    [SerializeField] private int[] cumulativeExpThresholds = { 0, 60, 140, 260, 420 };

    [Header("Wieże (M7 DRAFT)")]
    [SerializeField] private float towerMaxHealth = 120f;

    [Header("Baza")]
    [SerializeField] private float baseMaxHealth = 500f;
    [SerializeField] [Range(0f, 1f)] private float baseStartHealthPercent = 0.65f;
    [SerializeField] private int repairGoldCost = 30;
    [SerializeField] private float repairHealAmount = 80f;

    [Header("Domyślne nagrody (gdy brak w EnemyDefinition)")]
    [SerializeField] private int defaultExpReward = 8;
    [SerializeField] private int defaultGoldReward = 5;

    public float TowerMaxHealth => towerMaxHealth;
    public float BaseMaxHealth => baseMaxHealth;
    public float BaseStartHealthPercent => baseStartHealthPercent;
    public int RepairGoldCost => repairGoldCost;
    public float RepairHealAmount => repairHealAmount;
    public int DefaultExpReward => defaultExpReward;
    public int DefaultGoldReward => defaultGoldReward;

    public int GetTeamLevelForExp(int totalExp)
    {
        var level = 1;
        if (cumulativeExpThresholds == null || cumulativeExpThresholds.Length == 0)
            return level;

        for (var i = 0; i < cumulativeExpThresholds.Length; i++)
        {
            if (totalExp >= cumulativeExpThresholds[i])
                level = i + 1;
        }

        return level;
    }

    public int GetExpRequiredForLevel(int level)
    {
        if (cumulativeExpThresholds == null || level <= 1)
            return 0;

        var index = Mathf.Clamp(level - 1, 0, cumulativeExpThresholds.Length - 1);
        return cumulativeExpThresholds[index];
    }

    public int GetExpToNextLevel(int totalExp, int currentLevel)
    {
        var nextLevel = currentLevel + 1;
        if (cumulativeExpThresholds == null || nextLevel > cumulativeExpThresholds.Length)
            return 0;

        return Mathf.Max(0, cumulativeExpThresholds[nextLevel - 1] - totalExp);
    }
}
