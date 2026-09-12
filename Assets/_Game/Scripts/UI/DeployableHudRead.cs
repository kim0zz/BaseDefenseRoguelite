using UnityEngine;

/// <summary>
/// HUD counts for deployables — uses registry + orbit host when present.
/// </summary>
public static class DeployableHudRead
{
    public static bool TryGetBombLine(GameObject owner, out string line)
    {
        line = null;
        if (!TryReadBombCounts(owner, out var count, out var cap))
            return false;

        line = CombatHudCopy.FormatBombCap(count, cap);
        return true;
    }

    public static bool TryGetOrbitalLine(GameObject owner, out string line)
    {
        line = null;
        if (!TryReadOrbitalCounts(owner, out var ready, out var total) || total <= 0)
            return false;

        line = CombatHudCopy.FormatOrbitalPips(ready, total);
        return true;
    }

    public static bool TryReadBombCounts(GameObject owner, out int count, out int cap)
    {
        count = 0;
        cap = 0;
        if (owner == null || !OwnerUsesDeployables(owner))
            return false;

        count = DeployableRegistry.GetNormalCount(owner);
        cap = DeployableRegistry.GetCap(owner);
        return cap > 0;
    }

    public static bool TryReadOrbitalCounts(GameObject owner, out int ready, out int total)
    {
        ready = 0;
        total = 0;
        if (owner == null) return false;

        var host = owner.GetComponent<OrbitingDeployableHost>();
        if (host == null || host.TotalCount <= 0)
            return false;

        ready = host.ReadyCount;
        total = host.TotalCount;
        return true;
    }

    private static bool OwnerUsesDeployables(GameObject owner)
    {
        if (owner.GetComponent<OrbitingDeployableHost>() != null)
            return true;

        var skills = owner.GetComponent<PlayerSkillController>();
        if (skills == null) return false;
        for (var i = 0; i < PlayerSkillController.SkillSlotCount; i++)
        {
            var skill = skills.GetSkill(i);
            if (skill != null && (skill.ShapeType == SkillShapeType.PlaceDeployable
                || skill.ShapeType == SkillShapeType.DetonateOwned
                || skill.ShapeType == SkillShapeType.LaunchNearestOwned))
                return true;
        }

        return false;
    }
}
