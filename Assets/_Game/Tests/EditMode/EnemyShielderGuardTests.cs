using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Shielder redirect — ochrona sojusznika za Shielderem (M8.2).
/// </summary>
public class EnemyShielderGuardTests
{
    private const float ProtectRadius = 3.5f;

    [TearDown]
    public void TearDown()
    {
        EnemyRegistry.Clear();
    }

    [Test]
    public void IsProtected_SameLaneBehindShielder_InRadius_ReturnsTrue()
    {
        var allyPos = new Vector3(0f, 0f, 16f);
        var shielderPos = new Vector3(0f, 0f, 14f);

        Assert.IsTrue(EnemyShielderGuardMath.IsProtected(
            AttackLineId.Center, allyPos,
            AttackLineId.Center, shielderPos,
            ProtectRadius));
    }

    [Test]
    public void IsProtected_AllyInFrontOfShielder_ReturnsFalse()
    {
        var allyPos = new Vector3(0f, 0f, 12f);
        var shielderPos = new Vector3(0f, 0f, 14f);

        Assert.IsFalse(EnemyShielderGuardMath.IsProtected(
            AttackLineId.Center, allyPos,
            AttackLineId.Center, shielderPos,
            ProtectRadius));
    }

    [Test]
    public void IsProtected_DifferentLane_ReturnsFalse()
    {
        var allyPos = new Vector3(0f, 0f, 16f);
        var shielderPos = new Vector3(-14f, 0f, 14f);

        Assert.IsFalse(EnemyShielderGuardMath.IsProtected(
            AttackLineId.Center, allyPos,
            AttackLineId.Left, shielderPos,
            ProtectRadius));
    }

    [Test]
    public void Redirect_AllyBehindShielder_AllyTakesZero_ShielderTakesDamage()
    {
        var shielder = CreateEnemy(EnemyKind.Shielder, AttackLineId.Center, 50f);
        var grunt = CreateEnemy(EnemyKind.Grunt, AttackLineId.Center, 30f);

        shielder.transform.position = new Vector3(0f, 0f, 14f);
        grunt.transform.position = new Vector3(0.5f, 0f, 16f);

        var gruntHealth = grunt.GetComponent<Health>();
        var shielderHealth = shielder.GetComponent<Health>();
        var gruntHpBefore = gruntHealth.CurrentHealth;
        var shielderHpBefore = shielderHealth.CurrentHealth;

        gruntHealth.TakeDamage(10f);

        Assert.AreEqual(gruntHpBefore, gruntHealth.CurrentHealth, "Sojusznik nie powinien otrzymać obrażeń.");
        Assert.Less(shielderHealth.CurrentHealth, shielderHpBefore, "Shielder powinien otrzymać przekierowane obrażenia.");

        Cleanup(shielder, grunt);
    }

    [Test]
    public void Redirect_AllyInFront_NoRedirect()
    {
        var shielder = CreateEnemy(EnemyKind.Shielder, AttackLineId.Center, 50f);
        var grunt = CreateEnemy(EnemyKind.Grunt, AttackLineId.Center, 30f);

        shielder.transform.position = new Vector3(0f, 0f, 14f);
        grunt.transform.position = new Vector3(0f, 0f, 12f);

        var gruntHealth = grunt.GetComponent<Health>();
        var shielderHealth = shielder.GetComponent<Health>();
        var shielderHpBefore = shielderHealth.CurrentHealth;

        gruntHealth.TakeDamage(10f);

        Assert.AreEqual(20f, gruntHealth.CurrentHealth);
        Assert.AreEqual(shielderHpBefore, shielderHealth.CurrentHealth);

        Cleanup(shielder, grunt);
    }

    [Test]
    public void Redirect_DeadShielder_NoRedirect()
    {
        var shielder = CreateEnemy(EnemyKind.Shielder, AttackLineId.Center, 50f);
        var grunt = CreateEnemy(EnemyKind.Grunt, AttackLineId.Center, 30f);

        shielder.transform.position = new Vector3(0f, 0f, 14f);
        grunt.transform.position = new Vector3(0f, 0f, 16f);

        shielder.GetComponent<Health>().ForceDeath();

        var gruntHealth = grunt.GetComponent<Health>();
        gruntHealth.TakeDamage(10f);

        Assert.AreEqual(20f, gruntHealth.CurrentHealth);

        Cleanup(shielder, grunt);
    }

    [Test]
    public void Redirect_ShielderOwnDamage_NoRedirect()
    {
        var shielder = CreateEnemy(EnemyKind.Shielder, AttackLineId.Center, 50f);
        shielder.transform.position = new Vector3(0f, 0f, 14f);

        var shielderHealth = shielder.GetComponent<Health>();
        shielderHealth.TakeDamage(10f);

        Assert.AreEqual(40f, shielderHealth.CurrentHealth);

        Cleanup(shielder);
    }

    [Test]
    public void Redirect_LethalHitOnShielder_NoOverflowToAlly()
    {
        var shielder = CreateEnemy(EnemyKind.Shielder, AttackLineId.Center, 15f);
        var grunt = CreateEnemy(EnemyKind.Grunt, AttackLineId.Center, 30f);

        shielder.transform.position = new Vector3(0f, 0f, 14f);
        grunt.transform.position = new Vector3(0f, 0f, 16f);

        var gruntHealth = grunt.GetComponent<Health>();
        var shielderHealth = shielder.GetComponent<Health>();

        gruntHealth.TakeDamage(25f);

        Assert.AreEqual(30f, gruntHealth.CurrentHealth);
        Assert.IsFalse(shielderHealth.IsAlive);

        Cleanup(shielder, grunt);
    }

    private static GameObject CreateEnemy(EnemyKind kind, AttackLineId lane, float hp)
    {
        var go = new GameObject($"Enemy_{kind}");
        go.AddComponent<Health>().Configure(hp);
        go.AddComponent<StatusEffectReceiver>();

        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        definition.ConfigureForTest(kind);
        if (kind == EnemyKind.Shielder)
            definition.ConfigureShielderForTest(ProtectRadius);

        var motor = go.AddComponent<EnemyLaneMotor>();
        typeof(EnemyLaneMotor)
            .GetField("separationStrength", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(motor, 0f);
        motor.Initialize(lane, definition);

        var controller = go.AddComponent<EnemyController>();
        controller.Configure(definition, lane);
        go.SetActive(true);
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
