using UnityEngine;

/// <summary>
/// Maszyna ataku gracza: windup → active/pocisk → recovery. Combo 3-hit gdy profil combo (M8.1b).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerCharacter))]
[RequireComponent(typeof(Health))]
public class PlayerAttackController : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayers = ~0;

    private readonly AttackCycle _cycle = new();
    private readonly ComboAttackCycle _comboCycle = new();
    private PlayerCharacter _player;
    private Health _health;
    private PlayerBuildState _build;
    private PlayerPersistentEffects _persistents;
    private WeaponPlaceholderView _view;
    private EffectiveCombatStats _stats;
    private WeaponDefinition _weapon;
    private AttackComboDefinition _combo;
    private bool _hasStats;
    private bool _useCombo;
    private Vector3 _attackFacing = Vector3.forward;
    private bool _pendingSwap;
    private bool _holdAttack;

    public bool IsAttacking => _useCombo ? _comboCycle.IsAttacking : _cycle.IsAttacking;
    // Read-only timing for ranged model animation; damage remains owned by the attack cycle.
    public float SingleAttackElapsed => _cycle.Elapsed;
    public float SingleAttackInterval => _cycle.Interval;
    public AttackPhase Phase => _useCombo ? _comboCycle.Phase : _cycle.Phase;
    public int ComboStep => _useCombo ? _comboCycle.CurrentHitIndex + 1 : 1;
    public WeaponFamily CycleFamily => IsAttacking
        ? (_useCombo ? WeaponFamily.Axe : _cycle.Family)
        : ActiveFamily;
    public Vector3 AttackFacing => _attackFacing;
    public Vector3 LockedFacing => _attackFacing;
    public float EffectiveRange => _stats.Range;
    public float EffectiveArc => _stats.ArcDegrees;
    public bool HasPendingSwap => _pendingSwap;

    public event System.Action BecameIdle;

    private WeaponFamily ActiveFamily =>
        _weapon != null ? _weapon.Family : WeaponFamily.Sword;

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _health = GetComponent<Health>();
        _build = GetComponent<PlayerBuildState>();
        _persistents = GetComponent<PlayerPersistentEffects>();
        _view = GetComponent<WeaponPlaceholderView>();
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.Died += CancelAttack;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.Died -= CancelAttack;
    }

    public void ApplyEffectiveStats(EffectiveCombatStats stats, WeaponDefinition weapon)
    {
        _stats = stats;
        _weapon = weapon;
        _combo = weapon?.MeleeProfile?.ComboProfile;
        _useCombo = _combo != null && _combo.HitCount > 0;
        _hasStats = weapon != null || stats.AttackInterval > 0f;
    }

    public void QueueWeaponSwap()
    {
        if (IsAttacking && (_useCombo ? _comboCycle.ShouldDeferWeaponSwap : _cycle.ShouldDeferWeaponSwap))
        {
            _pendingSwap = true;
            return;
        }

        ExecuteSwap();
    }

    public void CancelAttack()
    {
        _cycle.Cancel();
        _comboCycle.Cancel();
        ApplyIdleLocomotion();
        _view?.HideSwing();
    }

    private void Update()
    {
        if (_health == null || !_health.IsAlive || !_player.IsCombatEnabled)
        {
            if (IsAttacking)
                CancelAttack();
            return;
        }

        if (!_hasStats) return;

        if (_view == null) _view = GetComponent<WeaponPlaceholderView>();
        if (_build == null) _build = GetComponent<PlayerBuildState>();
        if (_persistents == null) _persistents = GetComponent<PlayerPersistentEffects>();

        _holdAttack = _player.TryReadAttackHeld();

        if (IsAttacking)
        {
            BufferSkillDuringRecovery();
            if (_useCombo)
                TickComboAttack();
            else
                TickSingleAttack();
            return;
        }

        if (_useCombo)
            _comboCycle.Tick(Time.deltaTime);

        if (!_player.TryReadAttackInput()) return;

        if (_useCombo)
            StartComboAttack();
        else
            StartSingleAttack();
    }

    private void TickSingleAttack()
    {
        TrackAimDuringWindup();
        var tick = _cycle.Tick(Time.deltaTime);
        _player.SetAttackMoveMultiplier(_cycle.MoveMultiplier);
        ApplyFacingLockForPhase(_cycle.Phase);

        if (tick.EnteredActive)
        {
            CommitAttackFacing();
            ResolveMeleeHits(_stats.Damage, _stats.Range, _stats.ArcDegrees, _stats.MaxTargets, _cycle.Feel.Knockback, 0.6f);
        }

        if (tick.SpawnProjectile)
        {
            CommitAttackFacing();
            if (ActiveFamily == WeaponFamily.ThrownExplosive)
                SpawnExplosiveProjectile();
            else
                SpawnBowProjectile();
        }

        if (tick.EnteredRecovery)
            UnlockFacing();

        if (tick.ReturnedToIdle)
            OnBecameIdle();
    }

    private void TickComboAttack()
    {
        if (_comboCycle.Phase == AttackPhase.Windup)
            _attackFacing = ReadAimFacing();

        var tick = _comboCycle.Tick(Time.deltaTime, _holdAttack);
        var speedMul = _persistents != null ? _persistents.GetAttackSpeedMultiplier() : 1f;
        _player.SetAttackMoveMultiplier(_comboCycle.MoveMultiplier);

        if (_comboCycle.Phase == AttackPhase.Active)
            _player.SetFacingLock(true, _attackFacing);
        else if (_comboCycle.Phase == AttackPhase.Windup)
            _player.SetFacingLock(false, _player.AimDirection);
        else
            UnlockFacing();

        if (_persistents != null)
            _persistents.ComboStepDisplay = _comboCycle.CurrentHitIndex + 1;

        if (tick.EnteredActive)
        {
            CommitAttackFacing();
            var hit = _comboCycle.CurrentHit;
            if (hit == null) return;
            var mods = _persistents != null
                ? _persistents.GetComboHitModifiers(_comboCycle.CurrentHitIndex)
                : ComboHitModifiers.Identity;
            var dmg = hit.Damage * mods.DamageMultiplier;
            if (_persistents != null)
                dmg = _persistents.ModifyOutgoing(dmg);
            var range = (hit.RangeMeters + mods.RangeBonus) * mods.RangeMultiplier;
            ResolveMeleeHits(dmg, range, hit.ArcDegrees, hit.MaxTargets, hit.Knockback, hit.Stagger * mods.StaggerMultiplier);
        }

        if (tick.EnteredRecovery)
            UnlockFacing();

        if (tick.ReturnedToIdle)
            OnBecameIdle();
    }

    private void StartComboAttack()
    {
        if (!_comboCycle.TryStart(_combo, _holdAttack)) return;
        _attackFacing = ReadAimFacing();
        UnlockFacing();
        _player.SetAttackMoveMultiplier(_comboCycle.MoveMultiplier);
        _view?.NotifyAttackStarted();
        _view?.RebuildWeapon();
    }

    private void StartSingleAttack()
    {
        var feel = WeaponFeelProfile.For(ActiveFamily);
        var interval = ResolveAttackInterval();
        if (!_cycle.TryStart(feel, interval)) return;

        _attackFacing = ReadAimFacing();
        UnlockFacing();
        _player.SetAttackMoveMultiplier(_cycle.MoveMultiplier);
        _view?.NotifyAttackStarted();
        _view?.RebuildWeapon();
    }

    private void BufferSkillDuringRecovery()
    {
        var phase = _useCombo ? _comboCycle.Phase : _cycle.Phase;
        if (phase != AttackPhase.Recovery) return;
        var skill = GetComponent<PlayerSkillController>();
        if (skill == null) return;

        for (var slot = 0; slot < PlayerSkillController.SkillSlotCount; slot++)
        {
            if (!_player.TryReadSkillInput(slot)) continue;
            skill.BufferCastFromAttackRecovery(slot);
            return;
        }
    }

    private void TrackAimDuringWindup()
    {
        if (_cycle.Phase != AttackPhase.Windup) return;
        _attackFacing = ReadAimFacing();
        UnlockFacing();
    }

    private void ApplyFacingLockForPhase(AttackPhase phase)
    {
        if (AttackFacingPolicy.LocksFacing(phase))
            _player.SetFacingLock(true, _attackFacing);
        else
            UnlockFacing();
    }

    private void CommitAttackFacing()
    {
        _attackFacing = ReadAimFacing();
    }

    private void UnlockFacing()
    {
        _player.SetFacingLock(false, _player.AimDirection);
    }

    private Vector3 ReadAimFacing()
    {
        var aim = _player.AimDirection;
        if (aim.sqrMagnitude > 0.01f) return aim.normalized;
        var facing = _player.FacingDirection;
        return facing.sqrMagnitude > 0.01f ? facing.normalized : Vector3.forward;
    }

    private void ResolveMeleeHits(
        float damage, float range, float arc, int maxTargets, float knockback, float stagger)
    {
        var origin = transform.position + Vector3.up * 0.5f;
        var forward = _attackFacing;
        var hits = Physics.OverlapSphere(origin, range, enemyLayers);
        var damaged = 0;
        var landed = false;
        var hitTargets = new System.Collections.Generic.List<IDamageable>();
        var uniqueEffects = GetComponent<PlayerUniqueWeaponEffects>();

        foreach (var hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            var hitPoint = hit.ClosestPoint(origin);
            if (!MeleeHitResolver.IsHitPointInMeleeArc(origin, forward, hitPoint, range, arc))
                continue;

            var damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) continue;
            if (hit.GetComponentInParent<PlayerCharacter>() != null) continue;

            damageable.TakeDamage(damage, gameObject);
            hit.GetComponentInParent<HitFlashFeedback>()?.PlayFlash();
            hit.GetComponentInParent<KnockbackReceiver>()?.AddFromOrigin(origin, knockback, gameObject, restoreToLaneAfter: true);

            var status = hit.GetComponentInParent<StatusEffectReceiver>();
            status?.AddStagger(stagger);
            TryApplyViperFangPoison(status);

            hitTargets.Add(damageable);
            landed = true;
            damaged++;
            if (damaged >= Mathf.Max(1, maxTargets)) break;
        }

        if (landed)
        {
            uniqueEffects?.OnMeleeHitResolved(hitTargets, knockback);
            var feel = CombatFeelService.Ensure();
            feel.RequestHitStop(_useCombo ? 0.06f : _cycle.Feel.HitStopSeconds);
            feel.RequestShake(_useCombo ? 0.2f : _cycle.Feel.Shake);
            SkillFeedbackAudio.Ensure().PlayImpact(null);
        }
    }

    private void TryApplyViperFangPoison(StatusEffectReceiver status)
    {
        if (status == null || _weapon == null || _weapon.WeaponId != "viper_fang") return;
        if (Random.value > 0.25f) return;
        status.Apply(StatusEffectType.Poison, 3f, 2f, gameObject);
    }

    private float ResolveAttackInterval()
    {
        if (_persistents != null && _persistents.TryGetAttackIntervalOverride(out var overrideSec))
            return Mathf.Max(0.05f, overrideSec);

        var interval = Mathf.Max(0.05f, _stats.AttackInterval);
        if (_persistents != null)
            interval /= _persistents.GetAttackSpeedMultiplier();
        return interval;
    }

    private void SpawnBowProjectile()
    {
        var origin = transform.position + Vector3.up * 0.6f + _attackFacing * 0.4f;
        var feel = _cycle.Feel;
        Projectile.Spawn(
            origin,
            _attackFacing,
            feel.ProjectileSpeed > 0f ? feel.ProjectileSpeed : ProjectileRules.ProjectileSpeed,
            _stats.Range,
            _stats.Damage,
            gameObject,
            feel);
    }

    private void SpawnExplosiveProjectile()
    {
        var origin = transform.position + Vector3.up * 0.6f + _attackFacing * 0.4f;
        var feel = _cycle.Feel;
        var profile = _weapon?.MeleeProfile;
        var baseSplash = profile != null && profile.SplashRadius > 0f
            ? profile.SplashRadius
            : DeployableTuning.BasicSplashRadius;
        var splash = _persistents != null
            ? _persistents.GetBasicSplashRadius(baseSplash)
            : baseSplash;
        var damage = _stats.Damage;
        if (_persistents != null)
            damage *= _persistents.GetBasicDamageMultiplier();

        var arcHeight = profile != null ? profile.ArcHeight : DeployableTuning.ThrownExplosiveArcPeak;
        var maxRange = _stats.Range > 0f ? _stats.Range : DeployableTuning.ThrownExplosiveMaxRange;
        var projectileProfile = new ProjectileProfile(
            ProjectileImpactMode.ExplodeOnFirstHitOrObstacle,
            splash,
            arcHeight,
            profile != null ? profile.MaxTargets : 5,
            3f);

        Projectile.Spawn(
            origin,
            _attackFacing,
            feel.ProjectileSpeed > 0f ? feel.ProjectileSpeed : DeployableTuning.ThrownExplosiveSpeed,
            maxRange,
            damage,
            gameObject,
            feel,
            projectileProfile);
    }

    private void OnBecameIdle()
    {
        ApplyIdleLocomotion();
        if (_persistents != null) _persistents.ComboStepDisplay = 0;
        BecameIdle?.Invoke();
        if (!_pendingSwap) return;
        _pendingSwap = false;
        ExecuteSwap();
    }

    private void ExecuteSwap()
    {
        _build?.CycleActiveWeapon();
    }

    private void ApplyIdleLocomotion()
    {
        _player.SetAttackMoveMultiplier(1f);
        UnlockFacing();
    }
}
