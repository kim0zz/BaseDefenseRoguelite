using System.Reflection;
using UnityEngine;

/// <summary>
/// Fabryka definicji skilli M7.5/M7.6/M8.1b — runtime gdy brak assetów.
/// </summary>
public static class SkillContentFactory
{
    public static SkillDefinition CreateTrzasniecie()
    {
        var skill = ScriptableObject.CreateInstance<SkillDefinition>();
        SetField(skill, "skillId", "pudzian_trzasniecie");
        SetField(skill, "displayName", "Stomp");
        SetField(skill, "ownerClass", PlayerClassId.Pudzian);
        SetField(skill, "skillSlotIndex", 0);
        SetField(skill, "windupSeconds", 0.30f);
        SetField(skill, "activeSeconds", 0.12f);
        SetField(skill, "recoverySeconds", 0.35f);
        SetField(skill, "windupMoveMultiplier", 0f);
        SetField(skill, "recoveryMoveMultiplier", 0.5f);
        SetField(skill, "locksFacingInActive", true);
        SetField(skill, "cooldownSeconds", 5f);
        SetField(skill, "maxCharges", 1);
        SetField(skill, "cooldownStartsOnActive", true);
        SetField(skill, "aimMode", SkillAimMode.Self);
        SetField(skill, "locomotion", SkillLocomotionMode.Root);
        SetField(skill, "hitPolicy", SkillHitPolicy.OnEnteredActive);
        SetField(skill, "shapeType", SkillShapeType.Circle);
        SetField(skill, "radiusMeters", 3f);
        SetField(skill, "maxTargets", 8);
        SetField(skill, "damage", 12f);
        SetField(skill, "controlMode", SkillControlMode.Stun);
        SetField(skill, "controlDurationSeconds", 1.0f);
        SetField(skill, "knockbackForce", 0f);
        SetField(skill, "bossStaggerContribution", 10f);
        SetField(skill, "targetMask", SkillTargetMask.Enemy | SkillTargetMask.Elite | SkillTargetMask.Boss);
        SetField(skill, "shakeStrength", 0.35f);
        SetField(skill, "rumbleLow", 0.4f);
        SetField(skill, "rumbleHigh", 0.6f);
        return skill;
    }

    public static SkillDefinition CreateNoChodzTu()
    {
        var skill = ScriptableObject.CreateInstance<SkillDefinition>();
        SetField(skill, "skillId", "pudzian_no_chodz_tu");
        SetField(skill, "displayName", "Prowokacja");
        SetField(skill, "ownerClass", PlayerClassId.Pudzian);
        SetField(skill, "skillSlotIndex", 1);
        SetField(skill, "windupSeconds", 0.20f);
        SetField(skill, "activeSeconds", 0.15f);
        SetField(skill, "recoverySeconds", 0.35f);
        SetField(skill, "windupMoveMultiplier", 0f);
        SetField(skill, "recoveryMoveMultiplier", 0.5f);
        SetField(skill, "locksFacingInActive", true);
        SetField(skill, "cooldownSeconds", 8f);
        SetField(skill, "maxCharges", 1);
        SetField(skill, "cooldownStartsOnActive", true);
        SetField(skill, "aimMode", SkillAimMode.Self);
        SetField(skill, "locomotion", SkillLocomotionMode.Root);
        SetField(skill, "hitPolicy", SkillHitPolicy.OnEnteredActive);
        SetField(skill, "shapeType", SkillShapeType.Circle);
        SetField(skill, "radiusMeters", 5.5f);
        SetField(skill, "maxTargets", 16);
        SetField(skill, "damage", 0f);
        SetField(skill, "controlMode", SkillControlMode.None);
        SetField(skill, "controlDurationSeconds", 0f);
        SetField(skill, "targetMask", SkillTargetMask.Enemy | SkillTargetMask.Elite);
        SetField(skill, "appliesThreatOverride", true);
        SetField(skill, "threatDurationSeconds", 4.0f);
        SetField(skill, "shakeStrength", 0.15f);
        SetField(skill, "rumbleLow", 0.25f);
        SetField(skill, "rumbleHigh", 0.35f);
        return skill;
    }

