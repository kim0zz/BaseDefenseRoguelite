using NUnit.Framework;
using UnityEngine;

public class AimMathTests
{
    [Test]
    public void TryAimOnGround_PointsFromOriginTowardHit()
    {
        var ray = new Ray(new Vector3(0f, 10f, 0f), Vector3.down);
        Assert.IsTrue(AimMath.TryAimOnGround(ray, 0f, new Vector3(-4f, 0f, 0f), out var dir));
        Assert.Greater(dir.x, 0.9f);
        Assert.AreEqual(0f, dir.y, 0.001f);
    }

    [Test]
    public void TryAimOnGround_RejectsWhenCursorOnPlayer()
    {
        var ray = new Ray(new Vector3(2f, 8f, 3f), Vector3.down);
        var from = new Vector3(2f, 0f, 3f);
        Assert.IsFalse(AimMath.TryAimOnGround(ray, 0f, from, out _));
    }

    [Test]
    public void TryWorldAimFromStick_IgnoresDeadzone()
    {
        Assert.IsFalse(AimMath.TryWorldAimFromStick(
            new Vector2(0.1f, 0.1f), Vector3.right, Vector3.forward, Vector3.up, out _));
    }

    [Test]
    public void TryWorldAimFromStick_UsesCameraPlanarAxes()
    {
        // Kamera top-down: forward = -Y, up = +Z (jak SharedCamera Euler 90,0,0).
        Assert.IsTrue(AimMath.TryWorldAimFromStick(
            Vector2.up, Vector3.right, Vector3.down, Vector3.forward, out var dir));
        Assert.Greater(dir.z, 0.9f);
        Assert.AreEqual(0f, dir.y, 0.001f);
    }

    [Test]
    public void ResolveFacing_PrefersAimOverMove()
    {
        var facing = AimMath.ResolveFacing(Vector3.forward, Vector3.right, true, Vector3.left);
        Assert.Less(facing.x, -0.9f);
    }

    [Test]
    public void ResolveFacing_FallsBackToMoveWhenNoAim()
    {
        var facing = AimMath.ResolveFacing(Vector3.forward, Vector3.right, false, Vector3.zero);
        Assert.Greater(facing.x, 0.9f);
    }

    [Test]
    public void ResolveFacing_KeepsCurrentWhenIdle()
    {
        var facing = AimMath.ResolveFacing(Vector3.left, Vector3.zero, false, Vector3.zero);
        Assert.Less(facing.x, -0.9f);
    }
}
