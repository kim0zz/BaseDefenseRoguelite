using UnityEngine;

/// <summary>
/// Matematyka wspólnej kamery — centroid, zoom AABB, clamp do mapy (M6.5).
/// </summary>
public static class SharedCameraMath
{
    public static Vector3 CalculateCentroid(Vector3[] positions)
    {
        if (positions == null || positions.Length == 0)
            return Vector3.zero;

        var sum = Vector3.zero;
        foreach (var p in positions)
            sum += p;
        return sum / positions.Length;
    }

    public static float CalculateOrthoSize(
        Vector3[] positions,
        Vector3 centroid,
        float aspect,
        float padding,
        float minSize,
        float maxSize)
    {
        if (positions == null || positions.Length == 0)
            return minSize;

        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minZ = float.MaxValue;
        var maxZ = float.MinValue;

        foreach (var p in positions)
        {
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minZ = Mathf.Min(minZ, p.z);
            maxZ = Mathf.Max(maxZ, p.z);
        }

        var width = maxX - minX + padding * 2f;
        var height = maxZ - minZ + padding * 2f;

        var sizeForHeight = height * 0.5f;
        var sizeForWidth = aspect > 0.0001f ? width / (2f * aspect) : sizeForHeight;
        var target = Mathf.Max(sizeForHeight, sizeForWidth, minSize);

        return Mathf.Clamp(target, minSize, maxSize);
    }

    public static Vector3 ClampFollowPosition(
        Vector3 centroid,
        float orthoSize,
        float aspect,
        Vector2 xBounds,
        Vector2 zBounds)
    {
        var halfHeight = orthoSize;
        var halfWidth = orthoSize * aspect;

        var mapWidth = xBounds.y - xBounds.x;
        var mapDepth = zBounds.y - zBounds.x;

        var cx = centroid.x;
        var cz = centroid.z;

        if (mapWidth > halfWidth * 2f)
            cx = Mathf.Clamp(cx, xBounds.x + halfWidth, xBounds.y - halfWidth);
        else
            cx = (xBounds.x + xBounds.y) * 0.5f;

        if (mapDepth > halfHeight * 2f)
            cz = Mathf.Clamp(cz, zBounds.x + halfHeight, zBounds.y - halfHeight);
        else
            cz = (zBounds.x + zBounds.y) * 0.5f;

        return new Vector3(cx, centroid.y, cz);
    }
}
