using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class RunFailTests
{
    [Test]
    public void RunFailRules_OneJoinedZeroLiving_IsWipe()
    {
        Assert.IsTrue(RunFailRules.IsTeamWipe(1, 0));
    }

    [Test]
    public void RunFailRules_TwoJoinedOneLiving_NotWipe()
    {
        Assert.IsFalse(RunFailRules.IsTeamWipe(2, 1));
    }

    [Test]
    public void RunFailRules_ZeroJoined_NotWipe()
    {
        Assert.IsFalse(RunFailRules.IsTeamWipe(0, 0));
    }

    [Test]
    public void RunFailRules_BaseDead_IsBaseFail()
    {
        Assert.IsTrue(RunFailRules.IsBaseFail(false));
        Assert.IsFalse(RunFailRules.IsBaseFail(true));
    }

    [Test]
    public void RunFailRules_EnemyCombatTicksOnlyDuringWaveActive()
    {
        Assert.IsTrue(RunFailRules.ShouldEnemyCombatTick(GameFlowState.WaveActive));
        Assert.IsFalse(RunFailRules.ShouldEnemyCombatTick(GameFlowState.RunFailed));
        Assert.IsFalse(RunFailRules.ShouldEnemyCombatTick(GameFlowState.RunWon));
        Assert.IsFalse(RunFailRules.ShouldEnemyCombatTick(GameFlowState.Intermission));
    }

    [Test]
    public void EnterRunFailed_BaseDestroyed_IsNotRunComplete()
    {
        var flow = CreateFlowManager();
        flow.EnterRunFailed(RunFailReason.BaseDestroyed);

        Assert.AreEqual(GameFlowState.RunFailed, flow.State);
        Assert.AreEqual(RunFailReason.BaseDestroyed, flow.FailReason);
        Assert.AreNotEqual(GameFlowState.RunComplete, flow.State);

        Object.DestroyImmediate(flow.gameObject);
    }

    [Test]
    public void EnterRunFailed_DoesNotOverwriteRunComplete()
    {
        var flow = CreateFlowManager();
        SetPrivateState(flow, GameFlowState.RunComplete);

        flow.EnterRunFailed(RunFailReason.TeamWiped);

        Assert.AreEqual(GameFlowState.RunComplete, flow.State);
        Object.DestroyImmediate(flow.gameObject);
    }

    [Test]
    public void EnterRunFailed_DoesNotOverwriteRunWon()
    {
        var flow = CreateFlowManager();
        flow.EnterRunWon();

        flow.EnterRunFailed(RunFailReason.TeamWiped);

        Assert.AreEqual(GameFlowState.RunWon, flow.State);
        Object.DestroyImmediate(flow.gameObject);
    }

    [Test]
    public void RunComplete_StateStillExists()
    {
        Assert.IsTrue(System.Enum.IsDefined(typeof(GameFlowState), GameFlowState.RunComplete));
    }

    [Test]
    public void SoloPlayerDeath_TriggersTeamWipedFail()
    {
        var flow = CreateFlowManager();
        var (player, health) = CreatePlayer(0);
        flow.RegisterJoinedPlayer(player);

        health.ForceDeath();

        Assert.AreEqual(GameFlowState.RunFailed, flow.State);
        Assert.AreEqual(RunFailReason.TeamWiped, flow.FailReason);

        Cleanup(flow.gameObject, player.gameObject);
    }

    [Test]
    public void TwoPlayers_OneDead_NotFailed_SecondDead_Fails()
    {
        var flow = CreateFlowManager();
        var (player1, health1) = CreatePlayer(0);
        var (player2, health2) = CreatePlayer(1);
        flow.RegisterJoinedPlayer(player1);
        flow.RegisterJoinedPlayer(player2);

        health1.ForceDeath();
        Assert.AreNotEqual(GameFlowState.RunFailed, flow.State);

        health2.ForceDeath();
        Assert.AreEqual(GameFlowState.RunFailed, flow.State);
        Assert.AreEqual(RunFailReason.TeamWiped, flow.FailReason);

        Cleanup(flow.gameObject, player1.gameObject, player2.gameObject);
    }

    [Test]
    public void BaseHealthZero_TriggersBaseDestroyedFail()
    {
        var flow = CreateFlowManager();
        var baseGo = new GameObject("Base");
        baseGo.AddComponent<BaseCore>();
        baseGo.AddComponent<BaseHealth>();
        var baseHealth = baseGo.GetComponent<Health>();
        baseHealth.Configure(50f);
        flow.RefreshFailListeners();

        baseHealth.ForceDeath();

        Assert.AreEqual(GameFlowState.RunFailed, flow.State);
        Assert.AreEqual(RunFailReason.BaseDestroyed, flow.FailReason);
        Assert.AreNotEqual(GameFlowState.RunComplete, flow.State);

        Cleanup(flow.gameObject, baseGo);
    }

    [Test]
    public void PlayerRespawn_CancelledOnTeamWipeFail()
    {
        var flow = CreateFlowManager();
        var (player1, health1) = CreatePlayer(0);
        var (player2, health2) = CreatePlayer(1);
        var respawn1 = player1.GetComponent<PlayerRespawn>();
        flow.RegisterJoinedPlayer(player1);
        flow.RegisterJoinedPlayer(player2);

        health1.ForceDeath();
        Assert.IsTrue(respawn1.IsRespawning);
        Assert.AreNotEqual(GameFlowState.RunFailed, flow.State);

        health2.ForceDeath();
        Assert.AreEqual(GameFlowState.RunFailed, flow.State);
        Assert.IsFalse(respawn1.IsRespawning);

        Cleanup(flow.gameObject, player1.gameObject, player2.gameObject);
    }

    [Test]
    public void CountJoinedAndLiving_UsesIsAliveNotIsDead()
    {
        var playerGo = new GameObject("P1");
        var player = playerGo.AddComponent<PlayerCharacter>();
        player.Initialize(0, PlayerInputMode.KeyboardMouse);
        var health = playerGo.AddComponent<Health>();
        health.Configure(100f);
        health.ForceDeath();

        var players = new List<PlayerCharacter> { player };
        RunFailRules.CountJoinedAndLiving(players, out var joined, out var living);

        Assert.AreEqual(1, joined);
        Assert.AreEqual(0, living);
        Assert.IsTrue(RunFailRules.IsTeamWipe(joined, living));

        Object.DestroyImmediate(playerGo);
    }

    private static GameFlowManager CreateFlowManager()
    {
        var go = new GameObject("GameFlowManager");
        return go.AddComponent<GameFlowManager>();
    }

    private static (PlayerCharacter player, Health health) CreatePlayer(int index)
    {
        var go = new GameObject($"Player_{index + 1}");
        var player = go.AddComponent<PlayerCharacter>();
        player.Initialize(index, PlayerInputMode.KeyboardMouse);
        var health = go.AddComponent<Health>();
        health.Configure(100f);
        go.AddComponent<PlayerRespawn>();
        return (player, health);
    }

    private static void SetPrivateState(GameFlowManager flow, GameFlowState state)
    {
        var field = typeof(GameFlowManager).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field, "GameFlowManager._state field missing.");
        field.SetValue(flow, state);
    }

    private static void Cleanup(params GameObject[] objects)
    {
        foreach (var go in objects)
        {
            if (go != null)
                Object.DestroyImmediate(go);
        }

        Time.timeScale = 1f;
    }
}