    public static SkillDefinition CreateByk()
    {
        var skill = ScriptableObject.CreateInstance<SkillDefinition>();
        SetField(skill, "skillId", "pudzian_byk");
        SetField(skill, "displayName", "Byk");
        SetField(skill, "ownerClass", PlayerClassId.Pudzian);
        SetField(skill, "skillSlotIndex", 2);
        SetField(skill, "windupSeconds", 0.25f);
        SetField(skill, "activeSeconds", 0.45f);
        SetField(skill, "recoverySeconds", 0.35f);
        SetField(skill, "windupMoveMultiplier", 0f);
        SetField(skill, "recoveryMoveMultiplier", 0.4f);
        SetField(skill, "locksFacingInActive", true);
        SetField(skill, "cooldownSeconds", 6f);
        SetField(skill, "maxCharges", 1);
        SetField(skill, "cooldownStartsOnActive", true);
        SetField(skill, "aimMode", SkillAimMode.FacingVector);
        SetField(skill, "locomotion", SkillLocomotionMode.Charge);
        SetField(skill, "hitPolicy", SkillHitPolicy.EveryTickWhileActiveOncePerTarget);
        SetField(skill, "shapeType", SkillShapeType.ChargeLine);
        SetField(skill, "chargeSpeed", 12.2f);
        SetField(skill, "chargeRangeMeters", 5.5f);
        SetField(skill, "capsuleWidthMeters", 1.6f);
        SetField(skill, "capsuleLengthMeters", 1.8f);
        SetField(skill, "maxTargets", 6);
        SetField(skill, "damage", 8f);
        SetField(skill, "controlMode", SkillControlMode.Shove);
        SetField(skill, "chargeContactMode", ChargeContactMode.SideShove);
        SetField(skill, "targetMask", SkillTargetMask.Enemy | SkillTargetMask.Elite | SkillTargetMask.Boss);
        SetField(skill, "bossStaggerContribution", 14f);
        SetField(skill, "hitStopSeconds", 0.06f);
        SetField(skill, "shakeStrength", 0.25f);
        SetField(skill, "rumbleLow", 0.35f);
        SetField(skill, "rumbleHigh", 0.5f);
        return skill;
    }

    public static SkillDefinition CreateSkok()
    {
        var skill = CreateTrzasniecie();
        SetField(skill, "skillId", "pudzian_skok");
        SetField(skill, "displayName", "Skok z Pierdolnięciem");
        SetField(skill, "aimMode", SkillAimMode.GroundPoint);
        SetField(skill, "locomotion", SkillLocomotionMode.Leap);
        SetField(skill, "hitPolicy", SkillHitPolicy.OnLand);
        return skill;
    }

    public static SkillDefinition CreateZryjMnie()
    {
        var skill = CreateNoChodzTu();
        SetField(skill, "skillId", "pudzian_zryj_mnie");
        SetField(skill, "displayName", "ŻRYJ MNIE");
        return skill;
    }

    public static SkillDefinition CreateSpychacz()
    {
        var skill = CreateByk();
        SetField(skill, "skillId", "pudzian_spychacz");
        SetField(skill, "displayName", "Spychacz");
        SetField(skill, "chargeContactMode", ChargeContactMode.Carry);
        SetField(skill, "capsuleWidthMeters", 2.2f);
        SetField(skill, "maxTargets", 8);
        return skill;
    }

    public static SkillDefinition CreateJaJestemBoss()
    {
        var skill = CreateTrzasniecie();
        SetField(skill, "skillId", "pudzian_ja_jestem_boss");
        SetField(skill, "displayName", "JA JESTEM BOSS");
        SetField(skill, "skillSlotIndex", SkillLoadout.UltimateSlot);
        SetField(skill, "activationMode", SkillActivationMode.Active);
        SetField(skill, "cooldownSeconds", 24f);
        SetField(skill, "windupSeconds", 0.25f);
        SetField(skill, "activeSeconds", 0.15f);
        SetField(skill, "recoverySeconds", 0.35f);
        SetField(skill, "damage", 0f);
        SetField(skill, "controlMode", SkillControlMode.None);
        return skill;
    }

    public static SkillDefinition CreateTrzesienieSwiata()
    {
        var skill = CreateTrzasniecie();
        SetField(skill, "skillId", "pudzian_trzesienie");
        SetField(skill, "displayName", "Trzęsienie Świata");
        SetField(skill, "skillSlotIndex", SkillLoadout.UltimateSlot);
        SetField(skill, "activationMode", SkillActivationMode.Active);
        SetField(skill, "shapeType", SkillShapeType.LaneWaves);
        SetField(skill, "windupSeconds", 0.90f);
        SetField(skill, "cooldownSeconds", 28f);
        SetField(skill, "damage", 28f);
        SetField(skill, "controlDurationSeconds", 1.4f);
        SetField(skill, "bossStaggerContribution", 22f);
        SetField(skill, "maxTargets", 16);
        SetField(skill, "radiusMeters", 3.2f);
        SetField(skill, "shakeStrength", 0.55f);
        return skill;
    }

