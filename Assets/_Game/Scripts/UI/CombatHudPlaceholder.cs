using UnityEngine;

/// <summary>
/// Placeholder HUD M5 — fala, EXP, kasa, baza, przerwa (OnGUI).
/// </summary>
[DisallowMultipleComponent]
public class CombatHudPlaceholder : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private SharedRunState runState;

    private readonly System.Collections.Generic.List<PlayerCharacter> _trackedPlayers = new();
    private string _waveStatus = "Przygotowanie...";
    private Texture2D _panelBackground;
    private Texture2D _hintBackground;

    private void Awake()
    {
        HudCanvasBootstrap.EnsureExists();

        if (waveManager == null)
            waveManager = FindAnyObjectByType<WaveManager>();
        if (gameFlowManager == null)
            gameFlowManager = FindAnyObjectByType<GameFlowManager>();
        if (runState == null)
            runState = FindAnyObjectByType<SharedRunState>();
        if (runState == null)
            runState = SharedRunState.Instance;

        _panelBackground = MakePanelTexture(new Color(0.04f, 0.05f, 0.07f, 0.94f));
        _hintBackground = MakePanelTexture(new Color(0.04f, 0.05f, 0.07f, 0.88f));
    }

    private void OnEnable()
    {
        if (waveManager == null) return;
        waveManager.WaveStarted += OnWaveStarted;
        waveManager.WaveCompleted += OnWaveCompleted;
    }

    private void OnDisable()
    {
        if (waveManager == null) return;
        waveManager.WaveStarted -= OnWaveStarted;
        waveManager.WaveCompleted -= OnWaveCompleted;
    }

    private void Update()
    {
        if (runState == null)
            runState = SharedRunState.Instance;

        _trackedPlayers.Clear();
        _trackedPlayers.AddRange(FindObjectsByType<PlayerCharacter>());
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

    private void OnGUI()
    {
        if (CombatHudView.IsCanvasActive) return;

        DrawWaveHud();
        DrawSkillHud();
        DrawContextHints();
        DrawRunFailedOverlay();
        DrawRunWonOverlay();
    }

    private void DrawWaveHud()
    {
        const float panelWidth = 620f;
        var playerCount = Mathf.Max(1, _trackedPlayers.Count);
        var hasBoss = HasActiveBossHud();
        var panelHeight = 110f + playerCount * 22f + (hasBoss ? 40f : 0f);
        var panelRect = new Rect(8f, 8f, panelWidth, panelHeight);

        GUI.DrawTexture(panelRect, _panelBackground);

        var y = panelRect.y + 8f;
        var compactStyle = MakeLabelStyle(13, FontStyle.Bold);
        var detailStyle = MakeLabelStyle(12, FontStyle.Normal);

        DrawProgressionLine(ref y, panelWidth, compactStyle);
        DrawFlowLine(ref y, panelWidth, detailStyle);
        DrawBaseLine(ref y, panelWidth, detailStyle);
        DrawTowerLine(ref y, panelWidth, detailStyle);
        DrawStatusLine(ref y, panelWidth, detailStyle);
        DrawBossLine(ref y, panelWidth, detailStyle);
        DrawPlayers(ref y, panelWidth, detailStyle);
    }

    private void DrawContextHints()
    {
        var flowState = gameFlowManager != null ? gameFlowManager.State : GameFlowState.WaveActive;
        if (flowState == GameFlowState.WaveActive ||
            flowState == GameFlowState.RunFailed ||
            flowState == GameFlowState.RunWon)
            return;

        var hintStyle = MakeLabelStyle(12, FontStyle.Normal);
        var hintRect = new Rect(8f, Screen.height - 72f, 640f, 64f);
        GUI.DrawTexture(hintRect, _hintBackground);

        var y = hintRect.y + 8f;
        if (flowState == GameFlowState.LevelUpPause)
        {
            GUI.Label(new Rect(hintRect.x + 10f, y, hintRect.width - 20f, 20f),
                "Awans: każdy gracz wybiera talent na swoim urządzeniu, potem Space/Enter = graj dalej.", hintStyle);
            return;
        }

        if (flowState == GameFlowState.RunComplete)
        {
            GUI.Label(new Rect(hintRect.x + 10f, y, hintRect.width - 20f, 20f),
                "Boss 1 pokonany — mid-run complete (placeholder M7).", hintStyle);
            return;
        }

        if (flowState == GameFlowState.Intermission)
        {
            GUI.Label(new Rect(hintRect.x + 10f, y, hintRect.width - 20f, 20f),
                "Przerwa: R = naprawa bazy | Enter/N = następna fala.", hintStyle);
            y += 20f;
            GUI.Label(new Rect(hintRect.x + 10f, y, hintRect.width - 20f, 20f),
                "W walce: cel mysz/stick | atak Space/LMB/West | skille Q/E/R lub East/North/LB.", hintStyle);
        }
    }

    private void DrawRunFailedOverlay()
    {
        if (gameFlowManager == null || gameFlowManager.State != GameFlowState.RunFailed)
            return;

        var panelWidth = Mathf.Min(720f, Screen.width - 32f);
        var panelHeight = 120f;
        var panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        GUI.DrawTexture(panelRect, _panelBackground);

        var titleStyle = MakeLabelStyle(24, FontStyle.Bold);
        var hintStyle = MakeLabelStyle(14, FontStyle.Normal);
        var headline = GetRunFailedHeadline();

        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 18f, panelRect.width - 32f, 34f),
            $"<color=#ff6666><b>{headline}</b></color>", titleStyle);
        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 62f, panelRect.width - 32f, 24f),
            "Świat w pauzie. Stop w edytorze kończy run.", hintStyle);
    }

    private void DrawRunWonOverlay()
    {
        if (gameFlowManager == null || gameFlowManager.State != GameFlowState.RunWon)
            return;

        var panelWidth = Mathf.Min(720f, Screen.width - 32f);
        var panelHeight = 120f;
        var panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        GUI.DrawTexture(panelRect, _panelBackground);

        var titleStyle = MakeLabelStyle(24, FontStyle.Bold);
        var hintStyle = MakeLabelStyle(14, FontStyle.Normal);

        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 18f, panelRect.width - 32f, 34f),
            "<color=#66ff88><b>WYGRANA — Warden pokonany</b></color>", titleStyle);
        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 62f, panelRect.width - 32f, 24f),
            "Świat w pauzie. Stop w edytorze kończy run.", hintStyle);
    }

    private static bool HasActiveBossHud()
    {
        var warden = BossWardenController.Active;
        if (warden != null && warden.Definition != null)
            return true;

        var ram = BossRamController.Active;
        return ram != null && ram.Definition != null;
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

    private static GUIStyle MakeLabelStyle(int fontSize, FontStyle fontStyle)
    {
        return new GUIStyle(GUI.skin.label)
        {
            richText = true,
            fontSize = fontSize,
            fontStyle = fontStyle,
            normal = { textColor = Color.white }
        };
    }

    private void DrawProgressionLine(ref float y, float panelWidth, GUIStyle style)
    {
        if (runState == null)
        {
            GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
                "<color=#ff8888>BRAK SharedRunState</color>", style);
            y += 22f;
            return;
        }

        var expToNext = runState.GetExpToNextLevel();
        var expLine = expToNext > 0
            ? $"EXP {runState.SharedExp} → lvl {runState.TeamLevel + 1}: {expToNext}"
            : $"EXP {runState.SharedExp} (max lvl {runState.TeamLevel})";
        GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
            $"<color=#ffdd66>{runState.Gold} zł</color>  |  lvl {runState.TeamLevel}  |  {expLine}",
            style);
        y += 22f;
    }

    private void DrawFlowLine(ref float y, float panelWidth, GUIStyle waveStyle)
    {
        var flowState = gameFlowManager != null ? gameFlowManager.State : GameFlowState.WaveActive;

        if (flowState == GameFlowState.LevelUpPause)
        {
            GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
                $"<color=#ffff88><b>AWANS (poz. {runState?.TeamLevel})</b></color>", waveStyle);
            y += 22f;
            return;
        }

        if (flowState == GameFlowState.Intermission)
        {
            GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
                $"<color=#aaffaa>{_waveStatus}</color>  |  <b>PRZERWA</b>", waveStyle);
            y += 22f;
            return;
        }

        if (flowState == GameFlowState.RunComplete)
        {
            GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
                "<color=#88ff88>Boss 1 pokonany — mid-run complete</color>", waveStyle);
            y += 22f;
            return;
        }

        if (flowState == GameFlowState.RunFailed)
        {
            GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
                $"<color=#ff6666><b>{GetRunFailedHeadline()}</b></color>", waveStyle);
            y += 22f;
            return;
        }

        if (flowState == GameFlowState.RunWon)
        {
            GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
                "<color=#66ff88><b>WYGRANA — Warden pokonany</b></color>", waveStyle);
            y += 22f;
            return;
        }

        if (waveManager != null && waveManager.State == WaveState.Active)
        {
            var timerColor = waveManager.TimeRemaining <= 15f ? "#ffaa44" : "#aaccff";
            var waveNum = waveManager.CurrentWave != null ? waveManager.CurrentWave.WaveNumber : 0;
            var totalWaves = GetTotalWaveCount();
            GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
                $"Fala {waveNum}/{totalWaves}  |  {_waveStatus}  |  <color={timerColor}>{waveManager.TimeRemaining:0}s</color>  |  " +
                $"wrogowie {waveManager.AliveEnemyCount}/{waveManager.TotalSpawned}",
                waveStyle);
        }
        else
        {
            GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f), _waveStatus, waveStyle);
        }

        y += 22f;
    }

    private void DrawBaseLine(ref float y, float panelWidth, GUIStyle style)
    {
        var baseHealth = FindAnyObjectByType<BaseHealth>();
        if (baseHealth == null) return;

        var pct = baseHealth.MaxHealth > 0f ? baseHealth.CurrentHealth / baseHealth.MaxHealth : 0f;
        var color = pct <= 0.25f ? "red" : pct <= 0.5f ? "yellow" : "#88ccff";
        GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
            $"Baza <color={color}>{baseHealth.CurrentHealth:0}/{baseHealth.MaxHealth:0}</color>",
            style);
        y += 22f;
    }

    private void DrawTowerLine(ref float y, float panelWidth, GUIStyle style)
    {
        var lowCount = 0;
        var destroyed = 0;
        foreach (var tower in TowerRegistry.All)
        {
            if (tower == null) continue;
            if (tower.IsDestroyed) destroyed++;
            else if (tower.MaxHealth > 0f && tower.CurrentHealth / tower.MaxHealth <= 0.35f) lowCount++;
        }

        if (lowCount == 0 && destroyed == 0) return;

        var color = destroyed > 0 ? "red" : "yellow";
        GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
            $"Wieże: <color={color}>niskie HP {lowCount} | zniszczone {destroyed}</color>", style);
        y += 22f;
    }

    private void DrawStatusLine(ref float y, float panelWidth, GUIStyle style)
    {
        var parts = new System.Collections.Generic.List<string>();
        foreach (var player in _trackedPlayers)
        {
            if (player == null) continue;
            var status = player.GetComponent<StatusEffectReceiver>();
            if (status == null) continue;
            foreach (var type in status.GetActiveTypesForHud())
                parts.Add($"P{player.PlayerIndex + 1}:{type}");
        }

        if (parts.Count == 0) return;
        GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f),
            $"Statusy: <color=#cc88ff>{string.Join(", ", parts)}</color>", style);
        y += 22f;
    }

    private void DrawBossLine(ref float y, float panelWidth, GUIStyle style)
    {
        var warden = BossWardenController.Active;
        if (warden != null && warden.Definition != null)
        {
            DrawWardenBossLine(ref y, panelWidth, style, warden);
            return;
        }

        var boss = BossRamController.Active;
        if (boss == null || boss.Definition == null) return;

        var hpPct = Mathf.Clamp01(boss.HealthRatio);
        var hpColor = hpPct <= 0.5f ? "#ff6644" : "#cc88ff";
        var phase = boss.State is BossRamState.TelegraphCharge or BossRamState.Charge
            ? "Szarża"
            : boss.IsEnraged ? "Enrage" : "Walka";
        var health = boss.GetComponent<Health>();
        var hpLabel = health != null
            ? $"{health.CurrentHealth:0}/{health.MaxHealth:0}"
            : $"{(hpPct * 100f):0}%";

        GUI.Label(new Rect(16f, y, panelWidth - 24f, 18f),
            $"BOSS {boss.Definition.DisplayName}  |  {phase}  |  <color={hpColor}>{hpLabel}</color>",
            style);
        y += 18f;

        var barRect = new Rect(16f, y, panelWidth - 24f, 14f);
        var prev = GUI.color;
        GUI.color = new Color(0.12f, 0.08f, 0.1f, 0.95f);
        GUI.DrawTexture(barRect, Texture2D.whiteTexture);
        GUI.color = hpPct <= 0.5f ? new Color(1f, 0.4f, 0.22f, 1f) : new Color(0.72f, 0.38f, 0.95f, 1f);
        GUI.DrawTexture(new Rect(barRect.x, barRect.y, barRect.width * Mathf.Max(0.02f, hpPct), barRect.height),
            Texture2D.whiteTexture);
        GUI.color = prev;
        y += 20f;
    }

    private static void DrawWardenBossLine(ref float y, float panelWidth, GUIStyle style, BossWardenController warden)
    {
        var hpPct = Mathf.Clamp01(warden.HealthRatio);
        var hpColor = hpPct <= 0.5f ? "#ff6644" : "#44ccaa";
        var phase = BossWardenLogic.GetPhaseHudLabel(warden.CurrentPhase);
        var health = warden.GetComponent<Health>();
        var hpLabel = health != null
            ? $"{health.CurrentHealth:0}/{health.MaxHealth:0}"
            : $"{(hpPct * 100f):0}%";

        GUI.Label(new Rect(16f, y, panelWidth - 24f, 18f),
            $"BOSS {warden.Definition.DisplayName}  |  {phase}  |  <color={hpColor}>{hpLabel}</color>",
            style);
        y += 18f;

        var barRect = new Rect(16f, y, panelWidth - 24f, 14f);
        var prev = GUI.color;
        GUI.color = new Color(0.08f, 0.12f, 0.1f, 0.95f);
        GUI.DrawTexture(barRect, Texture2D.whiteTexture);
        GUI.color = hpPct <= 0.5f ? new Color(1f, 0.4f, 0.22f, 1f) : new Color(0.22f, 0.72f, 0.58f, 1f);
        GUI.DrawTexture(new Rect(barRect.x, barRect.y, barRect.width * Mathf.Max(0.02f, hpPct), barRect.height),
            Texture2D.whiteTexture);
        GUI.color = prev;
        y += 20f;
    }

    private void DrawPlayers(ref float y, float panelWidth, GUIStyle style)
    {
        foreach (var player in _trackedPlayers)
        {
            if (player == null) continue;

            var health = player.GetComponent<Health>();
            var respawn = player.GetComponent<PlayerRespawn>();
            if (health == null) continue;

            var hpPct = health.MaxHealth > 0f ? health.CurrentHealth / health.MaxHealth : 0f;
            var hpColor = hpPct <= 0.25f ? "red" : hpPct <= 0.5f ? "yellow" : "white";
            var build = player.GetComponent<PlayerBuildState>();
            var classLabel = build?.ClassDefinition != null ? build.ClassDefinition.DisplayName : "?";
            var stats = build != null ? build.GetEffectiveStats() : default;
            var power = LootLoopConfig.LootEnabledInRun
                ? CombatHudCopy.FormatWeaponPower(
                    build?.GetWeapon(build.ActiveWeaponIndex)?.DisplayName ?? "-",
                    stats.Damage,
                    stats.AttackInterval)
                : CombatHudCopy.FormatBasicAttackPower(stats.Damage, stats.AttackInterval);

            var persistents = player.GetComponent<PlayerPersistentEffects>();
            var extras = FormatPersistentHud(player, persistents);
            var attack = player.GetComponent<PlayerAttackController>();
            var comboLabel = attack != null && attack.ComboStep > 0 ? $"  |  AA {attack.ComboStep}/3" : "";

            var label = $"P{player.PlayerIndex + 1} {classLabel}  " +
                        $"HP <color={hpColor}>{health.CurrentHealth:0}/{health.MaxHealth:0}</color>  |  " +
                        power + comboLabel + extras;

            if (respawn != null && respawn.IsRespawning)
                label += $"  |  resp {respawn.RespawnTimeRemaining:0.0}s";

            if (!player.IsCombatEnabled && (respawn == null || !respawn.IsRespawning))
                label += "  |  <color=red>MARTWY</color>";

            GUI.Label(new Rect(16f, y, panelWidth - 24f, 20f), label, style);
            y += 22f;
        }
    }

    private void DrawSkillHud()
    {
        var playerIndex = 0;
        foreach (var player in _trackedPlayers)
        {
            if (player == null) continue;
            var skills = player.GetComponent<PlayerSkillController>();
            if (skills == null) continue;

            const int maxSlots = PlayerSkillController.SkillSlotCount;
            var slotCount = skills.GetSkill(PlayerSkillController.UltimateSlot) != null
                ? maxSlots
                : SkillLoadout.ActiveSlotCount;
            const float size = 48f;
            const float gap = 6f;
            var blockWidth = slotCount * size + (slotCount - 1) * gap;
            var blockX = Screen.width - blockWidth - 16f - playerIndex * (blockWidth + 16f);
            var y = Screen.height - size - 40f;
            var isKeyboard = player.InputMode == PlayerInputMode.KeyboardMouse;
            var labelStyle = MakeLabelStyle(10, FontStyle.Bold);
            var nameStyle = MakeLabelStyle(9, FontStyle.Normal);

            for (var slot = 0; slot < slotCount; slot++)
            {
                var skill = skills.GetSkill(slot);
                var x = blockX + slot * (size + gap);
                var rect = new Rect(x, y, size, size);

                GUI.DrawTexture(rect, _panelBackground);
                DrawSkillRadial(rect, skills.GetCooldownNormalized(slot), skill != null);

                var passive = skill != null && skill.ActivationMode == SkillActivationMode.Passive;
                var keyHint = CombatHudCopy.GetSkillSlotKeyHint(slot, isKeyboard, passive);
                GUI.Label(new Rect(rect.x + 2f, rect.y + 14f, rect.width - 4f, 18f), keyHint, labelStyle);

                if (skill != null)
                {
                    var cdLabel = passive
                        ? $"{skill.DisplayName} (pasyw)"
                        : CombatHudCopy.FormatSkillCooldown(
                            skill.DisplayName,
                            skills.GetCooldownRemaining(slot),
                            skill.CooldownSeconds);
                    GUI.Label(new Rect(rect.x - 4f, rect.y + rect.height + 2f, rect.width + 48f, 16f),
                        cdLabel, nameStyle);
                }
            }

            playerIndex++;
        }
    }

    private static void DrawSkillRadial(Rect rect, float cooldownNormalized, bool hasSkill)
    {
        var ready = cooldownNormalized <= 0f;
        var prev = GUI.color;
        if (!hasSkill)
            GUI.color = new Color(0.2f, 0.2f, 0.24f, 0.7f);
        else
            GUI.color = ready ? new Color(0.3f, 0.85f, 0.45f, 0.95f) : new Color(0.85f, 0.35f, 0.25f, 0.95f);

        var inner = new Rect(rect.x + 6f, rect.y + 6f, rect.width - 12f, rect.height - 12f);
        GUI.DrawTexture(inner, Texture2D.whiteTexture);

        if (hasSkill && !ready)
        {
            GUI.color = new Color(0.08f, 0.08f, 0.1f, 0.85f);
            var coverHeight = inner.height * Mathf.Clamp01(cooldownNormalized);
            GUI.DrawTexture(new Rect(inner.x, inner.y, inner.width, coverHeight), Texture2D.whiteTexture);
        }

        GUI.color = prev;
    }

    private static string FormatPersistentHud(PlayerCharacter player, PlayerPersistentEffects persistents)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (persistents != null)
        {
            if (persistents.Has(PersistentEffectKind.FuryMeter))
                parts.Add($"Furia {persistents.FuryMeter:0}/{persistents.FuryThreshold:0}" +
                          (persistents.FuryActive ? " <color=#ff8844>AKTYWNA</color>" : ""));
            if (persistents.Has(PersistentEffectKind.HartStacks))
                parts.Add($"Hart {persistents.HartStacks}/{persistents.HartMaxStacks}" +
                          (persistents.HartReady ? " <color=#88ccff>READY</color>" : ""));
            if (persistents.LastChanceActive)
                parts.Add($"<color=#ff6666>OSTATNIA SZANSA</color> heal {persistents.LastChanceHealAccum:0}/{persistents.LastChanceSurviveThreshold:0}");
            if (persistents.KolosActive)
                parts.Add($"<color=#cc88ff>KOLOS {persistents.KolosRemaining:0.0}s</color>");
            if (CombatHudCopy.TryFormatAttackIntervalOverride(persistents, out var rapidLabel))
                parts.Add(rapidLabel);
        }

        if (player != null)
        {
            if (DeployableHudRead.TryGetBombLine(player.gameObject, out var bombLine))
                parts.Add(bombLine);
            if (DeployableHudRead.TryGetOrbitalLine(player.gameObject, out var orbitalLine))
                parts.Add(orbitalLine);
        }

        return parts.Count == 0 ? "" : "  |  " + string.Join("  ", parts);
    }

    private static Texture2D MakePanelTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
