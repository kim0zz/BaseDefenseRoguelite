using System;
using UnityEngine;

/// <summary>
/// Uniwersalny komponent HP — obrażenia, śmierć, leczenie (M3).
/// </summary>
[DisallowMultipleComponent]
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    private float _currentHealth;
    private bool _isDead;
    private GameObject _lastDamageSource;

    public bool IsAlive
    {
        get
        {
            var persistents = GetComponent<PlayerPersistentEffects>();
            if (persistents != null && persistents.KeepsAliveDuringLastChance())
                return true;
            return !_isDead && _currentHealth > 0f;
        }
    }

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => _isDead;
    public GameObject LastDamageSource => _lastDamageSource;

    public event Action<float, float> HealthChanged;
    public event Action<float, GameObject> Damaged;
    public event Action Died;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void Configure(float newMaxHealth)
    {
        maxHealth = Mathf.Max(1f, newMaxHealth);
        _currentHealth = maxHealth;
        _isDead = false;
        HealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    public void SetMaxHealthPreserveRatio(float newMaxHealth)
    {
        var ratio = maxHealth > 0f ? _currentHealth / maxHealth : 1f;
        maxHealth = Mathf.Max(1f, newMaxHealth);
        _currentHealth = Mathf.Clamp(maxHealth * ratio, 0f, maxHealth);
        _isDead = _currentHealth <= 0f;
        HealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    public void TakeDamage(float amount, GameObject source = null)
    {
        TakeDamage(amount, source, DamageHitFlags.None);
    }

    public void TakeDamage(float amount, GameObject source, DamageHitFlags flags)
    {
        if (_isDead || amount <= 0f) return;

        var immunity = GetComponent<DamageImmunity>();
        if (immunity != null && immunity.IsActive) return;

        if (source != null)
            _lastDamageSource = source;

        var isStatusTick = (flags & DamageHitFlags.StatusTick) != 0;
        var persistents = GetComponent<PlayerPersistentEffects>();
        if (persistents != null && !isStatusTick)
        {
            var processed = persistents.ProcessIncoming(amount, source);
            if (processed.Negated) return;

            Damaged?.Invoke(amount, source);

            if (processed.ConvertedToHeal)
            {
                Heal(processed.HealAmount);
                return;
            }

            amount = processed.FinalDamage;
        }
        else if (!isStatusTick)
        {
            Damaged?.Invoke(amount, source);
        }

        if (amount <= 0f) return;

        if (EnemyShielderGuard.TryRedirectIncomingDamage(this, amount, source))
            return;

        var damageTakenMul = GetComponent<DamageTakenMultiplier>();
        if (damageTakenMul != null)
            amount *= damageTakenMul.Multiplier;

        _currentHealth = Mathf.Max(0f, _currentHealth - amount);
        HealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth <= 0f)
        {
            if (TryPreventDeath(source))
                return;

            MarkDeadAndNotify();
        }
    }

    public void ForceDeath()
    {
        if (_isDead) return;
        _currentHealth = 0f;
        MarkDeadAndNotify();
    }

    private void MarkDeadAndNotify()
    {
        _isDead = true;
        Died?.Invoke();
        NotifyKillToSource();
    }

    private void NotifyKillToSource()
    {
        if (_lastDamageSource == null) return;
        if (GetComponent<PlayerCharacter>() != null) return;

        var killer = _lastDamageSource.GetComponentInParent<PlayerPersistentEffects>();
        killer?.OnEnemyKilledDuringKolos();
    }

    public void ReviveWithHealth(float hp)
    {
        _isDead = false;
        _currentHealth = Mathf.Clamp(hp, 1f, maxHealth);
        HealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    private bool TryPreventDeath(GameObject source)
    {
        var persistents = GetComponent<PlayerPersistentEffects>();
        return persistents != null && persistents.TryPreventDeath(this, source);
    }

    public void HealToFull()
    {
        _isDead = false;
        _currentHealth = maxHealth;
        HealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;

        var persistents = GetComponent<PlayerPersistentEffects>();
        var inLastChance = persistents != null && persistents.KeepsAliveDuringLastChance();
        if (_isDead && !inLastChance) return;

        if (inLastChance && _isDead)
            _isDead = false;

        var healAmount = amount;
        if (inLastChance && persistents != null && persistents.Has(PersistentEffectKind.HealAmpInLastChance))
            healAmount *= persistents.GetLastChanceHealAmpMultiplier();

        persistents?.NotifyHealDuringLastChance(healAmount);

        _currentHealth = Mathf.Min(maxHealth, _currentHealth + healAmount);
        HealthChanged?.Invoke(_currentHealth, maxHealth);
    }
}
