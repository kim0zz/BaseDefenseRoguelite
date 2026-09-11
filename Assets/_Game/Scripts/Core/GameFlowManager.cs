using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Stany pętli gry (M5 core loop).
/// </summary>
public enum GameFlowState
{
    WaveActive,
    Intermission,
    LevelUpPause,
    RunComplete,
    RunFailed,
    RunWon
}

/// <summary>
/// Orkiestracja core loop M5: fale, przerwy, EXP, naprawa bazy.
/// </summary>
[DisallowMultipleComponent]
public class GameFlowManager : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private WaveSequenceDefinition waveSequence;
    [SerializeField] private WaveDefinition[] wavesFallback;
    [SerializeField] private SharedRunState runState;
    [SerializeField] private ProgressionConfig progressionConfig;
    [SerializeField] private BuildFlowController buildFlowController;

    private GameFlowState _state;
    private int _waveIndex;
    private bool _resumeWaveAfterLevelUp;
    private RunFailReason _failReason = RunFailReason.None;
    private Health _baseHealthComponent;
    private readonly HashSet<Health> _trackedPlayerHealth = new();
    private readonly List<PlayerCharacter> _joinedPlayerScratch = new();

    public GameFlowState State => _state;
    public int CurrentWaveIndex => _waveIndex;
    public int TotalWaveCount => GetWaveCount();
    public RunFailReason FailReason => _failReason;

    private void Awake()
    {
        if (waveManager == null)
            waveManager = FindAnyObjectByType<WaveManager>();

        ResolveWaveSequenceReference();
        ResolveProgressionConfigReference();
        EnsureSharedRunState();
        EnsureBuildFlowController();

        if (runState != null && progressionConfig != null)
            runState.Configure(progressionConfig);
    }

    private void EnsureSharedRunState()
    {
        if (runState == null)
            runState = FindAnyObjectByType<SharedRunState>();

        if (runState != null) return;

        var go = GameObject.Find("RunProgression");
        if (go == null)
            go = new GameObject("RunProgression");

        runState = go.GetComponent<SharedRunState>();
        if (runState == null)
            runState = go.AddComponent<SharedRunState>();

        Debug.LogWarning("[GameFlowManager] Naprawiono SharedRunState w runtime (uszkodzony komponent sceny).");
    }

    private void EnsureBuildFlowController()
    {
        if (buildFlowController == null)
            buildFlowController = FindAnyObjectByType<BuildFlowController>();

        if (buildFlowController != null) return;

        var go = GameObject.Find("BuildSystem");
        if (go == null)
            go = new GameObject("BuildSystem");

        buildFlowController = go.GetComponent<BuildFlowController>();
        if (buildFlowController == null)
            buildFlowController = go.AddComponent<BuildFlowController>();

        if (go.GetComponent<BuildSystemBootstrap>() == null)
            go.AddComponent<BuildSystemBootstrap>();

        Debug.LogWarning("[GameFlowManager] Utworzono BuildFlowController w runtime.");
    }

    private void ResolveProgressionConfigReference()
    {
        if (progressionConfig != null) return;

#if UNITY_EDITOR
        progressionConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<ProgressionConfig>(
            "Assets/_Game/Config/M5_ProgressionConfig.asset");
        if (progressionConfig != null)
            Debug.LogWarning("[GameFlowManager] Przywrócono progressionConfig z assetu (zerwana referencja sceny).");
#endif
    }

    private void ResolveWaveSequenceReference()
    {
        if (waveSequence != null && waveSequence.WaveCount > 0)
            return;

#if UNITY_EDITOR
        waveSequence = UnityEditor.AssetDatabase.LoadAssetAtPath<WaveSequenceDefinition>(
            "Assets/_Game/Config/M7_WaveSequence.asset");
        if (waveSequence != null && waveSequence.WaveCount > 0)
        {
            Debug.LogWarning("[GameFlowManager] Przywrócono waveSequence z assetu (zerwana referencja sceny).");
            return;
        }

        waveSequence = UnityEditor.AssetDatabase.LoadAssetAtPath<WaveSequenceDefinition>(
            "Assets/_Game/Config/M5_WaveSequence.asset");
        if (waveSequence != null && waveSequence.WaveCount > 0)
        {
            Debug.LogWarning("[GameFlowManager] Przywrócono waveSequence z assetu (zerwana referencja sceny).");
            return;
        }
#endif

        if (wavesFallback != null && wavesFallback.Length > 0)
            Debug.LogWarning("[GameFlowManager] Używam wavesFallback — waveSequence niedostępne.");
    }

    private int GetWaveCount()
    {
        if (waveSequence != null && waveSequence.WaveCount > 0)
            return waveSequence.WaveCount;
        return wavesFallback != null ? wavesFallback.Length : 0;
    }

    private WaveDefinition GetWaveAt(int index)
    {
        if (waveSequence != null)
        {
            var wave = waveSequence.GetWave(index);
            if (wave != null)
                return wave;
        }

        if (wavesFallback == null || index < 0 || index >= wavesFallback.Length)
            return null;

        return wavesFallback[index];
    }

    private void Start()
    {
        runState?.ResetRun();
        RebuildMapIfNeeded();
        RefreshFailListeners();
        StartCoroutine(RefreshFailListenersNextFrame());
        BeginFirstWave();
    }

    private void OnEnable()
    {
        if (waveManager == null) return;
        waveManager.WaveStarted += OnWaveStarted;
        waveManager.WaveCompleted += OnWaveCompleted;
        if (runState != null)
            runState.LeveledUp += OnTeamLeveledUp;
    }

    private void OnDisable()
    {
        UnbindFailListeners();

        if (waveManager == null) return;
        waveManager.WaveStarted -= OnWaveStarted;
        waveManager.WaveCompleted -= OnWaveCompleted;
        if (runState != null)
            runState.LeveledUp -= OnTeamLeveledUp;
    }

    private void Update()
    {
        switch (_state)
        {
            case GameFlowState.Intermission:
                HandleIntermissionInput();
                break;
            case GameFlowState.LevelUpPause:
                HandleLevelUpInput();
                break;
        }
    }

    private void RebuildMapIfNeeded()
    {
        var mapBuilder = FindAnyObjectByType<MapGreyboxBuilder>();
        if (mapBuilder == null)
        {
            var go = GameObject.Find("MapGreybox");
            if (go == null)
                go = new GameObject("MapGreybox");

            mapBuilder = go.GetComponent<MapGreyboxBuilder>();
            if (mapBuilder == null)
                mapBuilder = go.AddComponent<MapGreyboxBuilder>();

            Debug.LogWarning("[GameFlowManager] Utworzono MapGreyboxBuilder w runtime.");
        }

        if (!mapBuilder.HasVisualMapChild())
            mapBuilder.BuildGreybox();
    }

    private void BeginFirstWave()
    {
        _waveIndex = 0;
        if (GetWaveCount() == 0)
        {
            Debug.LogError("[GameFlowManager] Brak fal — przypisz M5_WaveSequence lub wavesFallback.");
            return;
        }

        StartWaveAtIndex(_waveIndex);
    }

    private void StartWaveAtIndex(int index)
    {
        var wave = GetWaveAt(index);
        if (wave == null)
        {
            _state = GameFlowState.RunComplete;
            Debug.Log("[GameFlowManager] Run ukończony — brak kolejnych fal.");
            return;
        }

        _waveIndex = index;
        _state = GameFlowState.WaveActive;
        Time.timeScale = 1f;

        if (waveManager == null)
        {
            Debug.LogError("[GameFlowManager] Brak WaveManager — nie można rozpocząć fali.");
            return;
        }

        waveManager.StartWave(wave);
    }

    private void OnWaveStarted(WaveDefinition wave)
    {
        Debug.Log($"[GameFlowManager] Fala {wave.WaveNumber} — walka.");
    }

    private void OnWaveCompleted(WaveDefinition wave, bool endedByTimer)
    {
        if (_state == GameFlowState.LevelUpPause ||
            _state == GameFlowState.RunFailed ||
            _state == GameFlowState.RunComplete ||
            _state == GameFlowState.RunWon)
            return;

        Debug.Log($"[GameFlowManager] Fala {wave.WaveNumber} zakończona " +
                  $"({(endedByTimer ? "czas" : "wyczyść")}).");
        EnterIntermission(wave);
    }

    private void OnTeamLeveledUp(int newLevel)
    {
        if (_state == GameFlowState.LevelUpPause ||
            _state == GameFlowState.RunFailed ||
            _state == GameFlowState.RunComplete ||
            _state == GameFlowState.RunWon)
            return;

        _resumeWaveAfterLevelUp = _state == GameFlowState.WaveActive;
        EnterLevelUpPause();
    }

    private void EnterIntermission(WaveDefinition completedWave)
    {
        _state = GameFlowState.Intermission;
        if (_waveIndex + 1 >= GetWaveCount())
            Debug.Log("[GameFlowManager] Ostatnia fala zakończona — brak kolejnej. Wygrana wymaga pokonania Wardena.");
        else
            Debug.Log("[GameFlowManager] Przerwa. P1: R = naprawa bazy | Enter/N = następna fala.");
    }

    public void EnterRunWon()
    {
        if (_state == GameFlowState.RunFailed ||
            _state == GameFlowState.RunComplete ||
            _state == GameFlowState.RunWon)
            return;

        _state = GameFlowState.RunWon;
        Time.timeScale = 0f;

        foreach (var player in FindObjectsByType<PlayerCharacter>())
        {
            player.SetCombatEnabled(false);
            player.GetComponent<PlayerAttackController>()?.CancelAttack();
        }

        foreach (var respawn in FindObjectsByType<PlayerRespawn>())
            respawn.CancelRespawn();

        Debug.Log("[GameFlowManager] RunWon — Warden pokonany.");
    }

    public void EnterRunFailed(RunFailReason reason)
    {
        if (_state == GameFlowState.RunFailed ||
            _state == GameFlowState.RunComplete ||
            _state == GameFlowState.RunWon)
            return;
        if (reason == RunFailReason.None)
            return;

        _state = GameFlowState.RunFailed;
        _failReason = reason;
        Time.timeScale = 0f;

        foreach (var player in FindObjectsByType<PlayerCharacter>())
        {
            player.SetCombatEnabled(false);
            player.GetComponent<PlayerAttackController>()?.CancelAttack();
        }

        foreach (var respawn in FindObjectsByType<PlayerRespawn>())
            respawn.CancelRespawn();

        var reasonLabel = reason == RunFailReason.BaseDestroyed ? "baza upadła" : "drużyna wybita";
        Debug.Log($"[GameFlowManager] RunFailed — {reasonLabel}.");
    }

    /// <summary>Podpina listenery bazy i już obecnych graczy (M8.0).</summary>
    public void RefreshFailListeners()
    {
        BindBaseFailListener();
        foreach (var player in FindObjectsByType<PlayerCharacter>())
            RegisterJoinedPlayer(player);
    }

    /// <summary>Rejestruje dołączonego gracza do reguł wipe (P2–4 join).</summary>
    public void RegisterJoinedPlayer(PlayerCharacter player)
    {
        if (player == null) return;

        var health = player.GetComponent<Health>();
        if (health == null || _trackedPlayerHealth.Contains(health))
            return;

        _trackedPlayerHealth.Add(health);
        health.Died -= OnPlayerDied;
        health.Died += OnPlayerDied;
    }

    private IEnumerator RefreshFailListenersNextFrame()
    {
        yield return null;
        RefreshFailListeners();
    }

    private void BindBaseFailListener()
    {
        if (_baseHealthComponent != null)
        {
            _baseHealthComponent.Died -= OnBaseDied;
            _baseHealthComponent = null;
        }

        var baseHealth = FindAnyObjectByType<BaseHealth>();
        if (baseHealth == null) return;

        _baseHealthComponent = baseHealth.GetComponent<Health>();
        if (_baseHealthComponent == null) return;

        _baseHealthComponent.Died -= OnBaseDied;
        _baseHealthComponent.Died += OnBaseDied;
    }

    private void UnbindFailListeners()
    {
        if (_baseHealthComponent != null)
        {
            _baseHealthComponent.Died -= OnBaseDied;
            _baseHealthComponent = null;
        }

        foreach (var health in _trackedPlayerHealth)
        {
            if (health != null)
                health.Died -= OnPlayerDied;
        }

        _trackedPlayerHealth.Clear();
    }

    private void OnBaseDied()
    {
        if (_state == GameFlowState.RunFailed ||
            _state == GameFlowState.RunComplete ||
            _state == GameFlowState.RunWon)
            return;

        EnterRunFailed(RunFailReason.BaseDestroyed);
    }

    private void OnPlayerDied()
    {
        if (_state == GameFlowState.RunFailed ||
            _state == GameFlowState.RunComplete ||
            _state == GameFlowState.RunWon)
            return;

        EvaluateTeamWipe();
    }

    private void EvaluateTeamWipe()
    {
        _joinedPlayerScratch.Clear();
        _joinedPlayerScratch.AddRange(FindObjectsByType<PlayerCharacter>());
        RunFailRules.CountJoinedAndLiving(_joinedPlayerScratch, out var joined, out var living);

        if (RunFailRules.IsTeamWipe(joined, living))
            EnterRunFailed(RunFailReason.TeamWiped);
    }

    private void EnterLevelUpPause()
    {
        _state = GameFlowState.LevelUpPause;
        Time.timeScale = 0f;
        HealAllPlayers();
        buildFlowController?.BeginLevelUpSelections(runState?.TeamLevel ?? 1);
        Debug.Log($"[GameFlowManager] Awans drużyny (poz. {runState?.TeamLevel}). Każdy gracz wybiera talent.");
    }

    private static void HealAllPlayers()
    {
        foreach (var player in FindObjectsByType<PlayerCharacter>())
        {
            player.GetComponent<Health>()?.HealToFull();
            player.SetCombatEnabled(true);
        }
    }

    private void HandleIntermissionInput()
    {
        if (_state == GameFlowState.RunFailed) return;

        if (IsPlayer1RepairInput())
            FindAnyObjectByType<BaseHealth>()?.TryRepairFromSharedGold();

        if (IsPlayer1NextWaveInput())
        {
            if (_waveIndex + 1 >= GetWaveCount())
            {
                Debug.Log("[GameFlowManager] Brak kolejnej fali — wygrana wymaga pokonania Wardena na fali 10.");
                return;
            }

            StartWaveAtIndex(_waveIndex + 1);
        }
    }

    private void HandleLevelUpInput()
    {
        if (buildFlowController != null && !buildFlowController.LevelUpSelectionsComplete)
            return;

        // Overlay hides on complete; extra Enter/Space left timeScale at 0
        // (move frozen, aim still works because look is not scaled).
        ResumeAfterLevelUp();
    }

    private void ResumeAfterLevelUp()
    {
        if (_state != GameFlowState.LevelUpPause) return;

        Time.timeScale = 1f;

        if (_resumeWaveAfterLevelUp && waveManager != null && waveManager.State == WaveState.Active)
        {
            _state = GameFlowState.WaveActive;
            _resumeWaveAfterLevelUp = false;
            return;
        }

        _resumeWaveAfterLevelUp = false;

        if (waveManager != null && waveManager.State == WaveState.Completed)
            EnterIntermission(waveManager.CurrentWave);
        else
            _state = GameFlowState.Intermission;
    }

    private static bool IsPlayer1RepairInput()
    {
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard.rKey.wasPressedThisFrame;
    }

    private static bool IsPlayer1NextWaveInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return false;
        return keyboard.enterKey.wasPressedThisFrame || keyboard.nKey.wasPressedThisFrame;
    }
}
