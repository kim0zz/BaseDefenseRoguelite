using NUnit.Framework;
using UnityEngine;

public class ForcedMovementTests
{
    [Test]
    public void Elite_Knockback_IsScaledDown()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Elite);
        var receiver = go.GetComponent<ForcedMovementReceiver>();

        ForcedMovementResolver.Apply(
            ForcedMovementRequest.KnockbackFromOrigin(go, Vector3.zero, 4f), go);

        Assert.IsTrue(receiver.IsActive);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Boss_Knockback_IsBlocked()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Boss);
        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.KnockbackFromOrigin(go, Vector3.zero, 4f), go);

        Assert.IsFalse(outcome.Applied);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Elite_Stun_IsShortened()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Elite);
        var status = go.GetComponent<StatusEffectReceiver>();

        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.StunControl(go, 1.2f), go);

        Assert.IsTrue(outcome.Applied);
        Assert.AreEqual(1.2f * ForcedMovementResistanceMath.EliteStunScale, outcome.AppliedControlSeconds, 0.01f);
        Assert.IsTrue(status.IsStunned);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Boss_Stun_AppliesStaggerInstead()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Boss);
        var status = go.GetComponent<StatusEffectReceiver>();

        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.StunControl(go, 1.2f, staggerContribution: 12f), go);

        Assert.IsTrue(outcome.AppliedStaggerInsteadOfStun);
        Assert.Greater(status.StaggerAccumulated, 0f);
        Assert.IsFalse(status.IsStunned);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Structure_IsImmuneToControl()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Structure);
        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.StunControl(go, 1.2f), go);

        Assert.IsFalse(outcome.Applied);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Shove_MaintainsConstantSpeedForDuration_ThenStops()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Normal);
        var receiver = go.GetComponent<ForcedMovementReceiver>();
        go.transform.position = Vector3.zero;

        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.ShoveAlong(go, Vector3.forward, 4f, 0.5f, honorCollisions: false), go);
        Assert.IsTrue(outcome.Applied);
        Assert.IsTrue(receiver.IsActive);

        receiver.Tick(0.1f);
        Assert.AreEqual(0.4f, go.transform.position.z, 0.02f);
        Assert.AreEqual(4f, receiver.Velocity.magnitude, 0.02f);

        receiver.Tick(0.1f);
        Assert.AreEqual(0.8f, go.transform.position.z, 0.02f);
        Assert.AreEqual(4f, receiver.Velocity.magnitude, 0.02f);

        receiver.Tick(0.3f);
        Assert.IsFalse(receiver.IsShoveActive);
        Assert.AreEqual(0f, receiver.Velocity.sqrMagnitude, 0.0001f);
        Assert.IsFalse(receiver.IsActive);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Elite_Shove_ScalesSpeedNotDuration()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Elite);
        var receiver = go.GetComponent<ForcedMovementReceiver>();
        go.transform.position = Vector3.zero;

        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.ShoveAlong(go, Vector3.forward, 4f, 0.5f, honorCollisions: false), go);
        Assert.IsTrue(outcome.Applied);
        Assert.AreEqual(0.5f, receiver.ShoveRemaining, 0.01f);

        receiver.Tick(0.1f);
        var expectedSpeed = 4f * ForcedMovementResistanceMath.EliteDisplacementScale;
        Assert.AreEqual(expectedSpeed, receiver.Velocity.magnitude, 0.02f);
        Assert.AreEqual(0.4f, receiver.ShoveRemaining, 0.02f);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Boss_Shove_IsBlockedWithNoMovement()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Boss);
        var receiver = go.GetComponent<ForcedMovementReceiver>();
        go.transform.position = Vector3.zero;

        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.ShoveAlong(go, Vector3.forward, 4f, 0.5f), go);

        Assert.IsFalse(outcome.Applied);
        Assert.IsFalse(receiver.IsActive);
        Assert.AreEqual(Vector3.zero, go.transform.position);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Structure_Shove_IsBlockedWithNoMovement()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Structure);
        var receiver = go.GetComponent<ForcedMovementReceiver>();
        go.transform.position = Vector3.zero;

        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.ShoveAlong(go, Vector3.forward, 4f, 0.5f), go);

        Assert.IsFalse(outcome.Applied);
        Assert.IsFalse(receiver.IsActive);
        Assert.AreEqual(Vector3.zero, go.transform.position);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Charge_StopsOnBoss_WithZeroBossDisplacement()
    {
        var caster = new GameObject("Caster");
        caster.transform.position = Vector3.zero;
        var casterReceiver = caster.AddComponent<ForcedMovementReceiver>();

        var boss = CreateObstacle(ForcedMovementResistanceCategory.Boss, new Vector3(0f, 0f, 2f));

        ForcedMovementResolver.Apply(
            ForcedMovementRequest.CasterCharge(caster, Vector3.forward, 10f, 1f, honorCollisions: false), caster);

        for (var i = 0; i < 20; i++)
            casterReceiver.Tick(0.05f);

        Assert.AreEqual(ChargeStopReason.Boss, casterReceiver.LastChargeStopReason);
        Assert.Less(caster.transform.position.z, 1.8f);
        Assert.AreEqual(2f, boss.transform.position.z, 0.01f);

        Object.DestroyImmediate(caster);
        Object.DestroyImmediate(boss);
    }

    [Test]
    public void Charge_StopsOnStructure()
    {
        var caster = new GameObject("Caster");
        caster.transform.position = Vector3.zero;
        var casterReceiver = caster.AddComponent<ForcedMovementReceiver>();

        var structure = CreateObstacle(ForcedMovementResistanceCategory.Structure, new Vector3(0f, 0f, 1.5f));

        ForcedMovementResolver.Apply(
            ForcedMovementRequest.CasterCharge(caster, Vector3.forward, 10f, 1f, honorCollisions: false), caster);

        for (var i = 0; i < 20; i++)
            casterReceiver.Tick(0.05f);

        Assert.AreEqual(ChargeStopReason.Structure, casterReceiver.LastChargeStopReason);
        Assert.Less(caster.transform.position.z, 1.4f);

        Object.DestroyImmediate(caster);
        Object.DestroyImmediate(structure);
    }

    [Test]
    public void HonorCollisions_ClampInsideMapPlayArea()
    {
        if (MapPlayArea.Instance != null)
            Object.DestroyImmediate(MapPlayArea.Instance.gameObject);

        var playAreaGo = new GameObject("MapPlayArea");
        var playArea = playAreaGo.AddComponent<MapPlayArea>();
        playArea.SetBounds(new Vector2(-1f, 1f), new Vector2(-1f, 1f));
        Assert.AreEqual(playArea, MapPlayArea.Instance);

        var go = CreateTarget(ForcedMovementResistanceCategory.Normal);
        var receiver = go.GetComponent<ForcedMovementReceiver>();
        go.transform.position = new Vector3(0.9f, 0f, 0f);

        ForcedMovementResolver.Apply(
            ForcedMovementRequest.ShoveAlong(go, Vector3.right, 10f, 0.5f, honorCollisions: true), go);

        receiver.Tick(0.1f);
        Assert.LessOrEqual(go.transform.position.x, 1f + 0.001f);
        Assert.Greater(go.transform.position.x, 0.9f);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(playAreaGo);
    }

    [Test]
    public void CrowdControlImmunity_BlocksKnockback_AllowsStun()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Normal);
        var status = go.GetComponent<StatusEffectReceiver>();
        var immunity = go.AddComponent<CrowdControlImmunity>();
        immunity.Enable(knockback: true, stagger: true, stun: false, duration: 5f);

        var knockback = ForcedMovementResolver.Apply(
            ForcedMovementRequest.KnockbackFromOrigin(go, Vector3.zero, 4f), go);
        Assert.IsFalse(knockback.Applied);

        var stun = ForcedMovementResolver.Apply(
            ForcedMovementRequest.StunControl(go, 1.2f), go);
        Assert.IsTrue(stun.Applied);
        Assert.IsTrue(status.IsStunned);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void CrowdControlImmunity_DoesNotBlockCasterCharge()
    {
        var caster = new GameObject("Caster");
        caster.transform.position = Vector3.zero;
        var receiver = caster.AddComponent<ForcedMovementReceiver>();
        var immunity = caster.AddComponent<CrowdControlImmunity>();
        immunity.Enable(knockback: true, stagger: true, stun: false, duration: 5f);

        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.CasterCharge(caster, Vector3.forward, 10f, 0.2f, honorCollisions: false),
            caster);

        Assert.IsTrue(outcome.Applied);
        receiver.Tick(0.1f);
        Assert.Greater(caster.transform.position.z, 0.5f);

        Object.DestroyImmediate(caster);
    }

    [Test]
    public void CrowdControlImmunity_BlocksBossStaggerConversion()
    {
        var go = CreateTarget(ForcedMovementResistanceCategory.Boss);
        var status = go.GetComponent<StatusEffectReceiver>();
        var immunity = go.AddComponent<CrowdControlImmunity>();
        immunity.Enable(knockback: true, stagger: true, stun: false, duration: 5f);

        var outcome = ForcedMovementResolver.Apply(
            ForcedMovementRequest.StunControl(go, 1.2f, staggerContribution: 12f), go);

        Assert.IsFalse(outcome.Applied);
        Assert.AreEqual(0f, status.StaggerAccumulated);

        Object.DestroyImmediate(go);
    }

    private static GameObject CreateTarget(ForcedMovementResistanceCategory category)
    {
        var go = new GameObject("ForcedMoveTarget");
        var profile = go.AddComponent<ForcedMovementResistanceProfile>();
        profile.Configure(category);
        go.AddComponent<ForcedMovementReceiver>();
        go.AddComponent<StatusEffectReceiver>();
        return go;
    }

    private static GameObject CreateObstacle(ForcedMovementResistanceCategory category, Vector3 position)
    {
        var go = new GameObject($"Obstacle_{category}");
        go.transform.position = position;
        var profile = go.AddComponent<ForcedMovementResistanceProfile>();
        profile.Configure(category);
        var collider = go.AddComponent<SphereCollider>();
        collider.radius = 0.6f;
        return go;
    }
}
