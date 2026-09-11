using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// UI i logika buildów w przerwie oraz level-upie (M6/M7 co-op).
/// </summary>
[DisallowMultipleComponent]
public class BuildFlowController : MonoBehaviour
{
    private struct LevelUpSlot
    {
        public PlayerCharacter Player;
        public int SelectedIndex;
        public bool IsReady;
        public bool NeedsSelection;
    }

    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private SharedRunState runState;

    private readonly List<PlayerCharacter> _players = new();
    private readonly List<LevelUpSlot> _levelUpSlots = new();
    private bool _levelUpComplete;
    private int _transferSourcePlayer;
    private int _transferTargetPlayer = 1;
    private int _transferSlot = 1;

    public bool LevelUpSelectionsComplete => _levelUpComplete;

    public readonly struct LevelUpUiPlayer
    {
        public LevelUpUiPlayer(
            PlayerCharacter player,
            bool isReady,
            bool needsSelection,
            int selectedIndex,
            IReadOnlyList<TalentDefinition> options)
        {
            Player = player;
            IsReady = isReady;
            NeedsSelection = needsSelection;
            SelectedIndex = selectedIndex;
            Options = options;
        }

        public PlayerCharacter Player { get; }
        public bool IsReady { get; }
        public bool NeedsSelection { get; }
        public int SelectedIndex { get; }
        public IReadOnlyList<TalentDefinition> Options { get; }
    }

    public IReadOnlyList<LevelUpUiPlayer> GetLevelUpUiPlayers(int teamLevel)
    {
        var catalog = BuildSystemBootstrap.GetCatalogOrDefault();
        var result = new List<LevelUpUiPlayer>(_levelUpSlots.Count);

        for (var i = 0; i < _levelUpSlots.Count; i++)
        {
            var slot = _levelUpSlots[i];
            IReadOnlyList<TalentDefinition> options = null;

            if (slot.Player != null && !slot.IsReady && slot.NeedsSelection)
            {
                var build = slot.Player.GetComponent<PlayerBuildState>();
                if (build != null && catalog != null)
                    options = build.GetAvailableTalents(catalog, teamLevel);
            }

            result.Add(new LevelUpUiPlayer(
                slot.Player,
                slot.IsReady,
                slot.NeedsSelection,
                slot.SelectedIndex,
                options));
        }

        return result;
    }

    private void Awake()
    {
        if (gameFlowManager == null)
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();
        if (runState == null)
            runState = FindAnyObjectByType<SharedRunState>();

        EnsureBuildBootstrapExists();
    }

    private static void EnsureBuildBootstrapExists()
    {
        if (FindAnyObjectByType<BuildSystemBootstrap>() != null) return;

        var go = GameObject.Find("BuildSystem") ?? new GameObject("BuildSystem");
        if (go.GetComponent<BuildSystemBootstrap>() == null)
            go.AddComponent<BuildSystemBootstrap>();
    }

    public void BeginLevelUpSelections(int teamLevel)
    {
        RefreshPlayers();
        _levelUpComplete = false;
        _levelUpSlots.Clear();

        var catalog = BuildSystemBootstrap.GetCatalogOrDefault();
        foreach (var player in _players)
        {
            var build = player.GetComponent<PlayerBuildState>();
            build?.SetTeamLevel(teamLevel);
            var needs = build != null && build.NeedsTalentSelection(catalog, teamLevel);
            if (needs)
                build.MarkPendingTalentSelection(true);

            _levelUpSlots.Add(new LevelUpSlot
            {
                Player = player,
                SelectedIndex = 0,
                IsReady = !needs,
                NeedsSelection = needs
            });
        }

        EvaluateLevelUpComplete();
    }

    private void Update()
    {
        if (gameFlowManager == null) return;

        if (gameFlowManager.State == GameFlowState.LevelUpPause)
            HandleLevelUpInput();
        else if (gameFlowManager.State == GameFlowState.Intermission && LootLoopConfig.LootEnabledInRun)
            HandleIntermissionInput();
    }

