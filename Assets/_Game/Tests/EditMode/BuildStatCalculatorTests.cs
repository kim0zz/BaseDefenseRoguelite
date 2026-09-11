using NUnit.Framework;
using UnityEngine;

public class BuildStatCalculatorTests
{
    [Test]
    public void Compute_AppliesClassBaseAndTalentModifiers()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var jamie = catalog.GetClass(PlayerClassId.Jamie);
        var sword = catalog.GetWeaponById("miecz");
        var talent = ScriptableObject.CreateInstance<TalentDefinition>();

        var statsNoTalent = BuildStatCalculator.Compute(jamie, sword, null, null, 2);
        Assert.AreEqual(10f, statsNoTalent.Damage, 0.01f);

        SetTalentField(talent, "statModifiers", StatModifiers.Create(damageMultiplier: 1.1f));
        var statsWithTalent = BuildStatCalculator.Compute(jamie, sword, null, new[] { talent }, 2);
        Assert.AreEqual(11f, statsWithTalent.Damage, 0.01f);
    }

    [Test]
    public void Compute_ScalesHealthWithTeamLevel()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var jamie = catalog.GetClass(PlayerClassId.Jamie);

        var lvl1 = BuildStatCalculator.Compute(jamie, null, null, null, 1);
        var lvl3 = BuildStatCalculator.Compute(jamie, null, null, null, 3);

        Assert.Greater(lvl3.MaxHealth, lvl1.MaxHealth);
    }

    private static void SetTalentField(TalentDefinition talent, string fieldName, object value)
    {
        var field = typeof(TalentDefinition).GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(talent, value);
    }
}
