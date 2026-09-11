using UnityEngine;

/// <summary>
/// Placeholder ataku w strukturę — Rusher/Siege inaczej wyglądają na AFK.
/// </summary>
public static class StructureHitFeedback
{
    public static void Play(Vector3 from, Vector3 to)
    {
        from.y = 0.6f;
        to.y = 0.8f;
        var delta = to - from;
        var length = delta.magnitude;
        if (length < 0.05f)
            length = 0.05f;

        var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "StructureHitBeam";
        beam.transform.position = (from + to) * 0.5f;
        beam.transform.localScale = new Vector3(0.18f, 0.18f, length);
        if (delta.sqrMagnitude > 0.001f)
            beam.transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
        StripCollider(beam);
        Tint(beam, new Color(1f, 0.55f, 0.15f, 0.9f));
        Object.Destroy(beam, 0.18f);

        var pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pulse.name = "StructureHitPulse";
        pulse.transform.position = to;
        pulse.transform.localScale = Vector3.one * 1.35f;
        StripCollider(pulse);
        Tint(pulse, new Color(1f, 0.35f, 0.1f, 0.7f));
        Object.Destroy(pulse, 0.2f);
    }

    private static void StripCollider(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col != null)
            Object.Destroy(col);
    }

    private static void Tint(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        var mat = new Material(rend.sharedMaterial);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;
        rend.material = mat;
    }
}
