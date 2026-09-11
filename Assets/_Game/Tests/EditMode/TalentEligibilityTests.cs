using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TalentEligibilityTests
{
    [Test]
    public void Talent_HiddenWithoutRequiredTag()
    {
        var talent = Capstone("needs_heal", TalentRequirement.Create(requiresTags: new[] { TalentTag.Heal }));
        var snapshot = Snapshot(tags: new Dictionary<TalentTag, int> { { TalentTag.Aoe, 2 } });

        Assert.IsFalse(TalentEligibility.IsEligible(talent, snapshot, AlwaysUnlockedMetaQuery.Instance));
    }

    [Test]
    public void Talent_ShownWhenTagPresent()
    {
        var talent = Capstone("needs_heal", TalentRequirement.Create(requiresTags: new[] { TalentTag.Heal }));
        var snapshot = Snapshot(tags: new Dictionary<TalentTag, int> { { TalentTag.Heal, 1 } });

        Assert.IsTrue(TalentEligibility.IsEligible(talent, snapshot, AlwaysUnlockedMetaQuery.Instance));
    }

    [Test]
    public void Exclusion_BlocksWhenChosen()
    {
        var talent = Capstone("blocked", TalentRequirement.Create(excludesTalents: new[] { "other" }));
        var snapshot = Snapshot(chosen: new[] { "other" });

        Assert.IsFalse(TalentEligibility.IsEligible(talent, snapshot, AlwaysUnlockedMetaQuery.Instance));
    }

    [Test]
    public void TagCounts_RequireThreshold()
    {
        var talent = Capstone("aoe2", TalentRequirement.Create(
            requiresTagCounts: new[] { TagCountRequirement.Create(TalentTag.Aoe, 2) }));
        var one = Snapshot(tags: new Dictionary<TalentTag, int> { { TalentTag.Aoe, 1 } });
        var two = Snapshot(tags: new Dictionary<TalentTag, int> { { TalentTag.Aoe, 2 } });

        Assert.IsFalse(TalentEligibility.IsEligible(talent, one, AlwaysUnlockedMetaQuery.Instance));
        Assert.IsTrue(TalentEligibility.IsEligible(talent, two, AlwaysUnlockedMetaQuery.Instance));
    }

    [Test]
    public void RequiresUltimate_MustMatch()
    {
        var talent = Capstone("evo", TalentRequirement.Create(requiresUltimateId: "pudzian_piekielna_aura"));
        var wrong = Snapshot(ultimate: "pudzian_trzesienie");
        var right = Snapshot(ultimate: "pudzian_piekielna_aura");

        Assert.IsFalse(TalentEligibility.IsEligible(talent, wrong, AlwaysUnlockedMetaQuery.Instance));
        Assert.IsTrue(TalentEligibility.IsEligible(talent, right, AlwaysUnlockedMetaQuery.Instance));
    }

    [Test]
    public void MetaLocked_HiddenWhenQueryRejects()
    {
        var talent = Capstone("locked", TalentRequirement.Create(metaUnlockId: "meta_x"));
        var snapshot = Snapshot();

        Assert.IsTrue(TalentEligibility.IsEligible(talent, snapshot, AlwaysUnlockedMetaQuery.Instance));
        Assert.IsFalse(TalentEligibility.IsEligible(talent, snapshot, new LockedMetaQuery("meta_x")));
    }

    [Test]
    public void EmptyMetaId_AlwaysUnlocked()
    {
        var talent = Capstone("base", TalentRequirement.Create());
        Assert.IsTrue(TalentEligibility.IsEligible(talent, Snapshot(), new LockedMetaQuery("anything")));
    }

    private static TalentDefinition Capstone(string id, TalentRequirement req)
    {
        var talent = ScriptableObject.CreateInstance<TalentDefinition>();
        talent.ConfigureRuntime(id, id, "", PlayerClassId.Pudzian, 5, System.Array.Empty<TalentTag>(), req,
            TalentEffect.Persistent(PersistentEffectKind.Hart, OfferGenerator.GroupCapstone));
        return talent;
    }

    private static ProgressionSnapshot Snapshot(
        string[] chosen = null,
        string ultimate = "",
        Dictionary<TalentTag, int> tags = null)
    {
        return new ProgressionSnapshot(
            PlayerClassId.Pudzian,
            5,
            chosen ?? System.Array.Empty<string>(),
            ultimate,
            tags ?? new Dictionary<TalentTag, int>());
    }
}
