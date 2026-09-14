using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Standalone Siege reward UI; caller owns combat pause and wave transitions.</summary>
[DisallowMultipleComponent]
public sealed class SiegeRewardController : MonoBehaviour
{
    private sealed class Option
    {
        public string Title;
        public string Description;
        public TalentDefinition Talent;
        public int Slot;
        public SiegeUpgradeKind Kind;
        public int TalentLevel;
    }
    private sealed class Choice
    {
        public PlayerCharacter Player;
        public readonly List<Option> Options = new();
        public int Selected;
        public bool Ready;
        public int TalentLevel;
    }
    private readonly List<Choice> _choices = new();
    private readonly HashSet<int> _claimedWaves = new();
    private int _talentLevel;
    private int _openedFrame;
    private bool _large;
    private int _rewardWave;
    private BuildContentCatalog _catalog;
    public bool IsChoosing { get; private set; }
    public bool SelectionsComplete => !IsChoosing;
    public int PendingChoiceCount
    {
        get
        {
            var count = 0;
            foreach (var choice in _choices) if (!choice.Ready) count++;
            return count;
        }
    }
    public int ChoiceCount => _choices.Count;
    public bool HasChoices => _choices.Exists(choice => choice.Options.Count > 0);
    public event Action Completed;

    public static bool IsLargeReward(int wave) => wave == 0 || wave == 2 || wave == 4;
    public static int TalentLevelForWave(int wave) => 2 + wave / 2;

    public void BeginReward(int completedWave)
    {
        if (IsChoosing || completedWave < 0 || completedWave > 4 || !_claimedWaves.Add(completedWave)) return;
        _large = IsLargeReward(completedWave);
        _rewardWave = completedWave;
        _talentLevel = TalentLevelForWave(completedWave);
        _choices.Clear();
        var players = FindObjectsByType<PlayerCharacter>();
        Array.Sort(players, (a, b) => a.PlayerIndex.CompareTo(b.PlayerIndex));
        _catalog = BuildSystemBootstrap.GetCatalogOrDefault();
        foreach (var player in players)
            _choices.Add(BuildChoice(player));
        _openedFrame = Time.frameCount;
        IsChoosing = true;
        EvaluateComplete();
    }

    private void Update()
    {
        if (!IsChoosing || Time.frameCount <= _openedFrame + 1) return;
        foreach (var player in FindObjectsByType<PlayerCharacter>())
        {
            var known = false;
            foreach (var choice in _choices) if (choice.Player == player) { known = true; break; }
            if (!known) _choices.Add(BuildChoice(player));
        }
        foreach (var choice in _choices)
        {
            if (choice.Ready) continue;
            if (choice.Player == null) { choice.Ready = true; continue; }
            choice.Player.TryReadLevelUpOptionInput(choice.Options.Count, ref choice.Selected);
            choice.Selected = Mathf.Clamp(choice.Selected, 0, choice.Options.Count - 1);
            if (choice.Player.TryReadLevelUpConfirmInput()) Confirm(choice);
        }
        EvaluateComplete();
    }

    private Choice BuildChoice(PlayerCharacter player)
    {
        var choice = new Choice { Player = player };
        var build = player != null ? player.GetComponent<PlayerBuildState>() : null;
        if (_large && build != null)
        {
            choice.TalentLevel = NextTalentLevel(build, _talentLevel);
            if (choice.TalentLevel > 0)
            {
                build.SetTeamLevel(choice.TalentLevel);
                foreach (var talent in build.GetAvailableTalents(_catalog, choice.TalentLevel))
                {
                    choice.Options.Add(new Option { Title = talent.DisplayName, Description = ShortDescription(talent.Description), Talent = talent, TalentLevel = choice.TalentLevel });
                    if (choice.Options.Count == 3) break;
                }
            }
        }
        else if (!_large && player != null)
        {
            var upgrades = player.GetComponent<SiegeSkillUpgrades>() ?? player.gameObject.AddComponent<SiegeSkillUpgrades>();
            var skills = player.GetComponent<PlayerSkillController>();
            for (var offset = 0; offset < SkillLoadout.SlotCount && choice.Options.Count < 3; offset++)
            {
                var slot = (offset + (_rewardWave == 3 ? 1 : 0)) % SkillLoadout.SlotCount;
                var skill = skills != null ? skills.GetSkill(slot) : null;
                for (var k = 0; k < 3; k++)
                {
                    var kind = (SiegeUpgradeKind)((k + offset) % 3);
                    if (!upgrades.CanApply(slot, kind, skill)) continue;
                    choice.Options.Add(new Option { Title = skill.DisplayName, Description = upgrades.Describe(kind), Slot = slot, Kind = kind });
                    break;
                }
            }
        }
        choice.Ready = choice.Options.Count == 0;
        return choice;
    }

