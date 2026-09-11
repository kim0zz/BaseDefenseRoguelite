using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Testy polyline / clamp korytarza linii (M6.5).
/// </summary>
public class LanePathTests
{
    [Test]
    public void Waypoints_OrderedFarToBase()
    {
        var path = MapGreyboxLayout.GetLanePath(AttackLineId.Center);

        Assert.Greater(path.Waypoints[0].z, path.Waypoints[^1].z,
            "Spawn (daleko) musi być przed rdzeniem bazy wzdłuż Z.");
        Assert.Greater(path.Waypoints[1].z, path.Waypoints[2].z,
            "Choke przed wieżą linii.");
        Assert.Greater(path.Waypoints[2].z, path.Waypoints[3].z,
            "Wieża linii przed rdzeniem.");
    }

    [Test]
    public void ClampToCorridor_PointOffPath_ReturnsInsideHalfWidth()
    {
        var path = MapGreyboxLayout.GetLanePath(AttackLineId.Center);
        var offCorridor = new Vector3(5f, 0f, 14f);

        var clamped = path.ClampToCorridor(offCorridor);
        var onPath = path.GetClosestPointOnPath(clamped);
        var lateral = new Vector2(clamped.x - onPath.x, clamped.z - onPath.z).magnitude;

        Assert.LessOrEqual(lateral, path.HalfWidth + 0.01f);
    }

    [Test]
    public void ClampToCorridor_RightLanePoint_StaysOnRight_NotLeft()
    {
        var rightPath = MapGreyboxLayout.GetLanePath(AttackLineId.Right);
        var leftPath = MapGreyboxLayout.GetLanePath(AttackLineId.Left);

        var offRight = new Vector3(20f, 0f, 14f);
        var clampedRight = rightPath.ClampToCorridor(offRight);

        Assert.Greater(clampedRight.x, 5f, "Punkt na prawej linii nie powinien skoczyć na lewą stronę.");
        Assert.IsTrue(rightPath.IsInCorridor(clampedRight));

        var distRight = Vector3.Distance(clampedRight, rightPath.GetClosestPointOnPath(clampedRight));
        var distLeft = Vector3.Distance(clampedRight, leftPath.GetClosestPointOnPath(clampedRight));
        Assert.Less(distRight, distLeft, "Clamp powinien trzymać punkt bliżej prawej linii niż lewej.");
    }

    [Test]
    public void AdvanceAlongPath_MovesTowardBase()
    {
        var path = MapGreyboxLayout.GetLanePath(AttackLineId.Left);
        var start = path.Waypoints[0];
        start.y = 1f;

        var advanced = path.AdvanceAlongPath(start, 2f);
        Assert.Less(advanced.z, start.z, "Ruch wzdłuż ścieżki powinien zbliżać do bazy (mniejsze Z).");
    }

    [Test]
    public void AdvanceAlongPath_FromChoke_ReachesLineTower()
    {
        var path = MapGreyboxLayout.GetLanePath(AttackLineId.Left);
        var pos = path.Waypoints[1];
        pos.y = 1f;
        var tower = path.Waypoints[2];

        for (var i = 0; i < 80; i++)
            pos = path.AdvanceAlongPath(pos, 0.5f);

        var xz = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(tower.x, tower.z));
        Assert.Less(xz, 1.6f, "Rusher/Siege muszą dojść polyline'em do wieży, nie stanąć na choke.");
    }
}
