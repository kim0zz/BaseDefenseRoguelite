using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Efekty trwałe z talentów — fury, hart, aura, last chance, kolos (M8.1b).
/// </summary>
[DisallowMultipleComponent]
public class PlayerPersistentEffects : MonoBehaviour
{
    private readonly List<PersistentEffectBinding> _bindings = new();
    private readonly Dictionary<PersistentEffectKind, EffectTuning> _tuning = new();
    private readonly HashSet<PersistentEffectKind> _active = new();

    private Health _health;
    private PlayerCharacter _player;
    private Vector3 _baseScale = Vector3.one;
    private bool _scaleCaptured;
    private bool _reflecting;

    private float _furyMeter;
    private bool _furyActive;
    private float _furyTimer;
    private int _hartStacks;
    private bool _hartReady;
    private int _zryjConvertRemaining;
    private bool _lastChanceUsedThisWave;
    private bool _lastChanceActive;
    private float _lastChanceTimer;
    private float _lastChanceHealAccum;
    private bool _kolosActive;
    private float _kolosTimer;
    private float _kolosExtraDuration;
    private float _auraTimer;
    private float _moveShockwaveDistance;
    private float _moveShockwaveTimer;
    private float _kolosShoveTimer;
    private float _kolosTauntTimer;
    private float _agonyTimer;
    private float _fireTrailDistance;
    private float _secondWindOutgoingTimer;
    private float _secondWindIncomingTimer;
    private Vector3 _lastPosition;
    private Vector3 _lastMoveDirection = Vector3.forward;
    private float _movedDistanceThisFrame;
    private bool _hasLastPosition;

    private readonly Dictionary<Health, AuraRampState> _auraRamps = new();
    private readonly HashSet<Health> _chainIgnited = new();
    private readonly Queue<GameObject> _fireTrailSegments = new();
    private WaveManager _waveManager;

    private struct AuraRampState
    {
        public int ExtraTicks;
        public float GraceTimer;
        public float TimeInAura;
        public bool ChainIgnited;
    }

    public IReadOnlyCollection<PersistentEffectKind> Active => _active;

    public float FuryMeter => _furyMeter;
    public float FuryThreshold => GetTuningFloat(PersistentEffectKind.FuryMeter, 0, 45f);
    public bool FuryActive => _furyActive;
    public int HartStacks => _hartStacks;
    public int HartMaxStacks => GetTuningInt(PersistentEffectKind.HartStacks, 0, 5);
    public bool HartReady => _hartReady;
    public bool LastChanceActive => _lastChanceActive;
    public float LastChanceHealAccum => _lastChanceHealAccum;
    public float LastChanceSurviveThreshold
    {
        get
        {
            var health = ResolveHealth();
            return health != null ? health.MaxHealth * GetLastChanceSurviveFraction() : 0f;
        }
    }
    public bool KolosActive => _kolosActive;
    public float KolosRemaining => _kolosTimer;
    public bool HasHellAuraRamping => Has(PersistentEffectKind.HellAuraRamping);
    public float GetHellAuraRadius() => HasHellAuraRamping ? GetAuraRadius() : 0f;
    public int ComboStepDisplay { get; set; }

    public float GetTuningFloatPublic(PersistentEffectKind kind, int index, float fallback) =>
        GetTuningFloat(kind, index, fallback);

    public float GetLastChanceHealAmpMultiplier() =>
        GetTuningFloat(PersistentEffectKind.HealAmpInLastChance, 0, 1.75f);

    public static float ModifyOutgoingDamage(GameObject caster, float damage)
    {
        if (caster == null) return damage;
        var persistents = caster.GetComponent<PlayerPersistentEffects>();
        return persistents != null ? persistents.ModifyOutgoing(damage) : damage;
    }

    public void ReplaceAll(IReadOnlyList<PersistentEffectBinding> bindings)
    {
        _bindings.Clear();
        _tuning.Clear();
        _active.Clear();
        if (bindings != null)
        {
            foreach (var binding in bindings)
            {
                if (binding.Kind == PersistentEffectKind.None) continue;
                _bindings.Add(binding);
                _active.Add(binding.Kind);
                if (binding.Tuning != null)
                    _tuning[binding.Kind] = binding.Tuning;
            }
        }

        ApplyBodyScale();
    }

