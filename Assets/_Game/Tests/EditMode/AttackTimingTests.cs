using NUnit.Framework;
using UnityEngine;

public class AttackTimingTests
{
    [Test]
    public void FamilyFractions_SumToOne()
    {
        foreach (WeaponFamily family in System.Enum.GetValues(typeof(WeaponFamily)))
        {
            var feel = WeaponFeelProfile.For(family);
            Assert.AreEqual(1f, feel.FractionSum, 0.001f, family.ToString());
        }
    }

    [Test]
    public void Catalog_MapsIdsToFamilies_WithoutChangingAxeInterval()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();

        Assert.AreEqual(WeaponFamily.Axe, catalog.GetWeaponById("topor").Family);
        Assert.AreEqual(WeaponFamily.Axe, WeaponFamilyCatalog.FromId("mlot"));
        Assert.AreEqual(WeaponFamily.Bow, catalog.GetWeaponById("luk").Family);
        Assert.AreEqual(WeaponFamily.Sword, catalog.GetWeaponById("pika").Family);
        Assert.AreEqual(WeaponFamily.Sword, catalog.GetWeaponById("miecz").Family);
        Assert.AreEqual(WeaponFamily.Dagger, catalog.GetWeaponById("sztylet").Family);
        Assert.AreEqual(WeaponFamily.Sword, catalog.GetWeaponById("knights_edge").Family);
        Assert.AreEqual(WeaponFamily.Dagger, catalog.GetWeaponById("viper_fang").Family);

