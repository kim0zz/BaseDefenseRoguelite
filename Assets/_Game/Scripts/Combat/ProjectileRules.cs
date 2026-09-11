using UnityEngine;

/// <summary>
/// Filtr celów pocisku — wrogowie, nie sojusznicy / baza / loot.
/// </summary>
public static class ProjectileRules
{
    public const float ProjectileSpeed = 20f;
    public const float OverlapRadius = 0.28f;

    public static bool HasReachedMaxDistance(float travelled, float maxRange)
    {
        return travelled >= maxRange;
    }

    public static bool IsValidTarget(Component hit, GameObject owner)
    {
        if (hit == null || owner == null) return false;

        var hitTransform = hit.transform;
        if (hitTransform == owner.transform || hitTransform.IsChildOf(owner.transform))
            return false;

        if (hit.GetComponentInParent<PlayerCharacter>() != null)
            return false;

        var enemy = hit.GetComponentInParent<EnemyController>();
        if (enemy == null) return false;

        var damageable = hit.GetComponentInParent<IDamageable>();
        return damageable != null && damageable.IsAlive;
    }
}
