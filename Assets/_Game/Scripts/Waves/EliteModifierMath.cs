using UnityEngine;

/// <summary>
/// Matematyka modyfikatorów elit — bez MonoBehaviour (testowalne).
/// </summary>
public static class EliteModifierMath
{
    public static float ApplyMaxHealthMultiplier(EliteModifier modifier, float baseMaxHealth)
    {
        return modifier switch
        {
            EliteModifier.Armored => baseMaxHealth * 1.5f,
            _ => baseMaxHealth
        };
    }

    public static float ApplyMoveSpeedMultiplier(EliteModifier modifier, float baseMoveSpeed)
    {
        return modifier switch
        {
            EliteModifier.Frenzy => baseMoveSpeed * 1.25f,
            _ => baseMoveSpeed
        };
    }

    public static float ApplyAttackIntervalMultiplier(EliteModifier modifier, float baseInterval)
    {
        return modifier switch
        {
            EliteModifier.Frenzy => baseInterval * 0.75f,
            _ => baseInterval
        };
    }

    public static bool CountsAsEliteForLoot(EliteModifier modifier, EnemyKind kind)
    {
        if (modifier != EliteModifier.None) return true;
        return kind == EnemyKind.Hunter;
    }

    public static float GetDeathAoeDamage(EliteModifier modifier)
    {
        return modifier == EliteModifier.Unstable ? 12f : 0f;
    }

    public static float GetDeathAoeRadius(EliteModifier modifier)
    {
        return modifier == EliteModifier.Unstable ? 2.5f : 0f;
    }
}
