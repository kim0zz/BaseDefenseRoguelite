using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Overlap okręgu XZ + blokada przez przeszkody (M7.5-T2). Brak filtra laneId.
/// </summary>
public static class SkillCircleOverlap
{
    public struct HitTarget
    {
        public GameObject GameObject;
        public IDamageable Damageable;
        public AttackLineId LaneId;
        public Vector3 HitPoint;
    }

    public static List<HitTarget> Query(
        Vector3 center,
        float radius,
        int maxTargets,
        LayerMask targetLayers,
        LayerMask obstacleLayers,
        Func<Vector3, Vector3, bool> hasLineOfSight = null)
    {
        var results = new List<HitTarget>();
        if (radius <= 0f || maxTargets <= 0) return results;

        center.y = 0f;
        var overlaps = Physics.OverlapSphere(center + Vector3.up * 0.5f, radius, targetLayers);
        var los = hasLineOfSight ?? DefaultLineOfSight;

        foreach (var collider in overlaps)
        {
            if (collider == null) continue;
            var root = collider.transform.root.gameObject;
            if (IsBlocked(center, collider.transform.position, radius, obstacleLayers, los))
                continue;

            var damageable = collider.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) continue;

            var laneId = ResolveLaneId(collider.gameObject);
            results.Add(new HitTarget
            {
                GameObject = collider.gameObject,
                Damageable = damageable,
                LaneId = laneId,
                HitPoint = collider.ClosestPoint(center)
            });
        }

        results.Sort((a, b) =>
            Vector3.SqrMagnitude(a.HitPoint - center).CompareTo(Vector3.SqrMagnitude(b.HitPoint - center)));

        if (results.Count > maxTargets)
            results.RemoveRange(maxTargets, results.Count - maxTargets);

        return results;
    }

    public static bool IsBlocked(
        Vector3 center,
        Vector3 targetPosition,
        float radius,
        LayerMask obstacleLayers,
        Func<Vector3, Vector3, bool> hasLineOfSight)
    {
        var flatTarget = targetPosition;
        flatTarget.y = center.y;
        if (Vector3.Distance(center, flatTarget) > radius + 0.01f) return true;
        return !hasLineOfSight(center + Vector3.up * 0.5f, flatTarget + Vector3.up * 0.5f);
    }

    private static bool DefaultLineOfSight(Vector3 from, Vector3 to)
    {
        var playArea = MapPlayArea.Instance;
        if (playArea == null) return true;
        if (!playArea.Contains(to)) return false;
        var mid = (from + to) * 0.5f;
        return playArea.Contains(mid);
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