    private void HandleLevelUpInput()
    {
        if (_levelUpComplete) return;

        var teamLevel = runState != null ? runState.TeamLevel : 1;
        var catalog = BuildSystemBootstrap.GetCatalogOrDefault();

        for (var i = 0; i < _levelUpSlots.Count; i++)
        {
            var slot = _levelUpSlots[i];
            if (slot.IsReady || !slot.NeedsSelection || slot.Player == null) continue;

            var build = slot.Player.GetComponent<PlayerBuildState>();
            if (build == null || catalog == null) continue;

            var options = build.GetAvailableTalents(catalog, teamLevel);
            if (options.Count == 0)
            {
                slot.IsReady = true;
                slot.NeedsSelection = false;
                _levelUpSlots[i] = slot;
                continue;
            }

            // LevelUpSlot is a struct — must write back after option input or 1/2 never persist.
            slot.Player.TryReadLevelUpOptionInput(options.Count, ref slot.SelectedIndex);
            slot.SelectedIndex = LevelUpSelectionRules.ClampOptionIndex(slot.SelectedIndex, options.Count);
            _levelUpSlots[i] = slot;

            if (!slot.Player.TryReadLevelUpConfirmInput()) continue;

            var chosen = options[slot.SelectedIndex];
            build.TryApplyTalent(chosen, teamLevel);
            slot.IsReady = true;
            slot.NeedsSelection = false;
            _levelUpSlots[i] = slot;
        }

        EvaluateLevelUpComplete();
    }

    private void EvaluateLevelUpComplete()
    {
        if (_levelUpSlots.Count == 0)
        {
            _levelUpComplete = true;
            return;
        }

        foreach (var slot in _levelUpSlots)
        {
            if (!slot.IsReady)
                return;
        }

        _levelUpComplete = true;
    }

