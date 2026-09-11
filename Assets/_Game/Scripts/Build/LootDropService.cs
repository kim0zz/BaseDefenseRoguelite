using UnityEngine;

/// <summary>
/// Losowanie i spawn dropów (M6/M7).
/// </summary>
public static class LootDropService
{
    public static void TryDropFromEnemy(Vector3 position, bool eliteKill)
    {
        if (!LootLoopConfig.LootEnabledInRun) return;

        var catalog = BuildSystemBootstrap.GetCatalogOrDefault();
        if (catalog == null) return;

        var chance = eliteKill ? catalog.RareDropChance : catalog.CommonDropChance;
        if (Random.value > chance) return;

        if (Random.value < 0.55f)
        {
            var weapon = catalog.RollWeaponDrop(eliteKill);
            if (weapon != null)
                LootPickup.Spawn(position, weapon, null);
            return;
        }

        var item = catalog.RollItemDrop(eliteKill);
        if (item != null)
            LootPickup.Spawn(position, null, item);
    }

    public static void DropBossUnique(Vector3 position)
    {
        if (!LootLoopConfig.LootEnabledInRun) return;

        var catalog = BuildSystemBootstrap.GetCatalogOrDefault();
        if (catalog == null) return;

        var weapon = catalog.RollBossUniqueWeapon();
        if (weapon != null)
            LootPickup.Spawn(position, weapon, null);
    }
}
