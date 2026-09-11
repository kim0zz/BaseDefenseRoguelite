using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Byk — charge skill Pudziana (M7.6-T7, M8.1b baseline).
/// </summary>
public class BykSkillTests
{
    [Test]
    public void Factory_ChargeSpeedRangeDamageCooldown()
    {
        var skill = SkillContentFactory.CreateByk();

        Assert.AreEqual("pudzian_byk", skill.SkillId);
        Assert.AreEqual("Byk", skill.DisplayName);
        Assert.AreEqual(2, skill.SkillSlotIndex);
        Assert.AreEqual(12.2f, skill.ChargeSpeed, 0.05f);
        Assert.AreEqual(5.5f, skill.ChargeRangeMeters, 0.01f);
        Assert.AreEqual(1.6f, skill.CapsuleWidthMeters, 0.01f);
        Assert.AreEqual(1.8f, skill.CapsuleLengthMeters, 0.01f);
        Assert.AreEqual(8f, skill.Damage, 0.01f);
        Assert.AreEqual(6f, skill.CooldownSeconds, 0.01f);
        Assert.AreEqual(6, skill.MaxTargets);
        Assert.AreEqual(0.45f, skill.ActiveSeconds, 0.01f);
        Assert.AreEqual(ChargeContactMode.SideShove, skill.ChargeContactMode);
        Assert.AreEqual(SkillLocomotionMode.Charge, skill.Locomotion);
        Assert.AreEqual(14f, skill.BossStaggerContribution, 0.01f);
    }

    [Test]
    public void OncePerTarget_SecondApplyOnSameGameObject_DoesNotDamageAgain()
    {
        var skill = SkillContentFactory.CreateByk();
        var caster = new GameObject("Caster");
        var target = CreateGruntTarget(30f);
        var oncePerTarget = new SkillOncePerTargetSet();
        var hits = BuildCapsuleHits(target);

        var first = SkillHitResolver.ApplyOncePerTargetHits(
            skill, hits, caster, Vector3.forward, 0.5f, oncePerTarget, null);
        var second = SkillHitResolver.ApplyOncePerTargetHits(
            skill, hits, caster, Vector3.forward, 0.4f, oncePerTarget, null);

        Assert.AreEqual(1, first.HitCount);
        Assert.AreEqual(8f, first.TotalDamage, 0.01f);
        Assert.AreEqual(0, second.HitCount);
        Assert.AreEqual(22f, target.GetComponent<Health>().CurrentHealth, 0.01f);

        Cleanup(caster, target);
    }

    [Test]
    public void Charge_StopsOnBoss_WithZeroBossDisplacement()
    {
        var caster = new GameObject("Caster");
        caster.transform.position = Vector3.zero;
        var casterReceiver = caster.AddComponent<ForcedMovementReceiver>();

        var boss = CreateObstacle(ForcedMovementResistanceCategory.Boss, new Vector3(0f, 0f, 2f));
        var skill = SkillContentFactory.CreateByk();

        ForcedMovementResolver.Apply(
            ForcedMovementRequest.CasterCharge(caster, Vector3.forward, skill.ChargeSpeed, skill.ActiveSeconds, honorCollisions: false),
            caster);

        for (var i = 0; i < 20; i++)
            casterReceiver.Tick(0.05f);

        Assert.AreEqual(ChargeStopReason.Boss, casterReceiver.LastChargeStopReason);
        Assert.Less(caster.transform.position.z, 1.8f);

        Cleanup(caster, boss);
    }

    [Test]
    public void Trzasniecie_Baseline12_5s()
    {
        var skill = SkillContentFactory.CreateTrzasniecie();
        Assert.AreEqual("Stomp", skill.DisplayName);
        Assert.AreEqual(12f, skill.Damage, 0.01f);
        Assert.AreEqual(5f, skill.CooldownSeconds, 0.01f);
        Assert.AreEqual(0f, skill.KnockbackForce, 0.01f);
    }

    private static GameObject CreateGruntTarget(float hp)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        var health = go.AddComponent<Health>();
        health.Configure(hp);
        go.AddComponent<StatusEffectReceiver>();
        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        definition.ConfigureForTest(EnemyKind.Grunt);
        go.AddComponent<EnemyLaneMotor>().Initialize(AttackLineId.Center, definition);
        go.AddComponent<EnemyController>().Configure(definition, AttackLineId.Center);
        health.Configure(hp);
        return go;
    }

    private static List<SkillChargeCapsuleOverlap.HitTarget> BuildCapsuleHits(GameObject target)
    {
        return new List<SkillChargeCapsuleOverlap.HitTarget>
        {
            new()
            {
                GameObject = target,
                Damageable = target.GetComponent<Health>(),
                LaneId = AttackLineId.Center,
                HitPoint = target.transform.position
            }
        };
    }

    private static GameObject CreateObstacle(ForcedMovementResistanceCategory category, Vector3 position)
    {
        var go = new GameObject($"Obstacle_{category}");
        go.transform.position = position;
        var profile = go.AddComponent<ForcedMovementResistanceProfile>();
        profile.Configure(category);
        go.AddComponent<SphereCollider>().radius = 0.6f;
        return go;
    }

    private static void Cleanup(params Object[] objects)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
                Object.DestroyImmediate(obj);
        }
    }
}
