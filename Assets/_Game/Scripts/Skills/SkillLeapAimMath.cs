using UnityEngine;

/// <summary>
/// Celowanie skoku Trzaśnięcia (LeapStomp) — testowalne bez sceny (M7.6-T9).
/// </summary>
public static class SkillLeapAimMath
{
    public const float MaxLeapDistance = 6.5f;
    public const float MinLeapDistance = 0f;
    public const float FeetLandingEpsilon = 0.05f;

    public static float ClampDistance(float distance, float maxDistance = MaxLeapDistance)
    {
        return Mathf.Clamp(distance, MinLeapDistance, maxDistance);
    }

    public static Vector3 ResolvePointFromDirection(
        Vector3 casterFeet,
        Vector3 aimDirection,
        float distance,
        float maxDistance = MaxLeapDistance)
    {
        var clampedDistance = ClampDistance(distance, maxDistance);
        if (clampedDistance <= FeetLandingEpsilon)
            return FlattenFeet(casterFeet);

        var direction = AimMath.FlattenXz(aimDirection, Vector3.forward);
        return FlattenFeet(casterFeet + direction * clampedDistance);
    }

    public static bool TryResolveKeyboardPoint(
        Ray ray,
        float groundY,
        Vector3 casterFeet,
        float maxDistance,
        out Vector3 landingPoint)
    {
        landingPoint = FlattenFeet(casterFeet);
        var plane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
        if (!plane.Raycast(ray, out var enter) || enter < 0f)
            return false;

        var hit = ray.GetPoint(enter);
        var delta = hit - casterFeet;
        delta.y = 0f;
        var distance = ClampDistance(delta.magnitude, maxDistance);
        if (distance <= FeetLandingEpsilon)
            return true;

        landingPoint = FlattenFeet(casterFeet + delta.normalized * distance);
        return true;
    }

    public static Vector3 ResolveGamepadPoint(
        Vector3 casterFeet,
        Vector3 aimDirection,
        float stickMagnitude,
        float maxDistance = MaxLeapDistance)
    {
        var direction = AimMath.FlattenXz(aimDirection, Vector3.forward);
        float distance;
        if (stickMagnitude >= AimMath.RightStickDeadzone)
            distance = ClampDistance(stickMagnitude * maxDistance, maxDistance);
        else
            distance = maxDistance;

        return ResolvePointFromDirection(casterFeet, direction, distance, maxDistance);
    }

    public static Vector3 ResolveLastLegalPoint(
        Vector3 origin,
        Vector3 desired,
        System.Func<Vector3, bool> contains,
        System.Func<Vector3, Vector3> clamp)
    {
        origin = FlattenFeet(origin);
        desired = FlattenFeet(desired);

        if (contains == null)
            return desired;

        if (contains(desired))
            return desired;

        var lastLegal = origin;
        const int steps = 32;
        for (var i = 1; i <= steps; i++)
        {
            var t = i / (float)steps;
            var sample = Vector3.Lerp(origin, desired, t);
            sample.y = origin.y;
            if (contains(sample))
                lastLegal = sample;
            else
                break;
        }

        if (clamp != null)
            lastLegal = clamp(lastLegal);

        return lastLegal;
    }

    public static Vector3 ResolveLastLegalPoint(Vector3 origin, Vector3 desired, MapPlayArea playArea)
    {
        if (playArea == null)
            return FlattenFeet(desired);

        return ResolveLastLegalPoint(origin, desired, playArea.Contains, playArea.Clamp);
    }

    public static bool IsFeetLanding(Vector3 casterFeet, Vector3 landingPoint)
    {
        var delta = landingPoint - casterFeet;
        delta.y = 0f;
        return delta.sqrMagnitude <= FeetLandingEpsilon * FeetLandingEpsilon;
    }

    private static Vector3 FlattenFeet(Vector3 position)
    {
        position.y = 0f;
        return position;
    }
}
