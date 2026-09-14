using NUnit.Framework;
using UnityEngine;

public sealed class SiegeArenaTests
{
    [Test]
    public void RetreatMovesPlayersAndKeepsThemInsideInnerBounds()
    {
        var arenaGo = new GameObject("SiegeArenaTest");
        var playerGo = new GameObject("PlayerTest");
        try
        {
            var arena = arenaGo.AddComponent<SiegeArena>();
            if (arena.Gate == null) arena.SendMessage("Awake", SendMessageOptions.RequireReceiver);
            playerGo.AddComponent<PlayerCharacter>();
            playerGo.transform.position = new Vector3(20f, 1f, 20f);
            arena.CompleteRetreat();
            Assert.IsTrue(arena.IsInner);
            Assert.IsTrue(arena.Contains(playerGo.transform.position));
            Assert.AreEqual(arena.GetPlayerSpawn(0), playerGo.transform.position);
        }
        finally
        {
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(arenaGo);
        }
    }

    [Test]
    public void DefenseTargetSwitchesFromGateToCoreAndEnemyClampUsesInnerBounds()
    {
        var go = new GameObject("SiegeArenaTargetTest");
        try
        {
            var arena = go.AddComponent<SiegeArena>();
            if (arena.Gate == null) arena.SendMessage("Awake", SendMessageOptions.RequireReceiver);
            Assert.AreSame(arena.Gate, arena.DefenseTarget);
            arena.Gate.ForceDeath();
            Assert.AreSame(arena.Core, arena.DefenseTarget);
            arena.CompleteRetreat();
            var clamped = arena.ClampEnemy(new Vector3(100f, 1f, 100f));
            Assert.LessOrEqual(Mathf.Abs(clamped.x), arena.Config.HalfWidth);
            Assert.LessOrEqual(clamped.z, arena.Depth.y + 4f);
        }
        finally { Object.DestroyImmediate(go); }
    }
}
