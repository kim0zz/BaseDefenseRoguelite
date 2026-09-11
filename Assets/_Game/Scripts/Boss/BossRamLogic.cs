using UnityEngine;

/// <summary>
/// Logika czysta bossa Rama — testowalna bez sceny (M7-T5).
/// </summary>
public enum BossRamState
{
    Idle,
    TelegraphCharge,
    Charge,
    Impact,
    Recover,
    FocusPlayer,
    Stunned
}

public enum BossRamChargeTargetKind
{
    None,
    LineTower,
    Base,
    Player
}

public static class BossRamLogic
{
    public static bool ShouldEnterEnrage(float currentHealth, float maxHealth, float enrageRatio)
    {
        if (maxHealth <= 0f) return false;
        return currentHealth / maxHealth <= enrageRatio;
    }

    public static float GetTelegraphDuration(BossDefinition definition, bool enraged)
    {
        if (definition == null) return 1.5f;
        return enraged ? definition.TelegraphSecondsEnraged : definition.TelegraphSeconds;
    }

    public static float GetChargeCooldown(BossDefinition definition, bool enraged)
    {
        if (definition == null) return 5f;
        return enraged ? definition.ChargeCooldownEnraged : definition.ChargeCooldown;
    }

    public static bool ShouldInterruptCharge(
        BossDefinition definition,
        StatusEffectReceiver status,
        bool inTelegraphOrCharge)
    {
        if (!inTelegraphOrCharge || definition == null || status == null) return false;

        if (status.TryConsumeStagger(definition.StaggerInterruptThreshold))
            return true;

        return status.GetBurstDamageInWindow(definition.BurstInterruptWindow) >= definition.BurstInterruptThreshold;
    }

    public static float GetFocusDuration(BossDefinition definition, bool enraged)
    {
        if (definition == null) return enraged ? 2.2f : 3.5f;
        return enraged ? definition.FocusDurationEnraged : definition.FocusDuration;
    }

    public static bool ShouldExitFocus(float stateTimer) => stateTimer <= 0f;

    public static bool ShouldAbortFocusForCharge(float chargeCooldownTimer) => chargeCooldownTimer <= 0f;

    /// <summary>
    /// Priorytet szarży (PO playtest 2026-09-11): żywy gracz → wieża → baza.
    /// Baza tylko gdy nikt nie żyje — Ram nie orze rdzenia podczas walki.
    /// </summary>
    public static BossRamChargeTargetKind ResolveChargeTargetKind(
        bool hasLivingLineTower,
        bool hasLivingBase,
        bool hasLivingPlayer)
    {
        if (hasLivingPlayer) return BossRamChargeTargetKind.Player;
        if (hasLivingLineTower) return BossRamChargeTargetKind.LineTower;
        if (hasLivingBase) return BossRamChargeTargetKind.Base;
        return BossRamChargeTargetKind.None;
    }

    public static float GetChargeImpactDamage(bool targetIsStructure, float structureDamage, float playerDamage)
    {
        return targetIsStructure ? structureDamage : playerDamage;
    }

    public static Vector3 GetChargeAimPoint(Vector3 bossPosition, Vector3 targetPosition, float stopDistance)
    {
        var from = bossPosition;
        var to = targetPosition;
        from.y = 0f;
        to.y = 0f;
        var delta = to - from;
        var distance = delta.magnitude;
        if (distance <= 0.001f)
            return bossPosition;

        var travel = Mathf.Max(0f, distance - Mathf.Max(0.5f, stopDistance));
        var aim = from + delta / distance * travel;
        aim.y = bossPosition.y;
        return aim;
    }

    public static float GetFocusStopDistance(BossDefinition definition)
    {
        if (definition == null) return 2.2f;
        var bossRadius = EnemySeparation.RadiusFromBodyScale(definition.BodyScale);
        return GetFocusStopDistance(bossRadius, definition.PlayerBodyRadius, definition.FocusContactGap);
    }

    public static float GetFocusStopDistance(float bossRadius, float playerRadius, float contactGap)
    {
        return Mathf.Max(0.5f, bossRadius + Mathf.Max(0f, playerRadius) + Mathf.Max(0f, contactGap));
    }

    /// <summary>
    /// Krok XZ do pierścienia standoff: dodatni = zbliżenie, ujemny = odskok. Nie przecina stopu.
    /// </summary>
    public static float GetFocusMoveStep(float distance, float stopDistance, float maxStep)
    {
        if (maxStep <= 0f) return 0f;
        var delta = distance - stopDistance;
        if (Mathf.Abs(delta) <= 0.05f) return 0f;
        return Mathf.Clamp(delta, -maxStep, maxStep);
    }
}
