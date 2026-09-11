using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class RunWonTests
{
    [Test]
    public void EnterRunWon_SetsStateAndPausesWorld()
    {
        var flow = CreateFlowManager();
        flow.EnterRunWon();

        Assert.AreEqual(GameFlowState.RunWon, flow.State);
        Assert.AreEqual(0f, Time.timeScale);

        Cleanup(flow.gameObject);
    }

    [Test]
    public void EnterRunFailed_DoesNotOverwriteRunWon()
    {
        var flow = CreateFlowManager();
        flow.EnterRunWon();

        flow.EnterRunFailed(RunFailReason.BaseDestroyed);

        Assert.AreEqual(GameFlowState.RunWon, flow.State);
        Cleanup(flow.gameObject);
    }

    [Test]
    public void EnterRunWon_DoesNotOverwriteRunFailed()
    {
        var flow = CreateFlowManager();
        flow.EnterRunFailed(RunFailReason.TeamWiped);

        flow.EnterRunWon();

        Assert.AreEqual(GameFlowState.RunFailed, flow.State);
        Cleanup(flow.gameObject);
    }

    [Test]
    public void RunWon_IsDistinctFromRunCompleteAndRunFailed()
    {
        Assert.AreNotEqual(GameFlowState.RunComplete, GameFlowState.RunWon);
        Assert.AreNotEqual(GameFlowState.RunFailed, GameFlowState.RunWon);
    }

    [Test]
    public void RunFailRules_RunWon_DoesNotTickEnemyCombat()
    {
        Assert.IsFalse(RunFailRules.ShouldEnemyCombatTick(GameFlowState.RunWon));
    }

    [Test]
    public void EnterRunFailed_FromWaveActive_StillWorksWhenRunWonNotSet()
    {
        var flow = CreateFlowManager();
        SetPrivateState(flow, GameFlowState.WaveActive);

        flow.EnterRunFailed(RunFailReason.BaseDestroyed);

        Assert.AreEqual(GameFlowState.RunFailed, flow.State);
        Cleanup(flow.gameObject);
    }

    private static GameFlowManager CreateFlowManager()
    {
        var go = new GameObject("GameFlowManager");
        return go.AddComponent<GameFlowManager>();
    }

    private static void SetPrivateState(GameFlowManager flow, GameFlowState state)
    {
        var field = typeof(GameFlowManager).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        field.SetValue(flow, state);
    }

    private static void Cleanup(GameObject go)
    {
        if (go != null)
            Object.DestroyImmediate(go);
        Time.timeScale = 1f;
    }
}
