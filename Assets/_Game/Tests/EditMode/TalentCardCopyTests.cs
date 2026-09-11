using NUnit.Framework;

public class TalentCardCopyTests
{
    private static TalentDefinition FindTalent(string id)
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        foreach (var talent in catalog.AllTalents)
        {
            if (talent != null && talent.TalentId == id)
                return talent;
        }

        Assert.Fail($"Brak talentu {id} w domyślnym katalogu.");
        return null;
    }

    [Test]
    public void SkokCard_ContainsStompAndSkok()
    {
        var skok = FindTalent("pudzian_skok");
        var card = TalentCardCopy.FormatCard(skok);
        StringAssert.Contains("Stomp", card);
        StringAssert.Contains("skok", card);
    }

    [Test]
    public void ZryjCard_ContainsLecza()
    {
        var zryj = FindTalent("pudzian_zryj_mnie");
        var card = TalentCardCopy.FormatCard(zryj);
        StringAssert.Contains("leczą", card);
    }

    [Test]
    public void PiekielnaAura_IncludesPasywne()
    {
        var aura = FindTalent("pudzian_piekielna_aura");
        var card = TalentCardCopy.FormatCard(aura);
        StringAssert.Contains("Pasywne", card);
    }

    [Test]
    public void NieZabijecie_IncludesPasywne()
    {
        var lastStand = FindTalent("pudzian_nie_zabijecie_mnie");
        var card = TalentCardCopy.FormatCard(lastStand);
        StringAssert.Contains("Pasywne", card);
    }

    [Test]
    public void JaJestemBoss_IncludesAktywne()
    {
        var boss = FindTalent("pudzian_ja_jestem_boss");
        var card = TalentCardCopy.FormatCard(boss);
        StringAssert.Contains("Aktywne", card);
    }

    [Test]
    public void SkokTagsLine_IsNonEmpty()
    {
        var skok = FindTalent("pudzian_skok");
        var tags = TalentCardCopy.FormatTags(skok.Tags);
        Assert.IsFalse(string.IsNullOrWhiteSpace(tags));
        StringAssert.Contains("AOE", tags);
        StringAssert.Contains("MOBILITY", tags);
    }

    [Test]
    public void FormatTags_JoinsWithMiddleDot()
    {
        var tags = TalentCardCopy.FormatTags(new[] { TalentTag.Aoe, TalentTag.Taunt });
        Assert.AreEqual("AOE · TAUNT", tags);
    }
}
