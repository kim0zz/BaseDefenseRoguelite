using UnityEngine;

/// <summary>
/// API prowokacji dla skilli — osobna warstwa od ForcedMovement (M7.6-T2).
/// </summary>
public static class EnemyThreatService
{
    public static bool TryTaunt(GameObject enemy, PlayerCharacter taunter, float duration)
    {
        if (enemy == null || taunter == null || duration <= 0f) return false;

        if (enemy.GetComponent<BossRamController>() != null)
            return false;

        var controller = enemy.GetComponent<EnemyController>();
        if (controller == null || controller.Definition == null) return false;

        var kind = controller.Definition.Kind;
        if (!EnemyThreatMath.CanBeTaunted(kind, isBoss: false))
            return false;

        if (EnemyThreatMath.GruntRequiresTaunterInCorridor(kind))
        {
            var motor = enemy.GetComponent<EnemyLaneMotor>();
            if (motor == null || !IsTaunterInLaneCorridor(motor, taunter))
                return false;
        }

        var state = enemy.GetComponent<EnemyThreatState>();
        if (state == null)
            state = enemy.AddComponent<EnemyThreatState>();

        return state.TryApply(taunter, duration);
    }

    private static bool IsTaunterInLaneCorridor(EnemyLaneMotor motor, PlayerCharacter taunter)
    {
        if (taunter == null || !taunter.IsCombatEnabled) return false;

        var extraLeash = motor.LaneCorridorHalfWidthExtra();
        return motor.IsInLaneCorridor(taunter.transform.position, extraLeash);
    }
}
