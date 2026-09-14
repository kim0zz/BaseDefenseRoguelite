using System.Collections.Generic;
using UnityEngine;

public enum SiegeUpgradeKind { Damage, Cooldown, Area }

/// <summary>Per-player, slot-based boosts reapplied after talent mutations. DRAFT tuning.</summary>
[DisallowMultipleComponent]
public sealed class SiegeSkillUpgrades : MonoBehaviour
{
    [SerializeField] private float damagePerRank = 0.20f;
    [SerializeField] private float cooldownPerRank = 0.10f;
    [SerializeField] private float areaPerRank = 0.15f;
    [SerializeField] private int maximumRanks = 3;
    private readonly int[,] _ranks = new int[SkillLoadout.SlotCount, 3];
    private readonly List<SkillDefinition> _owned = new();

    public int GetRank(int slot, SiegeUpgradeKind kind) => _ranks[slot, (int)kind];
    public float GetMultiplier(int slot, SiegeUpgradeKind kind)
    {
        var rank = GetRank(slot, kind);
        return kind == SiegeUpgradeKind.Cooldown ? Mathf.Max(0.5f, 1f - rank * cooldownPerRank)
            : 1f + rank * (kind == SiegeUpgradeKind.Damage ? damagePerRank : areaPerRank);
    }

    public bool CanApply(int slot, SiegeUpgradeKind kind, SkillDefinition skill)
    {
        if (slot < 0 || slot >= SkillLoadout.SlotCount || skill == null || _ranks[slot, (int)kind] >= maximumRanks)
            return false;
        if (kind == SiegeUpgradeKind.Cooldown)
            return skill.ActivationMode == SkillActivationMode.Active && skill.CooldownSeconds > 0.35f;
        if (skill.ShapeType == SkillShapeType.PlaceAndDash || skill.ShapeType == SkillShapeType.DetonateOwned)
            return false; // These executors use independent charge tuning.
        if (kind == SiegeUpgradeKind.Damage)
            return skill.Damage > 0f && skill.ShapeType != SkillShapeType.LaunchNearestOwned;
        return skill.ShapeType == SkillShapeType.Circle || skill.ShapeType == SkillShapeType.Leap
            || skill.ShapeType == SkillShapeType.PlaceDeployable || skill.ShapeType == SkillShapeType.LaunchNearestOwned
            || skill.ShapeType == SkillShapeType.ChargeLine || skill.ShapeType == SkillShapeType.AimStripBurst;
    }

    public bool TryApply(int slot, SiegeUpgradeKind kind)
    {
        var controller = GetComponent<PlayerSkillController>();
        if (!CanApply(slot, kind, controller != null ? controller.GetSkill(slot) : null)) return false;
        _ranks[slot, (int)kind]++;
        controller.RefreshModifiers();
        return true;
    }

    public string Describe(SiegeUpgradeKind kind)
    {
        if (kind == SiegeUpgradeKind.Damage) return $"+{damagePerRank:P0} obrażeń";
        if (kind == SiegeUpgradeKind.Area) return $"+{areaPerRank:P0} obszaru";
        return $"−{cooldownPerRank:P0} czasu odnowienia";
    }

    public void ApplyToSlots(SkillDefinition[] slots)
    {
        ReleaseClones();
        for (var slot = 0; slot < slots.Length; slot++)
        {
            if (slots[slot] == null || (_ranks[slot, 0] + _ranks[slot, 1] + _ranks[slot, 2]) == 0) continue;
            var copy = Instantiate(slots[slot]);
            copy.hideFlags = HideFlags.DontSave;
            copy.ApplySiegeMultipliers(GetMultiplier(slot, SiegeUpgradeKind.Damage),
                GetMultiplier(slot, SiegeUpgradeKind.Cooldown), GetMultiplier(slot, SiegeUpgradeKind.Area));
            slots[slot] = copy;
            _owned.Add(copy);
        }
    }

    private void OnDestroy() => ReleaseClones();
    private void ReleaseClones()
    {
        foreach (var copy in _owned)
            if (copy != null)
            {
                if (Application.isPlaying) Destroy(copy);
                else DestroyImmediate(copy);
            }
        _owned.Clear();
    }
}
