using UnityEngine;

/// <summary>
/// Matematyka aury Support — zasięg XZ, bez self-buffu (M8.2).
/// </summary>
public static class EnemySupportBuffMath
{
    public static bool IsInRange(Vector3 a, Vector3 b, float radius)
    {
        if (radius <= 0f) return false;

        var dx = a.x - b.x;
        var dz = a.z - b.z;
        return dx * dx + dz * dz <= radius * radius;
    }

    public static bool ShouldBuffAlly(EnemyKind supportKind, Vector3 supportPos, Vector3 allyPos, float radius)
    {
        if (supportKind != EnemyKind.Support) return false;
        return IsInRange(supportPos, allyPos, radius);
    }

    /// <summary>Buffy nie stackują — zwraca ten sam mnożnik przy ponownym zastosowaniu.</summary>
    public static float CombineMultiplier(float current, float incoming)
    {
        return Mathf.Max(current, incoming);
    }
}
