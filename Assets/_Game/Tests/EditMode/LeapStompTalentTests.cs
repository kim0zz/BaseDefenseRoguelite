using System.Linq;
using NUnit.Framework;

public class LeapStompTalentTests
{
    [Test]
    public void PudzianCatalog_HasLeapStompAsLevel2MutationOfTrzasniecie()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var talents = catalog.BuildLevelOffer(new ProgressionSnapshot(
            PlayerClassId.Pudzian,
            2,
            System.Array.Empty<string>(),
            "",
            new System.Collections.Generic.Dictionary<TalentTag, int>()));

        var leap = talents.FirstOrDefault(t => t.TalentId == "pudzian_skok");
        Assert.IsNotNull(leap);
        Assert.AreEqual(SkillLocomotionMode.Leap, leap.Effect.ReplacementSkill.Locomotion);
        Assert.That(leap.Description, Does.Contain("0–6"));
        Assert.AreEqual(3, talents.Count);
    }
}
