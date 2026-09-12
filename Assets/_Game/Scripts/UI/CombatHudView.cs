using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// uGUI HUD walki + level-up (M9.0). Zastępuje OnGUI gdy aktywny.
/// </summary>
[DisallowMultipleComponent]
public class CombatHudView : MonoBehaviour
{
    private static CombatHudView _instance;

    [SerializeField] private WaveManager waveManager;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private SharedRunState runState;
    [SerializeField] private BuildFlowController buildFlowController;

    private HudSkin _skin;
    private Font _font;
    private Sprite _whiteSprite;
    private CanvasScaler _scaler;
    private bool _initialized;
    private string _waveStatus = "Przygotowanie...";

    private Canvas _canvas;
    private RectTransform _topBar;
    private Image _topBarImage;
    private Image _hintPanelImage;
    private Text _goldExpText;
    private Text _flowText;
    private Text _baseText;
    private Image _baseFill;
    private Text _towerText;
    private Text _statusText;
    private Text _bossText;
    private Image _bossFill;
    private GameObject _bossBarRoot;
    private RectTransform _playerRowsRoot;
    private RectTransform _skillHudRoot;
    private Text _contextHintText;
    private GameObject _runFailedOverlay;
    private Text _runFailedTitle;
    private GameObject _runWonOverlay;
    private GameObject _levelUpOverlay;
    private RectTransform _levelUpContent;
    private string _levelUpCacheKey = "";

    private readonly List<PlayerCharacter> _trackedPlayers = new();
    private readonly List<PlayerRowUi> _playerRows = new();
    private readonly List<SkillBlockUi> _skillBlocks = new();

    public static bool IsCanvasActive => _instance != null && _instance._initialized;

    private struct PlayerRowUi
    {
        public Text Label;
        public Image HpFill;
        public Text KitText;
        public GameObject HartRoot;
        public Image HartFill;
        public Text HartText;
    }

    private struct SkillBlockUi
    {
        public RectTransform Root;
        public Image[] SlotBackgrounds;
        public Image[] SlotIcons;
        public Image[] SlotCooldowns;
        public Text[] SlotKeys;
        public Text[] SlotLabels;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        ResolveReferences();
        BuildCanvas();
        _initialized = true;

        if (waveManager != null)
        {
            waveManager.WaveStarted += OnWaveStarted;
            waveManager.WaveCompleted += OnWaveCompleted;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        if (waveManager != null)
        {
            waveManager.WaveStarted -= OnWaveStarted;
            waveManager.WaveCompleted -= OnWaveCompleted;
        }
    }

    private void ResolveReferences()
    {
        if (waveManager == null)
            waveManager = FindAnyObjectByType<WaveManager>();
        if (gameFlowManager == null)
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();
        if (runState == null)
            runState = FindAnyObjectByType<SharedRunState>();
        if (runState == null)
            runState = SharedRunState.Instance;
        if (buildFlowController == null)
            buildFlowController = FindAnyObjectByType<BuildFlowController>();

        _skin = HudSkin.GetOrDefault();
        _font = ResolveFont();
        _whiteSprite = CreateWhiteSprite();
    }

    private void Update()
    {
        if (!_initialized) return;

        ResolveReferences();
        ApplyHudScale();
        RefreshTrackedPlayers();
        RefreshTopBar();
        RefreshPlayerRows();
        RefreshSkillHud();
        RefreshContextHints();
        RefreshOverlays();
        RefreshLevelUpOverlay();
    }

    private void OnWaveStarted(WaveDefinition wave)
    {
        _waveStatus = $"Fala {wave.WaveNumber} — w toku";
    }

    private void OnWaveCompleted(WaveDefinition wave, bool endedByTimer)
    {
        _waveStatus = endedByTimer
            ? $"Fala {wave.WaveNumber} — koniec (czas)"
            : $"Fala {wave.WaveNumber} — wyczyszczono!";
    }

    private void RefreshTrackedPlayers()
    {
        if (runState == null)
            runState = SharedRunState.Instance;

        _trackedPlayers.Clear();
        _trackedPlayers.AddRange(FindObjectsByType<PlayerCharacter>());
        _trackedPlayers.Sort((a, b) => a.PlayerIndex.CompareTo(b.PlayerIndex));
    }

