using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rejestr aktywnych wrogów — cache do separacji bez FindObjectsByType (M6.5).
/// </summary>
public static class EnemyRegistry
{
    private static readonly List<EnemyController> ActiveEnemies = new();

    public static IReadOnlyList<EnemyController> Active => ActiveEnemies;

    public static void Register(EnemyController enemy)
    {
        if (enemy == null || ActiveEnemies.Contains(enemy)) return;
        ActiveEnemies.Add(enemy);
    }

    public static void Unregister(EnemyController enemy)
    {
        if (enemy == null) return;
        ActiveEnemies.Remove(enemy);
    }

    public static void Clear()
    {
        ActiveEnemies.Clear();
    }
}
