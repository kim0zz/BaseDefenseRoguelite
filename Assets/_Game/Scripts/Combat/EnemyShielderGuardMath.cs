using UnityEngine;

/// <summary>
/// Matematyka ochrony Shieldera — ta sama linia, zasięg XZ, sojusznik za Shielderem (M8.2).
/// </summary>
public static class EnemyShielderGuardMath
{
    public static bool IsProtected(
        AttackLineId allyLane,
        Vector3 allyPos,
        AttackLineId shielderLane,
        Vector3 shielderPos,
        float radius)
    {
        if (radius <= 0f) return false;
        if (allyLane != shielderLane) return false;
        if (allyPos.z <= shielderPos.z) return false;

        var dx = allyPos.x - shielderPos.x;
        var dz = allyPos.z - shielderPos.z;
        return dx * dx + dz * dz <= radius * radius;
    }
}
