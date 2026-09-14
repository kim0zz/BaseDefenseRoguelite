using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Wykonawca umiejętności aktywnych gracza (M7.5-T2/T3, M7.6-T3 multi-skill).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerCharacter))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ForcedMovementReceiver))]
public class PlayerSkillController : MonoBehaviour
{
    public const int SkillSlotCount = SkillLoadout.SlotCount;
    public const int UltimateSlot = SkillLoadout.UltimateSlot;

    [SerializeField] private SkillDefinition[] skillSlots = new SkillDefinition[SkillSlotCount];
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private LayerMask obstacleLayers = 0;
    [SerializeField] private bool debugForceCrackZoneModifier;
    [SerializeField] private bool debugForceLeapStompModifier;

    private readonly SkillCastCycle _cycle = new();
    private readonly SkillLeapExecutor _leapExecutor = new();
    private readonly SkillLoadout _loadout = new();
    private readonly WindupDamageInterruptFlag _windupDamageInterrupt = new();
    private readonly List<SkillEffectKind>[] _slotEffects =
    {
        new List<SkillEffectKind>(),
        new List<SkillEffectKind>(),
        new List<SkillEffectKind>(),
        new List<SkillEffectKind>()
    };
    private readonly SkillDefinition[] _baseKit = new SkillDefinition[SkillLoadout.ActiveSlotCount];
    private readonly List<PersistentEffectBinding> _persistents = new();
    private readonly SkillOncePerTargetSet _chargeOncePerTarget = new();
    private PlayerPersistentEffects _persistentsHost;

    private PlayerCharacter _player;
    private Health _health;
    private PlayerAttackController _attack;
    private PlayerBuildState _build;
    private SkillTelegraphView _telegraph;
    private SkillConcreteFeedbackView _concreteFeedback;
    private CrowdControlImmunity _crowdControlImmunity;
    private GameFlowManager _flow;
    private ForcedMovementReceiver _forcedMovement;
    private int _activeSlot = -1;
    private Vector3 _lockedFacing = Vector3.forward;
    private bool _chargeAppliedFirstHitStop;
    private Vector3 _leapLandingPoint;
    private bool _leapHitsResolved;

    public SkillDefinition PrimarySkill => GetSkill(0);
    public SkillCastPhase Phase => _cycle.Phase;
    public float CooldownRemaining => GetCooldownRemaining(0);
    public float CooldownNormalized => GetCooldownNormalized(0);
    public bool IsCasting => _cycle.IsCasting;
    public float CastElapsed => _cycle.Elapsed;
    public int ActiveSlot => _activeSlot;
    public bool DebugForceCrackZoneModifier => debugForceCrackZoneModifier;
    public bool DebugForceLeapStompModifier => debugForceLeapStompModifier;

    public IReadOnlyList<SkillEffectKind> GetSlotEffects(int slot)
    {
        if (slot < 0 || slot >= _slotEffects.Length) return System.Array.Empty<SkillEffectKind>();
        return _slotEffects[slot];
    }

    public SkillDefinition GetSkill(int slot) => _loadout.GetSkill(slot);

    public float GetCooldownRemaining(int slot) => _loadout.GetCooldown(slot).Remaining;

    public float GetCooldownNormalized(int slot) => _loadout.GetCooldown(slot).Normalized;

    public void Configure(SkillDefinition skill) => ConfigureSlot(0, skill);

    public void Configure(IReadOnlyList<SkillDefinition> skills)
    {
        _loadout.Configure(skills);
        for (var i = 0; i < SkillLoadout.ActiveSlotCount; i++)
        {
            var skill = skills != null && i < skills.Count ? skills[i] : null;
            _baseKit[i] = skill;
            if (i < skillSlots.Length)
                skillSlots[i] = skill;
        }

        RebuildFromProgression();
    }

    public void ConfigureSlot(int slot, SkillDefinition skill, bool asBaseKit = true)
    {
        _loadout.ConfigureSlot(slot, skill);
        if (slot >= 0 && slot < skillSlots.Length)
            skillSlots[slot] = skill;
        if (asBaseKit && slot >= 0 && slot < _baseKit.Length)
            _baseKit[slot] = skill;
    }

    public void SetDebugForceCrackZone(bool enabled)
    {
        debugForceCrackZoneModifier = enabled;
        RebuildFromProgression();
    }

