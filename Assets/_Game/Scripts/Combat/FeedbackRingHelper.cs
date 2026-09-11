using UnityEngine;

/// <summary>
/// Wspólny LineRenderer-pierścień dla placeholderów feedbacku (M9.1).
/// </summary>
internal static class FeedbackRingHelper
{
    private const float RingHeight = 0.02f;
    private const int SegmentCount = 32;

    public static LineRenderer CreateRing(Transform parent, string childName, Color color, float radius)
    {
        var existing = parent.Find(childName);
        Transform root;
        if (existing != null)
        {
            root = existing;
        }
        else
        {
            var go = new GameObject(childName);
            go.transform.SetParent(parent, false);
            root = go.transform;
        }

        var ring = root.GetComponent<LineRenderer>();
        if (ring == null)
            ring = root.gameObject.AddComponent<LineRenderer>();

        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = SegmentCount;
        ring.startWidth = 0.08f;
        ring.endWidth = 0.08f;
        ring.startColor = color;
        ring.endColor = color;
        if (ring.sharedMaterial == null)
            ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.enabled = true;
        UpdateRing(ring, radius);
        return ring;
    }

    public static void UpdateRing(LineRenderer ring, float radius)
    {
        if (ring == null) return;

        var clampedRadius = Mathf.Max(0.05f, radius);
        for (var i = 0; i < ring.positionCount; i++)
        {
            var angle = i / (float)ring.positionCount * Mathf.PI * 2f;
            ring.SetPosition(
                i,
                new Vector3(Mathf.Cos(angle) * clampedRadius, RingHeight, Mathf.Sin(angle) * clampedRadius));
        }
    }

    public static void SetVisible(LineRenderer ring, bool visible)
    {
        if (ring != null)
            ring.enabled = visible;
    }
}
