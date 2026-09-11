using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Flanker bypass path — waypoint, linia, polyline (M8.2).
/// </summary>
public class EnemyFlankerPathTests
{
    [Test]
    public void BypassWaypoint_DiffersFromChoke_OnLeftLane()
    {
        var choke = MapGreyboxLayout.GetChokePosition(AttackLineId.Left);
        var bypass = MapGreyboxLayout.GetBypassWaypoint(AttackLineId.Left);

        Assert.AreEqual(MapGreyboxLayout.ChokeZ, bypass.z);
        Assert.AreNotEqual(choke.x, bypass.x);
        Assert.Less(bypass.x, choke.x, "Lewa linia: bypass na zewnątrz (mniejsze X).");
    }

    [Test]
    public void FlankerMotor_UsesBypassPath_AssignedLaneUnchanged()
    {
        var go = new GameObject("Flanker");
        var motor = go.AddComponent<EnemyLaneMotor>();
        typeof(EnemyLaneMotor)
            .GetField("separationStrength", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(motor, 0f);

        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        definition.ConfigureForTest(EnemyKind.Flanker);

        motor.Initialize(AttackLineId.Right, definition);

        Assert.AreEqual(AttackLineId.Right, motor.AssignedLane);

        var bypassPath = MapGreyboxLayout.GetBypassLanePath(AttackLineId.Right);
        var normalPath = MapGreyboxLayout.GetLanePath(AttackLineId.Right);

        Assert.AreNotEqual(bypassPath.Waypoints[1], normalPath.Waypoints[1]);
        Assert.AreEqual(bypassPath.Waypoints[1], motor.LanePath.Waypoints[1]);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(definition);
    }

    [Test]
    public void AdvanceAlongPath_FromSpawn_DoesNotPassThroughChoke()
    {
        var path = MapGreyboxLayout.GetBypassLanePath(AttackLineId.Center);
        var choke = MapGreyboxLayout.GetChokePosition(AttackLineId.Center);
        var start = path.Waypoints[0];
        start.y = 1f;

        var pos = start;
        for (var i = 0; i < 40; i++)
            pos = path.AdvanceAlongPath(pos, 0.5f);

        var distToChoke = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(choke.x, choke.z));
        Assert.Greater(distToChoke, 1.5f, "Flanker nie powinien przejść przez choke linii.");
        Assert.AreEqual(path.Waypoints[1].x, MapGreyboxLayout.GetBypassWaypoint(AttackLineId.Center).x, 0.01f);
    }

    [Test]
    public void Flanker_PrefersStructureOverPlayer()
    {
        Assert.IsTrue(StructureTargeting.PrefersStructureOverPlayer(EnemyKind.Flanker));
    }

    [Test]
    public void Flanker_StaysOnLane_IsFalse()
    {
        var go = new GameObject("Flanker");
        go.AddComponent<Health>().Configure(20f);
        go.AddComponent<StatusEffectReceiver>();

        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        definition.ConfigureForTest(EnemyKind.Flanker);

        var motor = go.AddComponent<EnemyLaneMotor>();
        motor.Initialize(AttackLineId.Center, definition);

        var controller = go.AddComponent<EnemyController>();
        controller.Configure(definition, AttackLineId.Center);

        var staysOnLane = typeof(EnemyController)
            .GetMethod("StaysOnLane", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(controller, null);

        Assert.IsFalse((bool)staysOnLane);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(definition);
    }
}
