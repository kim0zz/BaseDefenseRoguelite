using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Główna pętla pięciu fal Siege: nagroda, przygotowanie, fala, przejście/koniec.</summary>
[DefaultExecutionOrder(-150)]
[DisallowMultipleComponent]
public sealed class SiegeRunController : MonoBehaviour
{
    [SerializeField] private SiegeWaveDirector director;
    [SerializeField] private SiegeRewardController rewards;
    [SerializeField] private float preparationSeconds = 8f;
    [SerializeField] private EnemyDefinition[] visualTemplates;
    public int CurrentWave { get; private set; } = -1;
    public float PreparationRemaining { get; private set; }
    public bool IsTerminal { get; private set; }
    public bool IsVictory { get; private set; }
    public SiegeRewardDrop PendingDrop { get; private set; }
    public string LastWarning { get; private set; }
    public float WarningSeconds { get; private set; }
    public float RetreatRemaining => _retreating ? Mathf.Max(0f, _retreatTimer) : 0f;
    public event Action<string> Terminal;
    private SiegeArena _arena;
    private bool _started;
    private bool _waitingToStart;
    private float _retreatTimer;
    private bool _retreating;
    private bool _subscribedGate;
    private bool _subscribedCore;
    private SiegeWaveSequenceDefinition _runtimeSequence;
    private AttackLineId _warningLane;
    private Action<AttackLineId, string, float> _groupTelegraphedHandler;
    private Action _coreDiedHandler;

