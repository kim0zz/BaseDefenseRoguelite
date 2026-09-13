using UnityEngine;

/// <summary>
/// Czysta matematyka celowania XZ — testowalna bez Input System / sceny.
/// </summary>
public static class AimMath
{
    public const float RightStickDeadzone = 0.25f;
    public const float MinAimDistanceSqr = 0.0001f;

    public static Vector3 FlattenXz(Vector3 value, Vector3 fallback)
    {
        value.y = 0f;
        if (value.sqrMagnitude < MinAimDistanceSqr)
        {
            fallback.y = 0f;
            return fallback.sqrMagnitude < MinAimDistanceSqr ? Vector3.forward : fallback.normalized;
        }

        return value.normalized;
    }

    public static Vector3 CameraPlanarForward(Vector3 cameraForward, Vector3 cameraUp)
    {
        var forward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
        if (forward.sqrMagnitude >= MinAimDistanceSqr)
            return forward.normalized;

        forward = Vector3.ProjectOnPlane(cameraUp, Vector3.up);
        return forward.sqrMagnitude >= MinAimDistanceSqr ? forward.normalized : Vector3.forward;
    }

    public static Vector3 CameraPlanarRight(Vector3 cameraRight)
    {
        var right = Vector3.ProjectOnPlane(cameraRight, Vector3.up);
        return right.sqrMagnitude >= MinAimDistanceSqr ? right.normalized : Vector3.right;
    }

    public static bool TryAimOnGround(Ray ray, float groundY, Vector3 fromPosition, out Vector3 direction)
    {
        var plane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
        if (!plane.Raycast(ray, out var enter) || enter < 0f)
        {
            direction = default;
            return false;
        }

        var hit = ray.GetPoint(enter);
        var delta = hit - fromPosition;
        delta.y = 0f;
        if (delta.sqrMagnitude < MinAimDistanceSqr)
        {
            direction = default;
            return false;
        }

        direction = delta.normalized;
        return true;
    }

    public static bool TryWorldAimFromStick(
        Vector2 stick,
        Vector3 cameraRight,
        Vector3 cameraForward,
        Vector3 cameraUp,
        out Vector3 direction)
    {
        if (stick.sqrMagnitude < RightStickDeadzone * RightStickDeadzone)
        {
            direction = default;
            return false;
        }

        var planar = stick.normalized;
        var right = CameraPlanarRight(cameraRight);
        var forward = CameraPlanarForward(cameraForward, cameraUp);
        direction = FlattenXz(right * planar.x + forward * planar.y, Vector3.forward);
        return true;
    }

    public static Vector3 ResolveFacing(
        Vector3 currentFacing,
        Vector3 moveDirection,
        bool hasAim,
        Vector3 aimDirection)
    {
        if (hasAim)
            return FlattenXz(aimDirection, currentFacing);

        if (moveDirection.sqrMagnitude >= MinAimDistanceSqr)
            return FlattenXz(moveDirection, currentFacing);

        return FlattenXz(currentFacing, Vector3.forward);
    }

    /// <summary>
    /// Kierunek odskoku: bieżący ruch → ostatni ruch → zwrócenie postaci.
    /// </summary>
    public static Vector3 ResolveDashDirection(Vector3 currentMove, Vector3 lastMove, Vector3 facing)
    {
        if (currentMove.sqrMagnitude >= MinAimDistanceSqr)
            return FlattenXz(currentMove, facing);

        if (lastMove.sqrMagnitude >= MinAimDistanceSqr)
            return FlattenXz(lastMove, facing);

        return FlattenXz(facing, Vector3.forward);
    }
}
