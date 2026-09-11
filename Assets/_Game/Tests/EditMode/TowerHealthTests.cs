using NUnit.Framework;
using UnityEngine;

public class TowerHealthTests
{
    [TearDown]
    public void TearDown()
    {
        TowerRegistry.ClearForTests();
    }

    [Test]
    public void Tower_TakeDamage_MarksDestroyedAtZero()
    {
        var go = new GameObject("Tower");
        go.AddComponent<TowerMarker>().Configure(AttackLineId.Center, TowerRole.LineTower);
        var tower = go.AddComponent<TowerHealth>();

        var config = ScriptableObject.CreateInstance<ProgressionConfig>();
        tower.InitializeFromConfig(config, Color.blue);
        tower.TakeDamage(999f);

        Assert.IsTrue(tower.IsDestroyed);
        Assert.IsFalse(tower.IsOperational);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void SetDisabledByBoss_MakesTowerNonOperationalWhileHpPositive()
    {
        var go = new GameObject("Tower");
        go.AddComponent<TowerMarker>().Configure(AttackLineId.Center, TowerRole.LineTower);
        var tower = go.AddComponent<TowerHealth>();

        var config = ScriptableObject.CreateInstance<ProgressionConfig>();
        tower.InitializeFromConfig(config, Color.blue);
        Assert.IsTrue(tower.IsOperational);

        tower.SetDisabledByBoss(true);

        Assert.IsTrue(tower.CurrentHealth > 0f);
        Assert.IsFalse(tower.IsOperational);
        Assert.IsTrue(tower.IsDisabledByBoss);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(config);
    }
}
