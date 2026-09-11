using UnityEngine;

/// <summary>
/// Przekierowanie obrażeń sojusznika na żywego Shieldera w zasięgu (M8.2).
/// </summary>
public static class EnemyShielderGuard
{
    private static bool _redirectInProgress;

    public static bool TryRedirectIncomingDamage(Health targetHealth, float amount, GameObject source)
    {
        if (_redirectInProgress || targetHealth == null || amount <= 0f) return false;

        var allyController = targetHealth.GetComponent<EnemyController>();
        if (allyController == null || allyController.Definition == null) return false;

        var allyHealth = targetHealth;
        if (!allyHealth.IsAlive) return false;

        if (allyController.Definition.Kind == EnemyKind.Shielder)
            return false;

        var allyLane = allyController.AssignedLane;
        var allyPos = allyController.transform.position;

        foreach (var shielder in EnemyRegistry.Active)
        {
            if (shielder == null || shielder == allyController) continue;

            var shielderHealth = shielder.GetComponent<Health>();
            if (shielderHealth == null || !shielderHealth.IsAlive) continue;

            var shielderDef = shielder.Definition;
            if (shielderDef == null || shielderDef.Kind != EnemyKind.Shielder) continue;

            if (!EnemyShielderGuardMath.IsProtected(
                    allyLane,
                    allyPos,
                    shielder.AssignedLane,
                    shielder.transform.position,
                    shielderDef.ShieldProtectRadius))
                continue;

            _redirectInProgress = true;
            shielderHealth.TakeDamage(amount, source);
            _redirectInProgress = false;
            return true;
        }

        return false;
    }
}