    private void RefreshTopBar()
    {
        if (runState == null)
        {
            _goldExpText.text = "BRAK SharedRunState";
            _goldExpText.color = new Color(1f, 0.53f, 0.53f);
        }
        else
        {
            var expToNext = runState.GetExpToNextLevel();
            var expLine = expToNext > 0
                ? $"EXP {runState.SharedExp} → lvl {runState.TeamLevel + 1}: {expToNext}"
                : $"EXP {runState.SharedExp} (max lvl {runState.TeamLevel})";
            _goldExpText.text = $"{runState.Gold} zł  |  lvl {runState.TeamLevel}  |  {expLine}";
            _goldExpText.color = _skin.AccentColor;
        }

        _flowText.text = BuildFlowLine();
        _flowText.color = Color.white;

        var baseHealth = FindAnyObjectByType<BaseHealth>();
        if (baseHealth == null)
        {
            _baseText.text = "";
            _baseFill.fillAmount = 0f;
        }
        else
        {
            var pct = baseHealth.MaxHealth > 0f ? baseHealth.CurrentHealth / baseHealth.MaxHealth : 0f;
            _baseText.text = $"Baza {baseHealth.CurrentHealth:0}/{baseHealth.MaxHealth:0}";
            _baseFill.fillAmount = pct;
        }

        var lowCount = 0;
        var destroyed = 0;
        foreach (var tower in TowerRegistry.All)
        {
            if (tower == null) continue;
            if (tower.IsDestroyed) destroyed++;
            else if (tower.MaxHealth > 0f && tower.CurrentHealth / tower.MaxHealth <= 0.35f) lowCount++;
        }

        if (lowCount == 0 && destroyed == 0)
        {
            _towerText.text = "";
        }
        else
        {
            _towerText.text = destroyed > 0
                ? $"Wieże: niskie HP {lowCount} | zniszczone {destroyed}"
                : $"Wieże: niskie HP {lowCount} | zniszczone {destroyed}";
            _towerText.color = destroyed > 0 ? new Color(1f, 0.4f, 0.4f) : new Color(1f, 0.9f, 0.4f);
        }

        var statusParts = new List<string>();
        foreach (var player in _trackedPlayers)
        {
            if (player == null) continue;
            var status = player.GetComponent<StatusEffectReceiver>();
            if (status == null) continue;
            foreach (var type in status.GetActiveTypesForHud())
                statusParts.Add($"P{player.PlayerIndex + 1}:{type}");
        }

        _statusText.text = statusParts.Count == 0 ? "" : $"Statusy: {string.Join(", ", statusParts)}";
        RefreshBossLine();
    }

    private void RefreshBossLine()
    {
        var warden = BossWardenController.Active;
        if (warden != null && warden.Definition != null)
        {
            DrawBossHud(warden.Definition.DisplayName, BossWardenLogic.GetPhaseHudLabel(warden.CurrentPhase),
                warden.HealthRatio, warden.GetComponent<Health>(), new Color(0.22f, 0.72f, 0.58f));
            return;
        }

        var ram = BossRamController.Active;
        if (ram == null || ram.Definition == null)
        {
            _bossText.text = "";
            _bossFill.fillAmount = 0f;
            if (_bossBarRoot != null)
                _bossBarRoot.SetActive(false);
            return;
        }

        var phase = ram.State is BossRamState.TelegraphCharge or BossRamState.Charge
            ? "Szarża"
            : ram.IsEnraged ? "Enrage" : "Walka";
        DrawBossHud(ram.Definition.DisplayName, phase, ram.HealthRatio, ram.GetComponent<Health>(),
            new Color(0.72f, 0.38f, 0.95f));
    }

    private void DrawBossHud(string name, string phase, float ratio, Health health, Color barColor)
    {
        var hpPct = Mathf.Clamp01(ratio);
        var hpLabel = health != null
            ? $"{health.CurrentHealth:0}/{health.MaxHealth:0}"
            : $"{(hpPct * 100f):0}%";
        _bossText.text = $"BOSS {name}  |  {phase}  |  {hpLabel}";
        _bossFill.fillAmount = hpPct;
        _bossFill.color = hpPct <= 0.5f ? new Color(1f, 0.4f, 0.22f) : barColor;
        if (_bossBarRoot != null)
            _bossBarRoot.SetActive(true);
    }

