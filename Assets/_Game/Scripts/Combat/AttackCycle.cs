using UnityEngine;

/// <summary>
/// Czysta maszyna stanów ataku — testowalna bez sceny.
/// </summary>
public sealed class AttackCycle
{
    public AttackPhase Phase { get; private set; } = AttackPhase.Idle;
    public float Elapsed { get; private set; }
    public WeaponFamily Family { get; private set; }
    public WeaponFeelProfile Feel { get; private set; }
    public float Interval { get; private set; }

    public bool IsAttacking => Phase != AttackPhase.Idle;
    public bool ShouldDeferWeaponSwap => IsAttacking;

    public float MoveMultiplier =>
        Phase == AttackPhase.Windup || Phase == AttackPhase.Active
            ? Feel.MoveMultiplierDuringSwing
            : 1f;

    public bool TryStart(WeaponFeelProfile feel, float attackInterval)
    {
        if (IsAttacking) return false;
        if (attackInterval <= 0f) return false;

        Feel = feel;
        Family = feel.Family;
        Interval = attackInterval;
        Elapsed = 0f;
        Phase = AttackPhase.Windup;
        return true;
    }

    public AttackCycleTickResult Tick(float deltaTime)
    {
        var result = new AttackCycleTickResult { Phase = Phase };
        if (Phase == AttackPhase.Idle) return result;

        Elapsed += Mathf.Max(0f, deltaTime);
        var windup = Interval * Feel.WindupFraction;
        var active = Interval * Feel.ActiveFraction;

        if (Phase == AttackPhase.Windup && Elapsed >= windup)
        {
            if (Feel.IsRanged || active <= 0f)
            {
                result.SpawnProjectile = Feel.IsRanged;
                Phase = AttackPhase.Recovery;
                result.EnteredRecovery = true;
            }
            else
            {
                Phase = AttackPhase.Active;
                result.EnteredActive = true;
            }
        }

        if (Phase == AttackPhase.Active && Elapsed >= windup + active)
        {
            Phase = AttackPhase.Recovery;
            result.EnteredRecovery = true;
        }

        if (Phase == AttackPhase.Recovery && Elapsed >= Interval)
        {
            Phase = AttackPhase.Idle;
            Elapsed = 0f;
            result.ReturnedToIdle = true;
        }

        result.Phase = Phase;
        return result;
    }

    public void Cancel()
    {
        Phase = AttackPhase.Idle;
        Elapsed = 0f;
    }
}

public struct AttackCycleTickResult
{
    public AttackPhase Phase;
    public bool EnteredActive;
    public bool EnteredRecovery;
    public bool SpawnProjectile;
    public bool ReturnedToIdle;
}
