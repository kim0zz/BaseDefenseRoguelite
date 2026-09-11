using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Warstwa threat / taunt (M7.6-T2).
/// </summary>
public class EnemyThreatTests
{
    private const float TauntDuration = 3.5f;

    [Test]
    public void Grunt_TaunterInCorridor_TryTauntTrue_TargetIsTaunter()
    {
        var lane = AttackLineId.Left;
        var path = MapGreyboxLayout.GetLanePath(lane);
        var onLane = path.Waypoints[1];

        var (enemyGo, controller, _) = CreateEnemy(EnemyKind.Grunt, lane);
        enemyGo.transform.position = onLane;

        var taunter = CreateTaunter(onLane + Vector3.forward * 0.5f);

        Assert.IsTrue(EnemyThreatService.TryTaunt(enemyGo, taunter, TauntDuration));

        InvokeRefreshTargets(controller);
        Assert.AreSame(taunter, GetPrivateTarget(controller));

        Cleanup(enemyGo, taunter.gameObject);
    }

    [Test]
    public void Grunt_TaunterOutsideCorridor_TryTauntFalse_NoThreatApplied()
    {
        var lane = AttackLineId.Left;
        var path = MapGreyboxLayout.GetLanePath(lane);
        var onLane = path.Waypoints[1];
        var offCorridor = MapGreyboxLayout.GetFarEnd(AttackLineId.Center);

        var (enemyGo, controller, _) = CreateEnemy(EnemyKind.Grunt, lane);
        enemyGo.transform.position = onLane;

        var taunter = CreateTaunter(offCorridor);
        var motor = enemyGo.GetComponent<EnemyLaneMotor>();

        Assert.IsFalse(motor.IsInLaneCorridor(taunter.transform.position));
        Assert.IsFalse(EnemyThreatService.TryTaunt(enemyGo, taunter, TauntDuration));
        Assert.IsNull(enemyGo.GetComponent<EnemyThreatState>());

        InvokeRefreshTargets(controller);
        Assert.IsNull(GetPrivateTarget(controller));

        Cleanup(enemyGo, taunter.gameObject);
    }

    [Test]
    public void Rusher_ActiveTaunt_IgnoresStructureButKindStillPrefersStructure()
    {
        Assert.IsTrue(StructureTargeting.PrefersStructureOverPlayer(EnemyKind.Rusher));
        Assert.IsTrue(EnemyThreatMath.IgnoresStructureWhileTaunted(EnemyKind.Rusher, threatActive: true));
        Assert.IsFalse(EnemyThreatMath.IgnoresStructureWhileTaunted(EnemyKind.Rusher, threatActive: false));

        var (enemyGo, controller, _) = CreateEnemy(EnemyKind.Rusher, AttackLineId.Center);
        var taunter = CreateTaunter(Vector3.zero);

        Assert.IsTrue(EnemyThreatService.TryTaunt(enemyGo, taunter, TauntDuration));
        Assert.IsTrue(controller.IsAttackingStructureIgnoredBecauseTaunt);

        Cleanup(enemyGo, taunter.gameObject);
    }

    [Test]
    public void Hunter_Taunt_OverwritesHuntLock_LastWriteWins()
    {
        var (enemyGo, controller, _) = CreateEnemy(EnemyKind.Hunter, AttackLineId.Center);
        var first = CreateTaunter(new Vector3(-2f, 0f, 10f));
        var second = CreateTaunter(new Vector3(2f, 0f, 10f));

        SetPrivateField(controller, "_huntTarget", first);
        SetPrivateField(controller, "_target", first);

        Assert.IsTrue(EnemyThreatService.TryTaunt(enemyGo, first, TauntDuration));
        InvokeRefreshTargets(controller);
        Assert.AreSame(first, GetPrivateHuntTarget(controller));

        Assert.IsTrue(EnemyThreatService.TryTaunt(enemyGo, second, TauntDuration));
        var state = enemyGo.GetComponent<EnemyThreatState>();
        Assert.AreSame(second, state.Taunter);

        InvokeRefreshTargets(controller);
        Assert.AreSame(second, GetPrivateHuntTarget(controller));
        Assert.AreSame(second, GetPrivateTarget(controller));

        Cleanup(enemyGo, first.gameObject, second.gameObject);
    }

    [Test]
    public void BossRam_TryTauntFalse_ChargeTargetUnchanged()
    {
        var bossGo = new GameObject("BossRam");
        bossGo.AddComponent<Health>().Configure(500f);
        bossGo.AddComponent<StatusEffectReceiver>();
        bossGo.AddComponent<BossRamController>();

        var structure = new GameObject("Tower");
        structure.AddComponent<Health>().Configure(100f);
        var originalTarget = structure.GetComponent<Health>() as IDamageable;

        SetPrivateField(bossGo.GetComponent<BossRamController>(), "_chargeTarget", originalTarget);

        var taunter = CreateTaunter(Vector3.zero);
        Assert.IsFalse(EnemyThreatService.TryTaunt(bossGo, taunter, TauntDuration));

        var chargeTarget = GetPrivateField<IDamageable>(bossGo.GetComponent<BossRamController>(), "_chargeTarget");
        Assert.AreSame(originalTarget, chargeTarget);

        Cleanup(bossGo, taunter.gameObject, structure);
    }

