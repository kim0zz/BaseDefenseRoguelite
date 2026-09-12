using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DeployableRegistryTests
{
    [TearDown]
    public void TearDown()
    {
        DeployableRegistry.ResetForTests();
    }

    [Test]
    public void Cap3_FourthNormalDetonatesOldest()
    {
        var owner = CreateOwner();
        DeployableRegistry.SetCap(owner, 3);
        var detonated = new List<Deployable>();

        for (var i = 0; i < 4; i++)
        {
            var d = SpawnNormal(owner, new Vector3(i, 0f, 0f));
            d.Detonated += x => detonated.Add(x);
        }

        Assert.AreEqual(3, DeployableRegistry.GetNormalCount(owner));
        Assert.AreEqual(1, detonated.Count);
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void Cap10_EleventhDetonatesOldest_ChildOrbitalStrikeDoNotConsumeCap()
    {
        var owner = CreateOwner();
        var persistents = owner.GetComponent<PlayerPersistentEffects>();
        persistents.ReplaceAll(new List<PersistentEffectBinding>
        {
            new PersistentEffectBinding(PersistentEffectKind.DeployableCapBonus)
        });
        DeployableRegistry.SetCap(owner, persistents.GetDeployableCap());

        for (var i = 0; i < 10; i++)
            SpawnNormal(owner, new Vector3(i, 0f, 0f));

        Assert.AreEqual(10, DeployableRegistry.GetNormalCount(owner));

        SpawnNormal(owner, Vector3.right * 20f, DeployableCategory.Child);
        SpawnNormal(owner, Vector3.right * 21f, DeployableCategory.Orbital);
        SpawnNormal(owner, Vector3.right * 22f, DeployableCategory.Strike);
        Assert.AreEqual(10, DeployableRegistry.GetNormalCount(owner));

        var detonated = 0;
        var eleventh = SpawnNormal(owner, Vector3.right * 23f);
        eleventh.Detonated += _ => detonated++;
        Assert.AreEqual(10, DeployableRegistry.GetNormalCount(owner));
        Assert.AreEqual(1, detonated);

        Object.DestroyImmediate(owner);
    }

    [Test]
    public void DetonateAll_WorksOnIdleAndArmedMines()
    {
        var owner = CreateOwner();
        var idle = SpawnNormal(owner, Vector3.zero);
        var armed = SpawnNormal(owner, Vector3.right);
        armed.Motor.BeginArming();

        DeployableRegistry.DetonateAllDetonatable(owner);
        Assert.AreEqual(0, DeployableRegistry.GetNormalCount(owner));
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void Saper_AfterArmTime_EnemyInTriggerRadiusDetonates()
    {
        var owner = CreateOwner();
        owner.GetComponent<PlayerPersistentEffects>().ReplaceAll(new List<PersistentEffectBinding>
        {
            new PersistentEffectBinding(PersistentEffectKind.ArmToProximityMine)
        });

        var bomb = SpawnNormal(owner, Vector3.zero);
        bomb.Motor.BeginArming();

        var enemy = CreateEnemy(new Vector3(1.2f, 0f, 0f));
        bomb.Motor.GetType(); // ensure motor alive

        for (var t = 0; t < 11; t++)
        {
            if (bomb.IsDetonated) break;
            bomb.Motor.TickMotorForTests(0.1f);
        }

        Assert.IsTrue(bomb.IsDetonated);
        Object.DestroyImmediate(enemy);
        Object.DestroyImmediate(owner);
    }

    private static GameObject CreateOwner()
    {
        var go = new GameObject("Owner");
        go.AddComponent<Health>().Configure(100f);
        go.AddComponent<PlayerPersistentEffects>();
        DeployableRegistry.Ensure();
        return go;
    }

    private static Deployable SpawnNormal(GameObject owner, Vector3 pos, DeployableCategory category = DeployableCategory.Normal)
    {
        return Deployable.SpawnPlaceholder(pos, owner, category, 16f, 2.4f, 8f, 8, true);
    }

    [Test]
    public void Homing_DeadTargetRetargets()
    {
        var owner = CreateOwner();
        var bomb = SpawnNormal(owner, Vector3.zero);
        bomb.Motor.BeginHoming();

        var enemy = CreateEnemy(new Vector3(3f, 0f, 0f));
        bomb.Motor.RetargetHoming();
        Assert.AreEqual(enemy, bomb.Motor.HomingTargetForTests);

        enemy.GetComponent<Health>().ForceDeath();
        bomb.Motor.RetargetHoming();
        Assert.IsNull(bomb.Motor.HomingTargetForTests);

        Object.DestroyImmediate(enemy);
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void OrbitalRecharge_ReducesOnlyOtherSlots_ClampedToZero()
    {
        var owner = CreateOwner();
        var persistents = owner.GetComponent<PlayerPersistentEffects>();
        persistents.ReplaceAll(new List<PersistentEffectBinding>
        {
            new PersistentEffectBinding(PersistentEffectKind.OrbitalRechargeOnExplode)
        });

        var set = new OrbitingSlotSet(owner, persistents);
        set.SetRechargeTimerForTests(0, DeployableTuning.OrbitalRechargeSeconds);
        set.SetRechargeTimerForTests(1, 0.5f);
        set.TriggerRechargeReductionForTests(0);

        Assert.AreEqual(DeployableTuning.OrbitalRechargeSeconds, set.GetRechargeTimer(0), 0.01f);
        Assert.AreEqual(0f, set.GetRechargeTimer(1), 0.01f);

        Object.DestroyImmediate(owner);
    }

    private static GameObject CreateEnemy(Vector3 pos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.transform.position = pos;
        go.AddComponent<Health>().Configure(20f);
        var motor = go.AddComponent<EnemyLaneMotor>();
        var def = ScriptableObject.CreateInstance<EnemyDefinition>();
        motor.Initialize(AttackLineId.Center, def);
        go.AddComponent<EnemyController>().Configure(def, AttackLineId.Center);
        return go;
    }
}
