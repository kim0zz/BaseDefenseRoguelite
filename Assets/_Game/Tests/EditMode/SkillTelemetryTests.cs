using NUnit.Framework;

public class SkillTelemetryTests
{
    [SetUp]
    public void SetUp()
    {
        SkillTelemetry.Reset();
    }

    [Test]
    public void Telemetry_RecordsHitRateAndMultiLane()
    {
        SkillTelemetry.SetActiveSkill("pudzian_trzasniecie");

        SkillTelemetry.RecordUse(new SkillHitResolver.ResolveResult
        {
            HadHit = true,
            HitCount = 3,
            TotalDamage = 48f,
            TotalControlSeconds = 2.4f,
            DistinctLaneCount = 2
        }, cancelled: false);

        SkillTelemetry.RecordUse(new SkillHitResolver.ResolveResult
        {
            HadHit = false
        }, cancelled: false);

        Assert.AreEqual(2, SkillTelemetry.Uses);
        Assert.AreEqual(1, SkillTelemetry.UsesWithHit);
        Assert.AreEqual(0.5f, SkillTelemetry.HitRate, 0.01f);
        Assert.AreEqual(1, SkillTelemetry.MultiLaneUses);
    }

    [Test]
    public void Telemetry_PerSkillId_KeepsIndependentCounters()
    {
        SkillTelemetry.RecordUse("pudzian_trzasniecie", new SkillHitResolver.ResolveResult
        {
            HadHit = true,
            HitCount = 2,
            TotalDamage = 32f,
            TotalControlSeconds = 2.4f,
            DistinctLaneCount = 1
        }, cancelled: false);

        SkillTelemetry.RecordUse("pudzian_trzasniecie", new SkillHitResolver.ResolveResult
        {
            HadHit = false
        }, cancelled: false);

        SkillTelemetry.RecordUse("pudzian_byk", new SkillHitResolver.ResolveResult
        {
            HadHit = true,
            HitCount = 1,
            TotalDamage = 12f,
            TotalControlSeconds = 0f,
            DistinctLaneCount = 1
        }, cancelled: false);

        var trzasniecie = SkillTelemetry.BuildSummary("pudzian_trzasniecie");
        var byk = SkillTelemetry.BuildSummary("pudzian_byk");

        Assert.That(trzasniecie, Does.Contain("uses=2"));
        Assert.That(trzasniecie, Does.Contain("hit%=50%"));
        Assert.That(byk, Does.Contain("uses=1"));
        Assert.That(byk, Does.Contain("hit%=100%"));
        Assert.That(byk, Does.Contain("damage=12"));
    }

    [Test]
    public void Telemetry_OverloadWithoutId_UsesActiveSkill()
    {
        SkillTelemetry.SetActiveSkill("pudzian_no_chodz_tu");
        SkillTelemetry.RecordUse(new SkillHitResolver.ResolveResult { HadHit = true, HitCount = 1 }, cancelled: false);

        Assert.AreEqual(1, SkillTelemetry.Uses);
        Assert.That(SkillTelemetry.BuildSummary("pudzian_no_chodz_tu"), Does.Contain("uses=1"));
    }
}
