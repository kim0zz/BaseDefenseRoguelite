using System;
using UnityEngine;

/// <summary>
/// Modyfikatory statystyk buildu (M6 — na start stat boosty, bez złożonych efektów).
/// </summary>
[Serializable]
public struct StatModifiers
{
    [SerializeField] private float damageMultiplier;
    [SerializeField] private float attackSpeedMultiplier;
    [SerializeField] private float maxHealthBonus;
    [SerializeField] private float maxHealthMultiplier;
    [SerializeField] private float moveSpeedMultiplier;

    public float DamageMultiplier => damageMultiplier <= 0f ? 1f : damageMultiplier;
    public float AttackSpeedMultiplier => attackSpeedMultiplier <= 0f ? 1f : attackSpeedMultiplier;
    public float MaxHealthBonus => maxHealthBonus;
    public float MaxHealthMultiplier => maxHealthMultiplier <= 0f ? 1f : maxHealthMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier <= 0f ? 1f : moveSpeedMultiplier;

    public static StatModifiers Identity => default;

    public static StatModifiers Create(
        float damageMultiplier = 1f,
        float attackSpeedMultiplier = 1f,
        float maxHealthBonus = 0f,
        float maxHealthMultiplier = 1f,
        float moveSpeedMultiplier = 1f)
    {
        return new StatModifiers
        {
            damageMultiplier = damageMultiplier,
            attackSpeedMultiplier = attackSpeedMultiplier,
            maxHealthBonus = maxHealthBonus,
            maxHealthMultiplier = maxHealthMultiplier,
            moveSpeedMultiplier = moveSpeedMultiplier
        };
    }

    public static StatModifiers Combine(StatModifiers a, StatModifiers b)
    {
        return new StatModifiers
        {
            damageMultiplier = a.DamageMultiplier * b.DamageMultiplier,
            attackSpeedMultiplier = a.AttackSpeedMultiplier * b.AttackSpeedMultiplier,
            maxHealthBonus = a.MaxHealthBonus + b.MaxHealthBonus,
            maxHealthMultiplier = a.MaxHealthMultiplier * b.MaxHealthMultiplier,
            moveSpeedMultiplier = a.MoveSpeedMultiplier * b.MoveSpeedMultiplier
        };
    }
}
