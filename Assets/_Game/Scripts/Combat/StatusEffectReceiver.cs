using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nakładanie, tickowanie i wygasanie statusów (M7-T1).
/// </summary>
[DisallowMultipleComponent]
public class StatusEffectReceiver : MonoBehaviour
{
    private struct ActiveEffect
    {
        public StatusEffectType Type;
        public float Remaining;
        public float Magnitude;
        public float TickAccumulator;
    }

    [SerializeField] private float defaultStaggerThreshold = 25f;

    private readonly List<ActiveEffect> _effects = new();
    private Health _health;
    private float _staggerAccumulated;
    private float _burstWindowSeconds = 2f;
    private float _burstDamageInWindow;
    private float _burstWindowStart;

    public float StaggerAccumulated => _staggerAccumulated;
    public float DefaultStaggerThreshold => defaultStaggerThreshold;
    public bool IsStunned => HasEffect(StatusEffectType.Stun);
    public bool BlocksMovement => IsStunned;
    public bool BlocksAttack => IsStunned;
    public float MoveSpeedMultiplier { get; private set; } = 1f;

    public event Action StatusChanged;

    private void Awake()
    {
        _health = GetComponent<Health>();
        if (_health != null)
            _health.Damaged += OnDamaged;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.Damaged -= OnDamaged;
    }

    private void Update()
    {
        if (_effects.Count == 0 && _staggerAccumulated <= 0f) return;

        TickEffects(Time.deltaTime);
        UpdateMoveSpeedMultiplier();
        PruneExpiredBurstWindow();
    }

    public void ConfigureStaggerThreshold(float threshold)
    {
        defaultStaggerThreshold = Mathf.Max(1f, threshold);
    }

    public void Apply(StatusEffectType type, float duration, float magnitude, GameObject source = null)
    {
        switch (type)
        {
            case StatusEffectType.Stagger:
                AddStagger(magnitude > 0f ? magnitude : 1f);
                return;
            case StatusEffectType.Poison:
                RefreshOrAdd(type, duration, magnitude);
                break;
            default:
                RefreshOrAdd(type, duration, magnitude);
                break;
        }

        StatusChanged?.Invoke();
    }

    public void AddStagger(float amount)
    {
        if (amount <= 0f) return;
        _staggerAccumulated += amount;
        StatusChanged?.Invoke();
    }

    public bool TryConsumeStagger(float threshold)
    {
        if (_staggerAccumulated < threshold) return false;
        _staggerAccumulated = 0f;
        StatusChanged?.Invoke();
        return true;
    }

    public float GetBurstDamageInWindow(float windowSeconds)
    {
        PruneExpiredBurstWindow(windowSeconds);
        return _burstDamageInWindow;
    }

    public bool HasEffect(StatusEffectType type)
    {
        for (var i = 0; i < _effects.Count; i++)
        {
            if (_effects[i].Type == type && _effects[i].Remaining > 0f)
                return true;
        }

        return false;
    }

    public IReadOnlyList<StatusEffectType> GetActiveTypesForHud()
    {
        var result = new List<StatusEffectType>();
        for (var i = 0; i < _effects.Count; i++)
        {
            if (_effects[i].Remaining <= 0f) continue;
            if (!result.Contains(_effects[i].Type))
                result.Add(_effects[i].Type);
        }

        if (_staggerAccumulated > 0f && !result.Contains(StatusEffectType.Stagger))
            result.Add(StatusEffectType.Stagger);

        return result;
    }

    /// <summary>Test helper — symuluje tick statusów bez Update.</summary>
    public void TickForTests(float deltaTime)
    {
        TickEffects(deltaTime);
        UpdateMoveSpeedMultiplier();
    }

    private void OnDamaged(float amount, GameObject source)
    {
        if (amount <= 0f) return;

        var now = Time.time;
        if (now - _burstWindowStart > _burstWindowSeconds)
        {
            _burstWindowStart = now;
            _burstDamageInWindow = 0f;
        }

        _burstDamageInWindow += amount;
    }

    private void PruneExpiredBurstWindow(float windowSeconds = -1f)
    {
        var window = windowSeconds > 0f ? windowSeconds : _burstWindowSeconds;
        if (Time.time - _burstWindowStart <= window) return;
        _burstDamageInWindow = 0f;
    }

    private void RefreshOrAdd(StatusEffectType type, float duration, float magnitude)
    {
        for (var i = 0; i < _effects.Count; i++)
        {
            if (_effects[i].Type != type) continue;

            var entry = _effects[i];
            entry.Remaining = Mathf.Max(entry.Remaining, duration);
            entry.Magnitude = magnitude;
            _effects[i] = entry;
            return;
        }

        _effects.Add(new ActiveEffect
        {
            Type = type,
            Remaining = duration,
            Magnitude = magnitude
        });
    }

    private void TickEffects(float deltaTime)
    {
        var hadChanges = false;
        for (var i = _effects.Count - 1; i >= 0; i--)
        {
            var effect = _effects[i];
            effect.Remaining -= deltaTime;

            if (effect.Type == StatusEffectType.Poison && _health != null && _health.IsAlive)
            {
                effect.TickAccumulator += deltaTime;
                while (effect.TickAccumulator >= 1f)
                {
                    effect.TickAccumulator -= 1f;
                    _health.TakeDamage(effect.Magnitude);
                }
            }

            if (effect.Remaining <= 0f)
            {
                _effects.RemoveAt(i);
                hadChanges = true;
            }
            else
            {
                _effects[i] = effect;
            }
        }

        if (hadChanges)
            StatusChanged?.Invoke();
    }

    private void UpdateMoveSpeedMultiplier()
    {
        var multiplier = 1f;
        for (var i = 0; i < _effects.Count; i++)
        {
            if (_effects[i].Type != StatusEffectType.MoveSpeedBuff || _effects[i].Remaining <= 0f)
                continue;

            if (_effects[i].Magnitude < 0f)
                multiplier = Mathf.Min(multiplier, Mathf.Max(0.05f, 1f + _effects[i].Magnitude));
            else
                multiplier = Mathf.Max(multiplier, 1f + _effects[i].Magnitude);
        }

        MoveSpeedMultiplier = multiplier;
    }
}
