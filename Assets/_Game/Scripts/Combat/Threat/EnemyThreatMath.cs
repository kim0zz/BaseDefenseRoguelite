/// <summary>
/// Reguły prowokacji / tauntu (M7.6-T2). EditMode-friendly, bez stanu.
/// </summary>
public static class EnemyThreatMath
{
    public static bool CanBeTaunted(EnemyKind kind, bool isBoss)
    {
        if (isBoss) return false;
        return kind is EnemyKind.Grunt
            or EnemyKind.Hunter
            or EnemyKind.Rusher
            or EnemyKind.Carrier
            or EnemyKind.Siege
            or EnemyKind.Support
            or EnemyKind.Flanker
            or EnemyKind.Shielder;
    }

    /// <summary>StaysOnLane — taunter musi być w korytarzu linii wroga.</summary>
    public static bool GruntRequiresTaunterInCorridor(EnemyKind kind)
    {
        return kind is EnemyKind.Grunt
            or EnemyKind.Carrier
            or EnemyKind.Support
            or EnemyKind.Shielder;
    }

    /// <summary>Rusher/Siege/Flanker przestają atakować strukturę na czas aktywnego tauntu.</summary>
    public static bool IgnoresStructureWhileTaunted(EnemyKind kind, bool threatActive)
    {
        if (!threatActive) return false;
        return kind is EnemyKind.Rusher or EnemyKind.Siege or EnemyKind.Flanker;
    }
}
