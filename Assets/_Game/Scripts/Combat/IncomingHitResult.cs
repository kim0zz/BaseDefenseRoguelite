/// <summary>
/// Result of incoming damage pipeline (M8.1b).
/// </summary>
public struct IncomingHitResult
{
    public float FinalDamage;
    public float HealAmount;
    public bool Negated;
    public bool ConvertedToHeal;
    public bool HartConsumed;
    public bool CountedForMeter;
}