    public static SkillDefinition CreatePiekielnaAuraPulse()
    {
        var skill = CreateTrzasniecie();
        SetField(skill, "skillId", "pudzian_piekielna_aura");
        SetField(skill, "displayName", "Piekielna Aura");
        SetField(skill, "skillSlotIndex", SkillLoadout.UltimateSlot);
        SetField(skill, "activationMode", SkillActivationMode.Passive);
        SetField(skill, "cooldownSeconds", 0f);
        SetField(skill, "damage", 0f);
        SetField(skill, "controlMode", SkillControlMode.None);
        return skill;
    }

    public static SkillDefinition CreateNieZabijecieMnie()
    {
        var skill = CreateNoChodzTu();
        SetField(skill, "skillId", "pudzian_nie_zabijecie_mnie");
        SetField(skill, "displayName", "Nie zabijecie mnie");
        SetField(skill, "skillSlotIndex", SkillLoadout.UltimateSlot);
        SetField(skill, "activationMode", SkillActivationMode.Passive);
        SetField(skill, "cooldownSeconds", 0f);
        SetField(skill, "appliesThreatOverride", false);
        SetField(skill, "damage", 0f);
        return skill;
    }

    public static SkillDefinition CreateBombermanBomba()
    {
        var skill = ScriptableObject.CreateInstance<SkillDefinition>();
        SetField(skill, "skillId", "bomberman_bomba");
        SetField(skill, "displayName", "Bomba");
        SetField(skill, "ownerClass", PlayerClassId.Bomberman);
        SetField(skill, "skillSlotIndex", 0);
        SetField(skill, "windupSeconds", 0.12f);
        SetField(skill, "activeSeconds", 0.08f);
        SetField(skill, "recoverySeconds", 0.15f);
        SetField(skill, "windupMoveMultiplier", 0.5f);
        SetField(skill, "recoveryMoveMultiplier", 0.5f);
        SetField(skill, "locksFacingInActive", true);
        SetField(skill, "cooldownSeconds", 0.40f);
        SetField(skill, "maxCharges", 1);
        SetField(skill, "cooldownStartsOnActive", true);
        SetField(skill, "aimMode", SkillAimMode.FacingVector);
        SetField(skill, "locomotion", SkillLocomotionMode.Root);
        SetField(skill, "hitPolicy", SkillHitPolicy.OnEnteredActive);
        SetField(skill, "shapeType", SkillShapeType.PlaceDeployable);
        SetField(skill, "radiusMeters", 2.4f);
        SetField(skill, "maxTargets", 8);
        SetField(skill, "damage", 16f);
        SetField(skill, "controlMode", SkillControlMode.None);
        SetField(skill, "knockbackForce", 0f);
        SetField(skill, "bossStaggerContribution", 8f);
        SetField(skill, "targetMask", SkillTargetMask.Enemy | SkillTargetMask.Elite | SkillTargetMask.Boss);
        SetField(skill, "shakeStrength", 0.25f);
        SetField(skill, "rumbleLow", 0.3f);
        SetField(skill, "rumbleHigh", 0.45f);
        return skill;
    }

    public static SkillDefinition CreateBombermanDetonator()
    {
        var skill = ScriptableObject.CreateInstance<SkillDefinition>();
        SetField(skill, "skillId", "bomberman_detonator");
        SetField(skill, "displayName", "Detonator");
        SetField(skill, "ownerClass", PlayerClassId.Bomberman);
        SetField(skill, "skillSlotIndex", 1);
        SetField(skill, "windupSeconds", 0.08f);
        SetField(skill, "activeSeconds", 0.05f);
        SetField(skill, "recoverySeconds", 0.10f);
        SetField(skill, "windupMoveMultiplier", 0.5f);
        SetField(skill, "recoveryMoveMultiplier", 0.5f);
        SetField(skill, "locksFacingInActive", true);
        SetField(skill, "cooldownSeconds", 0.50f);
        SetField(skill, "maxCharges", 1);
        SetField(skill, "cooldownStartsOnActive", true);
        SetField(skill, "aimMode", SkillAimMode.Self);
        SetField(skill, "locomotion", SkillLocomotionMode.Root);
        SetField(skill, "hitPolicy", SkillHitPolicy.OnEnteredActive);
        SetField(skill, "shapeType", SkillShapeType.DetonateOwned);
        SetField(skill, "damage", 0f);
        SetField(skill, "controlMode", SkillControlMode.None);
        SetField(skill, "targetMask", SkillTargetMask.Enemy | SkillTargetMask.Elite | SkillTargetMask.Boss);
        SetField(skill, "shakeStrength", 0.2f);
        SetField(skill, "rumbleLow", 0.25f);
        SetField(skill, "rumbleHigh", 0.4f);
        return skill;
    }

