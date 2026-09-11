using NUnit.Framework;

public class SkillCastCycleTests
{
    [Test]
    public void CastCycle_ReachesActiveThenRecovery()
    {
        var skill = SkillContentFactory.CreateTrzasniecie();
        var cycle = new SkillCastCycle();
        Assert.IsTrue(cycle.TryStart(skill));

        cycle.Tick(skill.WindupSeconds);
        Assert.AreEqual(SkillCastPhase.Active, cycle.Phase);

        cycle.Tick(skill.ActiveSeconds);
        Assert.AreEqual(SkillCastPhase.Recovery, cycle.Phase);
    }

    [Test]
    public void WindupCancel_DoesNotImplyCooldownStarted()
    {
        var tracker = new SkillCooldownTracker();
        tracker.Configure(8f);
        var cycle = new SkillCastCycle();
        cycle.TryStart(SkillContentFactory.CreateTrzasniecie());
        cycle.CancelWindup();

        Assert.AreEqual(SkillCastPhase.Idle, cycle.Phase);
        Assert.IsTrue(tracker.IsReady);
    }

    [Test]
    public void Cooldown_StartsOnlyWhenRequested()
    {
        var tracker = new SkillCooldownTracker();
        tracker.Configure(8f);
        tracker.StartCooldown();
        tracker.Tick(1f);

        Assert.AreEqual(7f, tracker.Remaining, 0.01f);
    }

    [Test]
    public void Cooldown_PauseFreezesTimer()
    {
        var tracker = new SkillCooldownTracker();
        tracker.Configure(8f);
        tracker.StartCooldown();
        tracker.SetPaused(true);
        tracker.Tick(2f);
        Assert.AreEqual(8f, tracker.Remaining, 0.01f);
    }

    [Test]
    public void WindupInterrupt_AppliesCooldownRefund()
    {
        var skill = SkillContentFactory.CreateTrzasniecie();
        var tracker = new SkillCooldownTracker();
        tracker.Configure(skill.CooldownSeconds);
        tracker.StartCooldown();
        tracker.ApplyRefund(skill.WindupInterruptCooldownRefund);

        Assert.AreEqual(skill.CooldownSeconds * (1f - skill.WindupInterruptCooldownRefund), tracker.Remaining, 0.01f);
    }

    [Test]
    public void WindupDamageInterruptFlag_ConsumesOnce()
    {
        var flag = new WindupDamageInterruptFlag();
        flag.NotifyDamageDuringWindup();

        Assert.IsTrue(flag.ConsumePending());
        Assert.IsFalse(flag.ConsumePending());
    }

    [Test]
    public void WindupDamageInterrupt_CancelsCastWithPartialCooldown()
    {
        var skill = SkillContentFactory.CreateTrzasniecie();
        var cycle = new SkillCastCycle();
        var tracker = new SkillCooldownTracker();
        tracker.Configure(skill.CooldownSeconds);
        var flag = new WindupDamageInterruptFlag();

        cycle.TryStart(skill);
        flag.NotifyDamageDuringWindup();

        if (cycle.Phase == SkillCastPhase.Windup && flag.ConsumePending())
        {
            cycle.CancelWindup();
            tracker.StartCooldown();
            tracker.ApplyRefund(skill.WindupInterruptCooldownRefund);
        }

        Assert.AreEqual(SkillCastPhase.Idle, cycle.Phase);
        Assert.AreEqual(skill.CooldownSeconds * (1f - skill.WindupInterruptCooldownRefund), tracker.Remaining, 0.01f);
    }
}
