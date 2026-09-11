/// <summary>
/// Wynik zastosowania wymuszonego ruchu — do telemetrii (M7.5-T1).
/// </summary>
public readonly struct ForcedMovementOutcome
{
    public bool Applied { get; }
    public float AppliedControlSeconds { get; }
    public bool AppliedStaggerInsteadOfStun { get; }

    public ForcedMovementOutcome(bool applied, float appliedControlSeconds, bool appliedStaggerInsteadOfStun)
    {
        Applied = applied;
        AppliedControlSeconds = appliedControlSeconds;
        AppliedStaggerInsteadOfStun = appliedStaggerInsteadOfStun;
    }

    public static ForcedMovementOutcome None => new(false, 0f, false);
}
