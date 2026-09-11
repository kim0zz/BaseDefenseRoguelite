using NUnit.Framework;
using UnityEngine;

public class SkillFeedbackResolverTests
{
    [Test]
    public void Resolve_NullAuthored_ReturnsFallback()
    {
        var fallback = new GameObject("Fallback");
        Assert.AreSame(fallback, SkillFeedbackResolver.Resolve<GameObject>(null, fallback));
        Object.DestroyImmediate(fallback);
    }

    [Test]
    public void Resolve_NonNullAuthored_ReturnsAuthored()
    {
        var authored = new GameObject("Authored");
        var fallback = new GameObject("Fallback");
        Assert.AreSame(authored, SkillFeedbackResolver.Resolve(authored, fallback));
        Object.DestroyImmediate(authored);
        Object.DestroyImmediate(fallback);
    }

    [Test]
    public void HasAuthored_Null_ReturnsFalse()
    {
        Assert.IsFalse(SkillFeedbackResolver.HasAuthored(null));
    }

    [Test]
    public void HasAuthored_NonNull_ReturnsTrue()
    {
        var go = new GameObject("Authored");
        Assert.IsTrue(SkillFeedbackResolver.HasAuthored(go));
        Object.DestroyImmediate(go);
    }

    [Test]
    public void HasAuthored_DestroyedUnityObject_ReturnsFalse()
    {
        var go = new GameObject("Temp");
        Object.DestroyImmediate(go);
        Assert.IsFalse(SkillFeedbackResolver.HasAuthored(go));
    }
}
