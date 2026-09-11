using UnityEngine;

/// <summary>
/// Wybór struktury (wieża linii → baza) dla Rusher/Siege/Ram (M7).
/// </summary>
public static class StructureTargeting
{
    public static IDamageable ResolveStructureTarget(AttackLineId lane, float attackRange, Vector3 fromPosition)
    {
        var lineTower = TowerRegistry.GetLineTower(lane);
        if (lineTower != null && lineTower.IsOperational)
            return lineTower;

        var anyLineTower = TowerRegistry.PickRandomLivingLineTower();
        if (anyLineTower != null && anyLineTower.IsOperational)
            return anyLineTower;

        var baseHealth = Object.FindAnyObjectByType<BaseHealth>();
        return baseHealth != null && baseHealth.IsAlive ? baseHealth : null;
    }

    public static bool IsStructureInRange(IDamageable target, Vector3 fromPosition, float attackRange)
    {
        if (target == null || !target.IsAlive) return false;

        if (target is Component component)
        {
            var delta = component.transform.position - fromPosition;
            delta.y = 0f;
            return delta.sqrMagnitude <= attackRange * attackRange;
        }

        return false;
    }

    public static bool PrefersStructureOverPlayer(EnemyKind kind)
    {
        return kind is EnemyKind.Rusher or EnemyKind.Siege or EnemyKind.Flanker;
    }
}
