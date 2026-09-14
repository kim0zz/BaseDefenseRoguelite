using System;
using UnityEngine;

/// <summary>
/// Wspólny EXP i kasa bazy na czas runu (M5).
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public class SharedRunState : MonoBehaviour
{
    [SerializeField] private ProgressionConfig config;

    private int _gold;
    private int _sharedExp;
    private int _teamLevel = 1;
    private bool _siegeProgression;

    public void ConfigureSiegeProgression(bool enabled) => _siegeProgression = enabled;

    public static SharedRunState Instance { get; private set; }

    public ProgressionConfig Config => config;
    public int Gold => _gold;
    public int SharedExp => _sharedExp;
    public int TeamLevel => _teamLevel;

    public event Action<int> GoldChanged;
    public event Action<int, int> ExpChanged;
    public event Action<int> LeveledUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SharedRunState] Więcej niż jedna instancja — używam pierwszej.");
            return;
        }

        Instance = this;
        ResolveConfigReference();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Configure(ProgressionConfig progressionConfig)
    {
        config = progressionConfig;
    }

    private void ResolveConfigReference()
    {
        if (config != null) return;

#if UNITY_EDITOR
        config = UnityEditor.AssetDatabase.LoadAssetAtPath<ProgressionConfig>(
            "Assets/_Game/Config/M5_ProgressionConfig.asset");
        if (config != null)
        {
            Debug.LogWarning("[SharedRunState] Przywrócono config z assetu (zerwana referencja sceny).");
            return;
        }
#endif

        Debug.LogError("[SharedRunState] Brak ProgressionConfig — kasa/EXP nie będą działać.");
    }

    public void ResetRun()
    {
        _gold = 0;
        _sharedExp = 0;
        _teamLevel = 1;
        GoldChanged?.Invoke(_gold);
        ExpChanged?.Invoke(_sharedExp, GetExpToNextLevel());
    }

    public void AddKillRewards(int exp, int gold)
    {
        if (_siegeProgression) return;
        _gold += Mathf.Max(0, gold);
        _sharedExp += Mathf.Max(0, exp);
        GoldChanged?.Invoke(_gold);
        ExpChanged?.Invoke(_sharedExp, GetExpToNextLevel());

        if (config == null) return;

        var newLevel = config.GetTeamLevelForExp(_sharedExp);
        if (newLevel > _teamLevel)
        {
            _teamLevel = newLevel;
            LeveledUp?.Invoke(_teamLevel);
            Debug.Log($"[SharedRunState] Awans drużyny na poziom {_teamLevel} (EXP {_sharedExp}).");
        }
    }

    /// <summary>Nagroda gwarantowana trybu Siege; nie uruchamia klasycznej progresji kill.</summary>
    public void AddSiegeReward(int exp, int gold)
    {
        _gold += Mathf.Max(0, gold);
        _sharedExp += Mathf.Max(0, exp);
        GoldChanged?.Invoke(_gold);
        ExpChanged?.Invoke(_sharedExp, GetExpToNextLevel());
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || _gold < amount) return false;
        _gold -= amount;
        GoldChanged?.Invoke(_gold);
        return true;
    }

    public int GetExpToNextLevel()
    {
        if (config == null) return 0;
        return config.GetExpToNextLevel(_sharedExp, _teamLevel);
    }
}
