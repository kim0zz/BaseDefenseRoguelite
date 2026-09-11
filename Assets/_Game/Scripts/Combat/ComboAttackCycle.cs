using UnityEngine;

/// <summary>
/// Multi-hit combo attack cycle — pure, EditMode testable (M8.1b).
/// </summary>
public sealed class ComboAttackCycle
{
    public AttackPhase Phase { get; private set; } = AttackPhase.Idle;
    public int CurrentHitIndex { get; private set; }
    public float Elapsed { get; private set; }
    public float IdleSinceLastHit { get; private set; }
    public AttackComboDefinition Combo { get; private set; }
    public bool HoldContinue { get; private set; }

    public bool IsAttacking => Phase != AttackPhase.Idle;
    public bool ShouldDeferWeaponSwap => IsAttacking;

    public ComboHitDefinition CurrentHit =>
        Combo != null && CurrentHitIndex >= 0 && CurrentHitIndex < Combo.HitCount
            ? Combo.Hits[CurrentHitIndex]
            : null;

    public float MoveMultiplier =>
        Phase == AttackPhase.Windup || Phase == AttackPhase.Active ? 0.25f : 1f;

    public bool TryStart(AttackComboDefinition combo, bool holdInput = false)
    {
        if (combo == null || combo.HitCount == 0) return false;
        if (IsAttacking) return false;

        if (IdleSinceLastHit > combo.ComboResetSeconds)
            CurrentHitIndex = 0;
        else if (CurrentHitIndex >= combo.HitCount)
            CurrentHitIndex = 0;

        Combo = combo;
        HoldContinue = holdInput;
        Elapsed = 0f;
        Phase = AttackPhase.Windup;
        IdleSinceLastHit = 0f;
        return true;
    }

    public ComboAttackTickResult Tick(float deltaTime, bool holdInput = false)
    {
        var result = new ComboAttackTickResult { Phase = Phase, HitIndex = CurrentHitIndex };
        if (Phase == AttackPhase.Idle)
        {
            IdleSinceLastHit += Mathf.Max(0f, deltaTime);
            return result;
        }

        HoldContinue = holdInput;
        Elapsed += Mathf.Max(0f, deltaTime);
        var hit = CurrentHit;
        if (hit == null) return result;

        var windup = hit.WindupSeconds;
        var active = hit.ActiveSeconds;
        var recovery = hit.RecoverySeconds;

        if (Phase == AttackPhase.Windup && Elapsed >= windup)
        {
            Phase = AttackPhase.Active;
            result.EnteredActive = true;
        }

        if (Phase == AttackPhase.Active && Elapsed >= windup + active)
        {
            Phase = AttackPhase.Recovery;
            result.EnteredRecovery = true;
        }

        if (Phase == AttackPhase.Recovery && Elapsed >= windup + active + recovery)
        {
            var nextIndex = CurrentHitIndex + 1;
            if (HoldContinue && nextIndex < Combo.HitCount)
            {
                CurrentHitIndex = nextIndex;
                Elapsed = 0f;
                Phase = AttackPhase.Windup;
                result.AdvancedCombo = true;
            }
            else
            {
                Phase = AttackPhase.Idle;
                Elapsed = 0f;
                IdleSinceLastHit = 0f;
                var queuedIndex = CurrentHitIndex + 1;
                CurrentHitIndex = queuedIndex < Combo.HitCount ? queuedIndex : Combo.HitCount;
                result.ReturnedToIdle = true;
            }
        }

        result.Phase = Phase;
        result.HitIndex = CurrentHitIndex;
        return result;
    }

    public void Cancel()
    {
        Phase = AttackPhase.Idle;
        Elapsed = 0f;
    }

    public void ResetCombo()
    {
        CurrentHitIndex = 0;
        IdleSinceLastHit = Combo != null ? Combo.ComboResetSeconds + 1f : 0f;
    }
}

public struct ComboAttackTickResult
{
    public AttackPhase Phase;
    public int HitIndex;
    public bool EnteredActive;
    public bool EnteredRecovery;
    public bool ReturnedToIdle;
    public bool AdvancedCombo;
}
