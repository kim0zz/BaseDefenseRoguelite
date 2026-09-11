using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SkillModifierTests
{
    [Test]
    public void CrackZoneModifier_SpawnsSlowZoneOnHit()
    {
        SkillTelemetry.Reset();
        var caster = new GameObject("Caster");
        var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        target.AddComponent<Health>().Configure(30f);
        var profile = target.AddComponent<ForcedMovementResistanceProfile>();
        profile.Configure(ForcedMovementResistanceCategory.Normal);
        target.AddComponent<StatusEffectReceiver>();
        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        target.AddComponent<EnemyLaneMotor>().Initialize(AttackLineId.Center, definition);
        target.AddComponent<EnemyController>().Configure(definition, AttackLineId.Center);

        var skill = SkillContentFactory.CreateTrzasniecie();
        var hits = new List<SkillCircleOverlap.HitTarget>
        {
            new()
            {
                GameObject = target,
                Damageable = target.GetComponent<Health>(),
                LaneId = AttackLineId.Center,
                HitPoint = target.transform.position
            }
        };

        var effects = new List<SkillEffectKind> { SkillEffectKind.SpawnSlowZone };
        var result = SkillHitResolver.ApplyHits(skill, hits, caster, effects);

        Assert.IsTrue(result.HadHit);
        var zone = Object.FindAnyObjectByType<SlowZone>();
        Assert.IsNotNull(zone);

        Object.DestroyImmediate(zone.gameObject);
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(caster);
    }

    [Test]
    public void WithoutModifier_NoSlowZone()
    {
        var caster = new GameObject("Caster");
        var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        target.AddComponent<Health>().Configure(30f);
        target.AddComponent<ForcedMovementResistanceProfile>().Configure(ForcedMovementResistanceCategory.Normal);
        target.AddComponent<StatusEffectReceiver>();
        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        target.AddComponent<EnemyLaneMotor>().Initialize(AttackLineId.Center, definition);
        target.AddComponent<EnemyController>().Configure(definition, AttackLineId.Center);

        var skill = SkillContentFactory.CreateTrzasniecie();
        var hits = new List<SkillCircleOverlap.HitTarget>
        {
            new()
            {
                GameObject = target,
                Damageable = target.GetComponent<Health>(),
                LaneId = AttackLineId.Center,
                HitPoint = target.transform.position
            }
        };

        SkillHitResolver.ApplyHits(skill, hits, caster, System.Array.Empty<SkillEffectKind>());
        Assert.IsNull(Object.FindAnyObjectByType<SlowZone>());

        Object.DestroyImmediate(target);
        Object.DestroyImmediate(caster);
    }

    [Test]
    public void HealCasterOnHit_HealsPerTarget()
    {
        var caster = new GameObject("Caster");
        var health = caster.AddComponent<Health>();
        health.Configure(100f);
        health.TakeDamage(50f, null);

        var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        target.AddComponent<Health>().Configure(30f);
        target.AddComponent<ForcedMovementResistanceProfile>().Configure(ForcedMovementResistanceCategory.Normal);
        target.AddComponent<StatusEffectReceiver>();
        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        target.AddComponent<EnemyLaneMotor>().Initialize(AttackLineId.Center, definition);
        target.AddComponent<EnemyController>().Configure(definition, AttackLineId.Center);

        var skill = SkillContentFactory.CreateZryjMnie();
        var hits = new List<SkillCircleOverlap.HitTarget>
        {
            new()
            {
                GameObject = target,
                Damageable = target.GetComponent<Health>(),
                LaneId = AttackLineId.Center,
                HitPoint = target.transform.position
            }
        };

        SkillHitResolver.ApplyHits(skill, hits, caster, new[] { SkillEffectKind.HealCasterOnHit });
        Assert.AreEqual(50f + SkillEffectApplier.HealPerHit, health.CurrentHealth, 0.01f);

        Object.DestroyImmediate(target);
        Object.DestroyImmediate(caster);
    }
}
