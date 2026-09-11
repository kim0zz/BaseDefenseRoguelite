using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stan buildu gracza: klasa, bronie, itemy, talenty (M6).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerCharacter))]
public class PlayerBuildState : MonoBehaviour
{
    private readonly WeaponDefinition[] _weapons = new WeaponDefinition[BuildStatCalculator.WeaponSlotCount];
    private readonly ItemDefinition[] _items = new ItemDefinition[BuildStatCalculator.ItemSlotCount];
    private readonly List<TalentDefinition> _chosenTalents = new();
    private readonly List<string> _chosenBranchKeys = new();

    private ClassDefinition _classDefinition;
    private int _activeWeaponIndex;
    private int _teamLevel = 1;
    private bool _pendingTalentSelection;

    public ClassDefinition ClassDefinition => _classDefinition;
    public PlayerClassId ClassId => _classDefinition != null ? _classDefinition.ClassId : PlayerClassId.Jamie;
    public int ActiveWeaponIndex => _activeWeaponIndex;
    public IReadOnlyList<string> ChosenBranchKeys => _chosenBranchKeys;
    public IReadOnlyList<TalentDefinition> ChosenTalents => _chosenTalents;
    public bool PendingTalentSelection => _pendingTalentSelection;

    public event Action BuildChanged;

    public WeaponDefinition GetWeapon(int slot)
    {
        if (slot < 0 || slot >= _weapons.Length) return null;
        return _weapons[slot];
    }

    public ItemDefinition GetItem(int slot)
    {
        if (slot < 0 || slot >= _items.Length) return null;
        return _items[slot];
    }

    public void Initialize(ClassDefinition classDefinition, BuildContentCatalog catalog, int playerSlotIndex, int teamLevel)
    {
        _classDefinition = classDefinition ?? catalog?.GetDefaultClassForPlayerSlot(playerSlotIndex);
        _teamLevel = Mathf.Max(1, teamLevel);
        _activeWeaponIndex = 0;
        _chosenTalents.Clear();
        _chosenBranchKeys.Clear();
        _pendingTalentSelection = false;

        if (_classDefinition != null)
        {
            EquipWeaponInternal(0, _classDefinition.StartingWeaponPrimary);
            EquipWeaponInternal(1, _classDefinition.StartingWeaponSecondary);
        }

        ApplyStatsToCharacter();
        BuildChanged?.Invoke();
    }

    public void SetTeamLevel(int teamLevel)
    {
        _teamLevel = Mathf.Max(1, teamLevel);
        ApplyStatsToCharacter();
        BuildChanged?.Invoke();
    }

    public void MarkPendingTalentSelection(bool pending)
    {
        _pendingTalentSelection = pending;
        BuildChanged?.Invoke();
    }

    public ProgressionSnapshot CreateSnapshot(int teamLevel)
    {
        var ids = new string[_chosenTalents.Count];
        for (var i = 0; i < _chosenTalents.Count; i++)
            ids[i] = _chosenTalents[i] != null ? _chosenTalents[i].TalentId : "";

        return new ProgressionSnapshot(
            ClassId,
            teamLevel,
            ids,
            ProgressionApplier.ReadUltimateSkillId(_chosenTalents),
            TalentTagCounter.Compute(_chosenTalents));
    }

    public bool NeedsTalentSelection(BuildContentCatalog catalog, int teamLevel)
    {
        if (catalog == null || _classDefinition == null) return false;
        if (HasChosenTalentForLevel(teamLevel)) return false;

        var options = catalog.BuildLevelOffer(CreateSnapshot(teamLevel));
        return options.Count > 0;
    }

    public bool HasChosenTalentForLevel(int teamLevel)
    {
        foreach (var talent in _chosenTalents)
        {
            if (talent != null && talent.RequiredTeamLevel == teamLevel)
                return true;
        }
        return false;
    }

    public IReadOnlyList<TalentDefinition> GetAvailableTalents(BuildContentCatalog catalog, int teamLevel)
    {
        if (catalog == null) return System.Array.Empty<TalentDefinition>();
        return catalog.BuildLevelOffer(CreateSnapshot(teamLevel));
    }