    private string BuildFlowLine()
    {
        var flowState = gameFlowManager != null ? gameFlowManager.State : GameFlowState.WaveActive;

        if (flowState == GameFlowState.LevelUpPause)
            return $"AWANS (poz. {runState?.TeamLevel})";

        if (flowState == GameFlowState.Intermission)
            return $"{_waveStatus}  |  PRZERWA";

        if (flowState == GameFlowState.RunComplete)
            return "Boss 1 pokonany — mid-run complete";

        if (flowState == GameFlowState.RunFailed)
            return GetRunFailedHeadline();

        if (flowState == GameFlowState.RunWon)
            return "WYGRANA — Warden pokonany";

        if (waveManager != null && waveManager.State == WaveState.Active)
        {
            var waveNum = waveManager.CurrentWave != null ? waveManager.CurrentWave.WaveNumber : 0;
            var totalWaves = GetTotalWaveCount();
            return $"Fala {waveNum}/{totalWaves}  |  {_waveStatus}  |  {waveManager.TimeRemaining:0}s  |  " +
                   $"wrogowie {waveManager.AliveEnemyCount}/{waveManager.TotalSpawned}";
        }

        return _waveStatus;
    }

    private void RefreshPlayerRows()
    {
        EnsurePlayerRowCount(_trackedPlayers.Count);

        for (var i = 0; i < _playerRows.Count; i++)
        {
            var row = _playerRows[i];
            if (i >= _trackedPlayers.Count)
            {
                row.Label.text = "";
                row.KitText.text = "";
                row.HpFill.fillAmount = 0f;
                if (row.HartRoot != null)
                    row.HartRoot.SetActive(false);
                continue;
            }

            var player = _trackedPlayers[i];
            if (player == null) continue;

            var health = player.GetComponent<Health>();
            var respawn = player.GetComponent<PlayerRespawn>();
            if (health == null) continue;

            var hpPct = health.MaxHealth > 0f ? health.CurrentHealth / health.MaxHealth : 0f;
            row.HpFill.fillAmount = hpPct;

            var build = player.GetComponent<PlayerBuildState>();
            var classLabel = build?.ClassDefinition != null ? build.ClassDefinition.DisplayName : "?";
            var stats = build != null ? build.GetEffectiveStats() : default;
            var power = LootLoopConfig.LootEnabledInRun
                ? CombatHudCopy.FormatWeaponPower(
                    build?.GetWeapon(build.ActiveWeaponIndex)?.DisplayName ?? "-",
                    stats.Damage,
                    stats.AttackInterval)
                : CombatHudCopy.FormatBasicAttackPower(stats.Damage, stats.AttackInterval);

            var label = $"P{player.PlayerIndex + 1} {classLabel}  |  {power}";
            if (respawn != null && respawn.IsRespawning)
                label += $"  |  resp {respawn.RespawnTimeRemaining:0.0}s";
            if (!player.IsCombatEnabled && (respawn == null || !respawn.IsRespawning))
                label += "  |  MARTWY";

            row.Label.text = label;
            row.KitText.text = FormatKitMeters(player);
            RefreshHartMeter(row, player);
        }
    }

    private static string FormatKitMeters(PlayerCharacter player)
    {
        var parts = new List<string>();
        var persistents = player.GetComponent<PlayerPersistentEffects>();
        var attack = player.GetComponent<PlayerAttackController>();

        if (persistents != null)
        {
            if (persistents.Has(PersistentEffectKind.FuryMeter))
            {
                parts.Add(persistents.FuryActive
                    ? "Furia AKTYWNA"
                    : $"Furia {persistents.FuryMeter:0}/{persistents.FuryThreshold:0}");
            }

            if (persistents.LastChanceActive)
            {
                parts.Add(
                    $"Ostatnia szansa {persistents.LastChanceHealAccum:0}/{persistents.LastChanceSurviveThreshold:0}");
            }

            if (persistents.KolosActive)
                parts.Add($"KOLOS {persistents.KolosRemaining:0.0}s");
        }

        var comboStep = attack != null ? attack.ComboStep : 0;
        if (comboStep > 0)
            parts.Add($"AA {comboStep}/3");

        if (CombatHudCopy.TryFormatAttackIntervalOverride(persistents, out var rapidLabel))
            parts.Add(rapidLabel);

        if (DeployableHudRead.TryGetBombLine(player.gameObject, out var bombLine))
            parts.Add(bombLine);

        if (DeployableHudRead.TryGetOrbitalLine(player.gameObject, out var orbitalLine))
            parts.Add(orbitalLine);

        return string.Join("  |  ", parts);
    }

