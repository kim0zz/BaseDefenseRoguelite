/// <summary>
/// Flagi źródła obrażeń — tick statusu nie wywołuje on-hit procs (M8.4).
/// </summary>
[System.Flags]
public enum DamageHitFlags
{
    None = 0,
    StatusTick = 1
}
