using UnityEngine;

/// <summary>
/// Czyste reguły UI level-up (M7 co-op) — testowalne bez sceny.
/// </summary>
public static class LevelUpSelectionRules
{
    public static bool AllPlayersReady(bool[] readyFlags)
    {
        if (readyFlags == null || readyFlags.Length == 0) return false;
        foreach (var ready in readyFlags)
        {
            if (!ready) return false;
        }

        return true;
    }

    public static int ClampOptionIndex(int selectedIndex, int optionCount)
    {
        if (optionCount <= 0) return 0;
        return Mathf.Clamp(selectedIndex, 0, optionCount - 1);
    }

    /// <summary>
    /// FROZEN: świat wraca po zatwierdzeniu talentów przez wszystkich aktywnych.
    /// Nie wymaga drugiego Enter/Space — overlay już znika na complete.
    /// </summary>
    public static bool ShouldResumeWorldWhenReady(bool allPlayersReady)
    {
        return allPlayersReady;
    }
}
