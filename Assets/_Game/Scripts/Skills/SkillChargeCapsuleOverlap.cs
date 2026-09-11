using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Overlap kapsuły przed casterem wzdłuż aim — M7.6-T7 Byk.
/// </summary>
public static class SkillChargeCapsuleOverlap
{
    public struct HitTarget
    {
        public GameObject GameObject;
        public IDamageable Damageable;
        public AttackLineId LaneId;
        public Vector3 HitPoint;
    }

    public static List<HitTarget> Query(
        Vector3 origin,
        Vector3 direction,
        float widthMeters,
        float lengthMeters,
        int maxTargets,
        LayerMask targetLayers)
    {
        var results = new List<HitTarget>();
        if (widthMeters <= 0f || lengthMeters <= 0f || maxTargets <= 0) return results;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.forward;
        direction.Normalize();

        origin.y = 0f;
        var radius = widthMeters * 0.5f;
        var start = origin + direction * 0.5f + Vector3.up * 0.5f;
        var end = start + direction * lengthMeters;

        var overlaps = Physics.OverlapCapsule(start, end, radius, targetLayers);
        var seen = new HashSet<GameObject>();

        foreach (var collider in overlaps)
        {
            if (collider == null) continue;
            var root = collider.transform.root.gameObject;
            if (!seen.Add(root)) continue;

            var damageable = collider.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) continue;

            results.Add(new HitTarget
            {
                GameObject = root,
                Damageable = damageable,
                LaneId = ResolveLaneId(collider.gameObject),
                HitPoint = collider.ClosestPoint(origin)
            });
        }

        results.Sort((a, b) =>
            Vector3.Dot(a.HitPoint - origin, direction).CompareTo(Vector3.Dot(b.HitPoint - origin, direction)));

        if (results.Count > maxTargets)
            results.RemoveRange(maxTargets, results.Count - maxTargets);

        return results;
    }

    private static AttackLineId ResolveLaneId(GameObject go)
    {
        var enemy = go.GetComponentInParent<EnemyController>();
        if (enemy != null) return enemy.AssignedLane;

        var boss = go.GetComponentInParent<BossRamController>();
        if (boss != null) return AttackLineId.Center;

        return AttackLineId.Center;
    }
}
