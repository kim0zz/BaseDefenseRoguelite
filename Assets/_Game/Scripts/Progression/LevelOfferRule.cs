using System;
using UnityEngine;

/// <summary>
/// Receptura oferty na jednym poziomie.
/// </summary>
public enum OfferRecipeKind
{
    OneMutationPerActive,
    MixIndependentAndFollowup,
    AllEligibleGrantUltimate,
    AllEligible
}

[Serializable]
public class LevelOfferRule
{
    [SerializeField] private int level = 2;
    [SerializeField] private OfferRecipeKind recipe = OfferRecipeKind.AllEligible;
    [SerializeField] private int minCount = 1;
    [SerializeField] private int maxCount = 4;
    [SerializeField] private string independentGroup = "core";
    [SerializeField] private int followupFromLevel = 2;

    public int Level => level;
    public OfferRecipeKind Recipe => recipe;
    public int MinCount => minCount;
    public int MaxCount => maxCount;
    public string IndependentGroup => independentGroup ?? "core";
    public int FollowupFromLevel => followupFromLevel;

    public static LevelOfferRule Create(
        int level,
        OfferRecipeKind recipe,
        int minCount,
        int maxCount,
        string independentGroup = "core",
        int followupFromLevel = 2)
    {
        return new LevelOfferRule
        {
            level = level,
            recipe = recipe,
            minCount = minCount,
            maxCount = maxCount,
            independentGroup = independentGroup,
            followupFromLevel = followupFromLevel
        };
    }
}
