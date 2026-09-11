using UnityEngine;

/// <summary>
/// Marker rdzenia bazy — punkt odniesienia dla spawnu, ekonomii i warunków przegranej (M2 greybox).
/// </summary>
[DisallowMultipleComponent]
public class BaseCore : MonoBehaviour
{
    public static BaseCore Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BaseCore] Więcej niż jeden rdzeń bazy — używam pierwszego.");
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

/// <summary>
/// HP rdzenia bazy — naprawa między falami (M5).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(BaseCore))]
public class BaseHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private ProgressionConfig config;

    private Health _health;

    public bool IsAlive => _health != null && _health.IsAlive;
    public float CurrentHealth => _health != null ? _health.CurrentHealth : 0f;
    public float MaxHealth => _health != null ? _health.MaxHealth : 0f;

    private void Awake()
    {
        _health = GetComponent<Health>();
        if (_health == null)
            _health = gameObject.AddComponent<Health>();
    }

    public void InitializeFromConfig(ProgressionConfig progressionConfig)
    {
        config = progressionConfig;
        if (config == null) return;

        _health.Configure(config.BaseMaxHealth);
        var startHp = config.BaseMaxHealth * config.BaseStartHealthPercent;
        _health.TakeDamage(Mathf.Max(0f, config.BaseMaxHealth - startHp));
    }

    public void TakeDamage(float amount, GameObject source = null)
    {
        _health?.TakeDamage(amount, source);
    }

    public void Heal(float amount)
    {
        _health?.Heal(amount);
    }

    public bool TryRepairFromSharedGold()
    {
        var runState = SharedRunState.Instance;
        if (runState == null || config == null || !IsAlive) return false;
        if (CurrentHealth >= MaxHealth - 0.5f) return false;
        if (!runState.TrySpendGold(config.RepairGoldCost)) return false;

        Heal(config.RepairHealAmount);
        Debug.Log($"[BaseHealth] Naprawiono bazę (+{config.RepairHealAmount:0} HP). Koszt: {config.RepairGoldCost} złota.");
        return true;
    }
}
