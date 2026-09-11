using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Kontrakt facing przy ruchu wzdłuż linii (M6.5 fix).
/// </summary>
public class EnemyLaneMotorTests
{
    [Test]
    public void AdvanceAlongLane_FromSpawn_FacesMovementDelta()
    {
        var go = new GameObject("TestEnemy");
        var motor = go.AddComponent<EnemyLaneMotor>();

        typeof(EnemyLaneMotor)
            .GetField("separationStrength", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(motor, 0f);

        motor.Initialize(AttackLineId.Left, null);

        var path = MapGreyboxLayout.GetLanePath(AttackLineId.Left);
        var start = path.Waypoints[0];
        start.y = 1f;
        go.transform.position = start;
        go.transform.rotation = Quaternion.identity;

        motor.AdvanceAlongLane(2f);

        var movement = go.transform.position - start;
        movement.y = 0f;

        Assert.Greater(movement.sqrMagnitude, 0.01f, "Advance powinien zmienić pozycję.");

        var forward = go.transform.forward;
        forward.y = 0f;
        var dot = Vector3.Dot(forward.normalized, movement.normalized);
        Assert.Greater(dot, 0.99f, "Rotacja powinna być w kierunku ruchu wzdłuż linii.");

        Object.DestroyImmediate(go);
    }
}