    public void ReplaceAll(IReadOnlyList<PersistentEffectKind> kinds)
    {
        var bindings = new List<PersistentEffectBinding>();
        if (kinds != null)
        {
            foreach (var kind in kinds)
            {
                if (kind != PersistentEffectKind.None)
                    bindings.Add(new PersistentEffectBinding(kind));
            }
        }

        ReplaceAll(bindings);
    }

    public bool Has(PersistentEffectKind kind) => _active.Contains(kind);

    public float GetAttackSpeedMultiplier()
    {
        var mul = 1f;
        if (_furyActive)
            mul *= 1f + GetTuningFloat(PersistentEffectKind.FuryMeter, 1, 0.25f);
        if (_kolosActive && Has(PersistentEffectKind.WkurwionyKolos))
            mul *= 1f + GetTuningFloat(PersistentEffectKind.WkurwionyKolos, 0, 0.20f);
        return mul;
    }

    public float GetOutgoingDamageMultiplier()
    {
        var mul = 1f;
        if (_furyActive)
            mul *= 1f + GetTuningFloat(PersistentEffectKind.FuryMeter, 2, 0.20f);
        if (_secondWindOutgoingTimer > 0f)
            mul *= 1f + GetTuningFloat(PersistentEffectKind.SecondWind, 0, 0.25f);
        return mul;
    }

    public ComboHitModifiers GetComboHitModifiers(int hitIndex)
    {
        var mods = ComboHitModifiers.Identity;
        if (hitIndex != 2) return mods;

        if (_furyActive)
        {
            mods.DamageMultiplier *= 1f + GetTuningFloat(PersistentEffectKind.FuryMeter, 3, 0.15f);
            mods.RangeBonus += GetTuningFloat(PersistentEffectKind.FuryMeter, 4, 0.4f);
            mods.StaggerMultiplier *= GetTuningFloat(PersistentEffectKind.FuryMeter, 5, 1.5f);
        }

        if (_kolosActive && Has(PersistentEffectKind.WkurwionyKolos))
            mods.DamageMultiplier *= 1f + GetTuningFloat(PersistentEffectKind.WkurwionyKolos, 1, 0.30f);

        if (_kolosActive)
        {
            mods.RangeMultiplier *= GetTuningFloat(PersistentEffectKind.KolosForm, 2, 1.35f);
            mods.StaggerMultiplier *= GetStaggerBoostMultiplier();
        }

        return mods;
    }

    public float GetSkillRadiusMultiplier() =>
        _kolosActive ? GetTuningFloat(PersistentEffectKind.KolosForm, 2, 1.35f) : 1f;

    public float ModifyOutgoing(float damage)
    {
        if (damage <= 0f) return damage;
        return damage * GetOutgoingDamageMultiplier();
    }

    public IncomingHitResult ProcessIncoming(float amount, GameObject source)
    {
        var result = new IncomingHitResult { FinalDamage = amount, CountedForMeter = true };
        if (amount <= 0f) return result;

        if (!_furyActive && Has(PersistentEffectKind.FuryMeter))
            _furyMeter += amount;

        if (Has(PersistentEffectKind.HartStacks) && !_hartReady)
            _hartStacks = Mathf.Min(HartMaxStacks, _hartStacks + 1);

        if (_hartReady && Has(PersistentEffectKind.HartStacks))
        {
            result.Negated = true;
            result.HartConsumed = true;
            result.FinalDamage = 0f;
            _hartStacks = 0;
            _hartReady = false;
            ApplyHartStun(source);
            return result;
        }

        if (_hartStacks >= HartMaxStacks && Has(PersistentEffectKind.HartStacks))
            _hartReady = true;

        if (Has(PersistentEffectKind.ConvertTauntHitsToHeal)
            && _zryjConvertRemaining > 0
            && IsSourceTauntedByThisPlayer(source))
        {
            _zryjConvertRemaining--;
            result.ConvertedToHeal = true;
            result.HealAmount = amount;
            result.FinalDamage = 0f;
            return result;
        }

        if (_furyActive)
            result.FinalDamage *= 1f + GetTuningFloat(PersistentEffectKind.FuryMeter, 6, 0.10f);
        if (_kolosActive && Has(PersistentEffectKind.WkurwionyKolos))
            result.FinalDamage *= 1f + GetTuningFloat(PersistentEffectKind.WkurwionyKolos, 2, 0.15f);
        if (_secondWindIncomingTimer > 0f)
            result.FinalDamage *= 1f + GetTuningFloat(PersistentEffectKind.SecondWind, 1, 0.15f);

        return result;
    }

