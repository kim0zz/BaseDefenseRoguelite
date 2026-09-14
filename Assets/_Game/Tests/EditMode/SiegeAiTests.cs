using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public sealed class SiegeAiTests
{
    [TestCase(EnemyKind.Rusher)]
    [TestCase(EnemyKind.Siege)]
    [TestCase(EnemyKind.Flanker)]
    public void StructureFocusedSiegeKindsPreferDefenseTarget(EnemyKind kind)
    {
        Assert.IsTrue(StructureTargeting.PrefersStructureOverPlayer(kind));
    }

    [Test]
    public void SiegeTargetGateThenCoreDoesNotCreatePlayerXpSource()
    {
        var arenaGo = new GameObject("SiegeAiArenaTest");
        var attacker = new GameObject("SiegeAttacker");
        try
        {
            var arena = arenaGo.AddComponent<SiegeArena>();
            arena.EnsureInitialized();
            var target = StructureTargeting.ResolveStructureTarget(AttackLineId.Center, 2f, attacker.transform.position);
            Assert.AreSame(arena.Gate, target);
            arena.Gate.ForceDeath();
            target = StructureTargeting.ResolveStructureTarget(AttackLineId.Center, 2f, attacker.transform.position);
            Assert.AreSame(arena.Core, target);
            Assert.IsNull(arena.Gate.LastDamageSource);
            Assert.IsNull(arena.Core.LastDamageSource);
        }
        finally { Object.DestroyImmediate(attacker); Object.DestroyImmediate(arenaGo); }
    }

    [Test]
    public void SiegeStructureAttackStartsReadableWindupBeforeDamage()
    {
        var arenaGo = new GameObject("SiegeWindupArenaTest");
        var enemyGo = new GameObject("BurzycielTest");
        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        try
        {
            var arena = arenaGo.AddComponent<SiegeArena>();
            if (arena.Gate == null) arena.SendMessage("Awake", SendMessageOptions.RequireReceiver);
            definition.ConfigureForTest(EnemyKind.Siege);
            var health = enemyGo.AddComponent<Health>();
            health.Configure(100f);
            enemyGo.AddComponent<StatusEffectReceiver>();
            enemyGo.AddComponent<EnemyLaneMotor>();
            var controller = enemyGo.AddComponent<EnemyController>();
            controller.Configure(definition, AttackLineId.Center);
            enemyGo.transform.position = arena.Gate.transform.position + Vector3.forward * 1.2f;
            var method = typeof(EnemyController).GetMethod("TryAttackStructure", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(controller, new object[] { arena.Gate });
            Assert.Greater(controller.StructureWindupRemaining, 0f);
            Assert.AreEqual(arena.Gate.MaxHealth, arena.Gate.CurrentHealth);
        }
        finally
        {
            Object.DestroyImmediate(enemyGo);
            Object.DestroyImmediate(arenaGo);
            Object.DestroyImmediate(definition);
        }
    }
}
