using UnityEngine;

/// <summary>
/// Receptury oferty level-upu per klasa (nie UI).
/// </summary>
[CreateAssetMenu(fileName = "ClassProgressionTable", menuName = "Game/Progression/Class Progression Table")]
public class ClassProgressionTable : ScriptableObject
{
    [SerializeField] private PlayerClassId classId;
    [SerializeField] private string[] startingActiveSkillIds;
    [SerializeField] private LevelOfferRule[] levels;

    public PlayerClassId ClassId => classId;
    public string[] StartingActiveSkillIds => startingActiveSkillIds ?? System.Array.Empty<string>();
    public LevelOfferRule[] Levels => levels ?? System.Array.Empty<LevelOfferRule>();

    public LevelOfferRule GetRule(int teamLevel)
    {
        if (levels == null) return null;
        foreach (var rule in levels)
        {
            if (rule != null && rule.Level == teamLevel)
                return rule;
        }

        return null;
    }

    public void InitializeRuntime(PlayerClassId id, string[] skillIds, LevelOfferRule[] rules)
    {
        classId = id;
        startingActiveSkillIds = skillIds;
        levels = rules;
    }
}
