using NUnit.Framework;
using UnityEngine;

public class HudLayoutTests
{
    [Test]
    public void DefaultScale_ShrinksReferenceResolution()
    {
        var reference = HudLayout.ScaledReferenceResolution(HudLayout.DefaultUiScale);
        Assert.Less(reference.x, HudLayout.ReferenceWidth);
        Assert.Less(reference.y, HudLayout.ReferenceHeight);
        Assert.AreEqual(HudLayout.ReferenceWidth / HudLayout.DefaultUiScale, reference.x, 0.01f);
    }

    [Test]
    public void ZeroScale_ClampsToMinimum()
    {
        var clamped = HudLayout.ClampUiScale(0f);
        Assert.AreEqual(HudLayout.MinUiScale, clamped);
        var reference = HudLayout.ScaledReferenceResolution(0f);
        Assert.AreEqual(HudLayout.ReferenceWidth / HudLayout.MinUiScale, reference.x, 0.01f);
    }

    [Test]
    public void HugeScale_ClampsToMaximum()
    {
        Assert.AreEqual(HudLayout.MaxUiScale, HudLayout.ClampUiScale(99f));
    }

    [Test]
    public void TopBarScale_ClampsAndDefaultsBelowOne()
    {
        Assert.Less(HudLayout.DefaultTopBarScale, 1f);
        Assert.AreEqual(HudLayout.MinTopBarScale, HudLayout.ClampTopBarScale(0f));
        Assert.AreEqual(HudLayout.MaxTopBarScale, HudLayout.ClampTopBarScale(9f));
        Assert.AreEqual(HudLayout.DefaultTopBarScale, HudLayout.ClampTopBarScale(HudLayout.DefaultTopBarScale));
    }
}
