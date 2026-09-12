using System.Collections.Generic;
using NUnit.Framework;

public class BombermanProgressionTests
{
    private const string Kasetowa = "bomberman_kasetowa";
    private const string Huk = "bomberman_wiekszy_huk";
    private const string Ogluszajacy = "bomberman_wybuch_ogluszajacy";
    private const string Saper = "bomberman_saper";
    private const string Piroman = "bomberman_piroman";
    private const string Torpedy = "bomberman_mini_torpedy";
    private const string Ladunek = "bomberman_ladunek_kumulacyjny";
    private const string Kula = "bomberman_kula_bilardowa";
    private const string Rapid = "bomberman_szybkostrzelnosc";
    private const string Orbital = "bomberman_orbitale";
    private const string Nalot = "bomberman_nalot";
    private const string Karabin = "bomberman_karabin_maszynowy";

    [Test]
    public void Level2_AllEligibleInGroup_OffersThreeMutationsRegardlessOfStartingSkills()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var snapshot = new ProgressionSnapshot(
            PlayerClassId.Bomberman,
            2,
            System.Array.Empty<string>(),
            "",
            new Dictionary<TalentTag, int>());

        var offer = catalog.BuildLevelOffer(snapshot);
        Assert.AreEqual(3, offer.Count);
        var ids = Ids(offer);
        Assert.That(ids, Does.Contain(Kasetowa));
        Assert.That(ids, Does.Contain(Huk));
        Assert.That(ids, Does.Contain(Ogluszajacy));
    }

    [Test]
    public void Level3_FollowupDependsOnLevel2_SaperAndPiromanAlwaysPresent()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();

        var afterKasetowa = catalog.BuildLevelOffer(Snapshot(3, Kasetowa));
        Assert.AreEqual(3, Ids(afterKasetowa).Count);
        Assert.That(Ids(afterKasetowa), Does.Contain(Saper));
        Assert.That(Ids(afterKasetowa), Does.Contain(Piroman));
        Assert.That(Ids(afterKasetowa), Does.Contain(Torpedy));
        Assert.That(Ids(afterKasetowa), Does.Not.Contain(Ladunek));
        Assert.That(Ids(afterKasetowa), Does.Not.Contain(Kula));

        var afterHuk = catalog.BuildLevelOffer(Snapshot(3, Huk));
        Assert.That(Ids(afterHuk), Does.Contain(Ladunek));
        Assert.That(Ids(afterHuk), Does.Not.Contain(Torpedy));
        Assert.That(Ids(afterHuk), Does.Not.Contain(Kula));

        var afterOgluszajacy = catalog.BuildLevelOffer(Snapshot(3, Ogluszajacy));
        Assert.That(Ids(afterOgluszajacy), Does.Contain(Kula));
        Assert.That(Ids(afterOgluszajacy), Does.Not.Contain(Torpedy));
        Assert.That(Ids(afterOgluszajacy), Does.Not.Contain(Ladunek));
    }

    [Test]
    public void Level4_AlwaysThreeUltimatesRegardlessOfPriorPicks()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var offer = catalog.BuildLevelOffer(Snapshot(4, Kasetowa, Torpedy));
        Assert.AreEqual(3, offer.Count);
        var ids = Ids(offer);
        Assert.That(ids, Does.Contain(Rapid));
        Assert.That(ids, Does.Contain(Orbital));
        Assert.That(ids, Does.Contain(Nalot));
    }

    [Test]
    public void AllTwentySevenBuilds_HaveAtLeastOneLevel5Offer()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var table = catalog.GetProgression(PlayerClassId.Bomberman);
        var l2 = new[] { Kasetowa, Huk, Ogluszajacy };
        var l3ByL2 = new Dictionary<string, string[]>
        {
            { Kasetowa, new[] { Saper, Piroman, Torpedy } },
            { Huk, new[] { Saper, Piroman, Ladunek } },
            { Ogluszajacy, new[] { Saper, Piroman, Kula } }
        };
        var ultis = new[] { Rapid, Orbital, Nalot };
        var expectedCounts = BuildExpectedLevel5Counts();

        var buildIndex = 0;
        foreach (var pick2 in l2)
        foreach (var pick3 in l3ByL2[pick2])
        foreach (var ulti in ultis)
        {
            buildIndex++;
            var chosen = new[] { pick2, pick3, ulti };
            var offer = catalog.BuildLevelOffer(Snapshot(5, chosen));
            var rule = table.GetRule(5);
            Assert.GreaterOrEqual(offer.Count, 1, $"Build #{buildIndex} ({pick2}/{pick3}/{ulti}) had 0 L5 offers");
            Assert.GreaterOrEqual(offer.Count, rule.MinCount, $"Build #{buildIndex} below min count");
            Assert.LessOrEqual(offer.Count, rule.MaxCount, $"Build #{buildIndex} above max count");
            if (expectedCounts.TryGetValue(buildIndex, out var expectedN))
                Assert.AreEqual(expectedN, offer.Count, $"Build #{buildIndex} L5 count mismatch");
        }
    }

    [Test]
    public void Karabin_EligibleOnlyWhenBasicTagCountAtLeastTwo()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();

        Assert.That(Ids(catalog.BuildLevelOffer(Snapshot(5, Huk, Piroman, Rapid))),
            Does.Contain(Karabin));
        Assert.That(Ids(catalog.BuildLevelOffer(Snapshot(5, Huk, Ladunek, Orbital))),
            Does.Contain(Karabin));

        Assert.That(Ids(catalog.BuildLevelOffer(Snapshot(5, Kasetowa, Saper, Rapid))),
            Does.Not.Contain(Karabin));
        Assert.That(Ids(catalog.BuildLevelOffer(Snapshot(5, Ogluszajacy, Saper, Rapid))),
            Does.Not.Contain(Karabin));
    }

    [Test]
    public void TalentEligibility_ClassFilter_ExcludesOtherClassCards()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var pudzianSnapshot = Snapshot(2, PlayerClassId.Pudzian);
        foreach (var talent in catalog.AllTalents)
        {
            if (talent.PlayerClass != PlayerClassId.Pudzian) continue;
            Assert.IsTrue(TalentEligibility.IsEligible(talent, pudzianSnapshot, AlwaysUnlockedMetaQuery.Instance));
        }

        foreach (var talent in catalog.AllTalents)
        {
            if (talent.PlayerClass != PlayerClassId.Bomberman) continue;
            Assert.IsFalse(TalentEligibility.IsEligible(talent, pudzianSnapshot, AlwaysUnlockedMetaQuery.Instance));
        }
    }

    private static Dictionary<int, int> BuildExpectedLevel5Counts()
    {
        return new Dictionary<int, int>
        {
            { 1, 2 }, { 2, 4 }, { 3, 3 },
            { 4, 4 }, { 5, 6 }, { 6, 5 },
            { 7, 3 }, { 8, 5 }, { 9, 4 },
            { 10, 3 }, { 11, 3 }, { 12, 3 },
            { 13, 4 }, { 14, 4 }, { 15, 4 },
            { 16, 4 }, { 17, 5 }, { 18, 5 },
            { 19, 3 }, { 20, 4 }, { 21, 4 },
            { 22, 2 }, { 23, 3 }, { 24, 3 },
            { 25, 3 }, { 26, 4 }, { 27, 3 }
        };
    }

    private static ProgressionSnapshot Snapshot(int level, params string[] chosen)
    {
        return Snapshot(level, PlayerClassId.Bomberman, chosen);
    }

    private static ProgressionSnapshot Snapshot(int level, PlayerClassId classId, params string[] chosen)
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var talents = Find(catalog, chosen);
        return new ProgressionSnapshot(
            classId,
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
}
