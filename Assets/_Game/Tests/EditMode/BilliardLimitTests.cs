using NUnit.Framework;

public class BilliardLimitTests
{
    [Test]
    public void Billiard_StopsAtThreeCollisions()
    {
        Assert.IsTrue(CollisionChainMotor.WouldExceedLimits(3, 0f, 0f, elite: false));
        Assert.IsFalse(CollisionChainMotor.WouldExceedLimits(2, 0f, 0f, elite: false));
    }

    [Test]
    public void Billiard_StopsAt085Seconds()
    {
        Assert.IsTrue(CollisionChainMotor.WouldExceedLimits(0, 0.85f, 0f, elite: false));
        Assert.IsFalse(CollisionChainMotor.WouldExceedLimits(0, 0.84f, 0f, elite: false));
    }

    [Test]
    public void Billiard_StopsAt65Meters()
    {
        Assert.IsTrue(CollisionChainMotor.WouldExceedLimits(0, 0f, 6.5f, elite: false));
        Assert.IsFalse(CollisionChainMotor.WouldExceedLimits(0, 0f, 6.4f, elite: false));
    }

    [Test]
    public void Billiard_Elite_HasCollisionCapOne()
    {
        Assert.IsTrue(CollisionChainMotor.WouldExceedLimits(1, 0f, 0f, elite: true));
        Assert.IsFalse(CollisionChainMotor.WouldExceedLimits(0, 0f, 0f, elite: true));
    }
}
