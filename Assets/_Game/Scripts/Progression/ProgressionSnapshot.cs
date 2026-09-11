using System.Collections.Generic;

/// <summary>
/// Stan buildu do eligibility (bez MonoBehaviour).
/// </summary>
public readonly struct ProgressionSnapshot
{
    public ProgressionSnapshot(
        PlayerClassId classId,
        int teamLevel,
        IReadOnlyList<string> chosenTalentIds,
        string chosenUltimateSkillId,
        IReadOnlyDictionary<TalentTag, int> tagCounts)
    {
        ClassId = classId;
        TeamLevel = teamLevel;
        ChosenTalentIds = chosenTalentIds ?? System.Array.Empty<string>();
        ChosenUltimateSkillId = chosenUltimateSkillId ?? "";
        TagCounts = tagCounts;
    }

    public PlayerClassId ClassId { get; }
    public int TeamLevel { get; }
    public IReadOnlyList<string> ChosenTalentIds { get; }
    public string ChosenUltimateSkillId { get; }
    public IReadOnlyDictionary<TalentTag, int> TagCounts { get; }

    public bool HasTalent(string talentId)
    {
        if (string.IsNullOrEmpty(talentId)) return false;
        foreach (var id in ChosenTalentIds)
        {
            if (id == talentId) return true;
        }

        return false;
    }

    public int GetTagCount(TalentTag tag)
    {
        if (TagCounts == null) return 0;
        return TagCounts.TryGetValue(tag, out var count) ? count : 0;
    }
}