        Assert.AreEqual(1.6f, catalog.GetWeaponById("topor").MeleeProfile.AttackInterval, 0.001f);
        var bulawa = catalog.GetWeaponById("bulawa");
        Assert.IsNotNull(bulawa);
        Assert.IsTrue(bulawa.MeleeProfile.HasCombo);
        Assert.AreEqual(3, bulawa.MeleeProfile.ComboProfile.HitCount);
        Assert.AreEqual(10f, catalog.GetWeaponById("miecz").MeleeProfile.Damage, 0.001f);
        Assert.AreEqual(0.8f, catalog.GetWeaponById("miecz").MeleeProfile.AttackInterval, 0.001f);
        Assert.AreEqual(2f, catalog.GetWeaponById("miecz").MeleeProfile.Range, 0.001f);
    }

    [Test]
    public void Cycle_SwordDoesNotEnterActiveOnStartFrame()
    {
        var cycle = new AttackCycle();
        var feel = WeaponFeelProfile.For(WeaponFamily.Sword);
        Assert.IsTrue(cycle.TryStart(feel, 0.8f));
        Assert.AreEqual(AttackPhase.Windup, cycle.Phase);

        var startTick = cycle.Tick(0f);
        Assert.IsFalse(startTick.EnteredActive);
        Assert.AreEqual(AttackPhase.Windup, cycle.Phase);
    }

    [Test]
    public void Cycle_SwordEntersActiveAfterWindup()
    {
        var cycle = new AttackCycle();
        var feel = WeaponFeelProfile.For(WeaponFamily.Sword);
        const float interval = 0.8f;
        cycle.TryStart(feel, interval);

        var windup = interval * feel.WindupFraction;
        var tick = cycle.Tick(windup);
        Assert.IsTrue(tick.EnteredActive);
        Assert.AreEqual(AttackPhase.Active, tick.Phase);
        Assert.AreEqual(windup, cycle.Elapsed, 0.0001f);
    }

    [Test]
    public void Cycle_SwordEntersRecoveryAfterActiveWindow()
    {
        var cycle = new AttackCycle();
        var feel = WeaponFeelProfile.For(WeaponFamily.Sword);
        const float interval = 0.8f;
        cycle.TryStart(feel, interval);

        cycle.Tick(interval * feel.WindupFraction);
        var tick = cycle.Tick(interval * feel.ActiveFraction);
        Assert.IsTrue(tick.EnteredRecovery);
        Assert.AreEqual(AttackPhase.Recovery, tick.Phase);
        Assert.IsFalse(AttackFacingPolicy.LocksFacing(tick.Phase));
    }

    [Test]
    public void FacingPolicy_LocksOnlyDuringActive()
    {
        Assert.IsFalse(AttackFacingPolicy.LocksFacing(AttackPhase.Idle));
        Assert.IsFalse(AttackFacingPolicy.LocksFacing(AttackPhase.Windup));
        Assert.IsTrue(AttackFacingPolicy.LocksFacing(AttackPhase.Active));
        Assert.IsFalse(AttackFacingPolicy.LocksFacing(AttackPhase.Recovery));
    }

    [Test]
    public void Cycle_DaggerReachesHitFasterThanAxe()
    {
        const float daggerInterval = 0.35f;
        const float axeInterval = 1.6f;
        var daggerFeel = WeaponFeelProfile.For(WeaponFamily.Dagger);
        var axeFeel = WeaponFeelProfile.For(WeaponFamily.Axe);

        var daggerWindup = daggerInterval * daggerFeel.WindupFraction;
        var axeWindup = axeInterval * axeFeel.WindupFraction;
        Assert.Less(daggerWindup, axeWindup);
    }

    [Test]
    public void Cycle_BowSpawnsProjectileAtEndOfWindup_NotMeleeActive()
    {
        var cycle = new AttackCycle();
        var feel = WeaponFeelProfile.For(WeaponFamily.Bow);
        const float interval = 0.9f;
        cycle.TryStart(feel, interval);

        var tick = cycle.Tick(interval * feel.WindupFraction);
        Assert.IsTrue(tick.SpawnProjectile);
        Assert.IsTrue(tick.EnteredRecovery);
        Assert.IsFalse(tick.EnteredActive);
        Assert.AreEqual(AttackPhase.Recovery, tick.Phase);
        Assert.IsFalse(AttackFacingPolicy.LocksFacing(tick.Phase));
    }

    [Test]
    public void Cycle_ReturnsIdleAfterFullInterval()
    {
        var cycle = new AttackCycle();
        cycle.TryStart(WeaponFeelProfile.For(WeaponFamily.Sword), 0.8f);
        var tick = cycle.Tick(0.8f);
        Assert.IsTrue(tick.ReturnedToIdle);
        Assert.AreEqual(AttackPhase.Idle, cycle.Phase);
        Assert.IsFalse(cycle.ShouldDeferWeaponSwap);
    }

    [Test]
    public void Cycle_DefersSwapWhileAttacking()
    {
        var cycle = new AttackCycle();
        Assert.IsFalse(cycle.ShouldDeferWeaponSwap);
        cycle.TryStart(WeaponFeelProfile.For(WeaponFamily.Axe), 1.6f);
        Assert.IsTrue(cycle.ShouldDeferWeaponSwap);
        cycle.Cancel();
        Assert.IsFalse(cycle.ShouldDeferWeaponSwap);
    }

    [Test]
    public void Cycle_MoveMultiplierOnlyDuringWindupAndActive()
    {
        var cycle = new AttackCycle();
        var feel = WeaponFeelProfile.For(WeaponFamily.Axe);
        cycle.TryStart(feel, 1.6f);
        Assert.AreEqual(0.25f, cycle.MoveMultiplier, 0.001f);

        cycle.Tick(1.6f * feel.WindupFraction + 1.6f * feel.ActiveFraction);
        Assert.AreEqual(AttackPhase.Recovery, cycle.Phase);
        Assert.AreEqual(1f, cycle.MoveMultiplier, 0.001f);
    }

    [Test]
    public void Cycle_CanRestartImmediatelyAfterReturningToIdle()
    {
        var cycle = new AttackCycle();
        cycle.TryStart(WeaponFeelProfile.For(WeaponFamily.Dagger), 0.35f);
        var tick = cycle.Tick(0.35f);
        Assert.IsTrue(tick.ReturnedToIdle);
        Assert.IsTrue(cycle.TryStart(WeaponFeelProfile.For(WeaponFamily.Dagger), 0.35f));
        Assert.AreEqual(AttackPhase.Windup, cycle.Phase);
    }

    [Test]
    public void MeleeArc_RejectsTargetBehindAndOutOfRange()
    {
        var origin = Vector3.zero;
        var forward = Vector3.forward;
        Assert.IsTrue(MeleeHitResolver.IsInMeleeArc(origin, forward, new Vector3(0f, 0f, 1f), 2f, 120f));
        Assert.IsFalse(MeleeHitResolver.IsInMeleeArc(origin, forward, new Vector3(0f, 0f, -1f), 2f, 120f));
        Assert.IsFalse(MeleeHitResolver.IsInMeleeArc(origin, forward, new Vector3(0f, 0f, 3f), 2f, 120f));
    }

    [Test]
    public void MeleeHitPoint_CountsOverlapAndSurfaceInsideRange()
    {
        var origin = Vector3.zero;
        var forward = Vector3.forward;

        Assert.IsTrue(MeleeHitResolver.IsHitPointInMeleeArc(origin, forward, origin, 1.6f, 120f),
            "Nakładka (ClosestPoint = origin) ma trafiać.");
        Assert.IsTrue(MeleeHitResolver.IsHitPointInMeleeArc(origin, forward, new Vector3(0f, 0f, 1.05f), 1.6f, 120f),
            "Powierzchnia dużego bossa w zasięgu sztyletu.");
        Assert.IsFalse(MeleeHitResolver.IsInMeleeArc(origin, forward, new Vector3(0f, 0f, 1.85f), 1.6f, 120f),
            "Stary check do środka przy hugie odrzucałby sztylet.");
        Assert.IsFalse(MeleeHitResolver.IsHitPointInMeleeArc(origin, forward, new Vector3(0f, 0f, -0.5f), 1.6f, 120f));
    }
}
