using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Efekty skilli na resolve (strefa, heal, impuls).
/// </summary>
public static class SkillEffectApplier
{
    public const float HealPerHit = 4f;

    private static int _activeDamageZones;
    /// <summary>
    /// Cap wspólny (Spalona Ziemia + Rozpadlina). 18 = 3 linie × kilka trafień ze spacingiem.
    /// </summary>
    public const int MaxDamageZones = 18;

    public static int ActiveDamageZoneCount => _activeDamageZones;

    public static void NotifyDamageZoneDestroyed() =>
        _activeDamageZones = Mathf.Max(0, _activeDamageZones - 1);

    public static void ApplyOnResolve(
        SkillDefinition definition,
        IReadOnlyList<SkillEffectKind> effects,
        GameObject caster,
        Vector3 origin,
        int hitCount,
        bool hadHit)
    {
        if (effects == null || caster == null) return;

        foreach (var effect in effects)
        {
            switch (effect)
            {
                case SkillEffectKind.SpawnSlowZone:
                    if (hadHit)
                        SlowZone.Spawn(origin, definition != null ? definition.RadiusMeters : 3f, caster);
                    break;
                case SkillEffectKind.SpawnDamageZone:
                    SpawnTrackedDamageZone(origin, 3f, caster, 6f, 4f);
                    break;
                case SkillEffectKind.HealCasterOnHit:
                    if (hitCount > 0)
                    {
                        var casterHealth = caster.GetComponent<Health>();
                        casterHealth?.Heal(hitCount * HealPerHit);
                    }
                    break;
                case SkillEffectKind.PulseRingOnWallStop:
                    SpawnPulseRing(origin, caster, definition);
                    break;
            }
        }
    }

    public static bool SpawnTrackedDamageZone(
        Vector3 origin, float radius, GameObject caster, float dps, float durationSeconds)
    {
        if (_activeDamageZones >= MaxDamageZones) return false;
        SlowZone.Spawn(origin, radius, caster, dps, durationSeconds, applySlow: false, trackCapacity: true);
        _activeDamageZones++;
        return true;
    }

    public static bool IsFarEnoughFromExisting(
        Vector3 candidate, IList<Vector3> existingOrigins, float minSpacing)
    {
        if (existingOrigins == null || existingOrigins.Count == 0 || minSpacing <= 0f)
            return true;

        var minSqr = minSpacing * minSpacing;
        for (var i = 0; i < existingOrigins.Count; i++)
        {
            if ((existingOrigins[i] - candidate).sqrMagnitude < minSqr)
                return false;
        }

        return true;
    }

    public static bool TrySpawnSpacedDamageZone(
        Vector3 origin,
        float radius,
        GameObject caster,
        float dps,
        float durationSeconds,
        IList<Vector3> existingOrigins,
        float minSpacing)
    {
        if (!IsFarEnoughFromExisting(origin, existingOrigins, minSpacing))
            return false;
        if (!SpawnTrackedDamageZone(origin, radius, caster, dps, durationSeconds))
            return false;

        existingOrigins?.Add(origin);
        return true;
    }

    public static void SpawnPulseRing(Vector3 origin, GameObject caster, SkillDefinition definition)
    {
        const float bonusDamage = 14f;
        const float stunNormal = 2f;
        const float stunElite = 1f;
        const float bossStagger = 16f;
        var radius = definition != null ? Mathf.Max(2.5f, definition.RadiusMeters * 0.5f) : 2.5f;
        var hits = SkillCircleOverlap.Query(origin, radius, 8, ~0, 0);
        foreach (var hit in hits)
        {
            if (hit.GameObject == null || hit.GameObject == caster) continue;
            if (hit.Damageable == null) continue;
            hit.Damageable.TakeDamage(bonusDamage, caster);

            var profile = hit.GameObject.GetComponentInParent<ForcedMovementResistanceProfile>();
            var category = profile != null ? profile.Category : ForcedMovementResistanceCategory.Normal;
            var stun = category == ForcedMovementResistanceCategory.Elite ? stunElite : stunNormal;
            var stagger = category == ForcedMovementResistanceCategory.Boss ? bossStagger : 0f;
            var control = ForcedMovementRequest.StunControl(caster, stun, stagger);
            ForcedMovementResolver.Apply(control, hit.GameObject);
        }
    }
}
