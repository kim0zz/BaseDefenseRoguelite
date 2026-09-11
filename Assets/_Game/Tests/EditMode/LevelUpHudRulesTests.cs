using NUnit.Framework;

public class LevelUpHudRulesTests
{
    [Test]
    public void LevelUpPause_WithController_ShowsOverlay()
    {
        Assert.IsTrue(LevelUpHudRules.ShouldShowOverlay(
            GameFlowState.LevelUpPause, hasBuildFlowController: true, selectionsComplete: false));
    }

    [Test]
    public void LevelUpPause_MissingController_DoesNotShowOverlay()
    {
        Assert.IsFalse(LevelUpHudRules.ShouldShowOverlay(
            GameFlowState.LevelUpPause, hasBuildFlowController: false, selectionsComplete: false));
    }

    [Test]
    public void WaveActive_DoesNotShowOverlay()
    {
        Assert.IsFalse(LevelUpHudRules.ShouldShowOverlay(
            GameFlowState.WaveActive, hasBuildFlowController: true, selectionsComplete: false));
    }
}
