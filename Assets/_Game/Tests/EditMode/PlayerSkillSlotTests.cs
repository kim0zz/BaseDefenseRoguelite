using NUnit.Framework;

/// <summary>
/// Testy multi-skill loadout (M7.6-T3).
/// </summary>
public class PlayerSkillSlotTests
{
    [Test]
    public void IndependentCooldowns_Skill0DoesNotResetSkill1()
    {
        var loadout = new SkillLoadout();
        var skill0 = SkillContentFactory.CreateTrzasniecie();
        var skill1 = SkillContentFactory.CreateTrzasniecie();

        loadout.ConfigureSlot(0, skill0);
        loadout.ConfigureSlot(1, skill1);

        loadout.GetCooldown(0).StartCooldown();
        loadout.GetCooldown(0).Tick(2f);

        Assert.IsFalse(loadout.GetCooldown(0).IsReady);
        Assert.IsTrue(loadout.GetCooldown(1).IsReady);
        Assert.AreEqual(0f, loadout.GetCooldown(1).Remaining, 0.01f);
    }

    [Test]
    public void SingleCastCycle_RejectsSecondStartWhileCasting()
    {
        var skill = SkillContentFactory.CreateTrzasniecie();
        var cycle = new SkillCastCycle();

        Assert.IsTrue(cycle.TryStart(skill));
        Assert.IsFalse(cycle.TryStart(skill));
        Assert.IsTrue(cycle.IsCasting);
    }

    [Test]
    public void Loadout_PauseFreezesAllCooldowns()
    {
        var loadout = new SkillLoadout();
        var skill = SkillContentFactory.CreateTrzasniecie();
        loadout.ConfigureSlot(0, skill);
        loadout.ConfigureSlot(1, skill);

        loadout.GetCooldown(0).StartCooldown();
        loadout.GetCooldown(1).StartCooldown();
        loadout.SetAllPaused(true);
        loadout.TickAll(3f);

        Assert.AreEqual(skill.CooldownSeconds, loadout.GetCooldown(0).Remaining, 0.01f);
        Assert.AreEqual(skill.CooldownSeconds, loadout.GetCooldown(1).Remaining, 0.01f);
    }

    [Test]
    public void WindupCancel_DoesNotStartSlotCooldown()
    {
        var skill = SkillContentFactory.CreateTrzasniecie();
        var loadout = new SkillLoadout();
        loadout.ConfigureSlot(0, skill);
        var cycle = new SkillCastCycle();

        cycle.TryStart(skill);
        cycle.CancelWindup();

        Assert.AreEqual(SkillCastPhase.Idle, cycle.Phase);
        Assert.IsTrue(loadout.GetCooldown(0).IsReady);
    }

    [Test]
    public void BufferSlot_ConsumesPerSlot()
    {
        var loadout = new SkillLoadout();
        loadout.BufferSlot(2);
        loadout.BufferSlot(1);

        Assert.AreEqual(1, loadout.ConsumeBufferedSlot());
        Assert.AreEqual(2, loadout.ConsumeBufferedSlot());
        Assert.IsFalse(loadout.ConsumeBufferedSlot().HasValue);
    }

    [Test]
    public void ConfigureSlot_OnlySlot0ForTrzasniecieRegression()
    {
        var loadout = new SkillLoadout();
        var trzasniecie = SkillContentFactory.CreateTrzasniecie();
        loadout.ConfigureSlot(0, trzasniecie);

        Assert.AreEqual(trzasniecie, loadout.GetSkill(0));
        Assert.IsNull(loadout.GetSkill(1));
        Assert.IsNull(loadout.GetSkill(2));
    }
}
