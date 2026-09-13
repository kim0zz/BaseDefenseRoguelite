using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BombermanKitRebuildTests
{
    [TearDown]
    public void TearDown()
    {
        DeployableRegistry.ResetForTests();
    }

    [Test]
    public void Kit_ReplacesDetonatorWithDash_AndKeepsPetardaUntouched()
    {
        var bomba = SkillContentFactory.CreateBombermanBomba();
        var dash = SkillContentFactory.CreateBombermanWybuchowyOdskok();
        var kick = SkillContentFactory.CreateBombermanKopniak();

        Assert.AreEqual("bomberman_bomba", bomba.SkillId);
        Assert.AreEqual(SkillShapeType.PlaceDeployable, bomba.ShapeType);
        Assert.AreEqual(0.40f, bomba.CooldownSeconds, 0.001f);

        Assert.AreEqual("bomberman_wybuchowy_odskok", dash.SkillId);
        Assert.AreEqual(SkillShapeType.PlaceAndDash, dash.ShapeType);
        Assert.AreEqual(SkillLocomotionMode.Charge, dash.Locomotion);
        Assert.IsFalse(dash.LocksFacingInActive);

        Assert.AreEqual(SkillShapeType.LaunchNearestOwned, kick.ShapeType);
        Assert.Greater(kick.CapsuleWidthMeters, 3f);
        Assert.Greater(kick.CapsuleLengthMeters, 3f);

        var table = BuildContentFactory.CreateDefaultCatalog().GetProgression(PlayerClassId.Bomberman);
        CollectionAssert.AreEqual(
            new[] { "bomberman_bomba", "bomberman_wybuchowy_odskok", "bomberman_kopniak" },
            table.StartingActiveSkillIds);

        Object.DestroyImmediate(bomba);
        Object.DestroyImmediate(dash);
        Object.DestroyImmediate(kick);
    }

    [Test]
    public void TalentCopy_HasNoDeadDetonatorOffers()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        foreach (var talent in catalog.AllTalents)
        {
            if (talent == null || talent.PlayerClass != PlayerClassId.Bomberman) continue;
            StringAssert.DoesNotContain("Detonator", talent.DisplayName);
            StringAssert.DoesNotContain("Detonator", talent.Description);
            StringAssert.DoesNotContain("detonator", talent.Description);
        }
    }

    [Test]
    public void DashDirection_PrefersCurrentThenLastThenFacing()
    {
        Assert.AreEqual(Vector3.right, AimMath.ResolveDashDirection(Vector3.right, Vector3.forward, Vector3.back));
        Assert.AreEqual(Vector3.forward, AimMath.ResolveDashDirection(Vector3.zero, Vector3.forward, Vector3.right));
        Assert.AreEqual(Vector3.left, AimMath.ResolveDashDirection(Vector3.zero, Vector3.zero, Vector3.left));
    }

    [Test]
    public void Place_SetsMainBombFuse_AndKickDoesNotResetIt()
    {
        var owner = CreateOwner();
        var skill = SkillContentFactory.CreateBombermanBomba();
        var bomb = DeployableRegistry.Place(owner, Vector3.zero, skill);

        Assert.IsTrue(bomb.HasFuse);
        Assert.AreEqual(DeployableTuning.MainBombFuseSeconds, bomb.RemainingFuse, 0.01f);

        bomb.TickFuseForTests(0.40f);
        var remaining = bomb.RemainingFuse;
        bomb.Motor.Kick(Vector3.forward);
        Assert.AreEqual(remaining, bomb.RemainingFuse, 0.01f);
        Assert.IsTrue(bomb.WasKicked);

        bomb.TickFuseForTests(remaining);
        Assert.IsTrue(bomb.IsDetonated);

        Object.DestroyImmediate(skill);
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void DashCharge_HasShorterFuseAndDoesNotConsumeNormalCap()
    {
        var owner = CreateOwner();
        var skill = SkillContentFactory.CreateBombermanBomba();
        DeployableRegistry.SetCap(owner, 3);
        for (var i = 0; i < 3; i++)
            DeployableRegistry.Place(owner, new Vector3(i, 0f, 0f), skill);

        Assert.AreEqual(3, DeployableRegistry.GetNormalCount(owner));
        var dash = DeployableRegistry.PlaceDashCharge(owner, Vector3.forward * 4f);
        Assert.AreEqual(3, DeployableRegistry.GetNormalCount(owner));
        Assert.AreEqual(DeployableCategory.DashCharge, dash.Category);
        Assert.AreEqual(DeployableTuning.DashChargeFuseSeconds, dash.RemainingFuse, 0.01f);
        Assert.AreEqual(DeployableTuning.DashChargeDamage, dash.Damage, 0.01f);

        dash.TickFuseForTests(DeployableTuning.DashChargeFuseSeconds);
        Assert.IsTrue(dash.IsDetonated);

        Object.DestroyImmediate(skill);
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void WalkingIntoIdleBomb_DoesNotDetonate_WithoutSaper()
    {
        var owner = CreateOwner();
        var bomb = SpawnIdle(owner, Vector3.zero);
        var enemy = CreateEnemy(new Vector3(0.3f, 0f, 0f), ForcedMovementResistanceCategory.Normal);
        Physics.SyncTransforms();

        bomb.Motor.TickMotorForTests(0.2f);
        Assert.IsFalse(bomb.IsDetonated);

        Object.DestroyImmediate(enemy);
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void ShovedEnemy_DetonatesStationaryBomb()
    {
        var owner = CreateOwner();
        var bomb = SpawnIdle(owner, Vector3.zero);
        var enemy = CreateEnemy(new Vector3(0.3f, 0f, 0f), ForcedMovementResistanceCategory.Normal);
        Physics.SyncTransforms();

        ForcedMovementResolver.Apply(
            ForcedMovementRequest.ShoveAlong(owner, Vector3.forward, 8f, 0.3f),
            enemy);

        bomb.Motor.TickMotorForTests(0.05f);
        Assert.IsTrue(bomb.IsDetonated);

        Object.DestroyImmediate(enemy);
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void KickedBomb_ExplodesOnEnemyContact()
    {
        var owner = CreateOwner();
        var bomb = SpawnIdle(owner, Vector3.zero);
        var enemy = CreateEnemy(new Vector3(0.2f, 0f, 0f), ForcedMovementResistanceCategory.Normal);
        Physics.SyncTransforms();

        bomb.Motor.Kick(Vector3.forward, 16f, 6.5f);
        bomb.Motor.TickMotorForTests(0.05f);
        Assert.IsTrue(bomb.IsDetonated);

        Object.DestroyImmediate(enemy);
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void Detonate_IsIdempotent_NoDoubleExplosion()
    {
        var owner = CreateOwner();
        var bomb = SpawnIdle(owner, Vector3.zero);
        var detonations = 0;
        bomb.Detonated += _ => detonations++;

        bomb.Detonate();
        bomb.Detonate();
        bomb.Detonate();
        Assert.AreEqual(1, detonations);

        Object.DestroyImmediate(owner);
    }

    [Test]
    public void KickArea_HitsWideFront_IgnoresBehind()
    {
        var origin = Vector3.zero;
        var aim = Vector3.forward;
        Assert.IsTrue(DeployableKickAreaResolver.Contains(origin, aim, 3.4f, 3.8f, new Vector3(0.5f, 0f, 2f)));
        Assert.IsTrue(DeployableKickAreaResolver.Contains(origin, aim, 3.4f, 3.8f, new Vector3(1.4f, 0f, 1.5f)));
        Assert.IsFalse(DeployableKickAreaResolver.Contains(origin, aim, 3.4f, 3.8f, new Vector3(0f, 0f, -1.5f)));
    }

    [Test]
    public void KickArea_ShovesNormals_NotBosses_AndLaunchesMultipleBombs()
    {
        var caster = CreateOwner();
        var skill = SkillContentFactory.CreateBombermanKopniak();
        var left = SpawnIdle(caster, new Vector3(-0.8f, 0f, 1.5f));
        var right = SpawnIdle(caster, new Vector3(0.8f, 0f, 1.5f));
        var grunt = CreateEnemy(new Vector3(0f, 0f, 1.6f), ForcedMovementResistanceCategory.Normal);
        var boss = CreateEnemy(new Vector3(0.4f, 0f, 1.8f), ForcedMovementResistanceCategory.Boss);
        Physics.SyncTransforms();

        DeployableKickAreaResolver.Execute(skill, caster, Vector3.zero, Vector3.forward, ~0);

        Assert.IsTrue(left.WasKicked);
        Assert.IsTrue(right.WasKicked);
        Assert.IsTrue(grunt.GetComponent<ForcedMovementReceiver>().IsShoveActive);
        Assert.IsFalse(boss.GetComponent<ForcedMovementReceiver>().IsShoveActive);

        Object.DestroyImmediate(skill);
        Object.DestroyImmediate(grunt);
        Object.DestroyImmediate(boss);
        Object.DestroyImmediate(caster);
    }

    [Test]
    public void Elite_ShoveIsWeakerThanNormal()
    {
        var normal = CreateEnemy(Vector3.zero, ForcedMovementResistanceCategory.Normal);
        var elite = CreateEnemy(Vector3.right * 4f, ForcedMovementResistanceCategory.Elite);

        ForcedMovementResolver.Apply(
            ForcedMovementRequest.ShoveAlong(normal, Vector3.forward, 10f, 0.3f),
            normal);
        ForcedMovementResolver.Apply(
            ForcedMovementRequest.ShoveAlong(elite, Vector3.forward, 10f, 0.3f),
            elite);

        Assert.AreEqual(10f, normal.GetComponent<ForcedMovementReceiver>().Velocity.magnitude, 0.05f);
        Assert.AreEqual(5f, elite.GetComponent<ForcedMovementReceiver>().Velocity.magnitude, 0.05f);

        Object.DestroyImmediate(normal);
        Object.DestroyImmediate(elite);
    }

    [Test]
    public void DashCharge_DoesNotInheritCluster()
    {
        var owner = CreateOwner();
        owner.GetComponent<PlayerPersistentEffects>().ReplaceAll(new List<PersistentEffectBinding>
        {
            new PersistentEffectBinding(PersistentEffectKind.ClusterOnExplode)
        });

        var beforeChildren = CountCategory(DeployableCategory.Child);
        var dash = DeployableRegistry.PlaceDashCharge(owner, Vector3.zero);
        dash.Detonate();
        Assert.AreEqual(beforeChildren, CountCategory(DeployableCategory.Child));

        Object.DestroyImmediate(owner);
    }

    private static int CountCategory(DeployableCategory category)
    {
        var count = 0;
        foreach (var d in Object.FindObjectsByType<Deployable>())
        {
            if (d != null && !d.IsDetonated && d.Category == category)
                count++;
        }

        return count;
    }

    private static GameObject CreateOwner()
    {
        var go = new GameObject("Owner");
        go.AddComponent<Health>().Configure(100f);
        go.AddComponent<PlayerPersistentEffects>();
        DeployableRegistry.Ensure();
        return go;
    }

    private static Deployable SpawnIdle(GameObject owner, Vector3 pos)
    {
        return Deployable.SpawnPlaceholder(pos, owner, DeployableCategory.Normal, 16f, 2.4f, 8f, 8, true);
    }

    private static GameObject CreateEnemy(Vector3 pos, ForcedMovementResistanceCategory category)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.transform.position = pos;
        go.AddComponent<Health>().Configure(40f);
        var motor = go.AddComponent<EnemyLaneMotor>();
        var def = ScriptableObject.CreateInstance<EnemyDefinition>();
        motor.Initialize(AttackLineId.Center, def);
        go.AddComponent<EnemyController>().Configure(def, AttackLineId.Center);
        go.AddComponent<ForcedMovementReceiver>();
        go.AddComponent<ForcedMovementResistanceProfile>().Configure(category);
        return go;
    }
}
