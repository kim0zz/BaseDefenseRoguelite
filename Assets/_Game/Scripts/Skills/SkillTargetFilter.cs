using UnityEngine;

/// <summary>
/// Filtr celów skilla — bez sojuszników, wież i bazy (M7.5-T3).
/// </summary>
public static class SkillTargetFilter
{
    public static bool IsValidTarget(GameObject target, SkillDefinition definition, GameObject caster)
    {
        if (target == null || definition == null || caster == null) return false;
        if (target == caster || target.transform.IsChildOf(caster.transform)) return false;
        if (target.GetComponentInParent<PlayerCharacter>() != null) return false;
        if (target.GetComponentInParent<TowerHealth>() != null) return false;
        if (target.GetComponentInParent<BaseHealth>() != null) return false;

        var enemy = target.GetComponentInParent<EnemyController>();
        if (enemy != null)
            return definition.TargetMask.HasFlag(SkillTargetMask.Enemy);

        var boss = target.GetComponentInParent<BossRamController>();
        if (boss != null)
            return definition.TargetMask.HasFlag(SkillTargetMask.Boss);

        return false;
    }
}
