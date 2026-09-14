using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Separacja XZ między wrogami — czysta matematyka testowalna w EditMode (M6.5).
/// </summary>
public static class EnemySeparation
{
    public static float RadiusFromBodyScale(Vector3 bodyScale)
    {
        return Mathf.Max(bodyScale.x, bodyScale.z) * 0.5f;
    }

    /// <summary>
    /// Jedna iteracja separacji. selfIndex wskazuje agenta w tablicy positions.
    /// </summary>
    public static Vector3 Apply(
        Vector3 position,
        int selfIndex,
        IReadOnlyList<Vector3> positions,
        IReadOnlyList<float> radii,
        float strength)
    {
        if (positions == null || radii == null || strength <= 0f)
            return position;

        if (selfIndex < 0 || selfIndex >= positions.Count || selfIndex >= radii.Count)
            return position;

        var separation = Vector3.zero;
        var selfRadius = radii[selfIndex];

        for (var i = 0; i < positions.Count; i++)
        {
            if (i == selfIndex) continue;

            var diff = position - positions[i];
            diff.y = 0f;
            var minDist = selfRadius + radii[i];
            var distanceSquared = diff.sqrMagnitude;

            if (distanceSquared < minDist * minDist)
            {
                if (distanceSquared < 0.00000001f)
                {
                    var angle = selfIndex * 2.399963f + i * 0.7f;
                    diff = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    distanceSquared = 0.00000001f;
                }

                var dist = Mathf.Sqrt(distanceSquared);
                separation += diff.normalized * (minDist - dist) * strength;
            }
        }

        var result = position + separation;
        result.y = position.y;
        return result;
    }
}
