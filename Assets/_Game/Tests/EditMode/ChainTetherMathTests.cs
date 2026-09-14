using NUnit.Framework;
using UnityEngine;

public sealed class ChainTetherMathTests
{
    private static ChainTetherTuning Baseline => ChainTetherTuning.BaselineDraft;

    [Test]
    public void SlackZone_HasZeroPull()
    {
        var result = ChainTetherMath.Evaluate(4.5f, 0f, 0f, Baseline);
        Assert.AreEqual(ChainTetherZone.Slack, result.Zone);
        Assert.AreEqual(0f, result.DampedAcceleration, 0.0001f);
    }

    [Test]
    public void SoftZone_HasPositivePull()
    {
        var result = ChainTetherMath.Evaluate(6.5f, 0f, 0f, Baseline);
        Assert.AreEqual(ChainTetherZone.Soft, result.Zone);
        Assert.Greater(result.DampedAcceleration, 0f);
    }

    [Test]
    public void HardZone_PullExceedsSoftAtSameRelativeSpeed()
    {
        var soft = ChainTetherMath.Evaluate(6.5f, 0f, 0f, Baseline);
        var hard = ChainTetherMath.Evaluate(10f, 0f, 0f, Baseline);
        Assert.AreEqual(ChainTetherZone.Soft, soft.Zone);
        Assert.AreEqual(ChainTetherZone.Hard, hard.Zone);
        Assert.Greater(hard.DampedAcceleration, soft.DampedAcceleration);
    }

    [Test]
    public void Damping_OpposesExtensionSpeed()
    {
        var extending = ChainTetherMath.Evaluate(6.5f, 2f, 0f, Baseline);
        var still = ChainTetherMath.Evaluate(6.5f, 0f, 0f, Baseline);
        Assert.Less(extending.DampedAcceleration, still.DampedAcceleration);
    }

    [Test]
    public void Evaluate_RejectsNaN_Distance()
    {
        Assert.Throws<System.ArgumentException>(() =>
            ChainTetherMath.Evaluate(float.NaN, 0f, 0f, Baseline));
    }

    [Test]
    public void Evaluate_ProducesFiniteValuesAtExtremeDistance()
    {
        var ok = ChainTetherMath.Evaluate(50f, 10f, 1f, Baseline);
        Assert.IsFalse(float.IsNaN(ok.DampedAcceleration));
        Assert.IsFalse(float.IsInfinity(ok.DampedAcceleration));
    }

    [Test]
    public void BeyondMaxStretch_ForceRemainsFinite()
    {
        var result = ChainTetherMath.Evaluate(40f, 0f, 0f, Baseline);
        Assert.IsFalse(float.IsNaN(result.DampedAcceleration));
        Assert.IsFalse(float.IsInfinity(result.DampedAcceleration));
        Assert.AreEqual(Baseline.maxPullAcceleration, result.DampedAcceleration, 0.0001f);
    }

    [Test]
    public void Agency_ReducesPullWhenInputAway()
    {
        var neutral = ChainTetherMath.Evaluate(6.5f, 0f, 0f, Baseline);
        var resisting = ChainTetherMath.Evaluate(6.5f, 0f, 1f, Baseline);
        Assert.Greater(neutral.DampedAcceleration, resisting.DampedAcceleration);
    }

    [Test]
    public void IntegrateTetherVelocity_KeepsMomentumAfterForceStops()
    {
        var afterPush = ChainTetherMath.IntegrateTetherVelocity(
            Vector3.zero, Vector3.right * 20f, 0.05f, 2f, 18f);
        Assert.Greater(afterPush.x, 0f);

        var coast = ChainTetherMath.IntegrateTetherVelocity(
            afterPush, Vector3.zero, 0.05f, 2f, 18f);
        Assert.Greater(coast.x, 0f);
        Assert.Less(coast.x, afterPush.x);
    }
}