    public void SetDebugForceLeapStomp(bool enabled)
    {
        debugForceLeapStompModifier = enabled;
        RebuildFromProgression();
    }

    public void RefreshModifiers() => RebuildFromProgression();

    public void ResetActiveCooldowns() => _loadout.ResetActiveCooldowns();

    public void RebuildFromProgression()
    {
        var slots = new SkillDefinition[SkillSlotCount];
        for (var i = 0; i < SkillLoadout.ActiveSlotCount; i++)
            slots[i] = _baseKit[i];

        for (var i = 0; i < _slotEffects.Length; i++)
            _slotEffects[i].Clear();
        _persistents.Clear();

        if (_build != null)
            ProgressionApplier.ApplyAll(_build.ChosenTalents, slots, _slotEffects, _persistents);

        if (debugForceLeapStompModifier && slots[0] != null
            && slots[0].Locomotion != SkillLocomotionMode.Leap)
            slots[0] = SkillContentFactory.CreateSkok();

        if (debugForceCrackZoneModifier && !_slotEffects[0].Contains(SkillEffectKind.SpawnSlowZone))
            _slotEffects[0].Add(SkillEffectKind.SpawnSlowZone);

        GetComponent<SiegeSkillUpgrades>()?.ApplyToSlots(slots);

        for (var i = 0; i < SkillSlotCount; i++)
            ConfigureSlot(i, slots[i], asBaseKit: false);

        if (_persistentsHost == null)
            _persistentsHost = GetComponent<PlayerPersistentEffects>();
        _persistentsHost?.ReplaceAll(_persistents);
        SyncOrbitingHost(slots);
    }

    private void SyncOrbitingHost(SkillDefinition[] slots)
    {
        var ult = slots != null && SkillLoadout.UltimateSlot < slots.Length
            ? slots[SkillLoadout.UltimateSlot]
            : null;
        var wantsOrbit = ult != null && ult.SkillId == "bomberman_orbitale";
        var host = GetComponent<OrbitingDeployableHost>();
        if (wantsOrbit)
        {
            if (host == null)
                host = gameObject.AddComponent<OrbitingDeployableHost>();
            host.EnsureStarted();
        }
        else if (host != null)
        {
            host.StopAndClear();
        }
    }

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _health = GetComponent<Health>();
        _attack = GetComponent<PlayerAttackController>();
        _build = GetComponent<PlayerBuildState>();
        _flow = FindAnyObjectByType<GameFlowManager>();

        var telegraphGo = new GameObject("SkillTelegraph");
        telegraphGo.transform.SetParent(transform, false);
        _telegraph = telegraphGo.AddComponent<SkillTelegraphView>();
        _concreteFeedback = GetComponent<SkillConcreteFeedbackView>();
        if (_concreteFeedback == null)
            _concreteFeedback = gameObject.AddComponent<SkillConcreteFeedbackView>();
        _crowdControlImmunity = GetComponent<CrowdControlImmunity>();
        if (_crowdControlImmunity == null)
            _crowdControlImmunity = gameObject.AddComponent<CrowdControlImmunity>();
        _forcedMovement = GetComponent<ForcedMovementReceiver>();
        _persistentsHost = GetComponent<PlayerPersistentEffects>();
        if (_persistentsHost == null)
            _persistentsHost = gameObject.AddComponent<PlayerPersistentEffects>();

        for (var i = 0; i < skillSlots.Length; i++)
        {
            _loadout.ConfigureSlot(i, skillSlots[i]);
            if (i < _baseKit.Length)
                _baseKit[i] = skillSlots[i];
        }

