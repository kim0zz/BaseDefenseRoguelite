using UnityEngine;

/// <summary>
/// Klin strefy ataku (XZ) — zbliżony do IsInMeleeArc, wąski przy graczu.
/// </summary>
public static class AttackArcGeometry
{
    public const float InnerRadius = 0.4f;

    public static float WidthAtDistance(float distance, float arcDegrees)
    {
        var clamped = Mathf.Max(0f, distance);
        return 2f * clamped * Mathf.Tan(arcDegrees * 0.5f * Mathf.Deg2Rad);
    }

    public static void WedgePoints(
        Vector3 facing,
        float range,
        float arcDegrees,
        out Vector3 start,
        out Vector3 left,
        out Vector3 right)
    {
        facing.y = 0f;
        if (facing.sqrMagnitude < 0.01f)
            facing = Vector3.forward;
        facing.Normalize();

        var usableRange = Mathf.Max(range, InnerRadius + 0.1f);
        start = facing * InnerRadius;
        var outer = facing * usableRange;
        var half = arcDegrees * 0.5f;
        left = Quaternion.Euler(0f, half, 0f) * outer;
        right = Quaternion.Euler(0f, -half, 0f) * outer;
    }
}
