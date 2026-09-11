using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Aura Support — zasięg, buff, brak self-buffu, timeout (M8.2).
/// </summary>
public class EnemySupportBuffTests
{
    private const float BuffRadius = 5f;
    private const float MoveMul = 1.2f;
    private const float DmgMul = 1.2f;
    private const float BuffDuration = 0.35f;

    [TearDown]
    public void TearDown()
    {
        EnemyRegistry.Clear();
    }

    [Test]
    public void IsInRange_UsesXZOnly_IgnoresY()
    {
        var a = new Vector3(0f, 0f, 0f);
        var b = new Vector3(3f, 10f, 4f);

        Assert.IsTrue(EnemySupportBuffMath.IsInRange(a, b, 5f));
        Assert.IsFalse(EnemySupportBuffMath.IsInRange(a, b, 4.9f));
    }

    [Test]
    public void CombineMultiplier_DoesNotStack_ReturnsMax()
    {
        Assert.AreEqual(1.2f, EnemySupportBuffMath.CombineMultiplier(1f, 1.2f));
        Assert.AreEqual(1.2f, EnemySupportBuffMath.CombineMultiplier(1.2f, 1.2f));
        Assert.AreEqual(1.3f, EnemySupportBuffMath.CombineMultiplier(1.2f, 1.3f));
    }

    [Test]
    public void SupportAura_GruntInRange_ReceivesMoveAndDamageBuff()
    {
        var support = CreateEnemy(EnemyKind.Support, AttackLineId.Center, isSupport: true);
        var grunt = CreateEnemy(EnemyKind.Grunt, AttackLineId.Center);

        support.transform.position = new Vector3(0f, 0f, 10f);
        grunt.transform.position = new Vector3(2f, 0f, 12f);

        InvokeTickSupportAura(support.GetComponent<EnemyController>());

        var gruntController = grunt.GetComponent<EnemyController>();
        Assert.AreEqual(MoveMul, gruntController.AllyMoveMultiplier, 0.001f);
        Assert.AreEqual(DmgMul, gruntController.AllyDamageMultiplier, 0.001f);

        Cleanup(support, grunt);
    }

    [Test]
    public void SupportAura_GruntOutOfRange_NoBuff()
    {
        var support = CreateEnemy(EnemyKind.Support, AttackLineId.Center, isSupport: true);
        var grunt = CreateEnemy(EnemyKind.Grunt, AttackLineId.Center);

        support.transform.position = new Vector3(0f, 0f, 10f);
        grunt.transform.position = new Vector3(10f, 0f, 10f);

        InvokeTickSupportAura(support.GetComponent<EnemyController>());

        var gruntController = grunt.GetComponent<EnemyController>();
        Assert.AreEqual(1f, gruntController.AllyMoveMultiplier);
        Assert.AreEqual(1f, gruntController.AllyDamageMultiplier);

        Cleanup(support, grunt);
    }

    [Test]
    public void SupportAura_DoesNotBuffSelf()
    {
        var support = CreateEnemy(EnemyKind.Support, AttackLineId.Center, isSupport: true);
        support.transform.position = new Vector3(0f, 0f, 10f);

        InvokeTickSupportAura(support.GetComponent<EnemyController>());

        var supportController = support.GetComponent<EnemyController>();
        Assert.AreEqual(1f, supportController.AllyMoveMultiplier);
        Assert.AreEqual(1f, supportController.AllyDamageMultiplier);

        Cleanup(support);
    }

    [Test]
    public void SupportAura_AfterSupportDeath_BuffExpiresAfterTimeout()
    {
        var support = CreateEnemy(EnemyKind.Support, AttackLineId.Center, isSupport: true);
        var grunt = CreateEnemy(EnemyKind.Grunt, AttackLineId.Center);

        support.transform.position = new Vector3(0f, 0f, 10f);
        grunt.transform.position = new Vector3(1f, 0f, 11f);

        var supportController = support.GetComponent<EnemyController>();
        InvokeTickSupportAura(supportController);

        var gruntController = grunt.GetComponent<EnemyController>();
        Assert.AreEqual(MoveMul, gruntController.AllyMoveMultiplier, 0.001f);

        support.GetComponent<Health>().ForceDeath();
        support.SetActive(false);

        InvokeTickAllyBuff(gruntController, BuffDuration + 0.05f);

        Assert.AreEqual(1f, gruntController.AllyMoveMultiplier);
        Assert.AreEqual(1f, gruntController.AllyDamageMultiplier);

        Cleanup(support, grunt);
    }

    [Test]
    public void ApplyAllyBuff_SetsMultipliersAndDuration()
    {
        var grunt = CreateEnemy(EnemyKind.Grunt, AttackLineId.Center);
        var controller = grunt.GetComponent<EnemyController>();

        controller.ApplyAllyBuff(MoveMul, DmgMul, BuffDuration);
        Assert.AreEqual(MoveMul, controller.AllyMoveMultiplier);
        Assert.AreEqual(DmgMul, controller.AllyDamageMultiplier);

        InvokeTickAllyBuff(controller, BuffDuration + 0.01f);
        Assert.AreEqual(1f, controller.AllyMoveMultiplier);
        Assert.AreEqual(1f, controller.AllyDamageMultiplier);

        Cleanup(grunt);
    }

    private static GameObject CreateEnemy(EnemyKind kind, AttackLineId lane, bool isSupport = false)
    {
        var go = new GameObject($"Enemy_{kind}");
        go.AddComponent<Health>().Configure(30f);
        go.AddComponent<StatusEffectReceiver>();

        var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
        definition.ConfigureForTest(kind);
        if (isSupport)
            definition.ConfigureSupportForTest(BuffRadius, MoveMul, DmgMul);

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

    private static void InvokeTickSupportAura(EnemyController controller)
    {
        typeof(EnemyController)
            .GetMethod("TickSupportAura", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(controller, null);
    }

    private static void InvokeTickAllyBuff(EnemyController controller, float deltaTime)
    {
        typeof(EnemyController)
            .GetMethod("TickAllyBuff", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(controller, new object[] { deltaTime });
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