    public bool TryApplyTalent(TalentDefinition talent, int teamLevel)
    {
        if (talent == null) return false;
        if (talent.RequiredTeamLevel != teamLevel) return false;
        if (talent.PlayerClass != ClassId) return false;
        if (HasChosenTalentForLevel(teamLevel)) return false;
        if (!TalentEligibility.IsEligible(talent, CreateSnapshot(teamLevel), AlwaysUnlockedMetaQuery.Instance))
            return false;

        _chosenTalents.Add(talent);
        if (!string.IsNullOrEmpty(talent.BranchKey))
            _chosenBranchKeys.Add(talent.BranchKey);
        _pendingTalentSelection = false;
        ApplyStatsToCharacter();
        BuildChanged?.Invoke();
        return true;
    }

    public bool EquipWeapon(int slot, WeaponDefinition weapon)
    {
        if (slot < 0 || slot >= _weapons.Length) return false;
        EquipWeaponInternal(slot, weapon);
        ApplyStatsToCharacter();
        BuildChanged?.Invoke();
        return true;
    }

    public bool EquipItem(int slot, ItemDefinition item)
    {
        if (slot < 0 || slot >= _items.Length) return false;
        _items[slot] = item;
        ApplyStatsToCharacter();
        BuildChanged?.Invoke();
        return true;
    }

    public int FindFirstFreeItemSlot()
    {
        for (var i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null)
                return i;
        }
        return -1;
    }

    public bool TryPickupWeapon(WeaponDefinition weapon)
    {
        if (weapon == null) return false;

        if (_weapons[0] == null)
            return EquipWeapon(0, weapon);
        if (_weapons[1] == null)
            return EquipWeapon(1, weapon);

        return EquipWeapon(_activeWeaponIndex, weapon);
    }

    public bool TryPickupItem(ItemDefinition item)
    {
        if (item == null) return false;
        var slot = FindFirstFreeItemSlot();
        if (slot < 0) return false;
        return EquipItem(slot, item);
    }

    public bool TrySwapWeaponSlotWith(PlayerBuildState other, int localSlot, int otherSlot)
    {
        if (other == null) return false;
        if (localSlot < 0 || localSlot >= _weapons.Length) return false;
        if (otherSlot < 0 || otherSlot >= BuildStatCalculator.WeaponSlotCount) return false;

        var temp = _weapons[localSlot];
        _weapons[localSlot] = other._weapons[otherSlot];
        other._weapons[otherSlot] = temp;

        ApplyStatsToCharacter();
        other.ApplyStatsToCharacter();
        BuildChanged?.Invoke();
        other.BuildChanged?.Invoke();
        return true;
    }

    public void CycleActiveWeapon()
    {
        if (_weapons[0] == null && _weapons[1] == null) return;
        if (_weapons[0] == null) _activeWeaponIndex = 1;
        else if (_weapons[1] == null) _activeWeaponIndex = 0;
        else _activeWeaponIndex = _activeWeaponIndex == 0 ? 1 : 0;

        ApplyStatsToCharacter();
        BuildChanged?.Invoke();
    }

    public EffectiveCombatStats GetEffectiveStats()
    {
        var activeWeapon = GetWeapon(_activeWeaponIndex) ?? GetWeapon(0) ?? GetWeapon(1);
        return BuildStatCalculator.Compute(
            _classDefinition,
            activeWeapon,
            _items,
            _chosenTalents.ToArray(),
            _teamLevel);
    }

    public void ApplyStatsToCharacter()
    {
        var stats = GetEffectiveStats();
        var character = GetComponent<PlayerCharacter>();
        character?.ApplyMoveSpeed(stats.MoveSpeed);

        var health = GetComponent<Health>();
        if (health != null)
            health.SetMaxHealthPreserveRatio(stats.MaxHealth);

        var attack = GetComponent<PlayerAttackController>();
        attack?.ApplyEffectiveStats(stats, GetWeapon(_activeWeaponIndex) ?? GetWeapon(0) ?? GetWeapon(1));
    }

    private void EquipWeaponInternal(int slot, WeaponDefinition weapon)
    {
        _weapons[slot] = weapon;
        if (_activeWeaponIndex == slot || GetWeapon(_activeWeaponIndex) == null)
            _activeWeaponIndex = slot;
    }
}
