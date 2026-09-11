using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Testy progresji M5 — EXP i poziomy drużyny.
/// </summary>
public class ProgressionConfigTests
{
    [Test]
    public void GetTeamLevelForExp_Level1AtStart()
    {
        var config = ScriptableObjectFactory.CreateProgressionConfig(new[] { 0, 60, 140 });
        Assert.AreEqual(1, config.GetTeamLevelForExp(0));
        Assert.AreEqual(1, config.GetTeamLevelForExp(59));
    }

    [Test]
    public void GetTeamLevelForExp_Level2AtThreshold()
    {
        var config = ScriptableObjectFactory.CreateProgressionConfig(new[] { 0, 60, 140 });
        Assert.AreEqual(2, config.GetTeamLevelForExp(60));
        Assert.AreEqual(2, config.GetTeamLevelForExp(139));
    }

    [Test]
    public void GetExpToNextLevel_ReturnsRemaining()
    {
        var config = ScriptableObjectFactory.CreateProgressionConfig(new[] { 0, 60, 140 });
        Assert.AreEqual(20, config.GetExpToNextLevel(40, 1));
    }
}

/// <summary>
/// Pomocnik testów bez assetów na dysku.
/// </summary>
internal static class ScriptableObjectFactory
{
    public static ProgressionConfig CreateProgressionConfig(int[] thresholds)
    {
        var config = ScriptableObject.CreateInstance<ProgressionConfig>();
        var field = typeof(ProgressionConfig).GetField("cumulativeExpThresholds",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(config, thresholds);
        return config;
    }
}
