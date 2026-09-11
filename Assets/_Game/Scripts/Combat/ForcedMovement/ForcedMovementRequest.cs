using UnityEngine;

/// <summary>
/// Kontrakt żądania wymuszonego ruchu / kontroli (M7.5-T1).
/// </summary>
public readonly struct ForcedMovementRequest
{
    public ForcedMovementMode Mode { get; }
    public GameObject Source { get; }
    public Vector3 Origin { get; }
    public Vector3 Direction { get; }
    public float Force { get; }
    public float Duration { get; }
    public bool HonorCollisions { get; }
    public bool RestoreToLaneAfter { get; }
    public float StaggerContribution { get; }

    public ForcedMovementRequest(
        ForcedMovementMode mode,
        GameObject source,
        Vector3 origin,
        Vector3 direction,
        float force,
        float duration,
        bool honorCollisions = true,
        bool restoreToLaneAfter = false,
        float staggerContribution = 0f)
    {
        Mode = mode;
        Source = source;
        Origin = origin;
        Direction = direction;
        Force = force;
        Duration = duration;
        HonorCollisions = honorCollisions;
        RestoreToLaneAfter = restoreToLaneAfter;
        StaggerContribution = staggerContribution;
    }

    public static ForcedMovementRequest KnockbackFromOrigin(
        GameObject source, Vector3 origin, float force, bool restoreToLaneAfter = false)
    {
        return new ForcedMovementRequest(
            ForcedMovementMode.Knockback,
            source,
            origin,
            Vector3.zero,
            force,
            0f,
            restoreToLaneAfter: restoreToLaneAfter);
    }

    public static ForcedMovementRequest StunControl(
        GameObject source, float duration, float staggerContribution = 0f)
    {
        return new ForcedMovementRequest(
            ForcedMovementMode.Stun,
            source,
            Vector3.zero,
            Vector3.zero,
            0f,
            duration,
            staggerContribution: staggerContribution);
    }

    public static ForcedMovementRequest ShoveAlong(
        GameObject source,
        Vector3 direction,
        float speed,
        float duration,
        bool honorCollisions = true)
    {
        return new ForcedMovementRequest(
            ForcedMovementMode.Shove,
            source,
            Vector3.zero,
            direction,
            speed,
            duration,
            honorCollisions);
    }

    public static ForcedMovementRequest CasterCharge(
        GameObject caster,
        Vector3 direction,
        float speed,
        float duration,
        bool honorCollisions = true)
    {
        return new ForcedMovementRequest(
            ForcedMovementMode.Charge,
            caster,
            Vector3.zero,
            direction,
            speed,
            duration,
            honorCollisions);
    }
}
