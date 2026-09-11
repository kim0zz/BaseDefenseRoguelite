/// <summary>
/// Skale odporności DRAFT — dane, nie logika skilla (M7.5-T1).
/// </summary>
public static class ForcedMovementResistanceMath
{
    public const float EliteDisplacementScale = 0.5f;
    public const float EliteStunScale = 0.5f;
    public const float BossStaggerContributionScale = 1f;

    public static float GetDisplacementScale(ForcedMovementResistanceCategory category)
    {
        return category switch
        {
            ForcedMovementResistanceCategory.Normal => 1f,
            ForcedMovementResistanceCategory.Elite => EliteDisplacementScale,
            ForcedMovementResistanceCategory.Boss => 0f,
            ForcedMovementResistanceCategory.Structure => 0f,
            _ => 1f
        };
    }

    public static float GetStunScale(ForcedMovementResistanceCategory category)
    {
        return category switch
        {
            ForcedMovementResistanceCategory.Normal => 1f,
            ForcedMovementResistanceCategory.Elite => EliteStunScale,
            ForcedMovementResistanceCategory.Boss => 0f,
            ForcedMovementResistanceCategory.Structure => 0f,
            _ => 1f
        };
    }

    public static bool AllowsDisplacement(ForcedMovementResistanceCategory category)
    {
        return GetDisplacementScale(category) > 0f;
    }

    public static bool ConvertsControlToStagger(ForcedMovementResistanceCategory category)
    {
        return category == ForcedMovementResistanceCategory.Boss;
    }
}