    private static void RefreshHartMeter(PlayerRowUi row, PlayerCharacter player)
    {
        if (row.HartRoot == null) return;

        var persistents = player != null ? player.GetComponent<PlayerPersistentEffects>() : null;
        var show = persistents != null && persistents.Has(PersistentEffectKind.HartStacks);
        row.HartRoot.SetActive(show);
        if (!show) return;

        var max = Mathf.Max(1, persistents.HartMaxStacks);
        row.HartText.text = CombatHudCopy.FormatHartPips(
            persistents.HartStacks, max, persistents.HartReady);
        row.HartFill.fillAmount = persistents.HartReady
            ? 1f
            : persistents.HartStacks / (float)max;
        row.HartFill.color = persistents.HartReady
            ? new Color(0.45f, 0.85f, 1f, 1f)
            : new Color(0.35f, 0.62f, 0.88f, 1f);
        row.HartText.color = persistents.HartReady
            ? new Color(0.75f, 0.95f, 1f, 1f)
            : Color.white;
    }

    private void RefreshSkillHud()
    {
        EnsureSkillBlockCount(_trackedPlayers.Count);

        for (var playerIndex = 0; playerIndex < _trackedPlayers.Count; playerIndex++)
        {
            var player = _trackedPlayers[playerIndex];
            var block = _skillBlocks[playerIndex];
            if (player == null || block.Root == null)
            {
                block.Root.gameObject.SetActive(false);
                continue;
            }

            block.Root.gameObject.SetActive(true);
            var skills = player.GetComponent<PlayerSkillController>();
            if (skills == null)
            {
                block.Root.gameObject.SetActive(false);
                continue;
            }

            var slotCount = skills.GetSkill(PlayerSkillController.UltimateSlot) != null
                ? PlayerSkillController.SkillSlotCount
                : SkillLoadout.ActiveSlotCount;
            var isKeyboard = player.InputMode == PlayerInputMode.KeyboardMouse;

            for (var slot = 0; slot < block.SlotBackgrounds.Length; slot++)
            {
                var visible = slot < slotCount;
                block.SlotBackgrounds[slot].gameObject.SetActive(visible);
                block.SlotIcons[slot].gameObject.SetActive(visible);
                block.SlotCooldowns[slot].gameObject.SetActive(visible);
                block.SlotKeys[slot].gameObject.SetActive(visible);
                block.SlotLabels[slot].gameObject.SetActive(visible);
                if (!visible) continue;

                var skill = skills.GetSkill(slot);
                var passive = skill != null && skill.ActivationMode == SkillActivationMode.Passive;
                var cdNorm = skills.GetCooldownNormalized(slot);
                var ready = cdNorm <= 0f;

                block.SlotBackgrounds[slot].color = skill != null
                    ? (ready ? new Color(0.18f, 0.22f, 0.28f, 0.95f) : new Color(0.14f, 0.16f, 0.20f, 0.95f))
                    : _skin.SlotEmptyColor;

                if (skill != null && skill.Icon != null)
                {
                    block.SlotIcons[slot].sprite = skill.Icon;
                    block.SlotIcons[slot].color = Color.white;
                }
                else
                {
                    block.SlotIcons[slot].sprite = _whiteSprite;
                    block.SlotIcons[slot].color = ready
                        ? new Color(0.30f, 0.85f, 0.45f, 0.95f)
                        : new Color(0.85f, 0.35f, 0.25f, 0.95f);
                }

                block.SlotCooldowns[slot].fillAmount = skill != null && !ready ? cdNorm : 0f;
                block.SlotKeys[slot].text = CombatHudCopy.GetSkillSlotKeyHint(slot, isKeyboard, passive);

                if (skill == null)
                {
                    block.SlotLabels[slot].text = "";
                }
                else if (passive)
                {
                    block.SlotLabels[slot].text = $"{skill.DisplayName} (PASYWNE)";
                }
                else
                {
                    block.SlotLabels[slot].text = CombatHudCopy.FormatSkillCooldown(
                        skill.DisplayName,
                        skills.GetCooldownRemaining(slot),
                        skill.CooldownSeconds);
                }
            }
        }
    }

    private void RefreshContextHints()
    {
        var flowState = gameFlowManager != null ? gameFlowManager.State : GameFlowState.WaveActive;
        if (flowState == GameFlowState.WaveActive ||
            flowState == GameFlowState.RunFailed ||
            flowState == GameFlowState.RunWon ||
            flowState == GameFlowState.LevelUpPause)
        {
            _contextHintText.text = "";
            return;
        }

        if (flowState == GameFlowState.RunComplete)
        {
            _contextHintText.text = "Boss 1 pokonany — mid-run complete (placeholder M7).";
            return;
        }

        if (flowState == GameFlowState.Intermission)
        {
            _contextHintText.text =
                "Przerwa: R = naprawa bazy | Enter/N = następna fala.\n" +
                "W walce: cel mysz/stick | atak Space/LMB/West | skille Q/E/R lub East/North/LB.";
            return;
        }

        _contextHintText.text = "";
    }

