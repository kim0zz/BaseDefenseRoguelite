using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Testy matematyki wspólnej kamery (M6.5).
/// </summary>
public class SharedCameraMathTests
{
    [Test]
    public void CalculateCentroid_SinglePlayer_ReturnsThatPosition()
    {
        var positions = new[] { new Vector3(3f, 1f, -5f) };
        var centroid = SharedCameraMath.CalculateCentroid(positions);

        Assert.AreEqual(3f, centroid.x, 0.001f);
        Assert.AreEqual(-5f, centroid.z, 0.001f);
    }

    [Test]
    public void CalculateCentroid_FourCorners_ReturnsCenterish()
    {
        var positions = new[]
        {
            new Vector3(-18f, 0f, -14f),
            new Vector3(18f, 0f, -14f),
            new Vector3(-18f, 0f, 26f),
            new Vector3(18f, 0f, 26f)
        };

        var centroid = SharedCameraMath.CalculateCentroid(positions);
        Assert.AreEqual(0f, centroid.x, 0.5f);
        Assert.AreEqual(6f, centroid.z, 0.5f);
    }

    [Test]
    public void CalculateOrthoSize_FourCorners_CoversAabbWithinMax()
    {
        var positions = new[]
        {
            new Vector3(-18f, 0f, -14f),
            new Vector3(18f, 0f, -14f),
            new Vector3(-18f, 0f, 26f),
            new Vector3(18f, 0f, 26f)
        };
        var centroid = SharedCameraMath.CalculateCentroid(positions);
        const float aspect = 16f / 9f;
        const float padding = 4f;
        const float minSize = 6f;
        const float maxSize = 28f;

        var ortho = SharedCameraMath.CalculateOrthoSize(positions, centroid, aspect, padding, minSize, maxSize);

        Assert.LessOrEqual(ortho, maxSize);
        Assert.Greater(ortho, minSize);
        Assert.GreaterOrEqual(ortho, 15f, "Rozrzut 4 graczy na rogach wymaga dużego zoomu.");
    }

    [Test]
    public void CalculateCentroid_TwoPlayersOppositeSides_NotLockedToFirst()
    {
        var leftOnly = new[] { new Vector3(-14f, 0f, 0f), new Vector3(-12f, 0f, 2f) };
        var opposite = new[] { new Vector3(-14f, 0f, 0f), new Vector3(14f, 0f, 0f) };

        var centroidLeft = SharedCameraMath.CalculateCentroid(leftOnly);
        var centroidOpposite = SharedCameraMath.CalculateCentroid(opposite);

        Assert.Less(centroidLeft.x, -10f);
        Assert.AreEqual(0f, centroidOpposite.x, 0.5f,
            "Centroid dwóch graczy na przeciwnych liniach powinien być bliżej środka mapy niż P1.");
    }
}
