using NUnit.Framework;
using UnityEngine;

public class ProgressionApplierTests
{
    [Test]
    public void MutateSkill_ReplacesSlotDefinition()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var skok = Find(catalog, "pudzian_skok");
        var slots = new[]
        {
            SkillContentFactory.CreateTrzasniecie(),
            SkillContentFactory.CreateNoChodzTu(),
            SkillContentFactory.CreateByk(),
            null
        };
        var effects = new[]
        {
            new System.Collections.Generic.List<SkillEffectKind>(),
            new System.Collections.Generic.List<SkillEffectKind>(),
            new System.Collections.Generic.List<SkillEffectKind>(),
            new System.Collections.Generic.List<SkillEffectKind>()
        };
        var persistents = new System.Collections.Generic.List<PersistentEffectBinding>();

        ProgressionApplier.Apply(skok, slots, effects, persistents);

        Assert.AreEqual("pudzian_skok", slots[0].SkillId);
        Assert.AreEqual(SkillLocomotionMode.Leap, slots[0].Locomotion);
        Assert.AreEqual("pudzian_no_chodz_tu", slots[1].SkillId);
    }

    [Test]
    public void TimedLastChance_PreventsFirstLethal()
    {
        var go = new GameObject("Tank");
        var health = go.AddComponent<Health>();
        health.Configure(135f);
        var persistents = go.AddComponent<PlayerPersistentEffects>();
        persistents.ReplaceAll(new[] { new PersistentEffectBinding(PersistentEffectKind.TimedLastChance,
            EffectTuning.Create(6f, 0.30f)) });

        health.TakeDamage(200f, null);

        Assert.IsTrue(health.IsAlive);
        Assert.IsTrue(persistents.LastChanceActive);

        Object.DestroyImmediate(go);
    }

    private static TalentDefinition Find(BuildContentCatalog catalog, string id)
    {
        foreach (var talent in catalog.AllTalents)
        {
            if (talent.TalentId == id)
                return talent;
        }

        return null;
    }
}
