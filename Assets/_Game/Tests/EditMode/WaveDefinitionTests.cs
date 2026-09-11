using NUnit.Framework;

/// <summary>
/// Testy logiki zakończenia fali (M4).
/// </summary>
public class WaveDefinitionTests
{
    [Test]
    public void ShouldComplete_WhenTimeRunsOut()
    {
        Assert.IsTrue(WaveDefinition.ShouldComplete(0f, 3, 6, true));
    }

    [Test]
    public void ShouldComplete_WhenAllEnemiesDeadAndAllSpawned()
    {
        Assert.IsTrue(WaveDefinition.ShouldComplete(45f, 0, 6, true));
    }

    [Test]
    public void ShouldNotComplete_WhenEnemiesStillAlive()
    {
        Assert.IsFalse(WaveDefinition.ShouldComplete(45f, 2, 6, true));
    }

    [Test]
    public void ShouldNotComplete_BeforeAllGroupsSpawnedEvenIfNoEnemiesAlive()
    {
        Assert.IsFalse(WaveDefinition.ShouldComplete(45f, 0, 0, false));
    }
}
