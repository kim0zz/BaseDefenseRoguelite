using System;
using UnityEngine;

/// <summary>
/// Respawn gracza po śmierci — FROZEN LOCAL_COOP: 20 s, przy bazie, bez i-frames.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerCharacter))]
[RequireComponent(typeof(Health))]
public class PlayerRespawn : MonoBehaviour
{
    public const float RespawnDelaySeconds = 20f;

    private PlayerCharacter _player;
    private Health _health;
    private Renderer _renderer;
    private GameFlowManager _flow;
    private float _respawnTimer;
    private bool _isRespawning;

    public bool IsRespawning => _isRespawning;
    public float RespawnTimeRemaining => _isRespawning ? _respawnTimer : 0f;

    public void CancelRespawn()
    {
        if (!_isRespawning) return;

        _isRespawning = false;
        _respawnTimer = 0f;
    }

    public event Action<float> RespawnTimerChanged;
    public event Action Respawned;

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _health = GetComponent<Health>();
        _renderer = GetComponentInChildren<Renderer>();
    }

    private void OnEnable()
    {
        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    /// <summary>Wywoływane po dynamicznym dodaniu komponentów (spawn gracza).</summary>
    public void Initialize()
    {
        if (_health == null) _health = GetComponent<Health>();
        if (_player == null) _player = GetComponent<PlayerCharacter>();
        BindEvents();
    }

    private void BindEvents()
    {
        if (_health == null) return;
        _health.Died -= OnDied;
        _health.Died += OnDied;
    }

    private void UnbindEvents()
    {
        if (_health == null) return;
        _health.Died -= OnDied;
    }

    private void Update()
    {
        if (!_isRespawning) return;

        if (_flow == null)
            _flow = FindAnyObjectByType<GameFlowManager>();
        if (_flow != null && _flow.State == GameFlowState.RunFailed)
            return;

        _respawnTimer -= Time.deltaTime;
        RespawnTimerChanged?.Invoke(_respawnTimer);

        if (_respawnTimer > 0f) return;

        CompleteRespawn();
    }

    private void OnDied()
    {
        _isRespawning = true;
        _respawnTimer = RespawnDelaySeconds;
        _player.SetCombatEnabled(false);
        GetComponent<PlayerAttackController>()?.CancelAttack();
        SetGhostVisual(true);
        RespawnTimerChanged?.Invoke(_respawnTimer);
    }

    private void CompleteRespawn()
    {
        _isRespawning = false;
        _health.HealToFull();
        _player.SetCombatEnabled(true);
        SetGhostVisual(false);

        var spawnPos = GetRespawnPosition();
        transform.position = spawnPos;

        Respawned?.Invoke();
        Debug.Log($"[PlayerRespawn] Gracz {_player.PlayerIndex + 1} wrócił przy bazie.");
    }

    private Vector3 GetRespawnPosition()
    {
        if (BaseCore.Instance != null)
            return BaseCore.Instance.transform.position + Vector3.up;

        return transform.position;
    }

    private void SetGhostVisual(bool isGhost)
    {
        if (_renderer == null) return;

        var mat = _renderer.material;
        var color = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
        color.a = isGhost ? 0.35f : 1f;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;
    }
}
