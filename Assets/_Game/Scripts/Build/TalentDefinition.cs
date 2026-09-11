using UnityEngine;

/// <summary>
/// Definicja talentu — karta oferty (M8.1).
/// </summary>
[CreateAssetMenu(fileName = "TalentDefinition", menuName = "Game/Build/Talent Definition")]
public class TalentDefinition : ScriptableObject
{
    [SerializeField] private string talentId = "talent";
    [SerializeField] private string displayName = "Talent";
    [SerializeField] private PlayerClassId playerClass = PlayerClassId.Pudzian;
    [SerializeField] private int requiredTeamLevel = 2;
    [SerializeField] private TalentTag[] tags;
    [SerializeField] private TalentRequirement requirement;
    [SerializeField] private TalentEffect effect;
    [SerializeField] [TextArea] private string description = "";
    [SerializeField] private Sprite icon;

    [Header("Legacy M6 (nie używać w nowych kartach)")]
    [SerializeField] private string branchKey = "";
    [SerializeField] private string requiredParentBranch = "";
    [SerializeField] private StatModifiers statModifiers;
    [SerializeField] private SkillModifierId skillModifier = SkillModifierId.None;

    public string TalentId => talentId;
    public string DisplayName => displayName;
    public PlayerClassId PlayerClass => playerClass;
    public int RequiredTeamLevel => requiredTeamLevel;
    public TalentTag[] Tags => tags ?? System.Array.Empty<TalentTag>();
    public TalentRequirement Requirement => requirement;
    public TalentEffect Effect => effect;
    public string Description => description;
    public Sprite Icon => icon;
    public string BranchKey => branchKey;
    public string RequiredParentBranch => requiredParentBranch;
    public StatModifiers StatModifiers => statModifiers;
    public SkillModifierId SkillModifier => skillModifier;

    public void ConfigureRuntime(
        string id,
        string name,
        string desc,
        PlayerClassId classId,
        int level,
        TalentTag[] cardTags,
        TalentRequirement req,
        TalentEffect fx)
    {
        talentId = id;
        displayName = name;
        description = desc;
        playerClass = classId;
        requiredTeamLevel = level;
        tags = cardTags;
        requirement = req;
        effect = fx;
        statModifiers = StatModifiers.Identity;
        skillModifier = SkillModifierId.None;
        branchKey = "";
        requiredParentBranch = "";
    }
}