    public void NotifyTauntPulseApplied()
    {
        _zryjConvertRemaining = GetTuningInt(PersistentEffectKind.ConvertTauntHitsToHeal, 0, 3);
    }

    public void NotifyHealDuringLastChance(float amount)
    {
        if (!_lastChanceActive || amount <= 0f) return;
        _lastChanceHealAccum += amount;
    }

    public bool TryPreventDeath(Health health, GameObject source)
    {
        if (!Has(PersistentEffectKind.TimedLastChance) || _lastChanceUsedThisWave || health == null)
            return false;

        _lastChanceUsedThisWave = true;
        _lastChanceActive = true;
        _lastChanceTimer = GetTuningFloat(PersistentEffectKind.TimedLastChance, 0, 6f);
        _lastChanceHealAccum = 0f;
        _agonyTimer = 0f;

        if (Has(PersistentEffectKind.AutoFuryOnLastChance))
            ActivateFury(force: true);

        return true;
    }

    public bool KeepsAliveDuringLastChance() => _lastChanceActive;

    public void ActivateKolosForm(float duration)
    {
        _kolosActive = true;
        _kolosTimer = duration;
        _kolosExtraDuration = 0f;
        _kolosShoveTimer = 0f;
        _kolosTauntTimer = 0f;
        _moveShockwaveDistance = 0f;
        _moveShockwaveTimer = 0f;
        ApplyBodyScale();

        if (Has(PersistentEffectKind.PeriodicTauntInForm))
            ApplyPeriodicTauntPulse();
    }

    public void OnEnemyKilledDuringKolos()
    {
        if (!_kolosActive || !Has(PersistentEffectKind.OnKillFormExtend)) return;
        var heal = GetTuningFloat(PersistentEffectKind.OnKillFormExtend, 0, 8f);
        var extend = GetTuningFloat(PersistentEffectKind.OnKillFormExtend, 1, 0.4f);
        var maxExtra = GetTuningFloat(PersistentEffectKind.OnKillFormExtend, 2, 4f);
        _health?.Heal(heal);
        var added = PersistentEffectRuntime.ApplyPozeraczExtend(_kolosExtraDuration, extend, maxExtra);
        if (added > 0f)
        {
            _kolosExtraDuration += added;
            _kolosTimer += added;
        }
    }

