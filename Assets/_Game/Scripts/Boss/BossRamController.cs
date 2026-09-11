using System.Collections;
using UnityEngine;

/// <summary>
/// Boss 1 — The Ram: szarża, interrupt, focus, enrage (M7-T5).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(StatusEffectReceiver))]
public class BossRamController : MonoBehaviour
{
    [SerializeField] private BossDefinition definition;

    private Health _health;
    private StatusEffectReceiver _status;
    private Renderer _renderer;
    private GameObject _telegraphVisual;
    private BossRamState _state = BossRamState.Idle;
    private float _stateTimer;
    private float _chargeCooldownTimer;
    private float _focusCooldownTimer;
    private float _focusAttackTimer;
    private Vector3 _chargeTargetPosition;
    private IDamageable _chargeTarget;
    private PlayerCharacter _focusTarget;
    private bool _enraged;
    private bool _isDying;

    public BossDefinition Definition => definition;
    public BossRamState State => _state;
    public bool IsEnraged => _enraged;
    public float HealthRatio => _health != null && _health.MaxHealth > 0f
        ? _health.CurrentHealth / _health.MaxHealth
        : 0f;

    public static BossRamController Active { get; private set; }

    public void Configure(BossDefinition bossDefinition)
    {
        definition = bossDefinition;
        ApplyDefinition();
    }

    private void Awake()
    {
        _health = GetComponent<Health>();
        _status = GetComponent<StatusEffectReceiver>();
        _renderer = GetComponentInChildren<Renderer>();
        _health.Died += OnDied;
        _health.HealthChanged += OnHealthChanged;
        if (_status != null)
            _status.ConfigureStaggerThreshold(definition != null ? definition.StaggerInterruptThreshold : 35f);
    }

    private void OnEnable()
    {
        Active = this;
    }

    private void OnDisable()
    {
        if (Active == this)
            Active = null;
    }

    private void Start()
    {
        ApplyDefinition();
    }

    private void ApplyDefinition()
    {
        if (definition == null) return;
        _health.Configure(definition.MaxHealth);
        transform.localScale = definition.BodyScale;
        if (_renderer != null)
        {
            var mat = _renderer.material;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", definition.BodyColor);
            else
                mat.color = definition.BodyColor;
        }
    }

    private void Update()
    {
        if (_isDying || _health == null || !_health.IsAlive || definition == null) return;
        if (!RunFailRules.ShouldEnemyCombatTick(ResolveFlowState())) return;

        UpdateEnrageFlag();

        if (_state == BossRamState.Stunned)
        {
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f)
                EnterState(BossRamState.Idle, 0f);
            return;
        }

        TryInterruptCharge();

        _stateTimer -= Time.deltaTime;
        _chargeCooldownTimer -= Time.deltaTime;
        _focusCooldownTimer -= Time.deltaTime;

