using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Testy separacji wrogów XZ (M6.5).
/// </summary>
public class EnemySeparationTests
{
    [Test]
    public void Apply_TwoAgentsAtSamePoint_SeparateAtLeastCombinedRadius()
    {
        var positions = new List<Vector3>
        {
            new(0f, 0f, 0f),
            new(0f, 0f, 0f)
        };
        var radii = new List<float> { 0.5f, 0.5f };
        const float strength = 1f;
        const float minDist = 1f;

        var separated = EnemySeparation.Apply(positions[0], 0, positions, radii, strength);
        positions[0] = separated;

        var dist = Vector3.Distance(positions[0], positions[1]);
        Assert.GreaterOrEqual(dist, minDist - 0.05f,
            "Po separacji odległość powinna być >= suma promieni (z tolerancją).");
    }

    [Test]
    public void RadiusFromBodyScale_UsesMaxHorizontalAxis()
    {
        var radius = EnemySeparation.RadiusFromBodyScale(new Vector3(0.8f, 1.2f, 0.9f));
        Assert.AreEqual(0.45f, radius, 0.001f);
    }

    [Test]
    public void Apply_SelfIndexOutOfRange_DoesNotThrow()
    {
        var positions = new List<Vector3> { Vector3.zero };
        var radii = new List<float> { 0.5f };
        Assert.DoesNotThrow(() => EnemySeparation.Apply(Vector3.zero, 2, positions, radii, 1f));
        Assert.DoesNotThrow(() => EnemySeparation.Apply(Vector3.zero, -1, positions, radii, 1f));
    }

    [Test]
    public void LivingBuffer_SkipsDeadRegistryEntries()
    {
        var alive = new[] { false, true, false, true };
        Assert.AreEqual(-1, EnemyLivingBuffer.IndexOf(alive, 0));
        Assert.AreEqual(0, EnemyLivingBuffer.IndexOf(alive, 1));
        Assert.AreEqual(-1, EnemyLivingBuffer.IndexOf(alive, 2));
        Assert.AreEqual(1, EnemyLivingBuffer.IndexOf(alive, 3));
        Assert.AreEqual(-1, EnemyLivingBuffer.IndexOf(alive, 4));
    }
}
