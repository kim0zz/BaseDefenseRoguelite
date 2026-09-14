using NUnit.Framework;
using UnityEngine;

public sealed class SiegeWaveTests
{
    private static EnemyDefinition Enemy()
    {
        var result = ScriptableObject.CreateInstance<EnemyDefinition>();
        result.ConfigureForTest(EnemyKind.Grunt);
        return result;
    }

    private static SiegeWaveDefinition Wave(int cap, params SiegeWaveGroup[] groups)
    {
        var result = ScriptableObject.CreateInstance<SiegeWaveDefinition>();
        result.ActiveCap = cap;
        result.Groups = groups;
        return result;
    }

    [Test]
    public void BudgetAndPlayerScalingAreFiniteAndDeterministic()
    {
        var enemy = Enemy();
        var wave = Wave(3, new SiegeWaveGroup { Enemy = enemy, Count = 2 },
            new SiegeWaveGroup { Enemy = enemy, Count = 3 });
        try
        {
            var schedule = new SiegeWaveSchedule(wave, 3);
            Assert.AreEqual(8, schedule.Budget);
            Assert.AreEqual(8, wave.GetBudget(3));
            Assert.AreEqual(0, schedule.Spawned);
        }
        finally { Object.DestroyImmediate(wave); Object.DestroyImmediate(enemy); }
    }

    [Test]
    public void ActiveCapThrottlesSpawningWithoutDroppingBudget()
    {
        var enemy = Enemy();
        var wave = Wave(2, new SiegeWaveGroup { Enemy = enemy, Count = 5, SpawnInterval = .01f });
        try
        {
            var schedule = new SiegeWaveSchedule(wave, 1);
            var calls = 0;
            schedule.Tick(1f, 0, false, null, (group, index) => { calls++; return true; });
            Assert.AreEqual(2, calls);
            Assert.AreEqual(2, schedule.Spawned);
            Assert.IsFalse(schedule.AllSpawned);

            schedule.Tick(1f, 2, false, null, (group, index) => { calls++; return true; });
            Assert.AreEqual(2, calls, "A full cap must pause emission, not consume the pending budget.");
            schedule.Tick(.1f, 0, false, null, (group, index) => { calls++; return true; });
            Assert.AreEqual(4, schedule.Spawned);
            Assert.AreEqual(1, schedule.Budget - schedule.Spawned);
        }
        finally { Object.DestroyImmediate(wave); Object.DestroyImmediate(enemy); }
    }

    [Test]
    public void PauseFreezesElapsedAnnouncementsAndSpawns()
    {
        var enemy = Enemy();
        var wave = Wave(4, new SiegeWaveGroup { Enemy = enemy, Count = 2, StartSeconds = 1f, TelegraphSeconds = 1f });
        try
        {
            var schedule = new SiegeWaveSchedule(wave, 1);
            var announcements = 0;
            var calls = 0;
            schedule.Tick(2f, 0, true, group => announcements++, (group, index) => { calls++; return true; });
            Assert.AreEqual(0f, schedule.Elapsed);
            Assert.AreEqual(0, announcements);
            Assert.AreEqual(0, calls);
            schedule.Tick(1f, 0, false, group => announcements++, (group, index) => { calls++; return true; });
            Assert.AreEqual(1, announcements);
            Assert.AreEqual(1, calls);
        }
        finally { Object.DestroyImmediate(wave); Object.DestroyImmediate(enemy); }
    }

    [Test]
    public void CompletionRequiresAllSpawnedAndNoAliveActors()
    {
        var enemy = Enemy();
        var wave = Wave(4, new SiegeWaveGroup { Enemy = enemy, Count = 1, StartSeconds = 0f });
        try
        {
            var schedule = new SiegeWaveSchedule(wave, 1);
            schedule.Tick(.1f, 0, false, null, (group, index) => true);
            Assert.IsTrue(schedule.AllSpawned);
            Assert.IsFalse(schedule.IsComplete(1));
            Assert.IsTrue(schedule.IsComplete(0));
        }
        finally { Object.DestroyImmediate(wave); Object.DestroyImmediate(enemy); }
    }

    [Test]
    public void FactoryCreatesTheFiveWaveDraftAndCleansRuntimeObjects()
    {
        var sequence = SiegeWaveFactory.CreateDefault();
        Assert.IsNotNull(sequence);
        try
        {
            Assert.AreEqual(5, sequence.Waves.Length);
            foreach (var wave in sequence.Waves)
            {
                Assert.IsNotNull(wave);
                Assert.IsNotEmpty(wave.Groups);
                foreach (var group in wave.Groups)
                {
                    Assert.IsNotNull(group);
                    Assert.IsNotNull(group.Enemy);
                    Assert.Greater(group.Count, 0);
                }
            }
        }
        finally
        {
            SiegeWaveFactory.DestroyRuntimeSequence(sequence);
            Assert.IsTrue(sequence == null);
        }
    }
}
