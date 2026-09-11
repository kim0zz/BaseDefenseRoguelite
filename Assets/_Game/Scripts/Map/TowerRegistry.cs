using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rejestr wież greybox — lookup po linii dla wrogów i bossa (M7).
/// </summary>
public static class TowerRegistry
{
    private static readonly List<TowerHealth> Towers = new();

    public static IReadOnlyList<TowerHealth> All => Towers;

    public static void Register(TowerHealth tower)
    {
        if (tower == null || Towers.Contains(tower)) return;
        Towers.Add(tower);
    }

    public static void Unregister(TowerHealth tower)
    {
        Towers.Remove(tower);
    }

    public static TowerHealth GetLineTower(AttackLineId line)
    {
        for (var i = 0; i < Towers.Count; i++)
        {
            var tower = Towers[i];
            if (tower == null || !tower.IsOperational) continue;
            if (tower.Role == TowerRole.LineTower && tower.AttackLine == line)
                return tower;
        }

        return null;
    }

    public static TowerHealth PickRandomLivingLineTower()
    {
        var candidates = new List<TowerHealth>();
        for (var i = 0; i < Towers.Count; i++)
        {
            var tower = Towers[i];
            if (tower != null && tower.IsOperational && tower.Role == TowerRole.LineTower)
                candidates.Add(tower);
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    public static void ClearForTests()
    {
        Towers.Clear();
    }
}