        switch (_state)
        {
            case BossRamState.Idle:
                UpdateIdle();
                break;
            case BossRamState.TelegraphCharge:
                UpdateTelegraph();
                break;
            case BossRamState.Charge:
                UpdateCharge();
                break;
            case BossRamState.Impact:
                UpdateImpact();
                break;
            case BossRamState.Recover:
                if (_stateTimer <= 0f) EnterState(BossRamState.Idle, 0f);
                break;
            case BossRamState.FocusPlayer:
                UpdateFocusPlayer();
                break;
        }
    }

    private void UpdateEnrageFlag()
    {
        _enraged = BossRamLogic.ShouldEnterEnrage(_health.CurrentHealth, _health.MaxHealth, definition.EnrageHealthRatio);
    }

    private void TryInterruptCharge()
    {
        if (_state != BossRamState.TelegraphCharge && _state != BossRamState.Charge) return;
        if (!BossRamLogic.ShouldInterruptCharge(definition, _status, true)) return;

        CancelTelegraphVisual();
        _status.Apply(StatusEffectType.Stun, definition.InterruptStunSeconds, 1f);
        EnterState(BossRamState.Stunned, definition.InterruptStunSeconds);
    }

    private void UpdateIdle()
    {
        if (_chargeCooldownTimer <= 0f)
        {
            BeginCharge();
            return;
        }

        if (_focusCooldownTimer <= 0f)
            BeginFocusPlayer();
    }

    private void BeginCharge()
    {
        _chargeTarget = PickChargeTarget(out _chargeTargetPosition);
        CreateTelegraphVisual(_chargeTargetPosition);
        var telegraph = BossRamLogic.GetTelegraphDuration(definition, _enraged);
        EnterState(BossRamState.TelegraphCharge, telegraph);
    }

    private void UpdateTelegraph()
    {
        if (_stateTimer > 0f) return;
        EnterState(BossRamState.Charge, 0f);
    }

    private void UpdateCharge()
    {
        var step = definition.ChargeSpeed * Time.deltaTime;
        var toTarget = _chargeTargetPosition - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude <= step + 0.5f)
        {
            transform.position = new Vector3(_chargeTargetPosition.x, transform.position.y, _chargeTargetPosition.z);
            EnterState(BossRamState.Impact, 0.15f);
            return;
        }

        var direction = toTarget.normalized;
        transform.position += direction * step;
        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void UpdateImpact()
    {
        CancelTelegraphVisual();
        if (_chargeTarget != null && _chargeTarget.IsAlive)
        {
            var structure = _chargeTarget is BaseHealth || _chargeTarget is TowerHealth;
            var damage = BossRamLogic.GetChargeImpactDamage(
                structure,
                definition.StructureImpactDamage,
                definition.FocusPlayerDamage);
            _chargeTarget.TakeDamage(damage, gameObject);
        }
        ApplyShockwave();
        _chargeCooldownTimer = BossRamLogic.GetChargeCooldown(definition, _enraged);
        EnterState(BossRamState.Recover, definition.RecoverSeconds);
    }

    private void UpdateFocusPlayer()
    {
        if (_focusTarget == null || !_focusTarget.IsCombatEnabled)
        {
            EnterState(BossRamState.Idle, 0f);
            return;
        }

        if (BossRamLogic.ShouldAbortFocusForCharge(_chargeCooldownTimer))
        {
            EnterState(BossRamState.Idle, 0f);
            return;
        }

        if (BossRamLogic.ShouldExitFocus(_stateTimer))
        {
            EnterState(BossRamState.Idle, 0f);
            return;
        }

        var targetPos = _focusTarget.transform.position;
        var toTarget = targetPos - transform.position;
        toTarget.y = 0f;
        var distance = toTarget.magnitude;
        var hitRange = BossRamLogic.GetFocusStopDistance(definition);
        var maxStep = definition.MoveSpeed * Time.deltaTime;

        var toward = distance < 0.01f
            ? FlattenHorizontal(transform.forward)
            : toTarget.normalized;
        if (toward.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(toward, Vector3.up);

        // Chase-through: GetFocusMoveStep celowo nieużywane — standoff reverse-glue
        // blokował ucieczkę gracza. Hit range zostaje GetFocusStopDistance.
        transform.position += toward * maxStep;

        _focusAttackTimer -= Time.deltaTime;
        if (distance <= hitRange + 0.05f && _focusAttackTimer <= 0f)
        {
            _focusTarget.GetComponent<Health>()?.TakeDamage(definition.FocusPlayerDamage, gameObject);
            _focusAttackTimer = definition.FocusAttackInterval;
        }
    }

    private void BeginFocusPlayer()
    {
        _focusTarget = PickRandomLivingPlayer();
        _focusCooldownTimer = BossRamLogic.GetChargeCooldown(definition, _enraged) * 0.6f;
        _focusAttackTimer = definition.FocusAttackInterval;
        EnterState(BossRamState.FocusPlayer, BossRamLogic.GetFocusDuration(definition, _enraged));
    }

    private IDamageable PickChargeTarget(out Vector3 targetPosition)
    {
        var tower = TowerRegistry.PickRandomLivingLineTower();
        var baseHealth = FindAnyObjectByType<BaseHealth>();
        var player = PickRandomLivingPlayer();
        var kind = BossRamLogic.ResolveChargeTargetKind(
            tower != null && tower.IsOperational,
            baseHealth != null && baseHealth.IsAlive,
            player != null);

        switch (kind)
        {
            case BossRamChargeTargetKind.LineTower:
                targetPosition = GetChargeAimPoint(tower.transform.position);
                return tower;
            case BossRamChargeTargetKind.Base:
                targetPosition = GetChargeAimPoint(baseHealth.transform.position);
                return baseHealth;
            case BossRamChargeTargetKind.Player:
                targetPosition = GetChargeAimPoint(player.transform.position);
                return player.GetComponent<Health>();
            default:
                targetPosition = transform.position + transform.forward * 5f;
                return null;
        }
    }

    private Vector3 GetChargeAimPoint(Vector3 targetPosition)
    {
        return BossRamLogic.GetChargeAimPoint(
            transform.position,
            targetPosition,
            BossRamLogic.GetFocusStopDistance(definition));
    }

    private void ApplyShockwave()
    {
        var radius = definition.ShockwaveRadius;
        var hitPlayers = new System.Collections.Generic.HashSet<PlayerCharacter>();
        var hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            var player = hit.GetComponentInParent<PlayerCharacter>();
            if (player != null)
            {
                TryShockwavePlayer(player, hitPlayers);
                continue;
            }

            var damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive && damageable is not BaseHealth)
                damageable.TakeDamage(definition.ShockwaveDamage * 0.5f, gameObject);
        }

        // Gracz nie ma collidera (PlayerJoinManager) — OverlapSphere go pomija.
        var players = FindObjectsByType<PlayerCharacter>();
        foreach (var player in players)
        {
            if (player == null || !player.IsCombatEnabled) continue;
            var delta = player.transform.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude <= radius * radius)
                TryShockwavePlayer(player, hitPlayers);
        }

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.position = transform.position + Vector3.up * 0.05f;
        ring.transform.localScale = new Vector3(radius * 2f, 0.05f, radius * 2f);
        var col = ring.GetComponent<Collider>();
        if (col != null) Destroy(col);
        var rend = ring.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = new Color(0.8f, 0.3f, 1f, 0.45f);
            rend.material = mat;
        }

        Destroy(ring, 0.5f);
    }

    private void TryShockwavePlayer(PlayerCharacter player, System.Collections.Generic.HashSet<PlayerCharacter> hitPlayers)
    {
        if (player == null || !player.IsCombatEnabled) return;
        if (!hitPlayers.Add(player)) return;

        player.GetComponent<Health>()?.TakeDamage(definition.ShockwaveDamage, gameObject);
        player.GetComponent<KnockbackReceiver>()?.AddFromOrigin(transform.position, 4f);
    }

    private void CreateTelegraphVisual(Vector3 targetPosition)
    {
        CancelTelegraphVisual();
        _telegraphVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _telegraphVisual.name = "RamChargeTelegraph";
        var from = transform.position;
        from.y = 0.1f;
        var to = targetPosition;
        to.y = 0.1f;
        var midpoint = (from + to) * 0.5f;
        var direction = to - from;
        var length = direction.magnitude;
        _telegraphVisual.transform.position = midpoint;
        _telegraphVisual.transform.localScale = new Vector3(1.2f, 0.08f, Mathf.Max(1f, length));
        if (direction.sqrMagnitude > 0.01f)
            _telegraphVisual.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        var col = _telegraphVisual.GetComponent<Collider>();
        if (col != null) Destroy(col);
        var rend = _telegraphVisual.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = new Color(1f, 0.2f, 0.2f, 0.55f);
            rend.material = mat;
        }
    }

    private void CancelTelegraphVisual()
    {
        if (_telegraphVisual == null) return;
        Destroy(_telegraphVisual);
        _telegraphVisual = null;
    }

    private void EnterState(BossRamState newState, float duration)
    {
        _state = newState;
        _stateTimer = duration;
    }

    private static Vector3 FlattenHorizontal(Vector3 value)
    {
        value.y = 0f;
        if (value.sqrMagnitude < 0.01f)
            return Vector3.forward;
        return value.normalized;
    }

    private static PlayerCharacter PickRandomLivingPlayer()
    {
        var players = FindObjectsByType<PlayerCharacter>();
        var living = new System.Collections.Generic.List<PlayerCharacter>();
        foreach (var player in players)
        {
            if (player.IsCombatEnabled)
                living.Add(player);
        }

        if (living.Count == 0) return null;
        return living[Random.Range(0, living.Count)];
    }

    private void OnHealthChanged(float current, float max)
    {
        UpdateEnrageFlag();
    }

    private void OnDied()
    {
        if (_isDying) return;
        _isDying = true;
        CancelTelegraphVisual();
        LootDropService.DropBossUnique(transform.position);
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        CancelTelegraphVisual();
        if (_health != null)
        {
            _health.Died -= OnDied;
            _health.HealthChanged -= OnHealthChanged;
        }
    }

    private static GameFlowState ResolveFlowState()
    {
        var flow = FindAnyObjectByType<GameFlowManager>();
        return flow != null ? flow.State : GameFlowState.WaveActive;
    }
}
