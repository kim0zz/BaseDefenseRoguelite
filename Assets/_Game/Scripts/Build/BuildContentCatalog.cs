using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Katalog contentu build systemu (M6) — lookup klas, broni, itemów, talentów.
/// </summary>
[CreateAssetMenu(fileName = "BuildContentCatalog", menuName = "Game/Build/Build Content Catalog")]
public class BuildContentCatalog : ScriptableObject
{
    [SerializeField] private ClassDefinition[] classes;
    [SerializeField] private WeaponDefinition[] weapons;
    [SerializeField] private ItemDefinition[] items;
    [SerializeField] private TalentDefinition[] talents;
    [SerializeField] private ClassProgressionTable[] progressionTables;
    [SerializeField] [Range(0f, 1f)] private float commonDropChance = 0.12f;
    [SerializeField] [Range(0f, 1f)] private float rareDropChance = 0.35f;

    public float CommonDropChance => commonDropChance;
    public float RareDropChance => rareDropChance;

    public ClassDefinition GetClass(PlayerClassId classId)
    {
        if (classes == null) return null;
        foreach (var entry in classes)
        {
            if (entry != null && entry.ClassId == classId)
                return entry;
        }
        return null;
    }

    public ClassDefinition GetDefaultClassForPlayerSlot(int playerSlotIndex)
    {
        var defaults = new[]
        {
            PlayerClassId.Pudzian,
            PlayerClassId.Cwel,
            PlayerClassId.Jamie,
            PlayerClassId.Cipak
        };
        var index = Mathf.Clamp(playerSlotIndex, 0, defaults.Length - 1);
        return GetClass(defaults[index]);
    }

    public WeaponDefinition GetWeaponById(string weaponId)
    {
        if (weapons == null || string.IsNullOrEmpty(weaponId)) return null;
        foreach (var weapon in weapons)
        {
            if (weapon != null && weapon.WeaponId == weaponId)
                return weapon;
        }
        return null;
    }

    public ItemDefinition GetItemById(string itemId)
    {
        if (items == null || string.IsNullOrEmpty(itemId)) return null;
        foreach (var item in items)
        {
            if (item != null && item.ItemId == itemId)
                return item;
        }
        return null;
    }

    public IReadOnlyList<TalentDefinition> AllTalents => talents ?? System.Array.Empty<TalentDefinition>();

    public ClassProgressionTable GetProgression(PlayerClassId classId)
    {
        if (progressionTables == null) return null;
        foreach (var table in progressionTables)
        {
            if (table != null && table.ClassId == classId)
                return table;
        }

        return null;
    }

    public IReadOnlyList<TalentDefinition> BuildLevelOffer(
        ProgressionSnapshot snapshot,
        IMetaUnlockQuery meta = null)
    {
        return OfferGenerator.Build(GetProgression(snapshot.ClassId), talents, snapshot, meta);
    }

    public IReadOnlyList<TalentDefinition> GetAvailableTalents(
        PlayerClassId classId,
        int teamLevel,
        IReadOnlyList<string> chosenTalentIds)
    {
        var snapshot = new ProgressionSnapshot(
            classId,
            teamLevel,
            chosenTalentIds,
            "",
            TalentTagCounter.Compute(FindTalents(chosenTalentIds)));
        return BuildLevelOffer(snapshot);
    }

    private List<TalentDefinition> FindTalents(IReadOnlyList<string> ids)
    {
        var found = new List<TalentDefinition>();
        if (ids == null || talents == null) return found;
        foreach (var id in ids)
        {
            foreach (var talent in talents)
            {
                if (talent != null && talent.TalentId == id)
                    found.Add(talent);
            }
        }

        return found;
    }

    public WeaponDefinition RollWeaponDrop(bool eliteKill)
    {
        if (weapons == null || weapons.Length == 0) return null;

        var pool = new List<WeaponDefinition>();
        foreach (var weapon in weapons)
        {
            if (weapon == null) continue;
            if (weapon.Rarity == LootRarity.Unique || weapon.IsBossUnique) continue;
            if (eliteKill && weapon.Rarity == LootRarity.Rare)
                pool.Add(weapon);
            else if (!eliteKill && weapon.Rarity == LootRarity.Common)
                pool.Add(weapon);
        }

        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    public WeaponDefinition RollBossUniqueWeapon()
    {
        if (weapons == null || weapons.Length == 0) return null;

        var pool = new List<WeaponDefinition>();
        foreach (var weapon in weapons)
        {
            if (weapon == null) continue;
            if (weapon.Rarity == LootRarity.Unique || weapon.IsBossUnique)
                pool.Add(weapon);
        }

        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    public ItemDefinition RollItemDrop(bool eliteKill)
    {
        if (items == null || items.Length == 0) return null;

        var pool = new List<ItemDefinition>();
        foreach (var item in items)
        {
            if (item == null) continue;
            if (eliteKill && item.Rarity == LootRarity.Rare)
                pool.Add(item);
            else if (!eliteKill && item.Rarity == LootRarity.Common)
                pool.Add(item);
        }

        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    /// <summary>Inicjalizacja runtime (fabryka domyślna M6).</summary>
    public void InitializeRuntime(
        ClassDefinition[] classEntries,
        WeaponDefinition[] weaponEntries,
        ItemDefinition[] itemEntries,
        TalentDefinition[] talentEntries,
        ClassProgressionTable[] progressionEntries = null,
        float commonChance = 0.12f,
        float rareChance = 0.35f)
    {
        classes = classEntries;
        weapons = weaponEntries;
        items = itemEntries;
        talents = talentEntries;
        progressionTables = progressionEntries;
        commonDropChance = commonChance;
        rareDropChance = rareChance;
    }
}

/// <summary>
/// Reguły gałęzi talentów zgodne z FROZEN PLAYER_PROGRESSION.md.
/// </summary>
public static class TalentPathRules
{
    public static bool IsBranchAvailable(TalentDefinition talent, IReadOnlyList<string> chosenBranchKeys)
    {
        if (talent == null) return false;

        var parent = talent.RequiredParentBranch;
        if (string.IsNullOrEmpty(parent))
            return true;

        if (chosenBranchKeys == null) return false;

        foreach (var key in chosenBranchKeys)
        {
            if (key == parent)
                return true;
        }

        return false;
    }

    public static bool HasChosenBranch(IReadOnlyList<string> chosenBranchKeys, int teamLevel)
    {
        if (chosenBranchKeys == null) return false;

        foreach (var key in chosenBranchKeys)
        {
            if (string.IsNullOrEmpty(key)) continue;
            if (key.Length == 1 && teamLevel is 2 or 4)
                return true;
            if (key.Length == 2 && teamLevel is 3 or 5)
                return true;
        }

        return false;
    }
}
