using UnityEngine;

/// <summary>
/// Wspólna warstwa fizyki umiejętności — skill i atak idą przez ten resolver (M7.5-T1, M7.6-T1).
/// </summary>
public static class ForcedMovementResolver
{
    public static ForcedMovementOutcome Apply(ForcedMovementRequest request, GameObject target)
    {
        if (target == null) return ForcedMovementOutcome.None;

        var immunity = target.GetComponent<CrowdControlImmunity>();
        if (immunity != null && immunity.Blocks(request.Mode))
            return ForcedMovementOutcome.None;

        var profile = target.GetComponent<ForcedMovementResistanceProfile>();
        var category = profile != null
            ? profile.Category
            : ForcedMovementResistanceCategory.Normal;

        return request.Mode switch
        {
            ForcedMovementMode.Knockback => ApplyKnockback(request, target, category),
            ForcedMovementMode.Pull => ApplyPull(request, target, category),
            ForcedMovementMode.Shove => ApplyShove(request, target, category),
            ForcedMovementMode.Launch => ApplyLaunch(request, target, category),
            ForcedMovementMode.Charge => ApplyCharge(request, target, category),
            ForcedMovementMode.Stun => ApplyStun(request, target, category, immunity),
            _ => ForcedMovementOutcome.None
        };
    }

    public static ForcedMovementResistanceCategory ResolveCategory(GameObject target)
    {
        if (target == null) return ForcedMovementResistanceCategory.Normal;
        var profile = target.GetComponent<ForcedMovementResistanceProfile>();
        return profile != null ? profile.Category : ForcedMovementResistanceCategory.Normal;
    }

    private static ForcedMovementOutcome ApplyKnockback(
        ForcedMovementRequest request, GameObject target, ForcedMovementResistanceCategory category)
    {
        if (request.Force <= 0f) return ForcedMovementOutcome.None;

        var scale = ForcedMovementResistanceMath.GetDisplacementScale(category);
        if (scale <= 0f) return ForcedMovementOutcome.None;

        var receiver = EnsureReceiver(target);
        var impulse = KnockbackMath.ImpulseVelocity(request.Origin, target.transform.position, request.Force * scale);
        receiver.ApplyImpulse(impulse, request.RestoreToLaneAfter);
        return new ForcedMovementOutcome(true, 0f, false);
    }

    private static ForcedMovementOutcome ApplyPull(
        ForcedMovementRequest request, GameObject target, ForcedMovementResistanceCategory category)
    {
        if (request.Force <= 0f) return ForcedMovementOutcome.None;

        var scale = ForcedMovementResistanceMath.GetDisplacementScale(category);
        if (scale <= 0f) return ForcedMovementOutcome.None;

        var receiver = EnsureReceiver(target);
        var toOrigin = request.Origin - target.transform.position;
        toOrigin.y = 0f;
        if (toOrigin.sqrMagnitude < 0.01f) return ForcedMovementOutcome.None;

        receiver.ApplyImpulse(toOrigin.normalized * (request.Force * scale), request.RestoreToLaneAfter);
        return new ForcedMovementOutcome(true, 0f, false);
    }

    private static ForcedMovementOutcome ApplyShove(
        ForcedMovementRequest request, GameObject target, ForcedMovementResistanceCategory category)
    {
        var scale = ForcedMovementResistanceMath.GetDisplacementScale(category);
        if (scale <= 0f || request.Duration <= 0f) return ForcedMovementOutcome.None;

        var receiver = EnsureReceiver(target);
        receiver.ApplyShove(
            request.Direction,
            request.Force * scale,
            request.Duration,
            request.HonorCollisions);
        return new ForcedMovementOutcome(true, 0f, false);
    }

    private static ForcedMovementOutcome ApplyLaunch(
        ForcedMovementRequest request, GameObject target, ForcedMovementResistanceCategory category)
    {
        var scale = ForcedMovementResistanceMath.GetDisplacementScale(category);
        if (scale <= 0f) return ForcedMovementOutcome.None;

        var status = target.GetComponent<StatusEffectReceiver>();
        status?.Apply(StatusEffectType.Stun, request.Duration, 1f, request.Source);
        return new ForcedMovementOutcome(true, request.Duration * scale, false);
    }

    private static ForcedMovementOutcome ApplyCharge(
        ForcedMovementRequest request, GameObject target, ForcedMovementResistanceCategory category)
    {
        // Caster charge — brak redukcji profilu celu; caster sam się porusza.
        var receiver = EnsureReceiver(target);
        receiver.StartCharge(
            request.Direction,
            request.Force,
            request.Duration,
            request.HonorCollisions);
        return new ForcedMovementOutcome(true, 0f, false);
    }

    private static ForcedMovementOutcome ApplyStun(
        ForcedMovementRequest request,
        GameObject target,
        ForcedMovementResistanceCategory category,
        CrowdControlImmunity immunity)
    {
        if (request.Duration <= 0f && request.StaggerContribution <= 0f)
            return ForcedMovementOutcome.None;

        var status = target.GetComponent<StatusEffectReceiver>();
        if (status == null) return ForcedMovementOutcome.None;

        if (ForcedMovementResistanceMath.ConvertsControlToStagger(category))
        {
            if (immunity != null && immunity.BlocksStagger)
                return ForcedMovementOutcome.None;

            var stagger = request.StaggerContribution > 0f
                ? request.StaggerContribution
                : request.Duration * 10f * ForcedMovementResistanceMath.BossStaggerContributionScale;
            status.AddStagger(stagger);
            return new ForcedMovementOutcome(true, 0f, true);
        }

        var stunScale = ForcedMovementResistanceMath.GetStunScale(category);
        if (stunScale <= 0f) return ForcedMovementOutcome.None;

        var appliedDuration = request.Duration * stunScale;
        status.Apply(StatusEffectType.Stun, appliedDuration, 1f, request.Source);
        return new ForcedMovementOutcome(true, appliedDuration, false);
    }

    private static ForcedMovementReceiver EnsureReceiver(GameObject target)
    {
        var receiver = target.GetComponent<ForcedMovementReceiver>();
        if (receiver != null) return receiver;

        // Legacy path — KnockbackReceiver zostaje dla kompatybilności AI, ale nowe żądania idą tutaj.
        receiver = target.AddComponent<ForcedMovementReceiver>();
        return receiver;
    }
}