    private Health ResolveHealth()
    {
        if (_health == null)
            _health = GetComponent<Health>();
        return _health;
    }

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        CaptureScale();
        if (ResolveHealth() != null)
            _health.Damaged += OnDamaged;
        _waveManager = FindAnyObjectByType<WaveManager>();
        if (_waveManager != null)
            _waveManager.WaveStarted += OnWaveStarted;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.Damaged -= OnDamaged;
        if (_waveManager != null)
            _waveManager.WaveStarted -= OnWaveStarted;
    }

    private void OnWaveStarted(WaveDefinition _) => _lastChanceUsedThisWave = false;

    private void Update()
    {
        TrackMovement();
        TickFury();
        TickKolos();
        TickLastChance();
        TickSecondWind();
        TickAuras();
        TickAgony();
        TickKolosFormMovement();
        TickPeriodicTaunt();
        TickFireTrail();
    }

    private void TrackMovement()
    {
        _movedDistanceThisFrame = 0f;
        if (!_hasLastPosition)
        {
            _lastPosition = transform.position;
            _hasLastPosition = true;
            return;
        }

        var delta = transform.position - _lastPosition;
        delta.y = 0f;
        _movedDistanceThisFrame = delta.magnitude;
        if (_movedDistanceThisFrame > 0.001f)
        {
            _lastMoveDirection = delta.normalized;
            _moveShockwaveDistance += _movedDistanceThisFrame;
            _fireTrailDistance += _movedDistanceThisFrame;
        }

        _lastPosition = transform.position;
    }

    private bool IsMovingThisFrame() => _movedDistanceThisFrame > 0.001f;

    public void TickFuryForTests(float deltaSeconds) => TickFuryAt(deltaSeconds);

    private void TickFury() => TickFuryAt(Time.deltaTime);

    private void TickFuryAt(float deltaSeconds)
    {
        if (_furyActive)
        {
            _furyTimer -= deltaSeconds;
            if (_furyTimer <= 0f)
            {
                _furyActive = false;
                _furyMeter = 0f;
            }

            return;
        }

        if (!Has(PersistentEffectKind.FuryMeter)) return;
        if (_furyMeter >= FuryThreshold)
            ActivateFury(force: false);
    }

    private void ActivateFury(bool force)
    {
        if (!force && _furyMeter < FuryThreshold) return;
        _furyActive = true;
        _furyTimer = GetTuningFloat(PersistentEffectKind.FuryMeter, 7, 6f);
    }

    private void TickKolos()
    {
        if (!_kolosActive) return;
        _kolosTimer -= Time.deltaTime;
        if (_kolosTimer <= 0f)
        {
            _kolosActive = false;
            _kolosExtraDuration = 0f;
            ApplyBodyScale();
        }
    }

    public void AdvanceLastChanceTimer(float deltaSeconds)
    {
        if (!_lastChanceActive) return;
        _lastChanceTimer -= deltaSeconds;
        if (_lastChanceTimer <= 0f)
            EndLastChanceWindow();
    }

    private void TickLastChance()
    {
        if (!_lastChanceActive) return;
        _lastChanceTimer -= Time.deltaTime;
        if (_lastChanceTimer > 0f) return;
        EndLastChanceWindow();
    }

    private void EndLastChanceWindow()
    {
        _lastChanceActive = false;
        var health = ResolveHealth();
        var threshold = LastChanceSurviveThreshold;
        if (PersistentEffectRuntime.LastChanceSurvives(_lastChanceHealAccum, threshold))
        {
            health?.ReviveWithHealth(Mathf.Max(1f, health.CurrentHealth));
            if (Has(PersistentEffectKind.SecondWind))
            {
                var duration = GetTuningFloat(PersistentEffectKind.SecondWind, 2, 4f);
                _secondWindOutgoingTimer = duration;
                _secondWindIncomingTimer = duration;
                GetComponent<PlayerSkillController>()?.ResetActiveCooldowns();
            }
        }
        else
        {
            health?.ForceDeath();
        }
    }

    private void TickSecondWind()
    {
        if (_secondWindOutgoingTimer > 0f) _secondWindOutgoingTimer -= Time.deltaTime;
        if (_secondWindIncomingTimer > 0f) _secondWindIncomingTimer -= Time.deltaTime;
    }

    private void TickAuras()
    {
        if (!Has(PersistentEffectKind.HellAuraRamping)) return;
        _auraTimer += Time.deltaTime;
        var tickInterval = GetTuningFloat(PersistentEffectKind.HellAuraRamping, 1, 0.5f);
        if (_auraTimer < tickInterval) return;
        _auraTimer = 0f;

        var baseRadius = GetAuraRadius();
        var baseDmg = GetTuningFloat(PersistentEffectKind.HellAuraRamping, 2, 3f);
        var maxExtra = GetTuningInt(PersistentEffectKind.HellAuraRamping, 1, 5);
        var grace = GetTuningFloat(PersistentEffectKind.HellAuraRamping, 3, 1.5f);
        var isOverheat = IsOverheatActive();
        var rampStep = isOverheat ? 2 : 1;
        var chainDelay = GetTuningFloat(PersistentEffectKind.ChainIgnite, 0, 3f);
        var igniteRadius = GetTuningFloat(PersistentEffectKind.ChainIgnite, 1, 2.5f);
        var igniteDuration = GetTuningFloat(PersistentEffectKind.ChainIgnite, 2, 4f);
        var ignitePerTick = GetTuningFloat(PersistentEffectKind.ChainIgnite, 3, 2f);
        var igniteDps = ignitePerTick > 0f ? ignitePerTick * 2f : 4f;

        var hits = Physics.OverlapSphere(transform.position + Vector3.up * 0.5f, baseRadius);
        var seen = new HashSet<Health>();
        var dealt = 0f;

        foreach (var collider in hits)
        {
            if (collider == null) continue;
            if (collider.GetComponentInParent<PlayerCharacter>() != null) continue;
            var health = collider.GetComponentInParent<Health>();
            if (health == null || !health.IsAlive) continue;
            seen.Add(health);
            if (!_auraRamps.TryGetValue(health, out var ramp)) ramp = new AuraRampState();
            ramp.GraceTimer = 0f;
            ramp.TimeInAura += tickInterval;
            var extra = Mathf.Min(maxExtra, ramp.ExtraTicks);
            var dmg = ModifyOutgoing(baseDmg + extra);
            health.TakeDamage(dmg, gameObject);
            dealt += dmg;
            ramp.ExtraTicks = Mathf.Min(maxExtra, ramp.ExtraTicks + rampStep);

            if (Has(PersistentEffectKind.ChainIgnite)
                && ramp.TimeInAura >= chainDelay
                && !ramp.ChainIgnited
                && PersistentEffectRuntime.CanChainIgniteTarget(_chainIgnited, health))
            {
                ramp.ChainIgnited = true;
                _chainIgnited.Add(health);
                SlowZone.Spawn(
                    health.transform.position,
                    igniteRadius,
                    gameObject,
                    igniteDps,
                    igniteDuration,
                    applySlow: false);
            }

            _auraRamps[health] = ramp;
        }

        foreach (var key in new List<Health>(_auraRamps.Keys))
        {
            if (seen.Contains(key)) continue;
            var ramp = _auraRamps[key];
            ramp.GraceTimer += tickInterval;
            if (ramp.GraceTimer >= grace)
                _auraRamps.Remove(key);
            else
                _auraRamps[key] = ramp;
        }

        if (Has(PersistentEffectKind.VampireAura) && dealt > 0f)
        {
            var heal = dealt * GetTuningFloat(PersistentEffectKind.VampireAura, 0, 0.20f);
            var cap = GetTuningFloat(PersistentEffectKind.VampireAura, 1, 12f) * tickInterval;
            _health?.Heal(Mathf.Min(heal, cap));
        }
    }

    private bool IsOverheatActive()
    {
        var health = ResolveHealth();
        return Has(PersistentEffectKind.OverheatAuraRamping)
            && health != null
            && health.MaxHealth > 0f
            && health.CurrentHealth / health.MaxHealth
            <= GetTuningFloat(PersistentEffectKind.OverheatAuraRamping, 0, 0.35f);
    }

    private float GetAuraRadius()
    {
        var radius = GetTuningFloat(PersistentEffectKind.HellAuraRamping, 0, 3.75f);
        if (IsOverheatActive())
            radius *= GetTuningFloat(PersistentEffectKind.OverheatAuraRamping, 1, 1.4f);
        return radius;
    }

    private void TickAgony()
    {
        if (!_lastChanceActive || !Has(PersistentEffectKind.AgonyPulses)) return;
        _agonyTimer += Time.deltaTime;
        var interval = GetTuningFloat(PersistentEffectKind.AgonyPulses, 2, 1f);
        if (_agonyTimer < interval) return;
        _agonyTimer = 0f;

        var radius = GetTuningFloat(PersistentEffectKind.AgonyPulses, 0, 3.5f);
        var damage = GetTuningFloat(PersistentEffectKind.AgonyPulses, 1, 10f);
        PersistentEffectRuntime.PulseAgonyTargets(
            transform.position, radius, damage, gameObject, ~0);
    }

    private void TickKolosFormMovement()
    {
        if (!_kolosActive || !IsMovingThisFrame()) return;

        var moveDir = _lastMoveDirection.sqrMagnitude > 0.01f ? _lastMoveDirection : transform.forward;

        _kolosShoveTimer += Time.deltaTime;
        var shovePeriod = GetTuningFloat(PersistentEffectKind.KolosForm, 3, 0.35f);
        if (_kolosShoveTimer >= shovePeriod)
        {
            _kolosShoveTimer = 0f;
            PersistentEffectRuntime.ApplyKolosShove(
                transform.position,
                2.5f,
                moveDir,
                GetTuningFloat(PersistentEffectKind.KolosForm, 4, 0.6f),
                GetTuningFloat(PersistentEffectKind.KolosForm, 5, 0.15f),
                0.20f,
                gameObject);
        }

        if (!Has(PersistentEffectKind.MoveShockwave)) return;

        _moveShockwaveTimer += Time.deltaTime;
        var distThreshold = GetTuningFloat(PersistentEffectKind.MoveShockwave, 3, 1.2f);
        var timeThreshold = GetTuningFloat(PersistentEffectKind.MoveShockwave, 4, 0.45f);
        if (!PersistentEffectRuntime.ShouldSpawnMoveShockwave(
                _moveShockwaveDistance, _moveShockwaveTimer, distThreshold, timeThreshold))
            return;

        _moveShockwaveDistance = 0f;
        _moveShockwaveTimer = 0f;
        PersistentEffectRuntime.PulseMoveShockwave(
            transform.position,
            GetTuningFloat(PersistentEffectKind.MoveShockwave, 1, 1.6f),
            GetTuningFloat(PersistentEffectKind.MoveShockwave, 0, 6f),
            GetTuningFloat(PersistentEffectKind.MoveShockwave, 2, 0.3f),
            gameObject,
            ~0);
    }

    private void TickPeriodicTaunt()
    {
        if (!_kolosActive || !Has(PersistentEffectKind.PeriodicTauntInForm)) return;
        _kolosTauntTimer += Time.deltaTime;
        var refresh = GetTuningFloat(PersistentEffectKind.PeriodicTauntInForm, 1, 2f);
        if (_kolosTauntTimer < refresh) return;
        _kolosTauntTimer = 0f;
        ApplyPeriodicTauntPulse();
    }

    private void ApplyPeriodicTauntPulse()
    {
        if (_player == null) return;
        var radius = GetTuningFloat(PersistentEffectKind.PeriodicTauntInForm, 0, 6f);
        var duration = GetTuningFloat(PersistentEffectKind.PeriodicTauntInForm, 1, 2f);
        var hits = Physics.OverlapSphere(transform.position + Vector3.up * 0.5f, radius);
        foreach (var col in hits)
        {
            if (col == null) continue;
            if (col.GetComponentInParent<PlayerCharacter>() != null) continue;
            EnemyThreatService.TryTaunt(col.gameObject, _player, duration);
        }
    }

    private void TickFireTrail()
    {
        if (!Has(PersistentEffectKind.FireTrail) || !IsMovingThisFrame()) return;

        var spawnEvery = GetTuningFloat(PersistentEffectKind.FireTrail, 3, 0.8f);
        if (_fireTrailDistance < spawnEvery) return;
        _fireTrailDistance = 0f;

        var maxSegments = Mathf.RoundToInt(GetTuningFloat(PersistentEffectKind.FireTrail, 4, 6f));
        var duration = GetTuningFloat(PersistentEffectKind.FireTrail, 0, 2f);
        var perTick = GetTuningFloat(PersistentEffectKind.FireTrail, 1, 3f);
        var dps = perTick * 2f;

        while (_fireTrailSegments.Count >= maxSegments && _fireTrailSegments.Count > 0)
        {
            var oldest = _fireTrailSegments.Dequeue();
            if (oldest != null)
                Destroy(oldest);
        }

        var zone = SlowZone.Spawn(transform.position, 1.5f, gameObject, dps, duration, applySlow: false);
        _fireTrailSegments.Enqueue(zone.gameObject);
    }

    private void OnDamaged(float amount, GameObject source)
    {
        if (_reflecting) return;
        if (!Has(PersistentEffectKind.DamageReflect)) return;
        if (source == null || source == gameObject) return;
        var target = source.GetComponentInParent<Health>();
        if (target == null || !target.IsAlive) return;

        _reflecting = true;
        var fraction = GetTuningFloat(PersistentEffectKind.DamageReflect, 0, 0.5f);
        target.TakeDamage(amount * fraction, gameObject);
        _reflecting = false;
    }

    private void ApplyHartStun(GameObject source)
    {
        if (source == null) return;
        var profile = source.GetComponentInParent<ForcedMovementResistanceProfile>();
        var category = profile != null ? profile.Category : ForcedMovementResistanceCategory.Normal;
        var stun = category switch
        {
            ForcedMovementResistanceCategory.Elite => GetTuningFloat(PersistentEffectKind.HartStacks, 1, 1f),
            ForcedMovementResistanceCategory.Boss => GetTuningFloat(PersistentEffectKind.HartStacks, 2, 0.5f),
            _ => GetTuningFloat(PersistentEffectKind.HartStacks, 0, 2f)
        };
        var req = ForcedMovementRequest.StunControl(gameObject, stun, 0f);
        ForcedMovementResolver.Apply(req, source);
    }

    private bool IsSourceTauntedByThisPlayer(GameObject source)
    {
        if (source == null) return false;
        var threat = source.GetComponentInParent<EnemyThreatState>();
        return threat != null && threat.IsActive && threat.Taunter != null
            && threat.Taunter.gameObject == gameObject;
    }

    private float GetLastChanceSurviveFraction()
    {
        if (Has(PersistentEffectKind.HealAmpInLastChance))
            return GetTuningFloat(PersistentEffectKind.HealAmpInLastChance, 1, 0.20f);
        return GetTuningFloat(PersistentEffectKind.TimedLastChance, 1, 0.30f);
    }

    private float GetStaggerBoostMultiplier() =>
        Has(PersistentEffectKind.StaggerBoost)
            ? GetTuningFloat(PersistentEffectKind.StaggerBoost, 0, 1.6f)
            : 1f;

    private float GetTuningFloat(PersistentEffectKind kind, int index, float fallback)
    {
        if (!_tuning.TryGetValue(kind, out var tuning) || tuning == null) return fallback;
        return index switch
        {
            0 => tuning.Float0,
            1 => tuning.Float1,
            2 => tuning.Float2,
            3 => tuning.Float3,
            4 => tuning.Float4,
            5 => tuning.Float5,
            6 => tuning.Float6,
            7 => tuning.Float7,
            _ => fallback
        };
    }

    private int GetTuningInt(PersistentEffectKind kind, int index, int fallback)
    {
        if (!_tuning.TryGetValue(kind, out var tuning) || tuning == null) return fallback;
        return index switch
        {
            0 => tuning.Int0,
            1 => tuning.Int1,
            2 => tuning.Int2,
            _ => fallback
        };
    }

    private void CaptureScale()
    {
        if (_scaleCaptured) return;
        _baseScale = transform.localScale;
        if (_baseScale.sqrMagnitude < 0.01f) _baseScale = Vector3.one;
        _scaleCaptured = true;
    }

    private void ApplyBodyScale()
    {
        CaptureScale();
        var scale = _kolosActive ? GetTuningFloat(PersistentEffectKind.KolosForm, 0, 1.7f) : 1f;
        transform.localScale = _baseScale * scale;
    }
}

public struct ComboHitModifiers
{
    public float DamageMultiplier;
    public float RangeMultiplier;
    public float RangeBonus;
    public float StaggerMultiplier;

    public static ComboHitModifiers Identity => new()
    {
        DamageMultiplier = 1f,
        RangeMultiplier = 1f,
        RangeBonus = 0f,
        StaggerMultiplier = 1f
    };
}
