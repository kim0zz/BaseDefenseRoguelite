using NUnit.Framework;
using UnityEngine;

public class StructureTargetingTests
{
    [Test]
    public void RusherSiegeFlanker_PreferStructures()
    {
        Assert.IsTrue(StructureTargeting.PrefersStructureOverPlayer(EnemyKind.Rusher));
        Assert.IsTrue(StructureTargeting.PrefersStructureOverPlayer(EnemyKind.Siege));
        Assert.IsTrue(StructureTargeting.PrefersStructureOverPlayer(EnemyKind.Flanker));
        Assert.IsFalse(StructureTargeting.PrefersStructureOverPlayer(EnemyKind.Grunt));
        Assert.IsFalse(StructureTargeting.PrefersStructureOverPlayer(EnemyKind.Support));
        Assert.IsFalse(StructureTargeting.PrefersStructureOverPlayer(EnemyKind.Shielder));
    }

    [Test]
    public void IsStructureInRange_UsesAttackRangeNotDetectRange()
    {
        Assert.IsFalse(StructureTargeting.IsStructureInRange(null, Vector3.zero, 1.5f));
    }
}