    public static SkillDefinition CreateBombermanKopniak()
    {
        var skill = ScriptableObject.CreateInstance<SkillDefinition>();
        SetField(skill, "skillId", "bomberman_kopniak");
        SetField(skill, "displayName", "Kopniak");
        SetField(skill, "ownerClass", PlayerClassId.Bomberman);
        SetField(skill, "skillSlotIndex", 2);
        SetField(skill, "windupSeconds", 0.15f);
        SetField(skill, "activeSeconds", 0.20f);
        SetField(skill, "recoverySeconds", 0.25f);
        SetField(skill, "windupMoveMultiplier", 0.5f);
        SetField(skill, "recoveryMoveMultiplier", 0.5f);
        SetField(skill, "locksFacingInActive", true);
        SetField(skill, "cooldownSeconds", 4.0f);
        SetField(skill, "maxCharges", 1);
        SetField(skill, "cooldownStartsOnActive", true);
        SetField(skill, "aimMode", SkillAimMode.FacingVector);
        SetField(skill, "locomotion", SkillLocomotionMode.Root);
        SetField(skill, "hitPolicy", SkillHitPolicy.OnEnteredActive);
        SetField(skill, "shapeType", SkillShapeType.LaunchNearestOwned);
        SetField(skill, "chargeSpeed", 14f);
        SetField(skill, "chargeRangeMeters", 7f);
        SetField(skill, "radiusMeters", 3.5f);
        SetField(skill, "damage", 0f);
        SetField(skill, "controlMode", SkillControlMode.None);
        SetField(skill, "targetMask", SkillTargetMask.Enemy | SkillTargetMask.Elite | SkillTargetMask.Boss);
        SetField(skill, "shakeStrength", 0.2f);
        SetField(skill, "rumbleLow", 0.3f);
        SetField(skill, "rumbleHigh", 0.45f);
        return skill;
    }

    public static SkillDefinition CreateBombermanSzybkostrzelnosc()
    {
        var skill = CreateBombermanBomba();
        SetField(skill, "skillId", "bomberman_szybkostrzelnosc");
        SetField(skill, "displayName", "Szybkostrzelność");
        SetField(skill, "skillSlotIndex", SkillLoadout.UltimateSlot);
        SetField(skill, "activationMode", SkillActivationMode.Active);
        SetField(skill, "shapeType", SkillShapeType.Circle);
        SetField(skill, "cooldownSeconds", 28f);
        SetField(skill, "windupSeconds", 0.15f);
        SetField(skill, "activeSeconds", 0.2f);
        SetField(skill, "recoverySeconds", 0.2f);
        SetField(skill, "damage", 0f);
        SetField(skill, "radiusMeters", 0f);
        SetField(skill, "maxTargets", 0);
        return skill;
    }

    public static SkillDefinition CreateBombermanOrbitale()
    {
        var skill = CreateBombermanBomba();
        SetField(skill, "skillId", "bomberman_orbitale");
        SetField(skill, "displayName", "Orbitale");
        SetField(skill, "skillSlotIndex", SkillLoadout.UltimateSlot);
        SetField(skill, "activationMode", SkillActivationMode.Active);
        SetField(skill, "shapeType", SkillShapeType.Circle);
        SetField(skill, "cooldownSeconds", 1f);
        SetField(skill, "windupSeconds", 0.1f);
        SetField(skill, "activeSeconds", 0.1f);
        SetField(skill, "recoverySeconds", 0.15f);
        SetField(skill, "damage", 0f);
        SetField(skill, "radiusMeters", 0f);
        SetField(skill, "maxTargets", 0);
        return skill;
    }

    public static SkillDefinition CreateBombermanNalot()
    {
        var skill = CreateBombermanBomba();
        SetField(skill, "skillId", "bomberman_nalot");
        SetField(skill, "displayName", "Nalot");
        SetField(skill, "skillSlotIndex", SkillLoadout.UltimateSlot);
        SetField(skill, "activationMode", SkillActivationMode.Active);
        SetField(skill, "shapeType", SkillShapeType.AimStripBurst);
        SetField(skill, "cooldownSeconds", 30f);
        SetField(skill, "windupSeconds", 1.0f);
        SetField(skill, "activeSeconds", 1.7f);
        SetField(skill, "recoverySeconds", 0.3f);
        SetField(skill, "damage", 14f);
        SetField(skill, "radiusMeters", 2.0f);
        SetField(skill, "chargeRangeMeters", 2.2f);
        SetField(skill, "maxTargets", 7);
        SetField(skill, "bossStaggerContribution", 10f);
        return skill;
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }
}
