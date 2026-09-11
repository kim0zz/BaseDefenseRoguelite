using UnityEngine;

/// <summary>
/// Maszyna faz umiejętności — testowalna bez sceny (M7.5-T2).
/// </summary>
public sealed class SkillCastCycle
{
    public SkillCastPhase Phase { get; private set; } = SkillCastPhase.Idle;
    public float Elapsed { get; private set; }
    public SkillDefinition Definition { get; private set; }
    private float _activeDurationOverride = -1f;

    public bool IsCasting => Phase != SkillCastPhase.Idle;

    public float MoveMultiplier
    {
        get
        {
            if (Definition == null) return 1f;
            return Phase switch
            {
                SkillCastPhase.Windup => Definition.WindupMoveMultiplier,
                SkillCastPhase.Active => Definition.WindupMoveMultiplier,
                SkillCastPhase.Recovery => Definition.RecoveryMoveMultiplier,
                _ => 1f
            };
        }
    }

    public void SetActiveDurationOverride(float seconds)
    {
        _activeDurationOverride = seconds > 0f ? seconds : -1f;
    }

    public bool TryStart(SkillDefinition definition)
    {
        if (IsCasting || definition == null) return false;

        Definition = definition;
        Elapsed = 0f;
        _activeDurationOverride = -1f;
        Phase = SkillCastPhase.Windup;
        return true;
    }

    public SkillCastTickResult Tick(float deltaTime)
    {
        var result = new SkillCastTickResult { Phase = Phase };
        if (!IsCasting || Definition == null) return result;

        Elapsed += Mathf.Max(0f, deltaTime);

        if (Phase == SkillCastPhase.Windup && Elapsed >= Definition.WindupSeconds)
        {
            Phase = SkillCastPhase.Active;
            result.EnteredActive = true;
        }

        var activeDuration = _activeDurationOverride > 0f
            ? _activeDurationOverride
            : Definition.ActiveSeconds;

        if (Phase == SkillCastPhase.Active && Elapsed >= Definition.WindupSeconds + activeDuration)
        {
            Phase = SkillCastPhase.Recovery;
            result.EnteredRecovery = true;
        }

        var total = Definition.WindupSeconds + activeDuration + Definition.RecoverySeconds;
        if (Phase == SkillCastPhase.Recovery && Elapsed >= total)
        {
            Phase = SkillCastPhase.Idle;
            Elapsed = 0f;
            Definition = null;
            result.ReturnedToIdle = true;
        }

        result.Phase = Phase;
        return result;
    }

    public void CancelWindup()
    {
        Phase = SkillCastPhase.Idle;
        Elapsed = 0f;
        Definition = null;
        _activeDurationOverride = -1f;
    }

    public void CancelImmediate()
    {
        CancelWindup();
    }

    /// <summary>
    /// Kończy fazę aktywną natychmiast — np. early stop szarży (M7.6-T7). CD pozostaje bez zmian.
    /// </summary>
    public bool SkipToRecovery()
    {
        if (Phase != SkillCastPhase.Active || Definition == null) return false;

        var activeDuration = _activeDurationOverride > 0f
            ? _activeDurationOverride
            : Definition.ActiveSeconds;
        Elapsed = Definition.WindupSeconds + activeDuration;
        Phase = SkillCastPhase.Recovery;
        return true;
    }

    public float RemainingActiveSeconds
    {
        get
        {
            if (Phase != SkillCastPhase.Active || Definition == null) return 0f;
            var activeDuration = _activeDurationOverride > 0f
                ? _activeDurationOverride
                : Definition.ActiveSeconds;
            var activeEnd = Definition.WindupSeconds + activeDuration;
            return Mathf.Max(0f, activeEnd - Elapsed);
        }
    }
}

public struct SkillCastTickResult
{
    public SkillCastPhase Phase;
    public bool EnteredActive;
    public bool EnteredRecovery;
    public bool ReturnedToIdle;
}

/// <summary>
/// Licznik odnowienia umiejętności — wspiera pauzę level-up (M7.5-T2/T3).
/// </summary>
public sealed class SkillCooldownTracker
{
    private float _remaining;
    private bool _paused;

    public float Remaining => _remaining;
    public float Normalized => MaxCooldown > 0f ? Mathf.Clamp01(_remaining / MaxCooldown) : 0f;
    public bool IsReady => _remaining <= 0f;
    public float MaxCooldown { get; private set; }

    public void Configure(float cooldownSeconds)
    {
        MaxCooldown = Mathf.Max(0f, cooldownSeconds);
    }

    public void SetPaused(bool paused)
    {
        _paused = paused;
    }

    public bool TryConsumeCharge()
    {
        if (!IsReady) return false;
        return true;
    }

    public void StartCooldown()
    {
        _remaining = MaxCooldown;
    }

    public void ApplyRefund(float refundFraction)
    {
        if (MaxCooldown <= 0f || refundFraction <= 0f) return;
        _remaining = Mathf.Max(0f, _remaining - MaxCooldown * refundFraction);
    }

    public void Tick(float deltaTime)
    {
        if (_paused || _remaining <= 0f) return;
        _remaining = Mathf.Max(0f, _remaining - deltaTime);
    }

    public void ResetCooldown()
    {
        _remaining = 0f;
    }
}

/// <summary>
/// Jednorazowa flaga przerwania windupu przez obrażenia (M7.5-T8).
/// </summary>
public sealed class WindupDamageInterruptFlag
{
    private bool _pending;

    public void NotifyDamageDuringWindup()
    {
        _pending = true;
    }

    public bool ConsumePending()
    {
        if (!_pending) return false;
        _pending = false;
        return true;
    }
}
