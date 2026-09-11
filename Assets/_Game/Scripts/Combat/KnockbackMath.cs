using UnityEngine;

/// <summary>
/// Mapowanie siły feel → przesunięcie. Nie zmienia DPS.
/// </summary>
public static class KnockbackMath
{
    public const float ImpulseScale = 2f;
    public const float Damping = 3f;

    public static float EstimatedDisplacement(float force)
    {
        if (force <= 0f || Damping <= 0f) return 0f;
        return force * ImpulseScale / Damping;
    }

    public static Vector3 ImpulseVelocity(Vector3 origin, Vector3 targetPosition, float force)
    {
        if (force <= 0f) return Vector3.zero;

        var dir = targetPosition - origin;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.back;
        else
            dir.Normalize();

        return dir * (force * ImpulseScale);
    }
}
