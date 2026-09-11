using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class NoChodzTuSkillTests
{
    [Test]
    public void Factory_Radius55_Damage0_Cooldown8_NoBoss()
    {
        var skill = SkillContentFactory.CreateNoChodzTu();

        Assert.AreEqual("pudzian_no_chodz_tu", skill.SkillId);
        Assert.AreEqual("Prowokacja", skill.DisplayName);
        Assert.AreEqual(5.5f, skill.RadiusMeters, 0.01f);
        Assert.AreEqual(0f, skill.Damage, 0.01f);
        Assert.AreEqual(8f, skill.CooldownSeconds, 0.01f);
        Assert.AreEqual(4.0f, skill.ThreatDurationSeconds, 0.01f);
        Assert.IsFalse(skill.TargetMask.HasFlag(SkillTargetMask.Boss));
    }

    [Test]
    public void Trzasniecie_Baseline_Stomp()
    {
        var skill = SkillContentFactory.CreateTrzasniecie();
        Assert.AreEqual("Stomp", skill.DisplayName);
        Assert.AreEqual(12f, skill.Damage, 0.01f);
        Assert.AreEqual(5f, skill.CooldownSeconds, 0.01f);
    }
}