        if (_build != null)
            _build.BuildChanged += OnBuildChanged;
    }

    private void OnDestroy()
    {
        if (_build != null)
            _build.BuildChanged -= OnBuildChanged;
    }

    private void OnEnable()
    {
        if (_health != null)
        {
            _health.Died += OnDied;
            _health.Damaged += OnDamaged;
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.Died -= OnDied;
            _health.Damaged -= OnDamaged;
        }
    }

    private void OnBuildChanged()
    {
        RefreshModifiers();
    }

    private void OnDamaged(float amount, GameObject source)
    {
        if (amount <= 0f || _cycle.Phase != SkillCastPhase.Windup) return;
        _windupDamageInterrupt.NotifyDamageDuringWindup();
    }

    private void OnDied()
    {
        _windupDamageInterrupt.ConsumePending();

        if (_cycle.Phase == SkillCastPhase.Windup)
        {
            RecordCancelledWindupTelemetry();
            CancelWindup(startCooldown: false);
        }
        else if (_cycle.IsCasting)
            AbortCastImmediate();
    }

    private void Update()
    {
        var paused = _flow != null &&
                     (_flow.State == GameFlowState.LevelUpPause || _flow.State == GameFlowState.RunFailed);
        _loadout.SetAllPaused(paused);

        if (paused && _cycle.Phase == SkillCastPhase.Windup)
        {
            RecordCancelledWindupTelemetry();
            CancelWindup(startCooldown: false);
            return;
        }

        if (_health == null || !_health.IsAlive || !_player.IsCombatEnabled)
        {
            if (_cycle.IsCasting)
                AbortCastImmediate();
            return;
        }

        var delta = Time.deltaTime;
        _loadout.TickAll(delta);

        if (_cycle.IsCasting)
        {
            TickCast(delta);
            return;
        }

        if (_player.PlayerIndex == 0)
            TryToggleDebugModifier();

        for (var slot = 0; slot < SkillSlotCount; slot++)
        {
            if (_player.TryReadSkillInput(slot))
                TryBeginCast(slot);
        }

        var buffered = _loadout.ConsumeBufferedSlot();
        if (buffered.HasValue && CanStartCast(buffered.Value))
            TryBeginCast(buffered.Value);
    }

    private SkillDefinition ActiveSkill =>
        _activeSlot >= 0 ? _loadout.GetSkill(_activeSlot) : null;

    private void TickCast(float delta)
    {
        if (_cycle.Phase == SkillCastPhase.Windup && _windupDamageInterrupt.ConsumePending())
        {
            RecordCancelledWindupTelemetry();
            CancelWindup(startCooldown: true, refund: true);
            return;
        }

        var skill = ActiveSkill;
        var isCharge = IsChargeSkill(skill);
        var isDash = IsDashPlace(skill);
        var isLeapStomp = IsLeapCast(skill);

        _player.SetAttackMoveMultiplier(GetCastMoveMultiplier(skill, isLeapStomp));
        ApplyFacingLock(skill);

        if (isCharge && _cycle.Phase == SkillCastPhase.Windup)
            UpdateChargeTelegraphAim();

        if (isLeapStomp && _cycle.Phase == SkillCastPhase.Windup)
            UpdateLeapWindupTelegraph(skill);

        var tick = _cycle.Tick(delta);
        var enteredRecovery = tick.EnteredRecovery;

        if (tick.EnteredActive)
        {
            CaptureLockedFacing();

            if (skill != null && skill.CooldownStartsOnActive)
                _loadout.GetCooldown(_activeSlot).StartCooldown();

            if (skill != null && skill.SkillId == "pudzian_ja_jestem_boss")
                _persistentsHost?.ActivateKolosForm(8f);

            if (skill != null && skill.ActivationMode == SkillActivationMode.Active
                && _activeSlot == UltimateSlot)
                _persistentsHost?.NotifyUltimateActivated();

            if (isDash)
                BeginDashActive(skill);
            else if (isCharge)
                BeginChargeActive(skill);
            else if (isLeapStomp)
                BeginLeapActive(skill);
            else
                ResolveActiveHits();
        }

        if (isLeapStomp && (_cycle.Phase == SkillCastPhase.Active || _leapExecutor.IsActive))
        {
            if (TickLeapActive(skill, delta))
                enteredRecovery = _cycle.Phase == SkillCastPhase.Recovery;
        }

        if ((isCharge || isDash) && _cycle.Phase == SkillCastPhase.Active)
        {
            if (TryHandleChargeEarlyStop())
                enteredRecovery = true;
            else if (isCharge)
                TickChargeActiveHits(skill);
        }

        if (enteredRecovery)
            OnEnteredRecovery(isCharge || isLeapStomp || isDash);

        if (tick.ReturnedToIdle)
        {
            _player.SetAttackMoveMultiplier(1f);
            UnlockFacing();
            _leapExecutor.Cancel();
            GetComponent<DamageImmunity>()?.Pop();
            _leapHitsResolved = false;
            _activeSlot = -1;
        }
    }

    private float GetRadiusMultiplier() =>
        _persistentsHost != null ? _persistentsHost.GetSkillRadiusMultiplier() : 1f;

    private void TryBeginCast(int slot)
    {
        if (!CanStartCast(slot)) return;
        var skill = _loadout.GetSkill(slot);
        if (skill == null) return;
        if (skill.ActivationMode == SkillActivationMode.Passive) return;
        if (!_loadout.GetCooldown(slot).TryConsumeCharge()) return;

        _activeSlot = slot;
        _leapHitsResolved = false;
        _leapExecutor.Cancel();
        SkillTelemetry.SetActiveSkill(skill.SkillId);
        _cycle.TryStart(skill);
        _player.SetAttackMoveMultiplier(_cycle.MoveMultiplier);

        if (IsLeapCast(skill))
        {
            _leapLandingPoint = ResolveLeapLandingPoint();
            _cycle.SetActiveDurationOverride(SkillLeapExecutor.DefaultTravelSeconds);
            _telegraph.BeginWindupAtWorldPosition(
                _leapLandingPoint,
                skill.RadiusMeters,
                skill.WindupSeconds,
                GetPlayerColor());
        }
        else if (skill.ShapeType == SkillShapeType.ChargeLine
                 || skill.ShapeType == SkillShapeType.AimStripBurst)
        {
            var range = skill.ShapeType == SkillShapeType.AimStripBurst
                ? DeployableTuning.AirstrikeSpacing * (skill.MaxTargets > 0 ? skill.MaxTargets : DeployableTuning.AirstrikeBurstCount)
                : skill.ChargeRangeMeters;
            var width = skill.ShapeType == SkillShapeType.AimStripBurst
                ? DeployableTuning.AirstrikeStripHalfWidth * 2f
                : skill.CapsuleWidthMeters;
            var aim = GetAimDirection();
            if (!_telegraph.BeginChargeLine(range, width, GetPlayerColor()))
            {
                AbortCastImmediate();
                Debug.LogWarning("[PlayerSkillController] Charge telegraph failed — cast aborted.");
                return;
            }

            _telegraph.SetChargeLineDirection(aim);
        }
        else
        {
            _telegraph.BeginWindup(
                skill.RadiusMeters,
                skill.WindupSeconds,
                GetPlayerColor(),
                skill.AppliesThreatOverride);
        }

        SpawnWindupTelegraphPrefab(skill);
        SkillFeedbackAudio.Ensure().PlayWindup(skill);

        Debug.Log($"[PlayerSkillController] P{_player.PlayerIndex + 1} slot {slot + 1} — {skill.DisplayName} windup.");
    }

    private bool CanStartCast(int slot)
    {
        if (slot < 0 || slot >= SkillSlotCount) return false;
        if (_loadout.GetSkill(slot) == null || _cycle.IsCasting) return false;
        if (!_loadout.GetCooldown(slot).IsReady) return false;
        if (_attack != null && _attack.IsAttacking && _attack.Phase == AttackPhase.Recovery)
            return true;
        if (_attack != null && _attack.IsAttacking) return false;
        return true;
    }

    public void BufferCastFromAttackRecovery(int slot)
    {
        _loadout.BufferSlot(slot);
    }

    private void ResolveActiveHits(Vector3? hitCenterOverride = null)
    {
        var skill = ActiveSkill;
        if (skill == null) return;

        var center = hitCenterOverride ?? transform.position;
        SpawnImpactFeedback(skill, center);

        if (skill.ShapeType == SkillShapeType.LaneWaves)
        {
            SkillLaneWaveExecutor.Execute(skill, gameObject, targetLayers);
            StartCooldownIfNeeded(skill);
            RecordUseTelemetry(skill, default);
            Debug.Log($"[PlayerSkillController] {skill.DisplayName} — fale na 3 linie.");
            return;
        }

        if (skill.ShapeType == SkillShapeType.PlaceDeployable)
        {
            var aim = GetAimDirection();
            var pos = transform.position + aim * DeployableTuning.PlaceOffsetMeters;
            DeployableRegistry.Place(gameObject, pos, skill);
            if (_persistentsHost != null && _persistentsHost.Has(PersistentEffectKind.ArmToProximityMine))
            {
                var placed = DeployableRegistry.FindNearest(gameObject, pos, 0.5f);
                placed?.Motor.BeginArming();
            }
            StartCooldownIfNeeded(skill);
            RecordUseTelemetry(skill, default);
            return;
        }

        if (skill.ShapeType == SkillShapeType.DetonateOwned)
        {
            DeployableRegistry.DetonateAllDetonatable(gameObject);
            StartCooldownIfNeeded(skill);
            RecordUseTelemetry(skill, default);
            return;
        }

        if (skill.ShapeType == SkillShapeType.LaunchNearestOwned)
        {
            ResolveLaunchNearestOwned(skill);
            StartCooldownIfNeeded(skill);
            RecordUseTelemetry(skill, default);
            return;
        }

        if (skill.ShapeType == SkillShapeType.AimStripBurst)
        {
            var aim = GetAimDirection();
            SkillAimStripBurstExecutor.Execute(skill, gameObject, transform.position, aim, targetLayers);
            StartCooldownIfNeeded(skill);
            RecordUseTelemetry(skill, default);
            return;
        }

        if (skill.ShapeType != SkillShapeType.Circle)
        {
            StartCooldownIfNeeded(skill);
            RecordUseTelemetry(skill, default);
            Debug.Log($"[PlayerSkillController] {skill.DisplayName} — kształt {skill.ShapeType}.");
            return;
        }

        var radius = skill.RadiusMeters * GetRadiusMultiplier();
        var hits = SkillCircleOverlap.Query(
            center,
            radius,
            skill.MaxTargets,
            targetLayers,
            obstacleLayers);

        var result = SkillHitResolver.ApplyHits(skill, hits, gameObject, GetActiveEffects(_activeSlot), center);
        StartCooldownIfNeeded(skill);

        if (skill.AppliesThreatOverride)
        {
            var threat = SkillThreatResolver.Apply(skill, hits, _player, GetPlayerColor());
            var telemetryResult = new SkillHitResolver.ResolveResult
            {
                HitCount = threat.TauntCount,
                HadHit = threat.HadSuccessfulTaunt,
                TotalDamage = 0f,
                TotalControlSeconds = 0f,
                DistinctLaneCount = result.DistinctLaneCount
            };
            RecordUseTelemetry(skill, telemetryResult);

            if (threat.HadSuccessfulTaunt)
            {
                _persistentsHost?.NotifyTauntPulseApplied();
                _concreteFeedback.ShowOutline(GetPlayerColor(), skill.ThreatDurationSeconds);

                var feel = CombatFeelService.Ensure();
                feel.RequestShakeAtEvent(skill.ShakeStrength, center);
                _player.RequestGamepadRumble(skill.RumbleLow, skill.RumbleHigh, 0.15f);
                Debug.Log($"[PlayerSkillController] {skill.DisplayName} taunt: {threat.TauntCount}.");
            }
            else
            {
                Debug.Log($"[PlayerSkillController] {skill.DisplayName} pudło — CD aktywny.");
            }

            return;
        }

        RecordUseTelemetry(skill, result);

        if (result.HadHit)
        {
            var feel = CombatFeelService.Ensure();
            feel.RequestHitStop(skill.HitStopSeconds);
            feel.RequestShakeAtEvent(skill.ShakeStrength, center);
            _player.RequestGamepadRumble(skill.RumbleLow, skill.RumbleHigh, 0.15f);
            Debug.Log($"[PlayerSkillController] {skill.DisplayName} trafienia: {result.HitCount}.");
        }
        else
        {
            Debug.Log($"[PlayerSkillController] {skill.DisplayName} pudło — CD aktywny.");
        }
    }

    private void StartCooldownIfNeeded(SkillDefinition skill)
    {
        if (!skill.CooldownStartsOnActive)
            _loadout.GetCooldown(_activeSlot).StartCooldown();
    }

    private IReadOnlyList<SkillEffectKind> GetActiveEffects(int slot)
    {
        if (slot < 0 || slot >= _slotEffects.Length)
            return System.Array.Empty<SkillEffectKind>();
        return _slotEffects[slot];
    }

    private void AbortCastImmediate()
    {
        if (_cycle.Phase == SkillCastPhase.Windup)
            _cycle.CancelWindup();
        else if (_cycle.IsCasting)
            _cycle.CancelImmediate();

        CleanupAfterCastEnd();
    }

    private void CleanupAfterCastEnd()
    {
        _telegraph.Hide();
        _leapExecutor.Cancel();
        _player.SetAttackMoveMultiplier(1f);
        UnlockFacing();
        GetComponent<DamageImmunity>()?.SetActive(false);
        _leapHitsResolved = false;
        _activeSlot = -1;
    }

    private void CancelWindup(bool startCooldown, bool refund = false)
    {
        var slot = _activeSlot;
        var skill = ActiveSkill;
        _cycle.CancelWindup();
        CleanupAfterCastEnd();

        if (startCooldown && skill != null && slot >= 0)
        {
            _loadout.GetCooldown(slot).StartCooldown();
            if (refund)
                _loadout.GetCooldown(slot).ApplyRefund(skill.WindupInterruptCooldownRefund);
        }
    }

    private void ApplyFacingLock(SkillDefinition skill)
    {
        if (skill == null) return;

        var chargeLock = skill.Locomotion == SkillLocomotionMode.Charge
            && (_cycle.Phase == SkillCastPhase.Active || _cycle.Phase == SkillCastPhase.Recovery);

        if ((skill.LocksFacingInActive && _cycle.Phase == SkillCastPhase.Active) || chargeLock)
        {
            _player.SetFacingLock(true, _lockedFacing);
            return;
        }

        if (_cycle.Phase == SkillCastPhase.Windup)
            UpdateWindupFacingPreview(skill);
    }

    private void UnlockFacing()
    {
        _player.SetFacingLock(false, _player.AimDirection);
    }

    private static bool IsChargeSkill(SkillDefinition skill) =>
        skill != null && skill.ShapeType == SkillShapeType.ChargeLine;

    private static bool IsDashPlace(SkillDefinition skill) =>
        skill != null && skill.ShapeType == SkillShapeType.PlaceAndDash;

    private float GetCastMoveMultiplier(SkillDefinition skill, bool isLeapStomp = false)
    {
        if (skill != null
            && skill.Locomotion == SkillLocomotionMode.Charge
            && _cycle.Phase == SkillCastPhase.Active)
            return 0f;

        if (isLeapStomp
            && (_cycle.Phase == SkillCastPhase.Active || _leapExecutor.IsActive))
            return 0f;

        return _cycle.MoveMultiplier;
    }

    private Vector3 GetAimDirection()
    {
        return _player.AimDirection.sqrMagnitude > 0.01f
            ? _player.AimDirection.normalized
            : _player.FacingDirection.normalized;
    }

    private void CaptureLockedFacing()
    {
        _lockedFacing = _player.FacingDirection.sqrMagnitude > 0.01f
            ? _player.FacingDirection.normalized
            : GetAimDirection();
    }

    private void UpdateWindupFacingPreview(SkillDefinition skill)
    {
        if (skill != null && skill.ShapeType == SkillShapeType.ChargeLine)
            _telegraph.SetChargeLineDirection(GetAimDirection());
    }

    private void UpdateChargeTelegraphAim()
    {
        _telegraph.SetChargeLineDirection(GetAimDirection());
    }

    private void BeginDashActive(SkillDefinition skill)
    {
        DeployableRegistry.PlaceDashCharge(gameObject, transform.position);

        var dashDir = AimMath.ResolveDashDirection(
            _player.CurrentMoveDirection,
            _player.LastMoveDirection,
            _player.FacingDirection);

        var charge = ForcedMovementRequest.CasterCharge(
            gameObject,
            dashDir,
            skill.ChargeSpeed > 0f ? skill.ChargeSpeed : DeployableTuning.DashSpeed,
            skill.ActiveSeconds,
            honorCollisions: true);
        ForcedMovementResolver.Apply(charge, gameObject);
        RecordUseTelemetry(skill, default);
    }

    private void BeginChargeActive(SkillDefinition skill)
    {
        _chargeOncePerTarget.Clear();
        _chargeAppliedFirstHitStop = false;

        if (skill.ChargeContactMode == ChargeContactMode.Carry)
            ChargeCarryTracker.BeginCharge(gameObject, _lockedFacing);

        var charge = ForcedMovementRequest.CasterCharge(
            gameObject,
            _lockedFacing,
            skill.ChargeSpeed,
            skill.ActiveSeconds,
            honorCollisions: true);
        ForcedMovementResolver.Apply(charge, gameObject);
    }

    private bool TryHandleChargeEarlyStop()
    {
        if (_forcedMovement == null) return false;

        var reason = _forcedMovement.LastChargeStopReason;
        if (reason != ChargeStopReason.Boss
            && reason != ChargeStopReason.Structure
            && reason != ChargeStopReason.PlayAreaEdge
            && reason != ChargeStopReason.Obstacle)
            return false;

        if (!_cycle.SkipToRecovery()) return false;

        if (_activeSlot >= 0
            && _slotEffects[_activeSlot].Contains(SkillEffectKind.PulseRingOnWallStop)
            && (reason == ChargeStopReason.Structure
                || reason == ChargeStopReason.Obstacle
                || reason == ChargeStopReason.PlayAreaEdge))
            SkillEffectApplier.SpawnPulseRing(transform.position, gameObject, ActiveSkill);

        _telegraph.Hide();
        return true;
    }

    private void TickChargeActiveHits(SkillDefinition skill)
    {
        var hits = SkillChargeCapsuleOverlap.Query(
            transform.position,
            _lockedFacing,
            skill.CapsuleWidthMeters,
            skill.CapsuleLengthMeters,
            skill.MaxTargets,
            targetLayers);

        var result = SkillHitResolver.ApplyOncePerTargetHits(
            skill,
            hits,
            gameObject,
            _lockedFacing,
            _cycle.RemainingActiveSeconds,
            _chargeOncePerTarget,
            GetActiveEffects(_activeSlot));

        if (!result.HadHit) return;

        var feel = CombatFeelService.Ensure();
        if (!_chargeAppliedFirstHitStop)
        {
            feel.RequestHitStop(skill.HitStopSeconds);
            _chargeAppliedFirstHitStop = true;
        }

        feel.RequestShakeAtEvent(skill.ShakeStrength, transform.position);
        _player.RequestGamepadRumble(skill.RumbleLow, skill.RumbleHigh, 0.15f);
        RecordUseTelemetry(skill, result);
        Debug.Log($"[PlayerSkillController] {skill.DisplayName} trafienia: {result.HitCount}.");
    }

    private void OnEnteredRecovery(bool isCharge)
    {
        _telegraph.Hide();
        if (isCharge)
            ChargeCarryTracker.EndCharge(gameObject);
        else
            UnlockFacing();
    }

    private void TryToggleDebugModifier()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.f8Key.wasPressedThisFrame)
        {
            SetDebugForceCrackZone(!debugForceCrackZoneModifier);
            Debug.Log($"[PlayerSkillController] Debug Pęknięcie = {debugForceCrackZoneModifier}");
        }

        if (keyboard.f9Key.wasPressedThisFrame)
        {
            SetDebugForceLeapStomp(!debugForceLeapStompModifier);
            Debug.Log($"[PlayerSkillController] Debug Skok = {debugForceLeapStompModifier}");
        }
    }

    private static bool IsLeapCast(SkillDefinition skill) =>
        skill != null && skill.Locomotion == SkillLocomotionMode.Leap;

    private Vector3 ResolveLeapLandingPoint()
    {
        var feet = transform.position;
        feet.y = 0f;
        Vector3 desired;

        if (_player.InputMode == PlayerInputMode.KeyboardMouse)
        {
            var cam = _player.AimCamera;
            var mouse = Mouse.current;
            if (mouse != null && cam != null)
            {
                var ray = cam.ScreenPointToRay(mouse.position.ReadValue());
                if (!SkillLeapAimMath.TryResolveKeyboardPoint(
                        ray,
                        transform.position.y,
                        feet,
                        SkillLeapAimMath.MaxLeapDistance,
                        out desired))
                    desired = feet;
            }
            else
            {
                desired = SkillLeapAimMath.ResolvePointFromDirection(
                    feet,
                    GetAimDirection(),
                    SkillLeapAimMath.MaxLeapDistance);
            }
        }
        else
        {
            var stickMagnitude = ReadRightStickMagnitude();
            desired = SkillLeapAimMath.ResolveGamepadPoint(
                feet,
                GetAimDirection(),
                stickMagnitude);
        }

        return SkillLeapAimMath.ResolveLastLegalPoint(feet, desired, MapPlayArea.Instance);
    }

    private float ReadRightStickMagnitude()
    {
        var pad = _player.AssignedGamepad;
        if (pad == null) return 0f;
        return pad.rightStick.ReadValue().magnitude;
    }

    private void UpdateLeapWindupTelegraph(SkillDefinition skill)
    {
        _leapLandingPoint = ResolveLeapLandingPoint();
        _telegraph.SetWorldCenter(_leapLandingPoint);
        UpdateWindupFacingPreview(skill);
    }

    private void BeginLeapActive(SkillDefinition skill)
    {
        _leapLandingPoint = ResolveLeapLandingPoint();
        _telegraph.SetWorldCenter(_leapLandingPoint);

        if (SkillLeapAimMath.IsFeetLanding(transform.position, _leapLandingPoint))
        {
            ResolveActiveHits(_leapLandingPoint);
            _leapHitsResolved = true;
            return;
        }

        var immunity = GetComponent<DamageImmunity>() ?? gameObject.AddComponent<DamageImmunity>();
        immunity.Push();
        _leapExecutor.Begin(
            transform,
            transform.position,
            _leapLandingPoint,
            SkillLeapExecutor.DefaultTravelSeconds,
            SkillLeapExecutor.DefaultArcHeight);
    }

    private bool TickLeapActive(SkillDefinition skill, float delta)
    {
        if (_leapHitsResolved)
            return false;

        if (_leapExecutor.IsActive)
        {
            if (!_leapExecutor.Tick(delta))
                return false;

            ResolveActiveHits(_leapExecutor.Destination);
            _leapHitsResolved = true;
            GetComponent<DamageImmunity>()?.Pop();
            return true;
        }

        if (_cycle.Phase == SkillCastPhase.Active && !_leapHitsResolved)
        {
            ResolveActiveHits(_leapLandingPoint);
            _leapHitsResolved = true;
            return true;
        }

        return false;
    }

    private void RecordUseTelemetry(SkillDefinition skill, SkillHitResolver.ResolveResult result)
    {
        var skillId = skill != null ? skill.SkillId : SkillTelemetry.UnknownSkillId;
        SkillTelemetry.RecordUse(skillId, result, cancelled: false);
    }

    private void RecordCancelledWindupTelemetry()
    {
        var skill = ActiveSkill;
        var skillId = skill != null ? skill.SkillId : SkillTelemetry.UnknownSkillId;
        SkillTelemetry.RecordCancelledWindup(skillId);
    }

    private Color GetPlayerColor()
    {
        var colors = new[]
        {
            new Color(1f, 0.2f, 0.2f),
            new Color(0.2f, 0.4f, 1f),
            new Color(0.2f, 0.9f, 0.3f),
            new Color(1f, 0.9f, 0.1f)
        };
        var index = Mathf.Clamp(_player.PlayerIndex, 0, colors.Length - 1);
        return colors[index];
    }

    private void SpawnWindupTelegraphPrefab(SkillDefinition skill)
    {
        if (skill == null || skill.TelegraphPrefab == null) return;

        if (skill.ShapeType == SkillShapeType.ChargeLine)
            return;

        var position = IsLeapCast(skill) ? _leapLandingPoint : transform.position;
        SkillVfxSpawner.SpawnTelegraph(skill.TelegraphPrefab, position, skill.WindupSeconds + 0.2f);
    }

    private void ResolveLaunchNearestOwned(SkillDefinition skill)
    {
        var aim = GetAimDirection();
        DeployableKickAreaResolver.Execute(skill, gameObject, transform.position, aim, targetLayers);
    }

    private void SpawnImpactFeedback(SkillDefinition skill, Vector3 center)
    {
        if (skill == null) return;

        if (skill.ShapeType == SkillShapeType.LaneWaves)
        {
            SkillFeedbackAudio.Ensure().PlayImpact(skill);
            return;
        }

        if (skill.ImpactPrefab != null)
            SkillVfxSpawner.SpawnImpact(skill.ImpactPrefab, center, 0.6f);
        else
            _telegraph.FlashActiveRing();

        SkillFeedbackAudio.Ensure().PlayImpact(skill);
    }
}
