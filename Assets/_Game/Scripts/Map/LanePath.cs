using UnityEngine;

/// <summary>
/// Polyline korytarza linii natarcia — waypoints spawn→choke→wieża linii→rdzeń + clamp boczny (M6.5).
/// </summary>
public readonly struct LanePath
{
    public readonly Vector3[] Waypoints;
    public readonly float HalfWidth;

    public LanePath(Vector3[] waypoints, float halfWidth)
    {
        Waypoints = waypoints;
        HalfWidth = halfWidth;
    }

    public static LanePath Create(Vector3 spawn, Vector3 choke, Vector3 lineTower, Vector3 baseCore, float halfWidth)
    {
        return new LanePath(new[] { spawn, choke, lineTower, baseCore }, halfWidth);
    }

    public int SegmentCount => Waypoints != null && Waypoints.Length > 1 ? Waypoints.Length - 1 : 0;

    public float TotalLength
    {
        get
        {
            var len = 0f;
            for (var i = 0; i < SegmentCount; i++)
                len += Vector3.Distance(Waypoints[i], Waypoints[i + 1]);
            return len;
        }
    }

    public Vector3 GetClosestPointOnPath(Vector3 worldPos)
    {
        if (Waypoints == null || Waypoints.Length == 0)
            return worldPos;

        var best = Waypoints[0];
        var bestDistSq = float.MaxValue;

        for (var i = 0; i < SegmentCount; i++)
        {
            var closest = ClosestPointOnSegment(worldPos, Waypoints[i], Waypoints[i + 1]);
            var distSq = (worldPos - closest).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = closest;
            }
        }

        return best;
    }

    public Vector3 ClampToCorridor(Vector3 worldPos)
    {
        var onPath = GetClosestPointOnPath(worldPos);
        var offset = worldPos - onPath;
        offset.y = 0f;

        var lateralDist = new Vector2(offset.x, offset.z).magnitude;
        if (lateralDist <= HalfWidth || lateralDist < 0.0001f)
            return worldPos;

        var lateralDir = offset / lateralDist;
        var clamped = onPath + lateralDir * HalfWidth;
        clamped.y = worldPos.y;
        return clamped;
    }

    public bool IsInCorridor(Vector3 worldPos, float extraLeash = 0f)
    {
        var onPath = GetClosestPointOnPath(worldPos);
        var offset = worldPos - onPath;
        offset.y = 0f;
        var limit = HalfWidth + extraLeash;
        return offset.sqrMagnitude <= limit * limit;
    }

    public float GetDistanceAlongPath(Vector3 worldPos)
    {
        if (Waypoints == null || Waypoints.Length < 2)
            return 0f;

        var bestDistAlong = 0f;
        var bestPerpDist = float.MaxValue;
        var accumulated = 0f;

        for (var i = 0; i < SegmentCount; i++)
        {
            var a = Waypoints[i];
            var b = Waypoints[i + 1];
            var segLen = Vector3.Distance(a, b);
            var closest = ClosestPointOnSegment(worldPos, a, b);
            var perpDist = (worldPos - closest).sqrMagnitude;

            if (perpDist < bestPerpDist)
            {
                bestPerpDist = perpDist;
                var alongSeg = segLen > 0.0001f ? Vector3.Distance(a, closest) : 0f;
                bestDistAlong = accumulated + alongSeg;
            }

            accumulated += segLen;
        }

        return bestDistAlong;
    }

    public Vector3 GetPositionAtDistance(float distance)
    {
        if (Waypoints == null || Waypoints.Length == 0)
            return Vector3.zero;

        var remaining = Mathf.Max(0f, distance);

        for (var i = 0; i < SegmentCount; i++)
        {
            var a = Waypoints[i];
            var b = Waypoints[i + 1];
            var segLen = Vector3.Distance(a, b);

            if (remaining <= segLen)
            {
                var t = segLen > 0.0001f ? remaining / segLen : 0f;
                var pos = Vector3.Lerp(a, b, t);
                pos.y = a.y;
                return pos;
            }

            remaining -= segLen;
        }

        return Waypoints[Waypoints.Length - 1];
    }

    public Vector3 AdvanceAlongPath(Vector3 worldPos, float distance)
    {
        var currentDist = GetDistanceAlongPath(worldPos);
        var targetDist = Mathf.Min(currentDist + distance, TotalLength);
        var advanced = GetPositionAtDistance(targetDist);
        advanced.y = worldPos.y;
        return ClampToCorridor(advanced);
    }

    public Vector3 MoveTowardAlongCorridor(Vector3 current, Vector3 target, float maxDistance)
    {
        var clampedTarget = ClampToCorridor(target);
        var toTarget = clampedTarget - current;
        toTarget.y = 0f;

        var dist = toTarget.magnitude;
        if (dist < 0.0001f)
            return current;

        var step = Mathf.Min(maxDistance, dist);
        var newPos = current + toTarget.normalized * step;
        return ClampToCorridor(newPos);
    }

    public static Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        var abSq = ab.sqrMagnitude;
        if (abSq < 0.0001f)
            return a;

        var t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / abSq);
        return a + ab * t;
    }
}
