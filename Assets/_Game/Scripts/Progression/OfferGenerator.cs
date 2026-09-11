using System.Collections.Generic;

/// <summary>
/// Buduje ofertę level-upu z receptury klasy + eligibility.
/// </summary>
public static class OfferGenerator
{
    public const string GroupMutation = "mutation";
    public const string GroupCore = "core";
    public const string GroupFollowup = "followup";
    public const string GroupUltimate = "ultimate";
    public const string GroupCapstone = "capstone";

    public static IReadOnlyList<TalentDefinition> Build(
        ClassProgressionTable table,
        IReadOnlyList<TalentDefinition> catalog,
        ProgressionSnapshot snapshot,
        IMetaUnlockQuery meta)
    {
        var result = new List<TalentDefinition>();
        if (table == null || catalog == null) return result;

        var rule = table.GetRule(snapshot.TeamLevel);
        if (rule == null) return result;

        meta ??= AlwaysUnlockedMetaQuery.Instance;

        switch (rule.Recipe)
        {
            case OfferRecipeKind.OneMutationPerActive:
                CollectOneMutationPerActive(result, table, catalog, snapshot, meta);
                break;
            case OfferRecipeKind.MixIndependentAndFollowup:
                CollectMix(result, catalog, snapshot, meta, rule);
                break;
            case OfferRecipeKind.AllEligibleGrantUltimate:
                CollectAllEligible(result, catalog, snapshot, meta, OfferGenerator.GroupUltimate,
                    TalentEffectOp.GrantSkill);
                break;
            default:
                CollectAllEligible(result, catalog, snapshot, meta, OfferGenerator.GroupCapstone, null);
                break;
        }

        return result;
    }

    private static void CollectOneMutationPerActive(
        List<TalentDefinition> result,
        ClassProgressionTable table,
        IReadOnlyList<TalentDefinition> catalog,
        ProgressionSnapshot snapshot,
        IMetaUnlockQuery meta)
    {
        foreach (var skillId in table.StartingActiveSkillIds)
        {
            if (string.IsNullOrEmpty(skillId)) continue;
            TalentDefinition match = null;
            foreach (var talent in catalog)
            {
                if (!IsLevelCard(talent, snapshot, meta)) continue;
                if (talent.Effect == null || talent.Effect.OfferGroup != GroupMutation) continue;
                if (talent.Effect.TargetSkillId != skillId) continue;
                match = talent;
                break;
            }

            if (match != null)
                result.Add(match);
        }
    }

    private static void CollectMix(
        List<TalentDefinition> result,
        IReadOnlyList<TalentDefinition> catalog,
        ProgressionSnapshot snapshot,
        IMetaUnlockQuery meta,
        LevelOfferRule rule)
    {
        foreach (var talent in catalog)
        {
            if (!IsLevelCard(talent, snapshot, meta)) continue;
            if (talent.Effect == null || talent.Effect.OfferGroup != rule.IndependentGroup) continue;
            result.Add(talent);
        }

            foreach (var talent in catalog)
        {
            if (!IsLevelCard(talent, snapshot, meta)) continue;
            if (talent.Effect == null || talent.Effect.OfferGroup != GroupFollowup) continue;
            result.Add(talent);
        }
    }

    private static void CollectAllEligible(
        List<TalentDefinition> result,
        IReadOnlyList<TalentDefinition> catalog,
        ProgressionSnapshot snapshot,
        IMetaUnlockQuery meta,
        string offerGroup,
        TalentEffectOp? requiredOp)
    {
        foreach (var talent in catalog)
        {
            if (!IsLevelCard(talent, snapshot, meta)) continue;
            if (talent.Effect == null || talent.Effect.OfferGroup != offerGroup) continue;
            if (requiredOp.HasValue && talent.Effect.Operation != requiredOp.Value) continue;
            result.Add(talent);
        }
    }

    private static bool IsLevelCard(
        TalentDefinition talent,
        ProgressionSnapshot snapshot,
        IMetaUnlockQuery meta)
    {
        if (talent == null) return false;
        if (talent.RequiredTeamLevel != snapshot.TeamLevel) return false;
        return TalentEligibility.IsEligible(talent, snapshot, meta);
    }
}
