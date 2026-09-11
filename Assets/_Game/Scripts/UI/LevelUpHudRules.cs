/// <summary>
/// Kiedy pokazać overlay awansu (M9.0 hotfix) — bez sceny.
/// </summary>
public static class LevelUpHudRules
{
    public static bool ShouldShowOverlay(
        GameFlowState flowState,
        bool hasBuildFlowController,
        bool selectionsComplete)
    {
        return flowState == GameFlowState.LevelUpPause
               && hasBuildFlowController
               && !selectionsComplete;
    }
}
