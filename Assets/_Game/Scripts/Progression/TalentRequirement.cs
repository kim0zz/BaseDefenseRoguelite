using System;
using UnityEngine;

/// <summary>
/// Wymagania karty — AND niepustych pól (M8.1).
/// </summary>
[Serializable]
public class TalentRequirement
{
    [SerializeField] private string[] requiresTalents;
    [SerializeField] private string requiresUltimateId;
    [SerializeField] private TalentTag[] requiresTags;
    [SerializeField] private TalentTag[] requiresAnyTags;
    [SerializeField] private TagCountRequirement[] requiresTagCounts;
    [SerializeField] private string[] excludesTalents;
    [SerializeField] private string metaUnlockId;
    [SerializeField] private int minLevel;

    public string[] RequiresTalents => requiresTalents ?? Array.Empty<string>();
    public string RequiresUltimateId => requiresUltimateId ?? "";
    public TalentTag[] RequiresTags => requiresTags ?? Array.Empty<TalentTag>();
    public TalentTag[] RequiresAnyTags => requiresAnyTags ?? Array.Empty<TalentTag>();
    public TagCountRequirement[] RequiresTagCounts => requiresTagCounts ?? Array.Empty<TagCountRequirement>();
    public string[] ExcludesTalents => excludesTalents ?? Array.Empty<string>();
    public string MetaUnlockId => metaUnlockId ?? "";
    public int MinLevel => minLevel;

    public static TalentRequirement Create(
        string[] requiresTalents = null,
        string requiresUltimateId = null,
        TalentTag[] requiresTags = null,
        TalentTag[] requiresAnyTags = null,
        TagCountRequirement[] requiresTagCounts = null,
        string[] excludesTalents = null,
        string metaUnlockId = null,
        int minLevel = 0)
    {
        return new TalentRequirement
        {
            requiresTalents = requiresTalents,
            requiresUltimateId = requiresUltimateId,
            requiresTags = requiresTags,
            requiresAnyTags = requiresAnyTags,
            requiresTagCounts = requiresTagCounts,
            excludesTalents = excludesTalents,
            metaUnlockId = metaUnlockId,
            minLevel = minLevel
        };
    }
}
