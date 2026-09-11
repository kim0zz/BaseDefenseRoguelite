using NUnit.Framework;

/// <summary>
/// Legacy drzewko A/B — nie używane w flow level-upu (M8.1).
/// </summary>
public class TalentPathRulesTests
{
    [Test]
    public void IsBranchAvailable_EmptyParentStillAllows()
    {
        var talent = ScriptableObjectTalent();
        Assert.IsTrue(TalentPathRules.IsBranchAvailable(talent, new string[0]));
    }

    private static TalentDefinition ScriptableObjectTalent()
    {
        var talent = UnityEngine.ScriptableObject.CreateInstance<TalentDefinition>();
        talent.ConfigureRuntime("x", "x", "", PlayerClassId.Pudzian, 2, System.Array.Empty<TalentTag>(),
            TalentRequirement.Create(), TalentEffect.Persistent(PersistentEffectKind.Hart, "core"));
        return talent;
    }
}