    private void RefreshOverlays()
    {
        var flowState = gameFlowManager != null ? gameFlowManager.State : GameFlowState.WaveActive;
        _runFailedOverlay.SetActive(flowState == GameFlowState.RunFailed);
        _runWonOverlay.SetActive(flowState == GameFlowState.RunWon);

        if (flowState == GameFlowState.RunFailed)
            _runFailedTitle.text = GetRunFailedHeadline();
    }

    private void RefreshLevelUpOverlay()
    {
        var show = LevelUpHudRules.ShouldShowOverlay(
            gameFlowManager != null ? gameFlowManager.State : GameFlowState.WaveActive,
            buildFlowController != null,
            buildFlowController != null && buildFlowController.LevelUpSelectionsComplete);

        _levelUpOverlay.SetActive(show);
        if (!show)
        {
            _levelUpCacheKey = "";
            return;
        }

        _levelUpOverlay.transform.SetAsLastSibling();

        var teamLevel = runState != null ? runState.TeamLevel : 1;
        var players = buildFlowController.GetLevelUpUiPlayers(teamLevel);
        var cacheKey = BuildLevelUpCacheKey(teamLevel, players);
        if (cacheKey == _levelUpCacheKey && _levelUpContent.childCount > 0)
            return;

        _levelUpCacheKey = cacheKey;
        ClearChildren(_levelUpContent);

        AddLevelUpText(
            $"AWANS (poz. {teamLevel}) — wybierz talent",
            28, FontStyle.Bold, _skin.AccentColor);
        AddLevelUpText(
            "Klawiatura: 1 / 2 / 3 = karta, Enter lub Space = zatwierdź",
            18, FontStyle.Normal, Color.white);

        var anyCards = false;
        foreach (var entry in players)
        {
            if (entry.Player == null) continue;

            var build = entry.Player.GetComponent<PlayerBuildState>();
            var status = entry.IsReady ? "gotowy" : "wybiera…";
            AddLevelUpText(
                $"P{entry.Player.PlayerIndex + 1} ({build?.ClassDefinition?.DisplayName}) — {status}",
                22, FontStyle.Bold, entry.IsReady ? new Color(0.53f, 1f, 0.53f) : new Color(1f, 0.8f, 0.4f));

            if (entry.IsReady || !entry.NeedsSelection || entry.Options == null) continue;

            for (var o = 0; o < entry.Options.Count; o++)
            {
                var selected = o == entry.SelectedIndex;
                var cardText = TalentCardCopy.FormatCard(entry.Options[o]);
                var prefix = selected ? $"► {o + 1}." : $"{o + 1}.";
                AddLevelUpCard($"{prefix}\n{cardText}", selected);
                anyCards = true;
            }
        }

        if (!anyCards)
        {
            AddLevelUpText(
                "Brak kart do wyboru — Enter = dalej.",
                20, FontStyle.Italic, new Color(1f, 0.85f, 0.5f));
        }
    }

    private void BuildCanvas()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 250;

        _scaler = gameObject.AddComponent<CanvasScaler>();
        _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _scaler.matchWidthOrHeight = 0.5f;
        ApplyHudScale();

        gameObject.AddComponent<GraphicRaycaster>();

        var panelSprite = _skin.PanelSprite != null ? _skin.PanelSprite : _whiteSprite;
        var barSprite = _skin.BarSprite != null ? _skin.BarSprite : _whiteSprite;

        var topBar = CreatePanel("TopBar", transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(540f, 250f), new Vector2(12f, -12f), panelSprite, _skin.ChromeColor);
        _topBar = topBar.GetComponent<RectTransform>();
        _topBarImage = topBar.GetComponent<Image>();

        _goldExpText = CreateText(topBar, "GoldExp", new Vector2(10f, -6f), new Vector2(520f, 22f), 15, FontStyle.Bold,
            _skin.AccentColor, TextAnchor.UpperLeft);
        _flowText = CreateText(topBar, "Flow", new Vector2(10f, -28f), new Vector2(520f, 20f), 13, FontStyle.Normal,
            Color.white, TextAnchor.UpperLeft);
        _baseText = CreateText(topBar, "BaseLabel", new Vector2(10f, -48f), new Vector2(520f, 16f), 12, FontStyle.Normal,
            Color.white, TextAnchor.UpperLeft);
        var baseBarBg = CreateImage(topBar, "BaseBarBg", new Vector2(10f, -66f), new Vector2(520f, 10f), barSprite,
            new Color(0.12f, 0.08f, 0.1f, 0.55f));
        _baseFill = CreateImage(baseBarBg.transform, "BaseBarFill", Vector2.zero, Vector2.zero, barSprite, _skin.HpFillColor);
        StretchFill(_baseFill.rectTransform);
        _baseFill.type = Image.Type.Filled;
        _baseFill.fillMethod = Image.FillMethod.Horizontal;

