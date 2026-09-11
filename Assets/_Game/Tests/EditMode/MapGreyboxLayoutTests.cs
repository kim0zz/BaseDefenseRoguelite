using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Testy granic greyboxu M6.5.
/// </summary>
public class MapGreyboxLayoutTests
{
    [Test]
    public void PlayArea_ContainsSpawnFarEndsAndCore()
    {
        Assert.IsTrue(MapGreyboxLayout.PlayAreaContainsAllSpawnsAndCore());
    }

    [Test]
    public void LanePaths_AllThreeLanes_HaveDistinctXAtSpawn()
    {
        var left = MapGreyboxLayout.GetLanePath(AttackLineId.Left);
        var center = MapGreyboxLayout.GetLanePath(AttackLineId.Center);
        var right = MapGreyboxLayout.GetLanePath(AttackLineId.Right);

        Assert.Less(left.Waypoints[0].x, center.Waypoints[0].x);
        Assert.Less(center.Waypoints[0].x, right.Waypoints[0].x);
    }
}