    private void Awake()
    {
        _arena = FindAnyObjectByType<SiegeArena>();
        director ??= GetComponent<SiegeWaveDirector>() ?? FindAnyObjectByType<SiegeWaveDirector>();
        rewards ??= GetComponent<SiegeRewardController>() ?? FindAnyObjectByType<SiegeRewardController>();
        if (_arena == null || director == null || rewards == null) { Debug.LogError("[Siege] Brakuje Arena/Director/Reward."); enabled = false; return; }
        director.WaveCompleted += OnWaveCompleted;
        _groupTelegraphedHandler = OnGroupTelegraphed;
        director.GroupTelegraphed += _groupTelegraphedHandler;
    }
    private void TryStartSiege()
    {
        if (_started || !PlayersReadyForSiege()) return;
        ConfigureDirector();
        DisableCombat();
        Time.timeScale = 0f;
        rewards.Completed += OnRewardCompleted;
        _started = true;
        rewards.BeginReward(0);
    }
    private static bool PlayersReadyForSiege()
    {
        var players = FindObjectsByType<PlayerCharacter>();
        if (players.Length == 0) return false;
        foreach (var player in players)
        {
            var build = player.GetComponent<PlayerBuildState>();
            var skills = player.GetComponent<PlayerSkillController>();
            if (build == null || build.ClassDefinition == null || skills == null) return false;
            var configuredSkills = 0;
            for (var slot = 0; slot < SkillLoadout.SlotCount; slot++)
                if (skills.GetSkill(slot) != null) configuredSkills++;
            // Classes intentionally may leave one or more loadout slots empty.
            // Readiness means the class kit has been applied, not that every
            // optional slot is occupied.
            if (configuredSkills == 0) return false;
        }
        return true;
    }
    private void Update()
    {
        // PlayerJoinManager creates the character before CombatBootstrap has added
        // its build and skills. Polling here is robust when Enter Play Mode options
        // preserve scene objects and does not lose the one-shot startup coroutine.
        if (!_started) { TryStartSiege(); return; }
        if (IsTerminal) return;
        SubscribeDefenseEvents();
        if (WarningSeconds > 0f) { WarningSeconds -= Time.unscaledDeltaTime; if (WarningSeconds <= 0f) _arena.SetEntranceWarning(_warningLane, false); }
        if (AllPlayersDead()) { EndRun(false, "Drużyna wybita"); return; }
        if (_retreating)
        {
            _retreatTimer -= Time.unscaledDeltaTime;
            if (_retreatTimer <= 0f) { _retreating = false; _arena.IsRetreating = false; _arena.CompleteRetreat(); director.SetSpawningPaused(false); }
            return;
        }
        if (!_waitingToStart) return;
        PreparationRemaining = Mathf.Max(0f, PreparationRemaining - Time.unscaledDeltaTime);
        if (PreparationRemaining <= 0f || StartPressed()) StartNextWave();
    }
    private void ConfigureDirector()
    {
        var players = FindObjectsByType<PlayerCharacter>();
        _runtimeSequence = SiegeWaveFactory.CreateDefault(visualTemplates);
        director.Configure(_runtimeSequence, _arena.GetSpawnPosition, players.Length);
    }
    public void StartNextWave()
    {
        if (IsTerminal || !_waitingToStart) return;
        _waitingToStart = false; PreparationRemaining = 0f; Time.timeScale = 1f;
        director.StartWave(CurrentWave + 1);
        CurrentWave = director.CurrentWaveIndex;
    }
    private void OnRewardCompleted()
    {
        if (IsTerminal) return;
        if (PendingDrop != null && !PendingDrop.Collected) PendingDrop.Collect();
        EnableCombat(); Time.timeScale = 1f; _waitingToStart = true; PreparationRemaining = preparationSeconds;
    }
    private void OnWaveCompleted(SiegeWaveDefinition wave)
    {
        if (IsTerminal || wave == null || CurrentWave < 0) return;
        var finished = CurrentWave;
        if (PendingDrop != null && !PendingDrop.Collected) PendingDrop.Collect();
        if (finished >= 4) { EndRun(true, "Twierdza obroniona"); return; }
        PendingDrop = SiegeRewardDrop.Spawn(_arena.Center, finished);
        director.SetSpawningPaused(true);
        StartCoroutine(BeginRewardAfterReveal(finished + 1));
    }
    private IEnumerator BeginRewardAfterReveal(int rewardWave)
    {
        yield return new WaitForSecondsRealtime(.7f);
        Time.timeScale = 0f; DisableCombat(); rewards.BeginReward(rewardWave);
    }
    private void SubscribeDefenseEvents()
    {
        if (!_subscribedGate && _arena.Gate != null) { _arena.Gate.Died += OnGateDied; _subscribedGate = true; }
        if (!_subscribedCore && _arena.Core != null)
        {
            _coreDiedHandler = OnCoreDied;
            _arena.Core.Died += _coreDiedHandler;
            _subscribedCore = true;
        }
        if (CurrentWave < 0 && director.CurrentWaveIndex >= 0) CurrentWave = director.CurrentWaveIndex;
    }
    private void OnGateDied()
    {
        if (_arena.IsInner || _retreating || IsTerminal) return;
        _retreating = true; _arena.IsRetreating = true; _retreatTimer = _arena.Config.RetreatSeconds; director.SetSpawningPaused(true);
        Debug.Log("[Siege] Brama padła — odwrót do rdzenia przez 3 sekundy.");
    }
    private void OnCoreDied() => EndRun(false, "Serce twierdzy zniszczone");
    private void OnGroupTelegraphed(AttackLineId lane, string label, float seconds)
    {
        if (IsTerminal || _arena == null) return;
        LastWarning = $"Natarcie: {label} ({lane})";
        WarningSeconds = Mathf.Max(0f, seconds);
        _warningLane = lane;
        _arena.SetEntranceWarning(lane, true);
    }
    private bool AllPlayersDead()
    {
        var players = FindObjectsByType<PlayerCharacter>();
        if (players.Length == 0) return false;
        foreach (var p in players) if (p.GetComponent<Health>()?.IsAlive == true) return false;
        return true;
    }
    private static bool StartPressed()
    {
        if (Keyboard.current?.enterKey.wasPressedThisFrame == true) return true;
        foreach (var pad in Gamepad.all) if (pad.startButton.wasPressedThisFrame) return true;
        return false;
    }
    private void DisableCombat() { foreach (var p in FindObjectsByType<PlayerCharacter>()) p.SetCombatEnabled(false); }
    private void EnableCombat() { foreach (var p in FindObjectsByType<PlayerCharacter>()) if (p.GetComponent<Health>()?.IsAlive != false) p.SetCombatEnabled(true); }
    private void EndRun(bool victory, string message)
    {
        if (IsTerminal) return; IsTerminal = true; IsVictory = victory; director.StopAndClear();
        if (PendingDrop != null && !PendingDrop.Collected)
        {
            Destroy(PendingDrop.gameObject);
            PendingDrop = null;
        }
        foreach (var p in FindObjectsByType<PlayerRespawn>()) p.CancelRespawn();
        DisableCombat(); Time.timeScale = 0f; Terminal?.Invoke(message); Debug.Log($"[Siege] {(victory ? "ZWYCIĘSTWO" : "KONIEC")}: {message}");
    }
    public void Restart() { if (!IsTerminal) return; var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); UnityEngine.SceneManagement.SceneManager.LoadScene(scene.path); }
    private void OnDestroy()
    {
        Time.timeScale = 1f;
        if (director != null)
        {
            director.WaveCompleted -= OnWaveCompleted;
            if (_groupTelegraphedHandler != null) director.GroupTelegraphed -= _groupTelegraphedHandler;
        }
        if (rewards != null) rewards.Completed -= OnRewardCompleted;
        if (_arena?.Gate != null && _subscribedGate) _arena.Gate.Died -= OnGateDied;
        if (_arena?.Core != null && _subscribedCore && _coreDiedHandler != null) _arena.Core.Died -= _coreDiedHandler;
        if (PendingDrop != null && !PendingDrop.Collected) Destroy(PendingDrop.gameObject);
        if (_runtimeSequence != null) SiegeWaveFactory.DestroyRuntimeSequence(_runtimeSequence);
    }
}
