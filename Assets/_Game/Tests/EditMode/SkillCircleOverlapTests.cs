using NUnit.Framework;
using UnityEngine;

public class SkillCircleOverlapTests
{
    [Test]
    public void CircleOverlap_DoesNotFilterByLaneId()
    {
        var center = Vector3.zero;
        var left = CreateEnemy("Left", new Vector3(1f, 0f, 0f), AttackLineId.Left);
        var right = CreateEnemy("Right", new Vector3(-1f, 0f, 0f), AttackLineId.Right);

        var hits = SkillCircleOverlap.Query(
            center,
            3f,
            8,
            ~0,
            0,
            (_, _) => true);

        Assert.GreaterOrEqual(hits.Count, 2);
        var lanes = new System.Collections.Generic.HashSet<AttackLineId>();
        foreach (var hit in hits)
            lanes.Add(hit.LaneId);
        Assert.IsTrue(lanes.Contains(AttackLineId.Left));
        Assert.IsTrue(lanes.Contains(AttackLineId.Right));

        Object.DestroyImmediate(left);
        Object.DestroyImmediate(right);
    }

    [Test]
    public void CircleOverlap_RespectsMaxTargets()
    {
        var spawned = new GameObject[5];
        for (var i = 0; i < spawned.Length; i++)
            spawned[i] = CreateEnemy($"E{i}", new Vector3(i * 0.5f, 0f, 0f), AttackLineId.Center);

        var hits = SkillCircleOverlap.Query(Vector3.zero, 3f, 3, ~0, 0, (_, _) => true);
        Assert.AreEqual(3, hits.Count);

        foreach (var go in spawned)
            Object.DestroyImmediate(go);
    }

    private static GameObject CreateEnemy(string name, Vector3 position, AttackLineId lane)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.position = position;
        go.AddComponent<Health>().Configure(20f);
        var motor = go.AddComponent<EnemyLaneMotor>();
        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        motor.Initialize(lane, definition);
        go.AddComponent<EnemyController>().Configure(definition, lane);
        return go;
    }
}
