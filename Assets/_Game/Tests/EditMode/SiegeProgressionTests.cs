using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class SiegeProgressionTests
{
    [TestCase(0, true, 2)]
    [TestCase(1, false, 2)]
    [TestCase(2, true, 3)]
    [TestCase(3, false, 3)]
    [TestCase(4, true, 4)]
    public void RewardSchedulePreservesTalentPrerequisiteOrder(int wave, bool large, int level)
    {
        Assert.AreEqual(large, SiegeRewardController.IsLargeReward(wave));
        Assert.AreEqual(level, SiegeRewardController.TalentLevelForWave(wave));
    }

    [Test]
    public void BoostsFollowMutatedSlotWithoutModifyingSharedDefinitionOrCompounding()
    {
        var go = new GameObject("SiegeUpgradeTest");
        var upgrades = go.AddComponent<SiegeSkillUpgrades>();
        var ranks = (int[,])typeof(SiegeSkillUpgrades).GetField("_ranks", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(upgrades);
        ranks[0, (int)SiegeUpgradeKind.Damage] = 2;
        ranks[0, (int)SiegeUpgradeKind.Cooldown] = 1;
        ranks[0, (int)SiegeUpgradeKind.Area] = 1;
        var original = SkillContentFactory.CreateTrzasniecie();
        var mutation = SkillContentFactory.CreateSkok();
        try
        {
            var originalDamage = original.Damage;
            var slots = new SkillDefinition[SkillLoadout.SlotCount];
            slots[0] = original;
            upgrades.ApplyToSlots(slots);
            Assert.AreNotSame(original, slots[0]);
            Assert.AreEqual(originalDamage * 1.4f, slots[0].Damage, 0.001f);
            Assert.AreEqual(originalDamage, original.Damage);

            slots[0] = mutation;
            upgrades.ApplyToSlots(slots);
            Assert.AreEqual(mutation.SkillId, slots[0].SkillId);
            Assert.AreEqual(mutation.Damage * 1.4f, slots[0].Damage, 0.001f);
            Assert.AreEqual(mutation.CooldownSeconds * 0.9f, slots[0].CooldownSeconds, 0.001f);
            Assert.AreEqual(mutation.RadiusMeters * 1.15f, slots[0].RadiusMeters, 0.001f);
            slots[0] = mutation;
            upgrades.ApplyToSlots(slots);
            Assert.AreEqual(mutation.Damage * 1.4f, slots[0].Damage, 0.001f);
        }
        finally
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(original);
            Object.DestroyImmediate(mutation);
        }
    }

    [Test]
    public void RankCapExcludesOfferAndCooldownMultiplierHasFloor()
    {
        var go = new GameObject("SiegeUpgradeTest");
        var upgrades = go.AddComponent<SiegeSkillUpgrades>();
        var skill = SkillContentFactory.CreateTrzasniecie();
        try
        {
            var ranks = (int[,])typeof(SiegeSkillUpgrades).GetField("_ranks", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(upgrades);
            Assert.IsTrue(upgrades.CanApply(0, SiegeUpgradeKind.Cooldown, skill));
            ranks[0, (int)SiegeUpgradeKind.Cooldown] = 3;
            Assert.IsFalse(upgrades.CanApply(0, SiegeUpgradeKind.Cooldown, skill));
            ranks[0, (int)SiegeUpgradeKind.Cooldown] = 100;
            Assert.AreEqual(0.5f, upgrades.GetMultiplier(0, SiegeUpgradeKind.Cooldown));
            Assert.IsFalse(upgrades.CanApply(-1, SiegeUpgradeKind.Damage, skill));
            Assert.IsFalse(upgrades.CanApply(0, SiegeUpgradeKind.Damage, null));
        }
        finally { Object.DestroyImmediate(go); Object.DestroyImmediate(skill); }
    }

    [Test]
    public void SkillCooldownCannotFallBelowReadableMinimum()
    {
        var skill = SkillContentFactory.CreateTrzasniecie();
        try
        {
            typeof(SkillDefinition).GetField("cooldownSeconds", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(skill, 0.4f);
            skill.ApplySiegeMultipliers(1f, 0.01f, 1f);
            Assert.AreEqual(0.35f, skill.CooldownSeconds, 0.001f);
        }
        finally { Object.DestroyImmediate(skill); }
    }
}
