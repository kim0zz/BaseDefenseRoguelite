using UnityEngine;

/// <summary>
/// Wyliczone staty bojowe gracza po zastosowaniu buildu (M6).
/// </summary>
public struct EffectiveCombatStats
{
    public float Damage;
    public float AttackInterval;
    public float Range;
    public float ArcDegrees;
    public int MaxTargets;
    public float MaxHealth;
    public float MoveSpeed;
}

/// <summary>
/// Agregacja statów z klasy, broni, itemów i talentów (M6).
/// </summary>
public static class BuildStatCalculator
{
    public const int WeaponSlotCount = 2;
    public const int ItemSlotCount = 6;

    public static EffectiveCombatStats Compute(
        ClassDefinition classDef,
        WeaponDefinition activeWeapon,
        ItemDefinition[] items,
        TalentDefinition[] talents,
        int teamLevel)
    {
        var mods = StatModifiers.Identity;

        if (classDef != null)
        {
            // Klasa wpływa na HP/ruch przez bazę — modyfikatory z itemów/talentów/broni osobno.
        }

        if (activeWeapon != null)
            mods = StatModifiers.Combine(mods, activeWeapon.BonusStats);

        if (items != null)
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                mods = StatModifiers.Combine(mods, item.StatModifiers);
            }
        }

        if (talents != null)
        {
            foreach (var talent in talents)
            {
                if (talent == null) continue;
                mods = StatModifiers.Combine(mods, talent.StatModifiers);
            }
        }

        var profile = activeWeapon != null ? activeWeapon.MeleeProfile : null;
        var baseDamage = profile != null ? profile.Damage : 10f;
        var baseInterval = profile != null ? profile.AttackInterval : 0.8f;
        var baseRange = profile != null ? profile.Range : 2f;
        var baseArc = profile != null ? profile.ArcDegrees : 120f;
        var baseTargets = profile != null ? profile.MaxTargets : 3;

        var baseHp = classDef != null ? classDef.BaseMaxHealth : 100f;
        var baseMove = classDef != null ? classDef.BaseMoveSpeed : 5f;
        if (classDef != null && teamLevel > 1)
        {
            var levelsAboveOne = teamLevel - 1;
            baseHp += classDef.MaxHealthPerTeamLevel * levelsAboveOne;
            baseMove += classDef.MoveSpeedPerTeamLevel * levelsAboveOne;
        }

        return new EffectiveCombatStats
        {
            Damage = baseDamage * mods.DamageMultiplier,
            AttackInterval = baseInterval / mods.AttackSpeedMultiplier,
            Range = baseRange,
            ArcDegrees = baseArc,
            MaxTargets = baseTargets,
            MaxHealth = baseHp * mods.MaxHealthMultiplier + mods.MaxHealthBonus,
            MoveSpeed = baseMove * mods.MoveSpeedMultiplier
        };
    }
}
