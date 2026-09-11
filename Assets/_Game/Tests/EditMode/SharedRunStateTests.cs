using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Testy SharedRunState — nagrody za kill muszą aktualizować kasę i EXP (M5).
/// </summary>
public class SharedRunStateTests
{
    [Test]
    public void AddKillRewards_UpdatesGoldAndExp()
    {
        var go = new GameObject("TestRunState");
        var runState = go.AddComponent<SharedRunState>();
        var config = ScriptableObjectFactory.CreateProgressionConfig(new[] { 0, 60, 140 });
        runState.Configure(config);
        runState.ResetRun();

        runState.AddKillRewards(10, 6);
        runState.AddKillRewards(10, 6);

        Assert.AreEqual(12, runState.Gold);
        Assert.AreEqual(20, runState.SharedExp);
        Assert.AreEqual(1, runState.TeamLevel);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void AddKillRewards_LevelsUpAtThreshold()
    {
        var go = new GameObject("TestRunState");
        var runState = go.AddComponent<SharedRunState>();
        var config = ScriptableObjectFactory.CreateProgressionConfig(new[] { 0, 60, 140 });
        runState.Configure(config);
        runState.ResetRun();

        var leveledUp = false;
        runState.LeveledUp += _ => leveledUp = true;

        for (var i = 0; i < 6; i++)
            runState.AddKillRewards(10, 6);

        Assert.IsTrue(leveledUp);
        Assert.AreEqual(2, runState.TeamLevel);
        Assert.AreEqual(60, runState.SharedExp);
        Assert.AreEqual(36, runState.Gold);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(config);
    }
}
