using NUnit.Framework;

public class PlaceholderSfxTests
{
    [Test]
    public void GeneratedClips_HavePositiveLengthAndSamples()
    {
        var windup = PlaceholderSfx.GetWindup();
        var impact = PlaceholderSfx.GetImpact();
        var stun = PlaceholderSfx.GetStun();

        Assert.NotNull(windup);
        Assert.NotNull(impact);
        Assert.NotNull(stun);

        Assert.Greater(windup.length, 0f);
        Assert.Greater(impact.length, 0f);
        Assert.Greater(stun.length, 0f);

        Assert.Greater(windup.samples, 0);
        Assert.Greater(impact.samples, 0);
        Assert.Greater(stun.samples, 0);
    }
}
