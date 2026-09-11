using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure helpers for persistent effect runtime — EditMode testable (M8.1b).
/// </summary>
public static class PersistentEffectRuntime
{
    public static bool LastChanceSurvives(float healAccum, float threshold) => healAccum >= threshold;

    public static float ApplyPozeraczExtend(float currentExtra, float extend, float maxExtra)
    {
        if (currentExtra >= maxExtra) return 0f;
        return Mathf.Min(extend, maxExtra - currentExtra);
    }

    public static float ComputeKrwawaHeal(float currentAccum, float perHit, float cap)
    {
        if (currentAccum >= cap) return 0f;
        return Mathf.Min(perHit, cap - currentAccum);
    }

    public static bool CanChainIgniteTarget<T>(ISet<T> alreadyIgnited, T targetId) where T : notnull =>
        !alreadyIgnited.Contains(targetId);

    public struct AuraRampSimState
    {
        public int ExtraTicks;
        public float GraceTimer;
    }

    public static void ApplyAuraEnemyTick(ref AuraRampSimState ramp, int rampStep, int maxExtra)
    {
        ramp.GraceTimer = 0f;
        ramp.ExtraTicks = Mathf.Min(maxExtra, ramp.ExtraTicks + rampStep);
    }

    public static float AuraDamageForTick(float baseDamage, int extraTicks) => baseDamage + extraTicks;

    public static void ApplyAuraGraceTick(ref AuraRampSimState ramp, float tickInterval) =>
        ramp.GraceTimer += tickInterval;

    public static bool ShouldClearAuraRamp(AuraRampSimState ramp, float graceSeconds) =>
        ramp.GraceTimer >= graceSeconds;

    public static bool ShouldSpawnMoveShockwave(
        float distanceSinceLast, float timeSinceLast, float distanceThreshold, float timeThreshold) =>
        distanceSinceLast >= distanceThreshold || timeSinceLast >= timeThreshold;

    public static int PulseAgonyTargets(
        Vector3 center, float radius, float damage, GameObject source, LayerMask layers)
    {
        var hits = Physics.OverlapSphere(center + Vector3.up * 0.5f, radius, layers);
        var count = 0;
        foreach (var col in hits)
        {
            if (col == null) continue;
            if (col.GetComponentInParent<PlayerCharacter>() != null) continue;
            var health = col.GetComponentInParent<Health>();
            if (health == null || !health.IsAlive) continue;
            health.TakeDamage(damage, source);
            count++;
        }

        return count;
    }

    public static int PulseMoveShockwave(
        Vector3 center, float radius, float damage, float staggerSeconds,
        GameObject source, LayerMask layers)
    {
        var hits = Physics.OverlapSphere(center + Vector3.up * 0.5f, radius, layers);
        var count = 0;
        foreach (var col in hits)
        {
            if (col == null) continue;
            if (col.GetComponentInParent<PlayerCharacter>() != null) continue;
            var health = col.GetComponentInParent<Health>();
            if (health == null || !health.IsAlive) continue;
            var dmg = PlayerPersistentEffects.ModifyOutgoingDamage(source, damage);
            health.TakeDamage(dmg, source);
            if (staggerSeconds > 0f)
            {
                var stun = ForcedMovementRequest.StunControl(source, staggerSeconds, 0f);
                ForcedMovementResolver.Apply(stun, col.gameObject);
            }

            count++;
        }

        return count;
    }

    public static void ApplyKolosShove(
        Vector3 center, float queryRadius, Vector3 moveDirection,
        float normalDist, float eliteDist, float shoveDuration,
        GameObject source)
    {
        var hits = Physics.OverlapSphere(center + Vector3.up * 0.5f, queryRadius);
        foreach (var col in hits)
        {
            if (col == null) continue;
            if (col.GetComponentInParent<PlayerCharacter>() != null) continue;
            var profile = col.GetComponentInParent<ForcedMovementResistanceProfile>();
            var category = profile != null ? profile.Category : ForcedMovementResistanceCategory.Normal;
            var dist = category switch
            {
                ForcedMovementResistanceCategory.Boss => 0f,
                ForcedMovementResistanceCategory.Elite => eliteDist,
                _ => normalDist
            };
            if (dist <= 0f) continue;

            var dir = moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : Vector3.forward;
            var speed = dist / Mathf.Max(0.01f, shoveDuration);
            var shove = ForcedMovementRequest.ShoveAlong(source, dir, speed, shoveDuration, honorCollisions: true);
            ForcedMovementResolver.Apply(shove, col.gameObject);
        }
    }
}
