using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class AttackIntervalOverrideTests
{
    [Test]
    public void TimedOverride_Returns022ThenRestores()
    {
        var go = new GameObject("Player");
        var persistents = go.AddComponent<PlayerPersistentEffects>();
        persistents.ReplaceAll(new List<PersistentEffectBinding>
        {
            new PersistentEffectBinding(
                PersistentEffectKind.TimedAttackIntervalOverride,
                EffectTuning.Create(
                    DeployableTuning.TimedOverrideInterval,
                    DeployableTuning.TimedOverrideDuration))
        });

        persistents.NotifyUltimateActivated();
        Assert.IsTrue(persistents.TryGetAttackIntervalOverride(out var timed));
        Assert.AreEqual(DeployableTuning.TimedOverrideInterval, timed, 0.001f);

        persistents.AdvanceTimedOverrideForTests(DeployableTuning.TimedOverrideDuration + 0.1f);
        Assert.IsFalse(persistents.TryGetAttackIntervalOverride(out _));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void PermanentOverride_WinsOverTimed()
    {
        var go = new GameObject("Player");
        var persistents = go.AddComponent<PlayerPersistentEffects>();
        persistents.ReplaceAll(new List<PersistentEffectBinding>
        {
            new PersistentEffectBinding(
                PersistentEffectKind.TimedAttackIntervalOverride,
                EffectTuning.Create(0.22f, 6f)),
            new PersistentEffectBinding(
                PersistentEffectKind.PermanentAttackIntervalOverride,
                EffectTuning.Create(DeployableTuning.PermanentOverrideInterval, DeployableTuning.PermanentDamageMultiplier))
        });

        persistents.NotifyUltimateActivated();
        Assert.IsTrue(persistents.TryGetAttackIntervalOverride(out var interval));
        Assert.AreEqual(DeployableTuning.PermanentOverrideInterval, interval, 0.001f);
        Assert.AreEqual(DeployableTuning.PermanentDamageMultiplier, persistents.GetBasicDamageMultiplier(), 0.001f);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Stacks_SameTargetBuilds_TargetChangeResets_TimeoutResets()
    {
        var go = new GameObject("Player");
        var persistents = go.AddComponent<PlayerPersistentEffects>();
        persistents.ReplaceAll(new List<PersistentEffectBinding>
        {
            new PersistentEffectBinding(PersistentEffectKind.PerTargetHitStacks)
        });

        var t1 = new GameObject("Target1");
        var t2 = new GameObject("Target2");

        Assert.AreEqual(10f, persistents.ApplyPerTargetHitMultiplier(t1, 10f), 0.001f);
        Assert.AreEqual(12f, persistents.ApplyPerTargetHitMultiplier(t1, 10f), 0.001f);
        Assert.AreEqual(10f, persistents.ApplyPerTargetHitMultiplier(t2, 10f), 0.001f);

        persistents.ApplyPerTargetHitMultiplier(t1, 10f);
        persistents.SetStackLastHitTimeForTests(Time.time - DeployableTuning.StackResetTimeout - 0.1f);
        Assert.AreEqual(10f, persistents.ApplyPerTargetHitMultiplier(t1, 10f), 0.001f);

        Object.DestroyImmediate(t1);
        Object.DestroyImmediate(t2);
        Object.DestroyImmediate(go);
    }
}
