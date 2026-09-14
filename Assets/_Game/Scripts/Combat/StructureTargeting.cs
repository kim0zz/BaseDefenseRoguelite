using UnityEngine;

/// <summary>
/// Wybór struktury (wieża linii → baza) dla Rusher/Siege/Ram (M7).
/// </summary>
public static class StructureTargeting
{
    public static IDamageable ResolveStructureTarget(AttackLineId lane, float attackRange, Vector3 fromPosition)
    {
        if (SiegeArena.Instance != null) return SiegeArena.Instance.DefenseTarget;
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
            var targetPosition = component.transform.position;
            if (SiegeArena.Instance != null && component.TryGetComponent<Collider>(out var collider))
            {
                var query = new Vector3(fromPosition.x, collider.bounds.center.y, fromPosition.z);
                targetPosition = collider.ClosestPoint(query);
            }
            var delta = targetPosition - fromPosition;
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
