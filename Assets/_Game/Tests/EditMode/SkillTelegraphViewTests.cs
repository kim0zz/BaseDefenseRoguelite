using NUnit.Framework;
using UnityEngine;

public class SkillTelegraphViewTests
{
    [Test]
    public void RingThenChargeLine_BothRenderersNonNull()
    {
        var root = new GameObject("TelegraphRoot");
        var view = root.AddComponent<SkillTelegraphView>();

        view.BeginWindup(2f, 0.5f, Color.red);
        Assert.IsTrue(view.BeginChargeLine(4f, 1f, Color.red));

        var ring = root.transform.Find("TelegraphRing")?.GetComponent<LineRenderer>();
        var line = root.transform.Find("TelegraphChargeLine")?.GetComponent<LineRenderer>();
        Assert.NotNull(ring, "TelegraphRing LineRenderer missing.");
        Assert.NotNull(line, "TelegraphChargeLine LineRenderer missing.");

        Object.DestroyImmediate(root);
    }

    [Test]
    public void SetWorldCenter_KeepsRingAtWorldPointWhenParentRotates()
    {
        var root = new GameObject("TelegraphRoot");
        root.transform.position = Vector3.zero;
        var view = root.AddComponent<SkillTelegraphView>();
        var worldTarget = new Vector3(5f, 0f, 0f);

        view.BeginWindupAtWorldPosition(worldTarget, 1f, 1f, Color.cyan);
        root.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        view.SetWorldCenter(worldTarget);

        var ring = root.transform.Find("TelegraphRing")?.GetComponent<LineRenderer>();
        Assert.NotNull(ring);

        var sum = Vector3.zero;
        for (var i = 0; i < ring.positionCount; i++)
            sum += root.transform.TransformPoint(ring.GetPosition(i));
        var avg = sum / ring.positionCount;

        Assert.AreEqual(worldTarget.x, avg.x, 0.25f);
        Assert.AreEqual(worldTarget.z, avg.z, 0.25f);

        Object.DestroyImmediate(root);
    }
}
