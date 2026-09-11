using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boss 2 — Warden: totemy, wyłączenie wieży, strefy szału (M8.3).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class BossWardenController : MonoBehaviour
{
    private enum WardenState
    {
        Idle,
        TelegraphSlam,
        ActiveSlam,
        Recover
    }

    [SerializeField] private WardenDefinition definition;

    private Health _health;
    private DamageTakenMultiplier _damageTakenMultiplier;
    private Renderer _renderer;
    private GameObject _slamTelegraphVisual;
    private WardenState _state = WardenState.Idle;
    private float _stateTimer;
    private float _slamCooldownTimer;
    private float _zoneCooldownTimer;
    private int _currentPhase;
    private bool _totemsSpawned;
    private bool _towerDisabled;
    private bool _isDying;
    private Vector3 _slamTargetPosition;
    private readonly List<Health> _totems = new();
    private readonly List<GameObject> _zoneVisuals = new();
    private readonly List<Coroutine> _zoneCoroutines = new();

    public WardenDefinition Definition => definition;
    public int CurrentPhase => _currentPhase;
    public float HealthRatio => _health != null && _health.MaxHealth > 0f
        ? _health.CurrentHealth / _health.MaxHealth
        : 0f;

    public static BossWardenController Active { get; private set; }

    public void Configure(WardenDefinition wardenDefinition)
    {
        definition = wardenDefinition;
        ApplyDefinition();
    }

    private void Awake()
    {
        _health = GetComponent<Health>();
        _damageTakenMultiplier = GetComponent<DamageTakenMultiplier>();
        if (_damageTakenMultiplier == null)
            _damageTakenMultiplier = gameObject.AddComponent<DamageTakenMultiplier>();
        _renderer = GetComponentInChildren<Renderer>();
        _health.Died += OnDied;
        _health.HealthChanged += OnHealthChanged;
    }

    private void OnEnable() => Active = this;

    private void OnDisable()
    {
        if (Active == this)
            Active = null;
    }

    private void Start() => ApplyDefinition();

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

        UpdatePhaseAndTriggers();
        UpdateDamageReduction();

        _slamCooldownTimer -= Time.deltaTime;
        if (_currentPhase >= 3)
            _zoneCooldownTimer -= Time.deltaTime;

        switch (_state)
        {
            case WardenState.Idle:
                UpdateIdle();
                break;
            case WardenState.TelegraphSlam:
                if (_stateTimer <= 0f)
                    EnterState(WardenState.ActiveSlam, 0.05f);
                else
                    _stateTimer -= Time.deltaTime;
                break;
            case WardenState.ActiveSlam:
                ApplySlamDamage();
                EnterState(WardenState.Recover, 0.35f);
                break;
            case WardenState.Recover:
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f)
                    EnterState(WardenState.Idle, 0f);
                break;
        }

        if (_currentPhase >= 3 && _zoneCooldownTimer <= 0f && _state == WardenState.Idle)
            BeginDangerZone();
    }

    private void UpdatePhaseAndTriggers()
    {
        var ratio = HealthRatio;
        _currentPhase = BossWardenLogic.AdvancePhase(_currentPhase, ratio);

        if (BossWardenLogic.ShouldSpawnTotemsOnce(_currentPhase, _totemsSpawned))
            SpawnTotems();

        if (BossWardenLogic.ShouldDisableTowerOnce(_currentPhase, _towerDisabled))
            DisableRandomTowerAndSummonSiege();
    }

    private void UpdateDamageReduction()
    {
        var anyTotemAlive = AnyTotemAlive();
        var mul = BossWardenLogic.GetDamageTakenMultiplier(anyTotemAlive, definition);
        _damageTakenMultiplier.SetMultiplier(mul);
    }

    private void UpdateIdle()
    {
        if (_slamCooldownTimer > 0f) return;
        BeginSlam();
    }

    private void BeginSlam()
    {
        _slamTargetPosition = PickSlamTargetPosition();
        CreateSlamTelegraph(_slamTargetPosition);
        EnterState(WardenState.TelegraphSlam, definition.SlamTelegraph);
    }

    private Vector3 PickSlamTargetPosition()
    {
        var player = PickNearestLivingPlayer();
        if (player != null)
            return player.transform.position;

        var baseHealth = FindAnyObjectByType<BaseHealth>();
        if (baseHealth != null && baseHealth.IsAlive)
            return baseHealth.transform.position;

        return transform.position + transform.forward * 3f;
    }

    private PlayerCharacter PickNearestLivingPlayer()
    {
        PlayerCharacter nearest = null;
        var bestDist = float.MaxValue;
        var origin = transform.position;
        foreach (var player in FindObjectsByType<PlayerCharacter>())
        {
            if (player == null || !player.IsCombatEnabled) continue;
            var delta = player.transform.position - origin;
            delta.y = 0f;
            var dist = delta.sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = player;
            }
        }

        return nearest;
    }

    private void ApplySlamDamage()
    {
        CancelSlamTelegraph();
        var radius = definition.SlamRadius;
        var origin = transform.position;
        var hitPlayers = new HashSet<PlayerCharacter>();
        var damage = definition.SlamDamage;

        var hits = Physics.OverlapSphere(origin, radius);
        foreach (var hit in hits)
        {
            var player = hit.GetComponentInParent<PlayerCharacter>();
            if (player != null)
                TrySlamPlayer(player, hitPlayers, damage);
        }

        foreach (var player in FindObjectsByType<PlayerCharacter>())
        {
            if (player == null || !player.IsCombatEnabled) continue;
            var delta = player.transform.position - origin;
            delta.y = 0f;
            if (delta.sqrMagnitude <= radius * radius)
                TrySlamPlayer(player, hitPlayers, damage);
        }

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.position = origin + Vector3.up * 0.05f;
        ring.transform.localScale = new Vector3(radius * 2f, 0.05f, radius * 2f);
        var col = ring.GetComponent<Collider>();
        if (col != null) Destroy(col);
        var rend = ring.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = new Color(0.1f, 0.55f, 0.45f, 0.45f);
            rend.material = mat;
        }

        Destroy(ring, 0.45f);
        _slamCooldownTimer = BossWardenLogic.GetSlamInterval(_currentPhase, definition);
    }

    private void TrySlamPlayer(PlayerCharacter player, HashSet<PlayerCharacter> hitPlayers, float damage)
    {
        if (player == null || !player.IsCombatEnabled) return;
        if (!hitPlayers.Add(player)) return;
        player.GetComponent<Health>()?.TakeDamage(damage, gameObject);
    }

    private void BeginDangerZone()
    {
        _zoneCooldownTimer = definition.ZoneInterval;
        var target = PickZoneTargetPosition();
        var routine = StartCoroutine(RunDangerZone(target));
        _zoneCoroutines.Add(routine);
    }

    private Vector3 PickZoneTargetPosition()
    {
        var players = FindObjectsByType<PlayerCharacter>();
        var living = new List<PlayerCharacter>();
        foreach (var player in players)
        {
            if (player != null && player.IsCombatEnabled)
                living.Add(player);
        }

        if (living.Count > 0)
            return living[Random.Range(0, living.Count)].transform.position;

        var tower = TowerRegistry.PickRandomLivingLineTower();
        if (tower != null)
            return tower.transform.position;

        return transform.position;
    }

    private IEnumerator RunDangerZone(Vector3 center)
    {
        center.y = 0f;
        var telegraph = CreateZoneVisual(center, definition.ZoneRadius, new Color(1f, 0.85f, 0.2f, 0.35f));
        _zoneVisuals.Add(telegraph);
        yield return new WaitForSeconds(definition.ZoneTelegraph);
        if (telegraph != null)
        {
            var rend = telegraph.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = rend.material;
                mat.color = new Color(1f, 0.25f, 0.15f, 0.55f);
            }
        }

        var elapsed = 0f;
        var tickTimer = 0f;
        while (elapsed < definition.ZoneDuration)
        {
            if (!RunFailRules.ShouldEnemyCombatTick(ResolveFlowState()))
                yield break;

            tickTimer -= Time.deltaTime;
            if (tickTimer <= 0f)
            {
                tickTimer = definition.ZoneTickInterval;
                DamagePlayersInRadius(center, definition.ZoneRadius, definition.ZoneTickDamage);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (telegraph != null)
        {
            _zoneVisuals.Remove(telegraph);
            Destroy(telegraph);
        }
    }

    private static void DamagePlayersInRadius(Vector3 center, float radius, float damage)
    {
        foreach (var player in FindObjectsByType<PlayerCharacter>())
        {
            if (player == null || !player.IsCombatEnabled) continue;
            var delta = player.transform.position - center;
            delta.y = 0f;
            if (delta.sqrMagnitude <= radius * radius)
                player.GetComponent<Health>()?.TakeDamage(damage, Active != null ? Active.gameObject : null);
        }
    }

    private static GameObject CreateZoneVisual(Vector3 center, float radius, Color color)
    {
        var zone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        zone.name = "WardenDangerZone";
        zone.transform.position = center + Vector3.up * 0.05f;
        zone.transform.localScale = new Vector3(radius * 2f, 0.1f, radius * 2f);
        var col = zone.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        var rend = zone.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = color;
            rend.material = mat;
        }

        return zone;
    }

    private void SpawnTotems()
    {
        _totemsSpawned = true;
        SpawnTotemAt(AttackLineId.Left);
        SpawnTotemAt(AttackLineId.Center);
        SpawnTotemAt(AttackLineId.Right);
    }

    private void SpawnTotemAt(AttackLineId line)
    {
        var pos = MapGreyboxLayout.GetChokePosition(line);
        pos.y = 1f;
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = $"WardenTotem_{line}";
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.9f, 1.2f, 0.9f);
        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);
        var rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = new Color(1f, 0.62f, 0.12f, 1f);
            rend.material = mat;
        }

        var health = go.AddComponent<Health>();
        health.Configure(definition.TotemMaxHealth);
        _totems.Add(health);
    }

    private bool AnyTotemAlive()
    {
        for (var i = _totems.Count - 1; i >= 0; i--)
        {
            var totem = _totems[i];
            if (totem == null)
            {
                _totems.RemoveAt(i);
                continue;
            }

            if (totem.IsAlive)
                return true;
        }

        return false;
    }

    private void DisableRandomTowerAndSummonSiege()
    {
        _towerDisabled = true;
        var tower = TowerRegistry.PickRandomLivingLineTower();
        if (tower == null) return;

        tower.SetDisabledByBoss(true);
        SummonSiegeOnLine(tower.AttackLine);
    }

    private void SummonSiegeOnLine(AttackLineId line)
    {
        if (definition.SiegeEnemyDefinition == null) return;

        var waveManager = FindAnyObjectByType<WaveManager>();
        var mapBuilder = FindAnyObjectByType<MapGreyboxBuilder>();
        for (var i = 0; i < definition.SiegeSummonCount; i++)
        {
            var pos = mapBuilder != null
                ? mapBuilder.GetLaneSpawnPosition(line, i, 1.2f)
                : MapGreyboxLayout.GetFarEnd(line) + Vector3.up;
            var enemy = EnemySpawner.Spawn(definition.SiegeEnemyDefinition, pos, i, line);
            if (enemy != null)
                waveManager?.RegisterMidWaveEnemy(enemy);
        }
    }

    private void CreateSlamTelegraph(Vector3 targetPosition)
    {
        CancelSlamTelegraph();
        _slamTelegraphVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _slamTelegraphVisual.name = "WardenSlamTelegraph";
        var from = transform.position;
        from.y = 0.1f;
        var to = targetPosition;
        to.y = 0.1f;
        var midpoint = (from + to) * 0.5f;
        var direction = to - from;
        var length = Mathf.Max(1f, direction.magnitude);
        _slamTelegraphVisual.transform.position = midpoint;
        _slamTelegraphVisual.transform.localScale = new Vector3(definition.SlamRadius * 2f, 0.08f, length);
        if (direction.sqrMagnitude > 0.01f)
            _slamTelegraphVisual.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        var col = _slamTelegraphVisual.GetComponent<Collider>();
        if (col != null) Destroy(col);
        var rend = _slamTelegraphVisual.GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = new Color(0.15f, 0.75f, 0.65f, 0.55f);
            rend.material = mat;
        }
    }

    private void CancelSlamTelegraph()
    {
        if (_slamTelegraphVisual == null) return;
        Destroy(_slamTelegraphVisual);
        _slamTelegraphVisual = null;
    }

    private void EnterState(WardenState newState, float duration)
    {
        _state = newState;
        _stateTimer = duration;
    }

    private void OnHealthChanged(float current, float max)
    {
        UpdatePhaseAndTriggers();
    }

    private void OnDied()
    {
        if (_isDying) return;
        _isDying = true;
        CancelSlamTelegraph();
        DespawnTotemsAndZones();
        FindAnyObjectByType<GameFlowManager>()?.EnterRunWon();
        StartCoroutine(DeathRoutine());
    }

    private void DespawnTotemsAndZones()
    {
        foreach (var totem in _totems)
        {
            if (totem != null)
                Destroy(totem.gameObject);
        }

        _totems.Clear();

        foreach (var zone in _zoneVisuals)
        {
            if (zone != null)
                Destroy(zone);
        }

        _zoneVisuals.Clear();

        foreach (var routine in _zoneCoroutines)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        _zoneCoroutines.Clear();
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        CancelSlamTelegraph();
        DespawnTotemsAndZones();
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
