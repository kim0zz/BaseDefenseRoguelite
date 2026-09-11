using System.Collections.Generic;

/// <summary>
/// Nakłada wybrane karty na kit (swap slota, efekty, persistents).
/// </summary>
public static class ProgressionApplier
{
    public static void ApplyAll(
        IReadOnlyList<TalentDefinition> chosen,
        SkillDefinition[] slots,
        List<SkillEffectKind>[] slotEffects,
        List<PersistentEffectBinding> persistents)
    {
        persistents?.Clear();
        if (slotEffects != null)
        {
            for (var i = 0; i < slotEffects.Length; i++)
            {
                slotEffects[i] ??= new List<SkillEffectKind>();
                slotEffects[i].Clear();
            }
        }

        if (chosen == null) return;
        foreach (var talent in chosen)
            Apply(talent, slots, slotEffects, persistents);
    }

    public static void Apply(
        TalentDefinition talent,
        SkillDefinition[] slots,
        List<SkillEffectKind>[] slotEffects,
        List<PersistentEffectBinding> persistents)
    {
        if (talent?.Effect == null) return;
        var fx = talent.Effect;
        var slot = ResolveSlot(fx, slots);

        switch (fx.Operation)
        {
            case TalentEffectOp.MutateSkill:
                if (slot >= 0 && slots != null && slot < slots.Length && fx.ReplacementSkill != null)
                    slots[slot] = fx.ReplacementSkill;
                AddEffect(slot, fx.AddedEffect, slotEffects);
                break;
            case TalentEffectOp.AddSkillEffect:
                AddEffect(slot, fx.AddedEffect, slotEffects);
                break;
            case TalentEffectOp.GrantSkill:
                if (slots != null && fx.GrantedSkill != null && SkillLoadout.UltimateSlot < slots.Length)
                    slots[SkillLoadout.UltimateSlot] = fx.GrantedSkill;
                AddEffect(SkillLoadout.UltimateSlot, fx.AddedEffect, slotEffects);
                break;
        }

        if (fx.PersistentKind != PersistentEffectKind.None && persistents != null)
            AddPersistent(persistents, fx.PersistentKind, fx.PersistentTuning);
    }

    public static void ApplyAllLegacy(
        IReadOnlyList<TalentDefinition> chosen,
        SkillDefinition[] slots,
        List<SkillEffectKind>[] slotEffects,
        List<PersistentEffectKind> persistents)
    {
        var bindings = new List<PersistentEffectBinding>();
        ApplyAll(chosen, slots, slotEffects, bindings);
        persistents?.Clear();
        foreach (var binding in bindings)
        {
            if (!persistents.Contains(binding.Kind))
                persistents.Add(binding.Kind);
        }
    }

    public static string ReadUltimateSkillId(IReadOnlyList<TalentDefinition> chosen)
    {
        if (chosen == null) return "";
        for (var i = chosen.Count - 1; i >= 0; i--)
        {
            var fx = chosen[i]?.Effect;
            if (fx == null || fx.Operation != TalentEffectOp.GrantSkill) continue;
            if (fx.GrantedSkill != null)
                return fx.GrantedSkill.SkillId;
        }

        return "";
    }

    private static void AddPersistent(
        List<PersistentEffectBinding> persistents,
        PersistentEffectKind kind,
        EffectTuning tuning)
    {
        foreach (var existing in persistents)
        {
            if (existing.Kind == kind) return;
        }

        persistents.Add(new PersistentEffectBinding(kind, tuning));
    }

    private static int ResolveSlot(TalentEffect fx, SkillDefinition[] slots)
    {
        if (fx.TargetSlotIndex >= 0) return fx.TargetSlotIndex;
        if (slots == null || string.IsNullOrEmpty(fx.TargetSkillId)) return -1;
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].SkillId == fx.TargetSkillId)
                return i;
        }

        return -1;
    }

    private static void AddEffect(int slot, SkillEffectKind kind, List<SkillEffectKind>[] slotEffects)
    {
        if (kind == SkillEffectKind.None || slotEffects == null) return;
        if (slot < 0 || slot >= slotEffects.Length) return;
        slotEffects[slot] ??= new List<SkillEffectKind>();
        if (!slotEffects[slot].Contains(kind))
            slotEffects[slot].Add(kind);
    }
}
