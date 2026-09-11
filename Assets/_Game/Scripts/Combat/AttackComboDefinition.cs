using System;
using UnityEngine;

/// <summary>
/// Per-hit combo data for melee weapons (M8.1b).
/// </summary>
[Serializable]
public class ComboHitDefinition
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float rangeMeters = 2.2f;
    [SerializeField] private float arcDegrees = 140f;
    [SerializeField] private float stagger = 4f;
    [SerializeField] private float knockback = 0.8f;
    [SerializeField] private float windupSeconds = 0.28f;
    [SerializeField] private float activeSeconds = 0.10f;
    [SerializeField] private float recoverySeconds = 0.28f;
    [SerializeField] private int maxTargets = 6;

    public float Damage => damage;
    public float RangeMeters => rangeMeters;
    public float ArcDegrees => arcDegrees;
    public float Stagger => stagger;
    public float Knockback => knockback;
    public float WindupSeconds => windupSeconds;
    public float ActiveSeconds => activeSeconds;
    public float RecoverySeconds => recoverySeconds;
    public int MaxTargets => maxTargets;
    public float TotalSeconds => windupSeconds + activeSeconds + recoverySeconds;

    public static ComboHitDefinition Create(
        float dmg, float range, float arc, float staggerVal, float kb,
        float windup, float active, float recovery, int targets)
    {
        return new ComboHitDefinition
        {
            damage = dmg, rangeMeters = range, arcDegrees = arc,
            stagger = staggerVal, knockback = kb,
            windupSeconds = windup, activeSeconds = active, recoverySeconds = recovery,
            maxTargets = targets
        };
    }
}

[Serializable]
public class AttackComboDefinition
{
    [SerializeField] private ComboHitDefinition[] hits;
    [SerializeField] private float comboResetSeconds = 1.25f;

    public ComboHitDefinition[] Hits => hits ?? Array.Empty<ComboHitDefinition>();
    public int HitCount => Hits.Length;
    public float ComboResetSeconds => comboResetSeconds;
    public float TotalCycleSeconds
    {
        get
        {
            var sum = 0f;
            foreach (var hit in Hits) sum += hit.TotalSeconds;
            return sum;
        }
    }

    public static AttackComboDefinition CreateBulawaBaseline()
    {
        return new AttackComboDefinition
        {
            comboResetSeconds = 1.25f,
            hits = new[]
            {
                ComboHitDefinition.Create(10f, 2.2f, 140f, 4f, 0.8f, 0.28f, 0.10f, 0.28f, 6),
                ComboHitDefinition.Create(10f, 2.2f, 140f, 4f, 0.8f, 0.28f, 0.10f, 0.28f, 6),
                ComboHitDefinition.Create(16f, 2.8f, 160f, 10f, 1.2f, 0.35f, 0.14f, 0.35f, 8)
            }
        };
    }
}
