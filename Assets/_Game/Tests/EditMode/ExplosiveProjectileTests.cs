using NUnit.Framework;
using UnityEngine;

public class ExplosiveProjectileTests
{
    [Test]
    public void BowProjectile_UsesDirectImpactMode()
    {
        var feel = WeaponFeelProfile.For(WeaponFamily.Bow);
        Assert.AreEqual(ProjectileImpactMode.Direct, ProjectileProfile.Direct.ImpactMode);
        Assert.IsTrue(feel.IsRanged);
        Assert.IsFalse(WeaponFeelProfile.For(WeaponFamily.Sword).IsRanged);
    }

    [Test]
    public void ThrownExplosive_IsRangedWithExplodeMode()
    {
        var feel = WeaponFeelProfile.For(WeaponFamily.ThrownExplosive);
        Assert.IsTrue(feel.IsRanged);
        Assert.AreEqual(DeployableTuning.ThrownExplosiveSpeed, feel.ProjectileSpeed, 0.01f);

        var profile = new ProjectileProfile(
            ProjectileImpactMode.ExplodeOnFirstHitOrObstacle,
            DeployableTuning.BasicSplashRadius,
            DeployableTuning.ThrownExplosiveArcPeak);
        Assert.AreEqual(ProjectileImpactMode.ExplodeOnFirstHitOrObstacle, profile.ImpactMode);
    }

    [Test]
    public void Cluster_SpawnsExactlyThreeChildren_ThatDoNotCluster()
    {
        DeployableRegistry.ResetForTests();
        var owner = new GameObject("Owner");
        owner.AddComponent<Health>().Configure(100f);
        var persistents = owner.AddComponent<PlayerPersistentEffects>();
        persistents.ReplaceAll(new System.Collections.Generic.List<PersistentEffectBinding>
        {
            new PersistentEffectBinding(PersistentEffectKind.ClusterOnExplode)
        });
        DeployableRegistry.Ensure();

        var before = CountChildren(owner);
        ExplosionResolver.Explode(Vector3.zero, 2.4f, 16f, owner, 8, 0f, true, generation: 0);
        var after = CountChildren(owner);
        Assert.AreEqual(3, after - before);

        foreach (var spawnedChild in FindChildren(owner))
            Assert.AreEqual(DeployableTuning.ClusterGeneration, spawnedChild.Generation);

        var firstChild = FindChildren(owner)[0];
        var childCountBefore = CountChildren(owner);
        firstChild.Detonate();
        Assert.AreEqual(childCountBefore - 1, CountChildren(owner));

        DeployableRegistry.ResetForTests();
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void NalotExecutor_DoesNotIncludePlayerCharacter()
    {
        var owner = new GameObject("Owner");
        var health = owner.AddComponent<Health>();
        health.Configure(100f);
        owner.AddComponent<PlayerCharacter>();

        var skill = ScriptableObject.CreateInstance<SkillDefinition>();
        SkillAimStripBurstExecutor.Execute(
            skill,
            owner,
            Vector3.zero,
            Vector3.forward,
            ~0);

        Assert.AreEqual(100f, health.CurrentHealth, 0.001f);
        Object.DestroyImmediate(owner);
        Object.DestroyImmediate(skill);
    }

    private static int CountChildren(GameObject owner)
    {
        return FindChildren(owner).Count;
    }

    private static System.Collections.Generic.List<Deployable> FindChildren(GameObject owner)
    {
        var list = new System.Collections.Generic.List<Deployable>();
        var all = Object.FindObjectsByType<Deployable>(FindObjectsInactive.Exclude);
        foreach (var d in all)
        {
            if (d.Owner == owner && d.Category == DeployableCategory.Child)
                list.Add(d);
        }

        return list;
    }
}
