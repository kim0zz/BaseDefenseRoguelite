using NUnit.Framework;

public class StunFeedbackRulesTests
{
    [Test]
    public void ShouldShow_TrueWhenStunned()
    {
        Assert.IsTrue(StunFeedbackRules.ShouldShow(true));
    }

    [Test]
    public void ShouldShow_FalseWhenNotStunned()
    {
        Assert.IsFalse(StunFeedbackRules.ShouldShow(false));
    }
}