    private void HandleIntermissionInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame) _transferSourcePlayer = 0;
        if (keyboard.digit2Key.wasPressedThisFrame) _transferSourcePlayer = 1;
        if (keyboard.digit3Key.wasPressedThisFrame) _transferSourcePlayer = 2;
        if (keyboard.digit4Key.wasPressedThisFrame) _transferSourcePlayer = 3;

        if (keyboard.qKey.wasPressedThisFrame)
            _transferSlot = 1;
        if (keyboard.wKey.wasPressedThisFrame)
            _transferSlot = 2;

        if (keyboard.fKey.wasPressedThisFrame)
            _transferTargetPlayer = (_transferTargetPlayer + 1) % Mathf.Max(1, _players.Count);

        if (!keyboard.gKey.wasPressedThisFrame) return;

        RefreshPlayers();
        if (_players.Count < 2) return;
        if (_transferSourcePlayer < 0 || _transferSourcePlayer >= _players.Count) return;
        if (_transferTargetPlayer < 0 || _transferTargetPlayer >= _players.Count) return;
        if (_transferSourcePlayer == _transferTargetPlayer) return;

        var sourceBuild = _players[_transferSourcePlayer].GetComponent<PlayerBuildState>();
        var targetBuild = _players[_transferTargetPlayer].GetComponent<PlayerBuildState>();
        if (sourceBuild == null || targetBuild == null) return;

        var slotIndex = _transferSlot - 1;
        sourceBuild.TrySwapWeaponSlotWith(targetBuild, slotIndex, slotIndex);
    }

    private void RefreshPlayers()
    {
        _players.Clear();
        _players.AddRange(FindObjectsByType<PlayerCharacter>());
        _players.Sort((a, b) => a.PlayerIndex.CompareTo(b.PlayerIndex));
    }

    private void OnGUI()
    {
        if (gameFlowManager == null) return;
        if (CombatHudView.IsCanvasActive) return;

        if (gameFlowManager.State == GameFlowState.LevelUpPause && !_levelUpComplete)
            DrawLevelUpPanel();
        else if (gameFlowManager.State == GameFlowState.Intermission && LootLoopConfig.LootEnabledInRun)
            DrawIntermissionBuildPanel();
    }

    private void DrawLevelUpPanel()
    {
        var teamLevel = runState != null ? runState.TeamLevel : 1;
        var optionRows = 0;
        for (var i = 0; i < _levelUpSlots.Count; i++)
        {
            var preview = _levelUpSlots[i];
            if (preview.Player == null || preview.IsReady) continue;
            var previewBuild = preview.Player.GetComponent<PlayerBuildState>();
            var catalog = BuildSystemBootstrap.GetCatalogOrDefault();
            var n = previewBuild != null ? previewBuild.GetAvailableTalents(catalog, teamLevel).Count : 0;
            if (n > optionRows) optionRows = n;
        }

        var rect = new Rect(Screen.width * 0.5f - 280f, Screen.height * 0.18f, 560f, 220f + optionRows * 20f);
        GUI.Box(rect, "");
        GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, rect.width - 24f, 24f),
            $"AWANS (poz. {teamLevel}) — każdy gracz wybiera własny talent");
        GUI.Label(new Rect(rect.x + 12f, rect.y + 32f, rect.width - 24f, 20f),
            "P1 KBM: 1–6 = wybór, Enter/Space = zatwierdź | Pad: West/East + South");

        var y = rect.y + 58f;
        for (var i = 0; i < _levelUpSlots.Count; i++)
        {
            var slot = _levelUpSlots[i];
            if (slot.Player == null) continue;

            var build = slot.Player.GetComponent<PlayerBuildState>();
            var status = slot.IsReady ? "<color=#88ff88>gotowy</color>" : "<color=#ffcc66>wybiera…</color>";
            GUI.Label(new Rect(rect.x + 12f, y, rect.width - 24f, 20f),
                $"P{slot.Player.PlayerIndex + 1} ({build?.ClassDefinition?.DisplayName}) — {status}");
            y += 20f;

            if (slot.IsReady || !slot.NeedsSelection || build == null) continue;

            var catalog = BuildSystemBootstrap.GetCatalogOrDefault();
            var options = build.GetAvailableTalents(catalog, teamLevel);
            for (var o = 0; o < options.Count; o++)
            {
                var prefix = o == slot.SelectedIndex ? ">> " : "   ";
                GUI.Label(new Rect(rect.x + 24f, y, rect.width - 36f, 18f),
                    $"{prefix}{o + 1}. {LootRarityUi.FormatTalentOption(options[o])}");
                y += 18f;
            }

            y += 4f;
        }
    }

    private void DrawIntermissionBuildPanel()
    {
        RefreshPlayers();
        var rect = new Rect(Screen.width - 360f, Screen.height - 260f, 350f, 250f);
        GUI.Box(rect, "");
        GUI.Label(new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 20f),
            "BUILD — wymiana broni (tylko przerwa)");
        GUI.Label(new Rect(rect.x + 8f, rect.y + 28f, rect.width - 16f, 18f),
            "P1: 1-4=źródło | Q/W=slot | F=cel | G=wymień");

        var y = rect.y + 52f;
        foreach (var player in _players)
        {
            var build = player.GetComponent<PlayerBuildState>();
            if (build == null) continue;

            var w1 = build.GetWeapon(0) != null
                ? LootRarityUi.FormatName(build.GetWeapon(0).DisplayName, build.GetWeapon(0).Rarity, build.GetWeapon(0).IsBossUnique)
                : "-";
            var w2 = build.GetWeapon(1) != null
                ? LootRarityUi.FormatName(build.GetWeapon(1).DisplayName, build.GetWeapon(1).Rarity, build.GetWeapon(1).IsBossUnique)
                : "-";
            var marker = player.PlayerIndex == _transferSourcePlayer ? ">" : " ";
            GUI.Label(new Rect(rect.x + 8f, y, rect.width - 16f, 18f),
                $"{marker} P{player.PlayerIndex + 1} [{build.ClassDefinition?.DisplayName}]: {w1} | {w2}");
            y += 36f;
        }
    }
}
