using NUnit.Framework;
using UnityEngine;

public class BossRamLogicTests
{
    [Test]
    public void Enrage_TriggersAtHalfHealth()
    {
        var def = ScriptableObject.CreateInstance<BossDefinition>();
        Assert.IsFalse(BossRamLogic.ShouldEnterEnrage(500f, 900f, 0.5f));
        Assert.IsTrue(BossRamLogic.ShouldEnterEnrage(400f, 900f, 0.5f));
        Object.DestroyImmediate(def);
    }

    [Test]
    public void Interrupt_StaggerThresholdConsumesAccumulated()
    {
        var def = ScriptableObject.CreateInstance<BossDefinition>();
        var go = new GameObject("BossStatus");
        var health = go.AddComponent<Health>();
        health.Configure(100f);
        var status = go.AddComponent<StatusEffectReceiver>();
        status.AddStagger(40f);

        Assert.IsTrue(BossRamLogic.ShouldInterruptCharge(def, status, true));
        Assert.AreEqual(0f, status.StaggerAccumulated);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(def);
    }

    [Test]
    public void TelegraphDuration_ShorterWhenEnraged()
    {
        var def = ScriptableObject.CreateInstance<BossDefinition>();
        var normal = BossRamLogic.GetTelegraphDuration(def, false);
        var enraged = BossRamLogic.GetTelegraphDuration(def, true);
        Assert.Less(enraged, normal);
        Object.DestroyImmediate(def);
    }

    [Test]
    public void FocusStopDistance_KeepsBodyOutsidePlayerAndSurfaceInDaggerRange()
    {
        const float bossScale = 2.2f;
        const float playerRadius = 0.5f;
        const float gap = 0.55f;
        var bossRadius = EnemySeparation.RadiusFromBodyScale(new Vector3(bossScale, bossScale, bossScale));
        var stop = BossRamLogic.GetFocusStopDistance(bossRadius, playerRadius, gap);

        Assert.Greater(stop, bossRadius + playerRadius, "Stop musi zostawić lukę między ciałami.");
        Assert.LessOrEqual(stop - bossRadius, 1.6f, "Powierzchnia bossa ma być w zasięgu sztyletu (1.6).");
        Assert.AreEqual(bossRadius + playerRadius + gap, stop, 0.001f);
    }

    [Test]
    public void FocusMoveStep_ApproachesWithoutCrossingStop_AndBacksOffWhenTooClose()
    {
        const float stop = 2.15f;
        const float maxStep = 0.2f;

        Assert.AreEqual(maxStep, BossRamLogic.GetFocusMoveStep(8f, stop, maxStep), 0.0001f);
        Assert.AreEqual(0f, BossRamLogic.GetFocusMoveStep(stop + 0.02f, stop, maxStep), 0.0001f);
        Assert.AreEqual(-maxStep, BossRamLogic.GetFocusMoveStep(0.4f, stop, maxStep), 0.0001f);
        Assert.AreEqual(0.1f, BossRamLogic.GetFocusMoveStep(stop + 0.1f, stop, maxStep), 0.0001f);
    }

    [Test]
    public void FocusDuration_UsesDefinitionDefaults()
    {
        var def = ScriptableObject.CreateInstance<BossDefinition>();
        Assert.AreEqual(3.5f, BossRamLogic.GetFocusDuration(def, false), 0.001f);
        Assert.AreEqual(2.2f, BossRamLogic.GetFocusDuration(def, true), 0.001f);
        Object.DestroyImmediate(def);
    }

    [Test]
    public void ShouldExitFocus_WhenTimerElapsed()
    {
        Assert.IsTrue(BossRamLogic.ShouldExitFocus(0f));
        Assert.IsTrue(BossRamLogic.ShouldExitFocus(-0.1f));
        Assert.IsFalse(BossRamLogic.ShouldExitFocus(0.5f));
    }

    [Test]
    public void ShouldAbortFocusForCharge_WhenCooldownReady()
    {
        Assert.IsTrue(BossRamLogic.ShouldAbortFocusForCharge(0f));
        Assert.IsTrue(BossRamLogic.ShouldAbortFocusForCharge(-1f));
        Assert.IsFalse(BossRamLogic.ShouldAbortFocusForCharge(0.5f));
    }

    [Test]
    public void ChargeTarget_LivingPlayerBeatsTowerAndBase()
    {
        Assert.AreEqual(
            BossRamChargeTargetKind.Player,
            BossRamLogic.ResolveChargeTargetKind(true, true, true));
    }

    [Test]
    public void ChargeTarget_LivingTowerWhenNoPlayers()
    {
        Assert.AreEqual(
            BossRamChargeTargetKind.LineTower,
            BossRamLogic.ResolveChargeTargetKind(true, true, false));
    }

    [Test]
    public void ChargeTarget_LivingBaseWhenNoPlayersOrTowers()
    {
        Assert.AreEqual(
            BossRamChargeTargetKind.Base,
            BossRamLogic.ResolveChargeTargetKind(false, true, false));
        Assert.AreEqual(
            BossRamChargeTargetKind.None,
            BossRamLogic.ResolveChargeTargetKind(false, false, false));
    }

    [Test]
    public void ChargeImpact_UsesPlayerDamageWhenNotStructure()
    {
        Assert.AreEqual(80f, BossRamLogic.GetChargeImpactDamage(true, 80f, 14f), 0.001f);
        Assert.AreEqual(14f, BossRamLogic.GetChargeImpactDamage(false, 80f, 14f), 0.001f);
    }

    [Test]
    public void ChargeAimPoint_StopsShortOfTarget()
    {
        var aim = BossRamLogic.GetChargeAimPoint(Vector3.zero, new Vector3(0f, 0f, 10f), 2f);
        Assert.AreEqual(8f, aim.z, 0.01f);
    }
}
