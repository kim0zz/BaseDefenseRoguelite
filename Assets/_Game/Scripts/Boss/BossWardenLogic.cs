using UnityEngine;

/// <summary>
/// Logika czysta bossa Warden — testowalna bez sceny (M8.3).
/// </summary>
public static class BossWardenLogic
{
    public static int GetPhaseFromHealthRatio(float healthRatio)
    {
        if (healthRatio > 0.70f) return 0;
        if (healthRatio > 0.40f) return 1;
        if (healthRatio > 0.20f) return 2;
        return 3;
    }

    public static int AdvancePhase(int currentHighestPhase, float healthRatio)
    {
        var phase = GetPhaseFromHealthRatio(healthRatio);
        return phase > currentHighestPhase ? phase : currentHighestPhase;
    }

    public static float GetSlamInterval(int phase, WardenDefinition definition)
    {
        if (definition == null) return phase >= 3 ? 2f : 3.4f;
        return phase >= 3 ? definition.SlamIntervalFrenzy : definition.SlamInterval;
    }

    public static float GetDamageTakenMultiplier(bool anyTotemAlive, WardenDefinition definition)
    {
        if (!anyTotemAlive || definition == null) return 1f;
        return definition.DamageTakenWhileTotemsAlive;
    }

    public static bool ShouldSpawnTotemsOnce(int phase, bool totemsAlreadySpawned) =>
        !totemsAlreadySpawned && phase >= 1;

    public static bool ShouldDisableTowerOnce(int phase, bool towerAlreadyDisabled) =>
        !towerAlreadyDisabled && phase >= 2;

    public static bool ShouldStartFrenzyZones(int phase) => phase >= 3;

    public static string GetPhaseHudLabel(int phase) => phase switch
    {
        1 => "Totemy",
        2 => "Wieża wyłączona",
        3 => "Szał",
        _ => "Walka"
    };
}