        _towerText = CreateText(topBar, "Towers", new Vector2(10f, -80f), new Vector2(520f, 16f), 12, FontStyle.Normal,
            new Color(1f, 0.9f, 0.4f), TextAnchor.UpperLeft);
        _statusText = CreateText(topBar, "Statuses", new Vector2(10f, -96f), new Vector2(520f, 16f), 12, FontStyle.Normal,
            new Color(0.8f, 0.53f, 1f), TextAnchor.UpperLeft);
        _bossText = CreateText(topBar, "Boss", new Vector2(10f, -112f), new Vector2(520f, 16f), 12, FontStyle.Normal,
            Color.white, TextAnchor.UpperLeft);
        var bossBarBg = CreateImage(topBar, "BossBarBg", new Vector2(10f, -130f), new Vector2(520f, 10f), barSprite,
            new Color(0.08f, 0.12f, 0.1f, 0.55f));
        _bossBarRoot = bossBarBg.gameObject;
        _bossFill = CreateImage(bossBarBg.transform, "BossBarFill", Vector2.zero, Vector2.zero, barSprite,
            new Color(0.22f, 0.72f, 0.58f));
        StretchFill(_bossFill.rectTransform);
        _bossFill.type = Image.Type.Filled;
        _bossFill.fillMethod = Image.FillMethod.Horizontal;
        _bossBarRoot.SetActive(false);

        _playerRowsRoot = CreatePanel("PlayerRows", topBar, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(520f, 110f), new Vector2(10f, -144f), null, Color.clear).GetComponent<RectTransform>();

        _skillHudRoot = CreatePanel("SkillHud", transform, new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(1f, 0f), new Vector2(520f, 220f), new Vector2(-12f, 12f), null, Color.clear).GetComponent<RectTransform>();

        var hintPanel = CreatePanel("ContextHints", transform, new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(0f, 0f), new Vector2(520f, 64f), new Vector2(12f, 12f), panelSprite, _skin.ChromeColor);
        _hintPanelImage = hintPanel.GetComponent<Image>();
        _contextHintText = CreateText(hintPanel, "Hint", new Vector2(10f, -6f), new Vector2(500f, 52f), 12, FontStyle.Normal,
            Color.white, TextAnchor.UpperLeft);
        _contextHintText.supportRichText = false;

        _runFailedOverlay = CreateOverlay("RunFailed", "PRZEGRANA", out _runFailedTitle);
        _runWonOverlay = CreateOverlay("RunWon", "WYGRANA — Warden pokonany", out _, new Color(0.4f, 1f, 0.53f));

        _levelUpOverlay = CreatePanel(
            "LevelUpOverlay",
            transform,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            panelSprite,
            new Color(0.02f, 0.02f, 0.04f, 0.88f)).gameObject;
        StretchFill(_levelUpOverlay.GetComponent<RectTransform>());

