using System.Collections.Generic;
using NUnit.Framework;

public class OfferGeneratorTests
{
    [Test]
    public void Level2_OffersOneMutationPerActive()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var offer = catalog.BuildLevelOffer(Snapshot(2));

        Assert.AreEqual(3, offer.Count);
        Assert.AreEqual("pudzian_skok", offer[0].TalentId);
        Assert.AreEqual("pudzian_zryj_mnie", offer[1].TalentId);
        Assert.AreEqual("pudzian_spychacz", offer[2].TalentId);
    }

    [Test]
    public void Level3_FollowupDependsOnLevel2()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var afterSkok = catalog.BuildLevelOffer(Snapshot(3, "pudzian_skok"));
        Assert.AreEqual(3, afterSkok.Count);
        Assert.That(Ids(afterSkok), Does.Contain("pudzian_wkurw"));
        Assert.That(Ids(afterSkok), Does.Contain("pudzian_hart"));
        Assert.That(Ids(afterSkok), Does.Contain("pudzian_spalona_ziemia"));
        Assert.That(Ids(afterSkok), Does.Not.Contain("pudzian_najezony"));

        var afterTaunt = catalog.BuildLevelOffer(Snapshot(3, "pudzian_zryj_mnie"));
        Assert.That(Ids(afterTaunt), Does.Contain("pudzian_najezony"));
        Assert.That(Ids(afterTaunt), Does.Not.Contain("pudzian_spalona_ziemia"));
    }

    [Test]
    public void Level4_AllUltimatesRegardlessOfPriorPicks()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var offer = catalog.BuildLevelOffer(Snapshot(4, "pudzian_skok", "pudzian_wkurw"));
        Assert.AreEqual(4, offer.Count);
        Assert.That(Ids(offer), Does.Contain("pudzian_ja_jestem_boss"));
        Assert.That(Ids(offer), Does.Contain("pudzian_trzesienie"));
        Assert.That(Ids(offer), Does.Contain("pudzian_piekielna_aura"));
        Assert.That(Ids(offer), Does.Contain("pudzian_nie_zabijecie_mnie"));
    }

    [Test]
    public void Level5_HidesEvolutionOfUnchosenUltimate()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var snapshot = new ProgressionSnapshot(
            PlayerClassId.Pudzian,
            5,
            new[] { "pudzian_skok", "pudzian_wkurw", "pudzian_piekielna_aura" },
            "pudzian_piekielna_aura",
            new Dictionary<TalentTag, int>
            {
                { TalentTag.Aoe, 2 },
                { TalentTag.Mobility, 1 },
                { TalentTag.Berserker, 1 },
                { TalentTag.Risk, 1 },
                { TalentTag.Aura, 1 },
                { TalentTag.Zone, 1 }
            });

        var offer = catalog.BuildLevelOffer(snapshot);
        Assert.That(Ids(offer), Does.Contain("pudzian_wampiryczny_ogien"));
        Assert.That(Ids(offer), Does.Contain("pudzian_przegrzanie"));
        Assert.That(Ids(offer), Does.Not.Contain("pudzian_prawdziwy_boss"));
        Assert.That(Ids(offer), Does.Not.Contain("pudzian_rozpadlina"));
        Assert.GreaterOrEqual(offer.Count, 1);
    }

    [Test]
    public void EveryLegalBuild_HasOfferInRecipeRange()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var table = catalog.GetProgression(PlayerClassId.Pudzian);
        var l2 = new[] { "pudzian_skok", "pudzian_zryj_mnie", "pudzian_spychacz" };
        var l3Options = new Dictionary<string, string[]>
        {
            { "pudzian_skok", new[] { "pudzian_wkurw", "pudzian_hart", "pudzian_spalona_ziemia" } },
            { "pudzian_zryj_mnie", new[] { "pudzian_wkurw", "pudzian_hart", "pudzian_najezony" } },
            { "pudzian_spychacz", new[] { "pudzian_wkurw", "pudzian_hart", "pudzian_w_sciane" } }
        };
        var ultis = new[]
        {
            "pudzian_ja_jestem_boss",
            "pudzian_trzesienie",
            "pudzian_piekielna_aura",
            "pudzian_nie_zabijecie_mnie"
        };

        foreach (var pick2 in l2)
        foreach (var pick3 in l3Options[pick2])
        foreach (var ulti in ultis)
            {
                var chosen = new[] { pick2, pick3, ulti };
                var talents = Find(catalog, chosen);
                var snapshot = new ProgressionSnapshot(
                    PlayerClassId.Pudzian,
                    5,
                    chosen,
                    Find(catalog, ulti).Effect.GrantedSkill.SkillId,
                    TalentTagCounter.Compute(talents));

                var offer = catalog.BuildLevelOffer(snapshot);
                var rule = table.GetRule(5);
                Assert.GreaterOrEqual(offer.Count, rule.MinCount, $"L5 empty after {pick2}/{pick3}/{ulti}");
                Assert.LessOrEqual(offer.Count, rule.MaxCount);
            }
    }

    [Test]
    public void Tags_SumFromChosenCards()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var chosen = Find(catalog, "pudzian_skok", "pudzian_spychacz");
        var counts = TalentTagCounter.Compute(chosen);
        Assert.AreEqual(2, counts[TalentTag.Aoe]);
        Assert.AreEqual(1, counts[TalentTag.Mobility]);
        Assert.AreEqual(1, counts[TalentTag.Control]);
    }

    private static ProgressionSnapshot Snapshot(int level, params string[] chosen)
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var talents = Find(catalog, chosen);
        return new ProgressionSnapshot(
            PlayerClassId.Pudzian,
            level,
            chosen,
            ProgressionApplier.ReadUltimateSkillId(talents),
            TalentTagCounter.Compute(talents));
    }

    private static List<string> Ids(IReadOnlyList<TalentDefinition> offer)
    {
        var ids = new List<string>();
        foreach (var talent in offer)
            ids.Add(talent.TalentId);
        return ids;
    }

    private static List<TalentDefinition> Find(BuildContentCatalog catalog, params string[] ids)
    {
        var list = new List<TalentDefinition>();
        foreach (var id in ids)
        {
            foreach (var talent in catalog.AllTalents)
            {
                if (talent.TalentId == id)
                    list.Add(talent);
            }
        }

        return list;
    }

    private static TalentDefinition Find(BuildContentCatalog catalog, string id)
    {
        foreach (var talent in catalog.AllTalents)
        {
            if (talent.TalentId == id)
                return talent;
        }

        return null;
    }
}
