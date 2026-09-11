using NUnit.Framework;
using UnityEngine;

public class CombatFeelServiceTests
{
    [Test]
    public void CanStart_OnlyWhenTimeScaleIsOne()
    {
        Assert.IsTrue(HitStopPolicy.CanStart(1f));
        Assert.IsFalse(HitStopPolicy.CanStart(0f));
        Assert.IsFalse(HitStopPolicy.CanStart(HitStopPolicy.HitStopTimeScale));
    }

    [Test]
    public void Restore_KeepsPauseWhenScaleIsZero()
    {
        Assert.AreEqual(0f, HitStopPolicy.ResolveRestoreScale(0f, 1f));
    }

    [Test]
    public void Restore_ReturnsPreHitStopScaleWhenNotPaused()
    {
        Assert.AreEqual(1f, HitStopPolicy.ResolveRestoreScale(HitStopPolicy.HitStopTimeScale, 1f));
    }

    [Test]
    public void ExtendRemaining_TakesMax()
    {
        Assert.AreEqual(0.10f, HitStopPolicy.ExtendRemaining(0.04f, 0.10f), 0.0001f);
        Assert.AreEqual(0.12f, HitStopPolicy.ExtendRemaining(0.12f, 0.03f), 0.0001f);
    }
}

public class ProjectileRulesTests
{
    [Test]
    public void HasReachedMaxDistance_AtAndBeyondRange()
    {
        Assert.IsFalse(ProjectileRules.HasReachedMaxDistance(2.9f, 3f));
        Assert.IsTrue(ProjectileRules.HasReachedMaxDistance(3f, 3f));
        Assert.IsTrue(ProjectileRules.HasReachedMaxDistance(4f, 3f));
    }

    [Test]
    public void IsValidTarget_RejectsOwnerAndPlayers_AcceptsEnemy()
    {
        var owner = new GameObject("Owner");
        owner.AddComponent<PlayerCharacter>();
        owner.AddComponent<BoxCollider>();

        var playerHit = new GameObject("Ally");
        playerHit.AddComponent<PlayerCharacter>();
        playerHit.AddComponent<Health>();

        var loot = new GameObject("Loot");
        loot.AddComponent<BoxCollider>();

        var enemy = new GameObject("Enemy");
        var health = enemy.AddComponent<Health>();
        health.Configure(18f);
        enemy.AddComponent<EnemyController>();
        var enemyCol = enemy.AddComponent<BoxCollider>();

        Assert.IsFalse(ProjectileRules.IsValidTarget(owner.GetComponent<BoxCollider>(), owner));
        Assert.IsFalse(ProjectileRules.IsValidTarget(playerHit.GetComponent<BoxCollider>(), owner));
        Assert.IsFalse(ProjectileRules.IsValidTarget(loot.GetComponent<BoxCollider>(), owner));
        Assert.IsTrue(ProjectileRules.IsValidTarget(enemyCol, owner));

        Object.DestroyImmediate(owner);
        Object.DestroyImmediate(playerHit);
        Object.DestroyImmediate(loot);
        Object.DestroyImmediate(enemy);
    }
}

public class CombatReadabilityFixTests
{
    [Test]
    public void ArcWedge_IsNarrowNearPlayer_NotFullFarWidth()
    {
        const float range = 2f;
        const float arc = 120f;
        var farWidth = AttackArcGeometry.WidthAtDistance(range, arc);
        var nearWidth = AttackArcGeometry.WidthAtDistance(AttackArcGeometry.InnerRadius, arc);
        Assert.Greater(farWidth, 6f);
        Assert.Less(nearWidth, farWidth * 0.35f);
    }

    [Test]
    public void Shake_SurvivesOneFrameAfterApplyThenDecay()
    {
        var stored = CameraShakeMath.StoreAmplitude(0f, 0.12f);
        Assert.Greater(stored, 0.4f);
        var afterFrame = CameraShakeMath.DecayAfterApply(stored, 1f / 60f);
        Assert.Greater(afterFrame, 0.35f);
    }

    [Test]
    public void Shake_OldDecayWouldKillAxeInOneFrame()
    {
        var raw = 0.12f;
        var killed = Mathf.MoveTowards(raw, 0f, 10f / 60f);
        Assert.AreEqual(0f, killed, 0.001f);
        var fixedAfter = CameraShakeMath.DecayAfterApply(CameraShakeMath.StoreAmplitude(0f, raw), 1f / 60f);
        Assert.Greater(fixedAfter, 0f);
    }

    [Test]
    public void Knockback_AxeMovesMoreThanSwordAndDagger()
    {
        var dagger = KnockbackMath.EstimatedDisplacement(0.15f);
        var sword = KnockbackMath.EstimatedDisplacement(0.60f);
        var axe = KnockbackMath.EstimatedDisplacement(1.80f);
        Assert.Greater(axe, 1f);
        Assert.Greater(axe, sword);
        Assert.Greater(sword, dagger);
        Assert.Less(dagger, 0.2f);
    }
}