        var sheet = CreatePanel(
            "LevelUpSheet",
            _levelUpOverlay.transform,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            panelSprite,
            new Color(0.10f, 0.11f, 0.16f, 0.98f));
        _levelUpContent = sheet.GetComponent<RectTransform>();
        _levelUpContent.offsetMin = new Vector2(72f, 40f);
        _levelUpContent.offsetMax = new Vector2(-72f, -40f);
        var layout = sheet.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 28, 28);
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        _levelUpOverlay.SetActive(false);
        ApplyHudScale();
    }

    private static string BuildLevelUpCacheKey(int teamLevel, IReadOnlyList<BuildFlowController.LevelUpUiPlayer> players)
    {
        var key = teamLevel.ToString();
        foreach (var entry in players)
        {
            key += $"|P{entry.Player?.PlayerIndex}:{entry.IsReady}:{entry.SelectedIndex}";
            if (entry.Options == null) continue;
            for (var i = 0; i < entry.Options.Count; i++)
                key += $":{entry.Options[i]?.TalentId}";
        }

        return key;
    }

    private GameObject CreateOverlay(string name, string title, out Text titleText, Color? titleColor = null)
    {
        var overlay = CreatePanel(name, transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(880f, 150f), Vector2.zero,
            _skin.PanelSprite != null ? _skin.PanelSprite : _whiteSprite, _skin.PanelColor).gameObject;
        titleText = CreateText(overlay.transform, "Title", new Vector2(20f, -22f), new Vector2(840f, 42f), 30, FontStyle.Bold,
            titleColor ?? new Color(1f, 0.4f, 0.4f), TextAnchor.UpperLeft);
        titleText.text = title;
        CreateText(overlay.transform, "Hint", new Vector2(20f, -76f), new Vector2(840f, 30f), 18, FontStyle.Normal,
            Color.white, TextAnchor.UpperLeft).text = "Świat w pauzie. Stop w edytorze kończy run.";
        overlay.SetActive(false);
        return overlay;
    }

    private void EnsurePlayerRowCount(int count)
    {
        while (_playerRows.Count < count)
        {
            var index = _playerRows.Count;
            var rowRoot = CreatePanel($"PlayerRow{index}", _playerRowsRoot, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, 64f), new Vector2(0f, -index * 68f), null, Color.clear);
            var label = CreateText(rowRoot, "Label", new Vector2(0f, 0f), new Vector2(520f, 16f), 13, FontStyle.Normal,
                Color.white, TextAnchor.UpperLeft);
            var hpBg = CreateImage(rowRoot, "HpBg", new Vector2(0f, -18f), new Vector2(520f, 8f), _whiteSprite,
                new Color(0.12f, 0.08f, 0.1f, 0.55f));
            var hpFill = CreateImage(hpBg.transform, "HpFill", Vector2.zero, Vector2.zero, _whiteSprite, _skin.HpFillColor);
            StretchFill(hpFill.rectTransform);
            hpFill.type = Image.Type.Filled;
            hpFill.fillMethod = Image.FillMethod.Horizontal;

            var hartRoot = CreatePanel("Hart", rowRoot, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(520f, 18f), new Vector2(0f, -28f), null, Color.clear).gameObject;
            var hartBg = CreateImage(hartRoot.transform, "HartBg", new Vector2(0f, -2f), new Vector2(520f, 12f), _whiteSprite,
                new Color(0.08f, 0.12f, 0.16f, 0.55f));
            var hartFill = CreateImage(hartBg.transform, "HartFill", Vector2.zero, Vector2.zero, _whiteSprite,
                new Color(0.35f, 0.62f, 0.88f, 1f));
            StretchFill(hartFill.rectTransform);
            hartFill.type = Image.Type.Filled;
            hartFill.fillMethod = Image.FillMethod.Horizontal;
            var hartText = CreateText(hartRoot.transform, "HartLabel", new Vector2(4f, 0f), new Vector2(512f, 18f), 13,
                FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            hartRoot.SetActive(false);

            var kit = CreateText(rowRoot, "Kit", new Vector2(0f, -48f), new Vector2(520f, 14f), 12, FontStyle.Normal,
                new Color(0.85f, 0.9f, 1f), TextAnchor.UpperLeft);

            _playerRows.Add(new PlayerRowUi
            {
                Label = label,
                HpFill = hpFill,
                KitText = kit,
                HartRoot = hartRoot,
                HartFill = hartFill,
                HartText = hartText
            });
        }
    }

    private void EnsureSkillBlockCount(int count)
    {
        while (_skillBlocks.Count < count)
        {
            var playerIndex = _skillBlocks.Count;
            const int maxSlots = PlayerSkillController.SkillSlotCount;
            const float size = 64f;
            const float gap = 8f;
            const float blockWidth = maxSlots * size + (maxSlots - 1) * gap;

            var root = CreatePanel($"SkillBlock{playerIndex}", _skillHudRoot, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(blockWidth, 120f),
                new Vector2(-playerIndex * (blockWidth + 20f), 0f), null, Color.clear).GetComponent<RectTransform>();

            var backgrounds = new Image[maxSlots];
            var icons = new Image[maxSlots];
            var cooldowns = new Image[maxSlots];
            var keys = new Text[maxSlots];
            var labels = new Text[maxSlots];

            for (var slot = 0; slot < maxSlots; slot++)
            {
                var x = slot * (size + gap);
                var slotRoot = CreatePanel($"Slot{slot}", root, new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(size, size), new Vector2(x, -8f),
                    _skin.PanelSprite != null ? _skin.PanelSprite : _whiteSprite, _skin.PanelColor);
                backgrounds[slot] = slotRoot.GetComponent<Image>();
                icons[slot] = CreateImage(slotRoot, "Icon", new Vector2(6f, -6f), new Vector2(size - 12f, size - 12f),
                    _whiteSprite, Color.white);
                cooldowns[slot] = CreateImage(icons[slot].transform, "Cd", Vector2.zero, Vector2.zero, _whiteSprite,
                    new Color(0.08f, 0.08f, 0.1f, 0.85f));
                StretchFill(cooldowns[slot].rectTransform);
                cooldowns[slot].type = Image.Type.Filled;
                cooldowns[slot].fillMethod = Image.FillMethod.Vertical;
                cooldowns[slot].fillOrigin = (int)Image.OriginVertical.Top;
                keys[slot] = CreateText(slotRoot, "Key", new Vector2(2f, -16f), new Vector2(size - 4f, 22f), 14,
                    FontStyle.Bold, Color.white, TextAnchor.UpperCenter);
                labels[slot] = CreateText(slotRoot, "Label", new Vector2(-4f, -size - 2f), new Vector2(size + 56f, 20f),
                    12, FontStyle.Normal, Color.white, TextAnchor.UpperLeft);
            }

            _skillBlocks.Add(new SkillBlockUi
            {
                Root = root,
                SlotBackgrounds = backgrounds,
                SlotIcons = icons,
                SlotCooldowns = cooldowns,
                SlotKeys = keys,
                SlotLabels = labels
            });
        }
    }

    private void AddLevelUpText(string text, int size, FontStyle style, Color color)
    {
        var go = new GameObject("Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(LayoutElement));
        go.transform.SetParent(_levelUpContent, false);
        var label = go.GetComponent<Text>();
        label.font = _font;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = color;
        label.alignment = TextAnchor.UpperLeft;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.text = text;
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = size + 10f;
        layout.preferredHeight = size + 14f;
        layout.flexibleWidth = 1f;
    }

    private void AddLevelUpCard(string text, bool selected)
    {
        var go = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        go.transform.SetParent(_levelUpContent, false);
        var image = go.GetComponent<Image>();
        image.sprite = _skin.PanelSprite != null ? _skin.PanelSprite : _whiteSprite;
        image.color = selected
            ? new Color(0.28f, 0.32f, 0.16f, 1f)
            : new Color(0.16f, 0.18f, 0.24f, 1f);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 156f;
        layout.preferredHeight = 156f;
        layout.flexibleWidth = 1f;

        var labelGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelGo.transform.SetParent(go.transform, false);
        var labelRect = labelGo.GetComponent<RectTransform>();
        StretchFill(labelRect);
        labelRect.offsetMin = new Vector2(18f, 12f);
        labelRect.offsetMax = new Vector2(-18f, -12f);
        var label = labelGo.GetComponent<Text>();
        label.font = _font;
        label.fontSize = 20;
        label.fontStyle = selected ? FontStyle.Bold : FontStyle.Normal;
        label.color = Color.white;
        label.alignment = TextAnchor.UpperLeft;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.text = text;
    }

    private static void ClearChildren(RectTransform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private int GetTotalWaveCount()
    {
        if (gameFlowManager != null && gameFlowManager.TotalWaveCount > 0)
            return gameFlowManager.TotalWaveCount;
        return 10;
    }

    private string GetRunFailedHeadline()
    {
        if (gameFlowManager == null)
            return "PRZEGRANA";

        return gameFlowManager.FailReason switch
        {
            RunFailReason.BaseDestroyed => "PRZEGRANA — Baza upadła",
            RunFailReason.TeamWiped => "PRZEGRANA — Drużyna wybita",
            _ => "PRZEGRANA"
        };
    }

    private static Font ResolveFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
            font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Segoe UI", "Tahoma" }, 16);
        return font;
    }

    private static Sprite CreateWhiteSprite()
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
    }

    private Transform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 size, Vector2 anchoredPos, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        return go.transform;
    }

    private Text CreateText(Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta, int fontSize,
        FontStyle fontStyle, Color color, TextAnchor alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        var text = go.GetComponent<Text>();
        text.font = _font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private Image CreateImage(Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private static void StretchFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void ApplyHudScale()
    {
        if (_scaler == null) return;
        var scale = _skin != null ? _skin.UiScale : HudLayout.DefaultUiScale;
        _scaler.referenceResolution = HudLayout.ScaledReferenceResolution(scale);

        var chromeScale = _skin != null ? _skin.TopBarScale : HudLayout.DefaultTopBarScale;
        if (_topBar != null)
            _topBar.localScale = new Vector3(chromeScale, chromeScale, 1f);
        if (_topBarImage != null && _skin != null)
            _topBarImage.color = _skin.ChromeColor;
        if (_hintPanelImage != null && _skin != null)
            _hintPanelImage.color = _skin.ChromeColor;
    }
}