    [Test]
    public void LastWriteWins_SecondTaunterReplacesFirst_RefreshesDuration()
    {
        var (enemyGo, _, _) = CreateEnemy(EnemyKind.Grunt, AttackLineId.Center);
        var first = CreateTaunter(new Vector3(-1f, 0f, 12f));
        var second = CreateTaunter(new Vector3(1f, 0f, 12f));

        Assert.IsTrue(EnemyThreatService.TryTaunt(enemyGo, first, TauntDuration));
        var state = enemyGo.GetComponent<EnemyThreatState>();
        Assert.AreSame(first, state.Taunter);

        Assert.IsTrue(EnemyThreatService.TryTaunt(enemyGo, second, TauntDuration));
        Assert.AreSame(second, state.Taunter);
        Assert.IsTrue(state.IsActive);

        state.Tick(TauntDuration - 0.1f);
        Assert.IsTrue(state.IsActive, "Duration powinien zostać odświeżony przez drugi taunt.");

        Cleanup(enemyGo, first.gameObject, second.gameObject);
    }

    [Test]
    public void AfterExpiry_RusherPrefersStructureAgain()
    {
        var (enemyGo, controller, _) = CreateEnemy(EnemyKind.Rusher, AttackLineId.Center);
        var taunter = CreateTaunter(Vector3.zero);

        Assert.IsTrue(EnemyThreatService.TryTaunt(enemyGo, taunter, TauntDuration));
        Assert.IsTrue(controller.IsAttackingStructureIgnoredBecauseTaunt);

        var state = enemyGo.GetComponent<EnemyThreatState>();
        state.Tick(TauntDuration);

        Assert.IsFalse(state.IsActive);
        Assert.IsFalse(controller.IsAttackingStructureIgnoredBecauseTaunt);
        Assert.IsFalse(EnemyThreatMath.IgnoresStructureWhileTaunted(EnemyKind.Rusher, state.IsActive));
        Assert.IsTrue(StructureTargeting.PrefersStructureOverPlayer(EnemyKind.Rusher));

        Cleanup(enemyGo, taunter.gameObject);
    }

    [Test]
    public void CanBeTaunted_RejectsBoss_AcceptsMobKinds()
    {
        Assert.IsFalse(EnemyThreatMath.CanBeTaunted(EnemyKind.Grunt, isBoss: true));
        Assert.IsTrue(EnemyThreatMath.CanBeTaunted(EnemyKind.Grunt, isBoss: false));
        Assert.IsTrue(EnemyThreatMath.CanBeTaunted(EnemyKind.Hunter, isBoss: false));
        Assert.IsTrue(EnemyThreatMath.CanBeTaunted(EnemyKind.Rusher, isBoss: false));
        Assert.IsTrue(EnemyThreatMath.CanBeTaunted(EnemyKind.Carrier, isBoss: false));
        Assert.IsTrue(EnemyThreatMath.CanBeTaunted(EnemyKind.Siege, isBoss: false));
        Assert.IsTrue(EnemyThreatMath.CanBeTaunted(EnemyKind.Support, isBoss: false));
        Assert.IsTrue(EnemyThreatMath.CanBeTaunted(EnemyKind.Flanker, isBoss: false));
        Assert.IsTrue(EnemyThreatMath.CanBeTaunted(EnemyKind.Shielder, isBoss: false));
    }

    [Test]
    public void GruntRequiresTaunterInCorridor_IncludesSupportAndShielder()
    {
        Assert.IsTrue(EnemyThreatMath.GruntRequiresTaunterInCorridor(EnemyKind.Grunt));
        Assert.IsTrue(EnemyThreatMath.GruntRequiresTaunterInCorridor(EnemyKind.Carrier));
        Assert.IsTrue(EnemyThreatMath.GruntRequiresTaunterInCorridor(EnemyKind.Support));
        Assert.IsTrue(EnemyThreatMath.GruntRequiresTaunterInCorridor(EnemyKind.Shielder));
        Assert.IsFalse(EnemyThreatMath.GruntRequiresTaunterInCorridor(EnemyKind.Hunter));
        Assert.IsFalse(EnemyThreatMath.GruntRequiresTaunterInCorridor(EnemyKind.Rusher));
        Assert.IsFalse(EnemyThreatMath.GruntRequiresTaunterInCorridor(EnemyKind.Flanker));
    }

    [Test]
    public void Flanker_IgnoresStructureWhileTaunted()
    {
        Assert.IsTrue(EnemyThreatMath.IgnoresStructureWhileTaunted(EnemyKind.Flanker, threatActive: true));
        Assert.IsFalse(EnemyThreatMath.IgnoresStructureWhileTaunted(EnemyKind.Flanker, threatActive: false));
    }

    private static (GameObject enemyGo, EnemyController controller, EnemyThreatState threat) CreateEnemy(
        EnemyKind kind, AttackLineId lane)
    {
        var go = new GameObject($"Enemy_{kind}");
        go.AddComponent<Health>().Configure(30f);
        go.AddComponent<StatusEffectReceiver>();

        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        definition.ConfigureForTest(kind);

        var motor = go.AddComponent<EnemyLaneMotor>();
        typeof(EnemyLaneMotor)
            .GetField("separationStrength", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(motor, 0f);
        motor.Initialize(lane, definition);

        var controller = go.AddComponent<EnemyController>();
        controller.Configure(definition, lane);

        return (go, controller, go.GetComponent<EnemyThreatState>());
    }

    private static PlayerCharacter CreateTaunter(Vector3 position)
    {
        var go = new GameObject("Taunter");
        go.transform.position = position;
        return go.AddComponent<PlayerCharacter>();
    }

    private static void InvokeRefreshTargets(EnemyController controller)
    {
        typeof(EnemyController)
            .GetMethod("RefreshTargets", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(controller, null);
    }

    private static PlayerCharacter GetPrivateTarget(EnemyController controller)
    {
        return GetPrivateField<PlayerCharacter>(controller, "_target");
    }

    private static PlayerCharacter GetPrivateHuntTarget(EnemyController controller)
    {
        return GetPrivateField<PlayerCharacter>(controller, "_huntTarget");
    }

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field?.GetValue(instance);
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        instance.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(instance, value);
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
