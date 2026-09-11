using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Raz na cel na cast — root GameObject (M7.6-T7 Byk).
/// </summary>
public sealed class SkillOncePerTargetSet
{
    private readonly HashSet<GameObject> _targets = new();

    public void Clear() => _targets.Clear();

    public bool Contains(GameObject target) =>
        target != null && _targets.Contains(target);

    public bool TryMark(GameObject target)
    {
        if (target == null) return false;
        return _targets.Add(target);
    }
}
