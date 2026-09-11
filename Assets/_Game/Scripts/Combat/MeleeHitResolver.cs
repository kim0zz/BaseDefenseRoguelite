using UnityEngine;

/// <summary>
/// Reguły trafienia melee (łuk + zasięg) — bez MonoBehaviour.
/// </summary>
public static class MeleeHitResolver
{
    public const float OverlapHitEpsilon = 0.01f;

    public static bool IsInMeleeArc(
        Vector3 origin,
        Vector3 forward,
        Vector3 targetPosition,
        float range,
        float arcDegrees)
    {
        var toTarget = targetPosition - origin;
        toTarget.y = 0f;
        var distance = toTarget.magnitude;
        if (distance < OverlapHitEpsilon || distance > range) return false;
        if (forward.sqrMagnitude < 0.01f) return false;
        return Vector3.Angle(forward, toTarget.normalized) <= arcDegrees * 0.5f;
    }

    /// <summary>
    /// Trafienie względem punktu na powierzchni (ClosestPoint).
    /// Nakładka / środek w origin = hit — duży boss nie jest nietrafialny od środka.
    /// </summary>
    public static bool IsHitPointInMeleeArc(
        Vector3 origin,
        Vector3 forward,
        Vector3 hitPoint,
        float range,
        float arcDegrees)
    {
        var toHit = hitPoint - origin;
        toHit.y = 0f;
        if (toHit.sqrMagnitude <= OverlapHitEpsilon * OverlapHitEpsilon)
            return true;

        return IsInMeleeArc(origin, forward, hitPoint, range, arcDegrees);
    }
}
