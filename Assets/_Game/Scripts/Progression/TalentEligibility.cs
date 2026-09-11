/// <summary>
/// Jedyny silnik eligibility — bez ifów na klasę.
/// </summary>
public static class TalentEligibility
{
    public static bool IsEligible(
        TalentDefinition talent,
        ProgressionSnapshot snapshot,
        IMetaUnlockQuery meta)
    {
        if (talent == null) return false;
        if (talent.PlayerClass != snapshot.ClassId) return false;

        var req = talent.Requirement;
        if (req != null && req.MinLevel > 0 && snapshot.TeamLevel < req.MinLevel)
            return false;

        meta ??= AlwaysUnlockedMetaQuery.Instance;
        var unlockId = req != null ? req.MetaUnlockId : "";
        if (!string.IsNullOrEmpty(unlockId) && !meta.IsUnlocked(unlockId))
            return false;

        if (req == null) return true;

        foreach (var id in req.RequiresTalents)
        {
            if (string.IsNullOrEmpty(id)) continue;
            if (!snapshot.HasTalent(id)) return false;
        }

        if (!string.IsNullOrEmpty(req.RequiresUltimateId)
            && snapshot.ChosenUltimateSkillId != req.RequiresUltimateId)
            return false;

        foreach (var tag in req.RequiresTags)
        {
            if (tag == TalentTag.None) continue;
            if (snapshot.GetTagCount(tag) < 1) return false;
        }

        var anyTags = req.RequiresAnyTags;
        if (anyTags.Length > 0)
        {
            var anyMet = false;
            foreach (var tag in anyTags)
            {
                if (tag == TalentTag.None) continue;
                if (snapshot.GetTagCount(tag) >= 1)
                {
                    anyMet = true;
                    break;
                }
            }

            if (!anyMet) return false;
        }

        foreach (var countReq in req.RequiresTagCounts)
        {
            if (countReq.Tag == TalentTag.None) continue;
            if (snapshot.GetTagCount(countReq.Tag) < countReq.MinCount) return false;
        }

        foreach (var excluded in req.ExcludesTalents)
        {
            if (string.IsNullOrEmpty(excluded)) continue;
            if (snapshot.HasTalent(excluded)) return false;
        }

        return true;
    }
}