    private static string ShortDescription(string description)
    {
        if (string.IsNullOrEmpty(description)) return "";
        return description.Length > 96 ? description.Substring(0, 93) + "…" : description;
    }

    public static int NextTalentLevel(PlayerBuildState build, int targetLevel)
    {
        if (build == null) return 0;
        for (var level = 2; level <= targetLevel; level++)
            if (!build.HasChosenTalentForLevel(level)) return level;
        return 0;
    }

    private void Confirm(Choice choice)
    {
        if (choice.Ready || choice.Player == null) return;
        var option = choice.Options[choice.Selected];
        if (_large)
        {
            var build = choice.Player.GetComponent<PlayerBuildState>();
            build.SetTeamLevel(option.TalentLevel);
            if (build.TryApplyTalent(option.Talent, option.TalentLevel))
            {
                var next = BuildChoice(choice.Player);
                choice.Options.Clear();
                choice.Options.AddRange(next.Options);
                choice.Selected = 0;
                choice.TalentLevel = next.TalentLevel;
                choice.Ready = next.Ready;
            }
        }
        else
            choice.Ready = choice.Player.GetComponent<SiegeSkillUpgrades>().TryApply(option.Slot, option.Kind);
        // Finish immediately after the final player's choice. This keeps the
        // reward flow deterministic while gameplay time is paused.
        EvaluateComplete();
    }

    private void EvaluateComplete()
    {
        foreach (var choice in _choices) if (!choice.Ready) return;
        IsChoosing = false;
        Completed?.Invoke();
    }

    private void OnGUI()
    {
        if (!IsChoosing) return;
        var oldMatrix = GUI.matrix;
        var oldColor = GUI.color;
        var oldDepth = GUI.depth;
        GUI.depth = -200;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1600f, Screen.height / 900f, 1));
        Fill(new Rect(0, 0, 1600, 900), new Color(0.015f, 0.025f, 0.05f, 0.96f));
        var title = new GUIStyle(GUI.skin.label) { fontSize = 38, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        title.normal.textColor = new Color(1f, 0.79f, 0.32f);
        GUI.Label(new Rect(80, 35, 1440, 55), _large ? "PRZEŁOM W BUILDZIE" : "WZMOCNIENIE UMIEJĘTNOŚCI", title);
        var subtitle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        subtitle.normal.textColor = new Color(0.75f, 0.83f, 0.9f);
        GUI.Label(new Rect(80, 90, 1440, 35), "Każdy obrońca wybiera własne ulepszenie. Oblężenie czeka na drużynę.", subtitle);
        var count = Mathf.Max(1, _choices.Count);
        var width = Mathf.Min(640, 1450f / count);
        for (var p = 0; p < _choices.Count; p++)
        {
            var choice = _choices[p];
            var x = (1600 - width * count) / 2 + p * width;
            var build = choice.Player != null ? choice.Player.GetComponent<PlayerBuildState>() : null;
            GUI.Label(new Rect(x, 150, width, 40), $"P{(choice.Player != null ? choice.Player.PlayerIndex + 1 : p + 1)} · {build?.ClassDefinition?.DisplayName}", subtitle);
            for (var o = 0; o < choice.Options.Count; o++)
            {
                var rect = new Rect(x + 10, 210 + o * 166, width - 20, 150);
                var selected = choice.Selected == o;
                Fill(rect, choice.Ready ? new Color(0.08f, 0.16f, 0.16f) : selected ? new Color(0.20f, 0.23f, 0.31f) : new Color(0.07f, 0.10f, 0.16f));
                if (selected) Fill(new Rect(rect.x, rect.y, 4, rect.height), new Color(1f, 0.79f, 0.32f));
                var nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, wordWrap = true };
                var bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
                nameStyle.normal.textColor = Color.white;
                bodyStyle.normal.textColor = new Color(0.75f, 0.83f, 0.9f);
                GUI.Label(new Rect(rect.x + 15, rect.y + 12, rect.width - 30, 55), $"{o + 1}. {choice.Options[o].Title}", nameStyle);
                GUI.Label(new Rect(rect.x + 15, rect.y + 67, rect.width - 30, 76), choice.Options[o].Description, bodyStyle);
                if (!choice.Ready && choice.Player != null
                    && choice.Player.PlayerIndex == 0
                    && choice.Player.InputMode == PlayerInputMode.KeyboardMouse
                    && GUI.Button(rect, GUIContent.none, GUIStyle.none)) { choice.Selected = o; Confirm(choice); }
            }
            GUI.Label(new Rect(x, 730, width, 50), choice.Ready ? "GOTOWY ✓" : "1–3 / ◀ ▶ · Enter / A", subtitle);
        }
        GUI.Label(new Rect(80, 825, 1440, 35), "Wspólna nagroda · indywidualny build", subtitle);
        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
        GUI.depth = oldDepth;
    }

    private static void Fill(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
