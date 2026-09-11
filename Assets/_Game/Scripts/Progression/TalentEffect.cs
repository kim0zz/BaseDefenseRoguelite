using System;
using UnityEngine;

/// <summary>
/// Payload efektu karty (jedna operacja wiodąca + opcjonalny persistent).
/// </summary>
[Serializable]
public class TalentEffect
{
    [SerializeField] private TalentEffectOp operation = TalentEffectOp.None;
    [SerializeField] private string targetSkillId;
    [SerializeField] private int targetSlotIndex = -1;
    [SerializeField] private SkillDefinition replacementSkill;
    [SerializeField] private SkillEffectKind addedEffect;
    [SerializeField] private SkillDefinition grantedSkill;
    [SerializeField] private PersistentEffectKind persistentKind;
    [SerializeField] private EffectTuning persistentTuning;
    [SerializeField] private string offerGroup;

    public TalentEffectOp Operation => operation;
    public string TargetSkillId => targetSkillId ?? "";
    public int TargetSlotIndex => targetSlotIndex;
    public SkillDefinition ReplacementSkill => replacementSkill;
    public SkillEffectKind AddedEffect => addedEffect;
    public SkillDefinition GrantedSkill => grantedSkill;
    public PersistentEffectKind PersistentKind => persistentKind;
    public EffectTuning PersistentTuning => persistentTuning;
    public string OfferGroup => offerGroup ?? "";

    public static TalentEffect Mutate(string targetSkillId, int slot, SkillDefinition replacement, string offerGroup,
        SkillEffectKind added = SkillEffectKind.None, PersistentEffectKind persistent = PersistentEffectKind.None,
        EffectTuning persistentTuning = null)
    {
        return new TalentEffect
        {
            operation = TalentEffectOp.MutateSkill,
            targetSkillId = targetSkillId,
            targetSlotIndex = slot,
            replacementSkill = replacement,
            addedEffect = added,
            persistentKind = persistent,
            persistentTuning = persistentTuning,
            offerGroup = offerGroup
        };
    }

    public static TalentEffect AddEffect(string targetSkillId, int slot, SkillEffectKind added, string offerGroup,
        PersistentEffectKind persistent = PersistentEffectKind.None, EffectTuning persistentTuning = null)
    {
        return new TalentEffect
        {
            operation = TalentEffectOp.AddSkillEffect,
            targetSkillId = targetSkillId,
            targetSlotIndex = slot,
            addedEffect = added,
            persistentKind = persistent,
            persistentTuning = persistentTuning,
            offerGroup = offerGroup
        };
    }

    public static TalentEffect Grant(SkillDefinition skill, string offerGroup,
        PersistentEffectKind persistent = PersistentEffectKind.None,
        SkillEffectKind added = SkillEffectKind.None, EffectTuning persistentTuning = null)
    {
        return new TalentEffect
        {
            operation = TalentEffectOp.GrantSkill,
            grantedSkill = skill,
            targetSlotIndex = SkillLoadout.UltimateSlot,
            persistentKind = persistent,
            addedEffect = added,
            persistentTuning = persistentTuning,
            offerGroup = offerGroup
        };
    }

    public static TalentEffect Persistent(PersistentEffectKind kind, string offerGroup, EffectTuning tuning = null)
    {
        return new TalentEffect
        {
            operation = TalentEffectOp.AttachPersistent,
            persistentKind = kind,
            persistentTuning = tuning,
            offerGroup = offerGroup
        };
    }
}
