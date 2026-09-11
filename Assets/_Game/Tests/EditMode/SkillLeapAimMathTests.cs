using NUnit.Framework;
using UnityEngine;

public class SkillLeapAimMathTests
{
    [TearDown]
    public void TearDown()
    {
        if (MapPlayArea.Instance != null)
            Object.DestroyImmediate(MapPlayArea.Instance.gameObject);
    }

    [Test]
    public void ClampDistance_RespectsZeroToSixMeters()
    {
        Assert.AreEqual(0f, SkillLeapAimMath.ClampDistance(-2f), 0.001f);
        Assert.AreEqual(0f, SkillLeapAimMath.ClampDistance(0f), 0.001f);
        Assert.AreEqual(3f, SkillLeapAimMath.ClampDistance(3f), 0.001f);
        Assert.AreEqual(6f, SkillLeapAimMath.ClampDistance(6f), 0.001f);
        Assert.AreEqual(6f, SkillLeapAimMath.ClampDistance(12f), 0.001f);
    }

    [Test]
    public void ResolvePointFromDirection_ClampsBeyondMaxDistance()
    {
        var origin = Vector3.zero;
        var point = SkillLeapAimMath.ResolvePointFromDirection(origin, Vector3.forward, 12f);

        Assert.AreEqual(6f, Vector3.Distance(origin, point), 0.001f);
    }

    [Test]
    public void ResolvePointFromDirection_FeetLandingStaysAtOrigin()
    {
        var origin = new Vector3(2f, 0f, 3f);
        var point = SkillLeapAimMath.ResolvePointFromDirection(origin, Vector3.right, 0f);

        Assert.AreEqual(origin.x, point.x, 0.001f);
        Assert.AreEqual(origin.z, point.z, 0.001f);
    }

    [Test]
    public void ResolveLastLegalPoint_StopsAtPlayAreaEdge()
    {
        var playAreaGo = new GameObject("MapPlayArea");
        var playArea = playAreaGo.AddComponent<MapPlayArea>();
        playArea.SetBounds(new Vector2(-1f, 1f), new Vector2(-1f, 1f));

        var origin = Vector3.zero;
        var desired = new Vector3(10f, 0f, 0f);
        var legal = SkillLeapAimMath.ResolveLastLegalPoint(origin, desired, playArea);

        Assert.LessOrEqual(legal.x, 1f + 0.001f);
        Assert.Greater(legal.x, 0.5f);
        Assert.AreEqual(0f, legal.z, 0.001f);

        Object.DestroyImmediate(playAreaGo);
    }

    [Test]
    public void ResolveGamepadPoint_UsesStickMagnitudeScaledToMax()
    {
        var origin = Vector3.zero;
        var point = SkillLeapAimMath.ResolveGamepadPoint(origin, Vector3.forward, 0.5f);

        Assert.AreEqual(3f, Vector3.Distance(origin, point), 0.001f);
    }

    [Test]
    public void Trzasniecie_WithoutModifier_RemainsSelfCircleSkill()
    {
        var skill = SkillContentFactory.CreateTrzasniecie();

        Assert.AreEqual(SkillAimMode.Self, skill.AimMode);
        Assert.AreEqual(SkillLocomotionMode.Root, skill.Locomotion);
        Assert.AreEqual(SkillHitPolicy.OnEnteredActive, skill.HitPolicy);
        Assert.AreEqual(SkillShapeType.Circle, skill.ShapeType);
        Assert.AreEqual(16f, skill.Damage, 0.01f);
    }
}
