using NUnit.Framework;
using UnityEngine;

public class BurnStatusTests
{
    [Test]
    public void BurnTick_DamagesTarget()
    {
        var go = new GameObject("BurnTarget");
        var health = go.AddComponent<Health>();
        health.Configure(100f);
        var receiver = go.AddComponent<StatusEffectReceiver>();

        receiver.Apply(StatusEffectType.Burn, DeployableTuning.BurnDuration, DeployableTuning.BurnTickDamage);
        receiver.TickForTests(DeployableTuning.BurnTickInterval);
        Assert.Less(health.CurrentHealth, 100f);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void StatusTick_DoesNotApplyBurnFromExplosion()
    {
        var owner = new GameObject("Caster");
        var persistents = owner.AddComponent<PlayerPersistentEffects>();
        persistents.ReplaceAll(new System.Collections.Generic.List<PersistentEffectBinding>
        {
            new PersistentEffectBinding(PersistentEffectKind.ApplyBurnOnPlayerDamage)
        });

        var target = new GameObject("Target");
        target.AddComponent<Health>().Configure(50f);
        var status = target.AddComponent<StatusEffectReceiver>();

        ExplosionResolver.Explode(
            target.transform.position,
            2f,
            5f,
            owner,
            8,
            0f,
            canApplyBurn: false);

        Assert.IsFalse(status.HasEffect(StatusEffectType.Burn));
        Object.DestroyImmediate(owner);
        Object.DestroyImmediate(target);
    }

    [Test]
    public void BurnTick_DoesNotReapplyBurnViaDamagedHook()
    {
        var go = new GameObject("BurnLoop");
        var health = go.AddComponent<Health>();
        health.Configure(100f);
        var receiver = go.AddComponent<StatusEffectReceiver>();
        var burnApplications = 0;
        health.Damaged += (_, _) => burnApplications++;

        receiver.Apply(StatusEffectType.Burn, 2f, 2f);
        receiver.TickForTests(0.5f);
        Assert.AreEqual(0, burnApplications);

        Object.DestroyImmediate(go);
    }
}
