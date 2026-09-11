using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Czysta logika warunków przegranej runu (M8.0) — testowalna bez sceny.
/// </summary>
public enum RunFailReason
{
    None,
    BaseDestroyed,
    TeamWiped
}

public static class RunFailRules
{
    public static bool IsTeamWipe(int joinedCount, int livingCount) =>
        joinedCount > 0 && livingCount == 0;

    public static bool IsBaseFail(bool baseAlive) => !baseAlive;

    public static bool ShouldEnemyCombatTick(GameFlowState flowState) =>
        flowState == GameFlowState.WaveActive;

    public static void CountJoinedAndLiving(
        IReadOnlyList<PlayerCharacter> players,
        out int joinedCount,
        out int livingCount)
    {
        joinedCount = 0;
        livingCount = 0;
        if (players == null) return;

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (player == null) continue;

            joinedCount++;
            var health = player.GetComponent<Health>();
            if (health != null && health.IsAlive)
                livingCount++;
        }
    }
}
