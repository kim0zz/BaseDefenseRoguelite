using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rozwiązuje trafienia i efekty skilla przez wspólną fizykę (M7.5-T2/T3, M8.1b Byk).
/// </summary>
public static class SkillHitResolver
{
    private static int _sideShoveAlternate;

    public struct ResolveResult
    {
        public int HitCount;
        public float TotalDamage;
        public float TotalControlSeconds;
        public int DistinctLaneCount;
        public bool HadHit;
    }

    public static ResolveResult ApplyHits(
        SkillDefinition definition,
        IReadOnlyList<SkillCircleOverlap.HitTarget> hits,
        GameObject caster,
        IReadOnlyList<SkillEffectKind> effects,
        Vector3? effectOrigin = null)
    {
        var result = new ResolveResult();
        if (definition == null || hits == null || caster == null) return result;
        var zoneCenter = effectOrigin ?? caster.transform.position;

        var lanes = new HashSet<AttackLineId>();
        var applied = 0;

        foreach (var hit in hits)
        {
            if (!SkillTargetFilter.IsValidTarget(hit.GameObject, definition, caster)) continue;

            if (definition.Damage > 0f)
            {
                var damage = PlayerPersistentEffects.ModifyOutgoingDamage(caster, definition.Damage);
                hit.Damageable.TakeDamage(damage, caster);
                hit.GameObject.GetComponentInParent<HitFlashFeedback>()?.PlayFlash();
            }

            if (definition.KnockbackForce > 0f)
            {
                var knockback = ForcedMovementRequest.KnockbackFromOrigin(
                    caster, caster.transform.position, definition.KnockbackForce, restoreToLaneAfter: false);
                ForcedMovementResolver.Apply(knockback, hit.GameObject);
            }

            if (definition.ControlMode == SkillControlMode.Stun && definition.ControlDurationSeconds > 0f)
            {
                var stun = ForcedMovementRequest.StunControl(
                    caster,
                    definition.ControlDurationSeconds,
                    definition.BossStaggerContribution);
                var outcome = ForcedMovementResolver.Apply(stun, hit.GameObject);
                if (outcome.Applied)
                    result.TotalControlSeconds += outcome.AppliedControlSeconds;
            }

            lanes.Add(hit.LaneId);
            result.TotalDamage += definition.Damage;
            applied++;
        }

        result.HitCount = applied;
        result.HadHit = applied > 0;
        result.DistinctLaneCount = lanes.Count;

        if (effects != null)
            SkillEffectApplier.ApplyOnResolve(definition, effects, caster, zoneCenter, result.HitCount, result.HadHit);

        return result;
    }

    public static ResolveResult ApplyOncePerTargetHits(
        SkillDefinition definition,
        IReadOnlyList<SkillChargeCapsuleOverlap.HitTarget> hits,
        GameObject caster,
        Vector3 shoveDirection,
        float shoveDurationRemaining,
        SkillOncePerTargetSet oncePerTarget,
        IReadOnlyList<SkillEffectKind> effects,
        Vector3? effectOrigin = null)
    {
        var result = new ResolveResult();
        if (definition == null || hits == null || caster == null || oncePerTarget == null) return result;
        var zoneCenter = effectOrigin ?? caster.transform.position;

        var lanes = new HashSet<AttackLineId>();
        var applied = 0;

        foreach (var hit in hits)
        {
            if (!SkillTargetFilter.IsValidTarget(hit.GameObject, definition, caster)) continue;
            if (!oncePerTarget.TryMark(hit.GameObject)) continue;

            if (definition.Damage > 0f)
            {
                var damage = PlayerPersistentEffects.ModifyOutgoingDamage(caster, definition.Damage);
                hit.Damageable.TakeDamage(damage, caster);
                hit.GameObject.GetComponentInParent<HitFlashFeedback>()?.PlayFlash();
            }

            ApplyChargeContact(definition, caster, hit, shoveDirection, shoveDurationRemaining);

            if (definition.BossStaggerContribution > 0f
                && hit.GameObject.GetComponentInParent<BossRamController>() != null)
            {
                var stagger = ForcedMovementRequest.StunControl(
                    caster,
                    0f,
                    definition.BossStaggerContribution);
                ForcedMovementResolver.Apply(stagger, hit.GameObject);
            }

            lanes.Add(hit.LaneId);
            result.TotalDamage += definition.Damage;
            applied++;
        }

        result.HitCount = applied;
        result.HadHit = applied > 0;
        result.DistinctLaneCount = lanes.Count;

        if (effects != null)
            SkillEffectApplier.ApplyOnResolve(definition, effects, caster, zoneCenter, result.HitCount, result.HadHit);

        return result;
    }

    private static void ApplyChargeContact(
        SkillDefinition definition,
        GameObject caster,
        SkillChargeCapsuleOverlap.HitTarget hit,
        Vector3 chargeDirection,
        float shoveDurationRemaining)
    {
        if (shoveDurationRemaining <= 0f) return;

        var profile = hit.GameObject.GetComponentInParent<ForcedMovementResistanceProfile>();
        var category = profile != null ? profile.Category : ForcedMovementResistanceCategory.Normal;

        if (definition.ChargeContactMode == ChargeContactMode.Carry
            && category == ForcedMovementResistanceCategory.Normal)
        {
            ChargeCarryTracker.Track(caster, hit.GameObject);
            return;
        }

        if (definition.ChargeContactMode == ChargeContactMode.SideShove
            && category == ForcedMovementResistanceCategory.Normal)
        {
            var side = Vector3.Cross(Vector3.up, chargeDirection).normalized;
            if (_sideShoveAlternate++ % 2 == 0) side = -side;
            var shove = ForcedMovementRequest.ShoveAlong(caster, side, 1.2f / 0.20f, 0.20f, honorCollisions: true);
            ForcedMovementResolver.Apply(shove, hit.GameObject);
            return;
        }

        if (definition.ChargeContactMode == ChargeContactMode.SideShove
            && category == ForcedMovementResistanceCategory.Elite)
        {
            var forward = ForcedMovementRequest.ShoveAlong(caster, chargeDirection, 0.35f / 0.20f, 0.20f, honorCollisions: true);
            ForcedMovementResolver.Apply(forward, hit.GameObject);
            ApplyCasterBrake(caster, 0.40f);
            return;
        }

        if (definition.ControlMode == SkillControlMode.Shove)
        {
            var shove = ForcedMovementRequest.ShoveAlong(
                caster, chargeDirection, definition.ChargeSpeed, shoveDurationRemaining, honorCollisions: true);
            ForcedMovementResolver.Apply(shove, hit.GameObject);
        }
    }

    private static void ApplyCasterBrake(GameObject caster, float speedMultiplier)
    {
        var receiver = caster.GetComponent<ForcedMovementReceiver>();
        receiver?.ApplySpeedMultiplier(speedMultiplier);
    }
}
