using NUnit.Framework;
using UnityEngine;

public class StatusEffectTests
{
    [Test]
    public void Poison_AppliesTicksAndExpires()
    {
        var go = new GameObject("StatusTarget");
        var health = go.AddComponent<Health>();
        health.Configure(100f);
        var receiver = go.AddComponent<StatusEffectReceiver>();

        receiver.Apply(StatusEffectType.Poison, 2.2f, 2f);
        receiver.TickForTests(1.1f);
        Assert.Less(health.CurrentHealth, 100f);
        Assert.IsTrue(receiver.HasEffect(StatusEffectType.Poison));

        receiver.TickForTests(1.2f);
        Assert.IsFalse(receiver.HasEffect(StatusEffectType.Poison));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Stagger_AccumulatesAndCanBeConsumed()
    {
        var go = new GameObject("StaggerTarget");
        go.AddComponent<StatusEffectReceiver>();
        var receiver = go.GetComponent<StatusEffectReceiver>();

        receiver.AddStagger(10f);
        receiver.AddStagger(15f);
        Assert.IsFalse(receiver.TryConsumeStagger(30f));
        Assert.IsTrue(receiver.TryConsumeStagger(25f));
        Assert.AreEqual(0f, receiver.StaggerAccumulated);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Stun_BlocksMovementFlag()
    {
        var go = new GameObject("StunTarget");
        var receiver = go.AddComponent<StatusEffectReceiver>();
        receiver.Apply(StatusEffectType.Stun, 1f, 1f);
        Assert.IsTrue(receiver.BlocksMovement);
        Assert.IsTrue(receiver.BlocksAttack);

        Object.DestroyImmediate(go);
    }
}
