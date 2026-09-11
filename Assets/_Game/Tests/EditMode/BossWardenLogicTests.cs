using NUnit.Framework;
using UnityEngine;

public class BossWardenLogicTests
{
    private WardenDefinition _definition;

    [SetUp]
    public void SetUp()
    {
        _definition = ScriptableObject.CreateInstance<WardenDefinition>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_definition != null)
            Object.DestroyImmediate(_definition);
    }

    [Test]
    public void GetPhaseFromHealthRatio_UsesLeadThresholds()
    {
        Assert.AreEqual(0, BossWardenLogic.GetPhaseFromHealthRatio(0.71f));
        Assert.AreEqual(1, BossWardenLogic.GetPhaseFromHealthRatio(0.70f));
        Assert.AreEqual(1, BossWardenLogic.GetPhaseFromHealthRatio(0.41f));
        Assert.AreEqual(2, BossWardenLogic.GetPhaseFromHealthRatio(0.40f));
        Assert.AreEqual(2, BossWardenLogic.GetPhaseFromHealthRatio(0.21f));
        Assert.AreEqual(3, BossWardenLogic.GetPhaseFromHealthRatio(0.20f));
        Assert.AreEqual(3, BossWardenLogic.GetPhaseFromHealthRatio(0.05f));
    }

    [Test]
    public void AdvancePhase_DoesNotRewindWhenHealed()
    {
        var highest = BossWardenLogic.AdvancePhase(0, 0.65f);
        Assert.AreEqual(1, highest);

        highest = BossWardenLogic.AdvancePhase(highest, 0.90f);
        Assert.AreEqual(1, highest, "Faza nie cofa się po wzroście HP.");
    }

    [Test]
    public void GetDamageTakenMultiplier_TotemsAlive_UsesQuarterDamage()
    {
        var mul = BossWardenLogic.GetDamageTakenMultiplier(true, _definition);
        Assert.AreEqual(0.25f, mul, 0.0001f);
    }

    [Test]
    public void GetDamageTakenMultiplier_NoTotems_FullDamage()
    {
        var mul = BossWardenLogic.GetDamageTakenMultiplier(false, _definition);
        Assert.AreEqual(1f, mul, 0.0001f);
    }

    [Test]
    public void GetSlamInterval_Phase3_UsesFrenzyInterval()
    {
        Assert.AreEqual(2f, BossWardenLogic.GetSlamInterval(3, _definition), 0.0001f);
        Assert.AreEqual(3.4f, BossWardenLogic.GetSlamInterval(0, _definition), 0.0001f);
    }

    [Test]
    public void ShouldSpawnTotemsOnce_OnlyOnceAtPhase1()
    {
        Assert.IsTrue(BossWardenLogic.ShouldSpawnTotemsOnce(1, false));
        Assert.IsFalse(BossWardenLogic.ShouldSpawnTotemsOnce(1, true));
        Assert.IsFalse(BossWardenLogic.ShouldSpawnTotemsOnce(0, false));
    }
}
