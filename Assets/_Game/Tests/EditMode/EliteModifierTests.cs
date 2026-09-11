using NUnit.Framework;

public class EliteModifierTests
{
    [Test]
    public void Armored_IncreasesMaxHealthProxy()
    {
        Assert.AreEqual(30f, EliteModifierMath.ApplyMaxHealthMultiplier(EliteModifier.Armored, 20f));
    }

    [Test]
    public void Frenzy_IncreasesMoveAndAttackSpeed()
    {
        Assert.AreEqual(5f, EliteModifierMath.ApplyMoveSpeedMultiplier(EliteModifier.Frenzy, 4f));
        Assert.Less(EliteModifierMath.ApplyAttackIntervalMultiplier(EliteModifier.Frenzy, 2f), 2f);
    }

    [Test]
    public void Frenzy_MoveSpeed_OnSupportKind_UsesSameMath()
    {
        // M8_Support baseline moveSpeed = 3.4 — elita Frenzy bez osobnego AI.
        Assert.AreEqual(4.25f, EliteModifierMath.ApplyMoveSpeedMultiplier(EliteModifier.Frenzy, 3.4f));
    }

    [Test]
    public void Carrier_IsNotEliteForLoot()
    {
        Assert.IsFalse(EliteModifierMath.CountsAsEliteForLoot(EliteModifier.None, EnemyKind.Carrier));
        Assert.IsTrue(EliteModifierMath.CountsAsEliteForLoot(EliteModifier.Frenzy, EnemyKind.Grunt));
        Assert.IsTrue(EliteModifierMath.CountsAsEliteForLoot(EliteModifier.None, EnemyKind.Hunter));
    }
}
