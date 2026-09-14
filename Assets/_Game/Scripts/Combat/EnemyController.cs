using System.Collections;
using UnityEngine;

/// <summary>
/// AI wrogów M3/M6.5/M7 — Grunt, Hunter, Rusher, Carrier, Siege + elity.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyLaneMotor))]
[RequireComponent(typeof(StatusEffectReceiver))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyDefinition definition;

    private static PlayerCharacter[] _siegePlayers;
    private static float _siegePlayersRefreshAt;
    private Health _health;
    private StatusEffectReceiver _status;
    private HitFlashFeedback _flash;
    private Renderer _renderer;
    private EnemyLaneMotor _motor;
    private KnockbackReceiver _knockback;
    private PlayerCharacter _target;
    private PlayerCharacter _huntTarget;
    private IDamageable _structureTarget;
    private float _attackCooldown;
    private float _structureWindupRemaining;
    private IDamageable _structureWindupTarget;
    private float _retargetTimer;
    private bool _isDying;
    private bool _wasKnockbackActive;
    private EliteModifier _eliteModifier = EliteModifier.None;
    private float _runtimeMoveSpeed;
    private float _runtimeAttackInterval;
    private float _runtimeMaxHealth;
    private EnemyThreatState _threatState;
    private bool _wasThreatActive;
    private float _allyMoveMul = 1f;
    private float _allyDamageMul = 1f;
    private float _allyBuffRemaining;
    // Support auras do not need frame-perfect refreshes.  Throttling this scan
    // keeps a large Siege horde from doing one full registry pass per Support
    // every rendered frame while retaining a responsive buff update cadence.
    private float _supportAuraRefreshRemaining;

    public event System.Action Attacked;

    public EnemyDefinition Definition => definition;
    public bool IsAlive => _health != null && _health.IsAlive && !_isDying;
    public EliteModifier EliteModifier => _eliteModifier;
    public AttackLineId AssignedLane => _motor != null ? _motor.AssignedLane : AttackLineId.Center;

    public bool IsAttackingStructureIgnoredBecauseTaunt =>
        definition != null &&
        EnemyThreatMath.IgnoresStructureWhileTaunted(definition.Kind, IsThreatActive);

    public bool IsThreatActive
    {
        get
        {
            if (_threatState == null)
                _threatState = GetComponent<EnemyThreatState>();
            return _threatState != null && _threatState.IsActive;
        }
    }

    public float AllyMoveMultiplier => _allyMoveMul;
    public float AllyDamageMultiplier => _allyDamageMul;
    public float StructureWindupRemaining => _structureWindupRemaining;

    public void ApplyAllyBuff(float moveMul, float damageMul, float durationSeconds)
    {
        _allyMoveMul = EnemySupportBuffMath.CombineMultiplier(_allyMoveMul, moveMul);
        _allyDamageMul = EnemySupportBuffMath.CombineMultiplier(_allyDamageMul, damageMul);
        _allyBuffRemaining = Mathf.Max(_allyBuffRemaining, durationSeconds);
    }

    public void Configure(EnemyDefinition enemyDefinition, AttackLineId lane, EliteModifier eliteModifier = EliteModifier.None)
    {
        EnsureRuntimeReferences();
        definition = enemyDefinition;
        _eliteModifier = eliteModifier;
        _motor.Initialize(lane, definition);
        ApplyDefinition();
    }

    private void Awake()
    {
        EnsureRuntimeReferences();
        _health.Died += OnDied;
    }

    private void EnsureRuntimeReferences()
    {
        _health ??= GetComponent<Health>();
        _status ??= GetComponent<StatusEffectReceiver>();
        _flash ??= GetComponent<HitFlashFeedback>();
        _renderer ??= GetComponentInChildren<Renderer>();
        _motor ??= GetComponent<EnemyLaneMotor>();
        _knockback ??= GetComponent<KnockbackReceiver>();
        _threatState ??= GetComponent<EnemyThreatState>();
    }

    private void OnEnable()
    {
        EnemyRegistry.Register(this);
    }

    private void OnDisable()
    {
        EnemyRegistry.Unregister(this);
    }

    private void Start()
    {
        ApplyDefinition();
        if (definition != null && definition.Kind == EnemyKind.Hunter)
            _huntTarget = PickRandomLivingPlayer();
    }

    private void ApplyDefinition()
    {
        if (definition == null) return;

        _runtimeMaxHealth = EliteModifierMath.ApplyMaxHealthMultiplier(_eliteModifier, definition.MaxHealth);
        _runtimeMoveSpeed = EliteModifierMath.ApplyMoveSpeedMultiplier(_eliteModifier, definition.MoveSpeed);
        _runtimeAttackInterval = EliteModifierMath.ApplyAttackIntervalMultiplier(_eliteModifier, definition.AttackInterval);

        _health.Configure(_runtimeMaxHealth);
        _flash?.SetBaseColor(definition.BodyColor);

        var scale = definition.BodyScale;
        if (_eliteModifier != EliteModifier.None)
            scale *= 1.15f;
        transform.localScale = scale;
    }

    private void Update()
    {
        if (_isDying || !_health.IsAlive || definition == null) return;
        if (SiegeArena.Instance != null && Time.timeScale <= 0f) return;
        if (!RunFailRules.ShouldEnemyCombatTick(ResolveFlowState())) return;
        if (_status != null && _status.BlocksMovement && _status.BlocksAttack)
        {
            _structureWindupRemaining = 0f;
            _structureWindupTarget = null;
            return;
        }

        var knockbackActive = _knockback != null && _knockback.IsActive;

        if (!knockbackActive && _wasKnockbackActive && StaysOnLane())
            _motor.ClampToLane();

        _wasKnockbackActive = knockbackActive;
        if (knockbackActive) return;

        if (_threatState == null)
            _threatState = GetComponent<EnemyThreatState>();
        _threatState?.Tick(Time.deltaTime);

        var threatActive = IsThreatActive;
        if (_wasThreatActive && !threatActive && definition.Kind == EnemyKind.Hunter)
            _huntTarget = null;
        _wasThreatActive = threatActive;

        TickAllyBuff(Time.deltaTime);
        TickSupportAura();

        _attackCooldown -= Time.deltaTime;
        _retargetTimer -= Time.deltaTime;

        if (_retargetTimer <= 0f)
        {
            RefreshTargets();
            _retargetTimer = definition.Kind == EnemyKind.Hunter ? 0.5f : 0.25f;
        }

        var moveSpeed = _runtimeMoveSpeed
            * (_status != null ? _status.MoveSpeedMultiplier : 1f)
            * _allyMoveMul
            * Time.deltaTime;

        if (SiegeArena.Instance != null)
        {
            UpdateSiegeMovement(moveSpeed, threatActive);
            return;
        }

        if (StructureTargeting.PrefersStructureOverPlayer(definition.Kind))
        {
            if (EnemyThreatMath.IgnoresStructureWhileTaunted(definition.Kind, threatActive))
                UpdateTauntedLanePursuit(moveSpeed);
            else
                UpdateStructureAttacker(moveSpeed);
        }
        else if (definition.Kind == EnemyKind.Hunter)
            UpdateHunterMovement(moveSpeed);
        else
            UpdateLaneGruntMovement(moveSpeed);
    }

    private void UpdateSiegeMovement(float moveSpeed, bool threatActive)
    {
        // Taunt overrides all archetypes immediately, including structure attackers.
        if (threatActive)
        {
            _structureWindupRemaining = 0f;
            _structureWindupTarget = null;
            _target = _threatState.Taunter;
            UpdateHunterMovement(moveSpeed);
            return;
        }
        if (definition.Kind == EnemyKind.Hunter ||
            (definition.Kind == EnemyKind.Grunt && _target != null && IsPlayerInGruntRange(_target)))
        {
            UpdateHunterMovement(moveSpeed);
            return;
        }
        UpdateStructureAttacker(moveSpeed);
    }

    private bool StaysOnLane()
    {
        if (SiegeArena.Instance != null) return false;
        return definition.Kind is EnemyKind.Grunt
            or EnemyKind.Carrier
            or EnemyKind.Support
            or EnemyKind.Shielder;
    }

    private void TickAllyBuff(float deltaTime)
    {
        if (_allyBuffRemaining <= 0f) return;

        _allyBuffRemaining -= deltaTime;
        if (_allyBuffRemaining > 0f) return;

        _allyMoveMul = 1f;
        _allyDamageMul = 1f;
        _allyBuffRemaining = 0f;
    }

    private void TickSupportAura()
    {
        if (definition.Kind != EnemyKind.Support) return;

        _supportAuraRefreshRemaining -= Time.deltaTime;
        if (_supportAuraRefreshRemaining > 0f) return;
        _supportAuraRefreshRemaining = 0.15f;

        var radius = definition.SupportBuffRadius;
        if (radius <= 0f) return;

        var moveMul = definition.SupportMoveSpeedMultiplier;
        var dmgMul = definition.SupportDamageMultiplier;
        if (moveMul <= 1f && dmgMul <= 1f) return;

        var supportPos = transform.position;
        foreach (var ally in EnemyRegistry.Active)
        {
            if (ally == null || ally == this) continue;
            if (ally.Definition == null) continue;
            if (!ally.IsAlive) continue;

            if (!EnemySupportBuffMath.ShouldBuffAlly(
                    EnemyKind.Support,
                    supportPos,
                    ally.transform.position,
                    radius))
                continue;

            ally.ApplyAllyBuff(moveMul, dmgMul, 0.35f);
        }
    }

    private void RefreshTargets()
    {
        if (IsThreatActive)
        {
            var taunter = _threatState != null ? _threatState.Taunter : null;
            if (taunter != null && taunter.IsCombatEnabled)
            {
                if (definition.Kind == EnemyKind.Hunter)
                {
                    _huntTarget = taunter;
                    _target = taunter;
                    return;
                }

                _target = taunter;
                return;
            }
        }

        if (SiegeArena.Instance != null || StructureTargeting.PrefersStructureOverPlayer(definition.Kind))
            _structureTarget = StructureTargeting.ResolveStructureTarget(AssignedLane, definition.AttackRange, transform.position);

        if (definition.Kind == EnemyKind.Hunter)
        {
            if (_huntTarget == null || !_huntTarget.IsCombatEnabled)
                _huntTarget = PickRandomLivingPlayer();
            _target = _huntTarget;
            return;
        }

        _target = FindNearestPlayerOnLane();
    }

    private void UpdateStructureAttacker(float moveSpeed)
    {
        if (_structureTarget == null || !_structureTarget.IsAlive)
            _structureTarget = StructureTargeting.ResolveStructureTarget(AssignedLane, definition.AttackRange, transform.position);

        if (_structureTarget != null &&
            StructureTargeting.IsStructureInRange(_structureTarget, transform.position, definition.AttackRange))
        {
            if (_structureTarget is Component structureComponent)
                _motor.FaceToward(structureComponent.transform.position);
            TryAttackStructure(_structureTarget);
            return;
        }

        // Straight-line steering to the tower stalls at choke corners.
        // Walk the polyline until in melee range of the structure.
        _structureWindupRemaining = 0f;
        _structureWindupTarget = null;
        _motor.AdvanceAlongLane(moveSpeed);
    }

    private void UpdateTauntedLanePursuit(float moveSpeed)
    {
        var taunter = _target;
        if (taunter == null || !taunter.IsCombatEnabled)
        {
            _motor.AdvanceAlongLane(moveSpeed);
            return;
        }

        var targetPos = taunter.transform.position;
        var toTarget = targetPos - transform.position;
        toTarget.y = 0f;
        var distance = toTarget.magnitude;

        if (distance > definition.AttackRange)
        {
            _motor.MoveTowardAlongCorridor(targetPos, moveSpeed);
            return;
        }

        TryAttackPlayer(taunter, definition.Damage);
    }

    private void UpdateLaneGruntMovement(float moveSpeed)
    {
        var hasLaneTarget = _target != null
            && _target.IsCombatEnabled
            && IsPlayerInGruntRange(_target);

        if (hasLaneTarget)
        {
            var targetPos = _target.transform.position;
            var toTarget = targetPos - transform.position;
            toTarget.y = 0f;
            var distance = toTarget.magnitude;

            if (distance > definition.AttackRange)
            {
                _motor.MoveTowardAlongCorridor(targetPos, moveSpeed);
                return;
            }

            TryAttackPlayer(_target, definition.Damage);
            return;
        }

        _motor.AdvanceAlongLane(moveSpeed);
    }

    private void UpdateHunterMovement(float moveSpeed)
    {
        if (_target == null || !_target.IsCombatEnabled)
        {
            _motor.AdvanceAlongLane(moveSpeed);
            return;
        }

        var targetPos = _target.transform.position;
        var toTarget = targetPos - transform.position;
        toTarget.y = 0f;
        var distance = toTarget.magnitude;

        if (distance > definition.AttackRange)
        {
            _motor.MoveDirect(targetPos, moveSpeed);
            return;
        }

        TryAttackPlayer(_target, definition.Damage);
    }

    private bool IsPlayerInGruntRange(PlayerCharacter player)
    {
        if (player == null || !player.IsCombatEnabled) return false;

        var toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > definition.DetectRange * definition.DetectRange)
            return false;

        if (SiegeArena.Instance != null) return true;
        var extraLeash = _motor.LaneCorridorHalfWidthExtra();
        return _motor.IsInLaneCorridor(player.transform.position, extraLeash);
    }

    private void TryAttackPlayer(PlayerCharacter player, float damage)
    {
        if (_attackCooldown > 0f || (_status != null && _status.BlocksAttack)) return;

        var playerHealth = player.GetComponent<Health>();
        if (playerHealth != null && playerHealth.IsAlive)
        {
            var scaled = damage * definition.PlayerDamageMultiplier * _allyDamageMul;
            playerHealth.TakeDamage(scaled, gameObject);
            player.GetComponent<HitFlashFeedback>()?.PlayFlash();
        }

        _attackCooldown = _runtimeAttackInterval;
        Attacked?.Invoke();
    }

    private void TryAttackStructure(IDamageable structure)
    {
        if (_attackCooldown > 0f || (_status != null && _status.BlocksAttack)) return;
        if (structure == null || !structure.IsAlive) return;

        // Burzyciel telegraphs its heavy hit for one second. Moving out of range,
        // changing target, taunt, or stun cancels the pending strike.
        if (definition.Kind == EnemyKind.Siege)
        {
            if (_structureWindupTarget != structure)
            {
                _structureWindupTarget = structure;
                _structureWindupRemaining = 1f;
                return;
            }

            _structureWindupRemaining -= Time.deltaTime;
            if (_structureWindupRemaining > 0f || !StructureTargeting.IsStructureInRange(
                    structure, transform.position, definition.AttackRange))
                return;
            _structureWindupRemaining = 0f;
            _structureWindupTarget = null;
        }

        structure.TakeDamage(definition.StructureDamage * _allyDamageMul, gameObject);
        _attackCooldown = _runtimeAttackInterval;
        Attacked?.Invoke();

        if (structure is Component hit)
            StructureHitFeedback.Play(transform.position, hit.transform.position);
    }

    private PlayerCharacter PickRandomLivingPlayer()
    {
        var players = GetPlayers();
        var living = new System.Collections.Generic.List<PlayerCharacter>();
        foreach (var player in players)
        {
            if (player != null && player.IsCombatEnabled)
                living.Add(player);
        }

        if (living.Count == 0) return null;
        return living[Random.Range(0, living.Count)];
    }

    private PlayerCharacter FindNearestPlayerOnLane()
    {
        var players = GetPlayers();
        PlayerCharacter nearest = null;
        var bestDist = definition.DetectRange * definition.DetectRange;
        var extraLeash = _motor.LaneCorridorHalfWidthExtra();

        foreach (var player in players)
        {
            if (player == null || !player.IsCombatEnabled) continue;
            if (SiegeArena.Instance == null && !_motor.IsInLaneCorridor(player.transform.position, extraLeash)) continue;

            var dist = Vector3.SqrMagnitude(player.transform.position - transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = player;
            }
        }

        return nearest;
    }

    private void OnDied()
    {
        if (_isDying) return;
        TriggerUnstableDeathAoe();
        GrantKillRewards();
        _isDying = true;
        StartCoroutine(DeathRoutine());
    }

    private void TriggerUnstableDeathAoe()
    {
        var damage = EliteModifierMath.GetDeathAoeDamage(_eliteModifier);
        var radius = EliteModifierMath.GetDeathAoeRadius(_eliteModifier);
        if (damage <= 0f || radius <= 0f) return;

        var hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            var player = hit.GetComponentInParent<PlayerCharacter>();
            if (player == null) continue;
            player.GetComponent<Health>()?.TakeDamage(damage, gameObject);
        }

        var burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        burst.transform.position = transform.position + Vector3.up * 0.4f;
        burst.transform.localScale = Vector3.one * radius * 2f;
        var col = burst.GetComponent<Collider>();
        if (col != null) Destroy(col);
        var rend = burst.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = new Color(1f, 0.3f, 0.1f, 0.35f);
            rend.material = mat;
        }

        Destroy(burst, 0.4f);
    }

    private void GrantKillRewards()
    {
        if (definition == null || SiegeArena.Instance != null) return;

        var runState = SharedRunState.Instance;
        if (runState == null) return;

        var exp = definition.ExpReward;
        var gold = definition.GoldReward;
        if (runState.Config != null)
        {
            if (exp <= 0) exp = runState.Config.DefaultExpReward;
            if (gold <= 0) gold = runState.Config.DefaultGoldReward;
        }

        runState.AddKillRewards(exp, gold);
        var eliteKill = EliteModifierMath.CountsAsEliteForLoot(_eliteModifier, definition.Kind);
        LootDropService.TryDropFromEnemy(transform.position, eliteKill);
    }

    private IEnumerator DeathRoutine()
    {
        var hpBar = GetComponent<EnemyHealthBar>();
        hpBar?.Hide();

        if (_renderer != null)
        {
            var mat = _renderer.material;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(1f, 0.85f, 0.1f, 1f));
            else
                mat.color = new Color(1f, 0.85f, 0.1f, 1f);
        }

        var duration = 0.35f;
        var elapsed = 0f;
        var startScale = transform.localScale;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.Died -= OnDied;
    }

    private static PlayerCharacter[] GetPlayers()
    {
        if (SiegeArena.Instance == null) return FindObjectsByType<PlayerCharacter>();
        if (_siegePlayers == null || Time.unscaledTime >= _siegePlayersRefreshAt || Time.unscaledTime < _siegePlayersRefreshAt - 0.5f)
        {
            _siegePlayers = FindObjectsByType<PlayerCharacter>();
            _siegePlayersRefreshAt = Time.unscaledTime + 0.5f;
        }
        return _siegePlayers;
    }

    private static GameFlowState ResolveFlowState()
    {
        if (SiegeArena.Instance != null)
            return GameFlowState.WaveActive;
        var flow = FindAnyObjectByType<GameFlowManager>();
        return flow != null ? flow.State : GameFlowState.WaveActive;
    }
}
