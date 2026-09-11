using NUnit.Framework;

public class CombatHudCopyTests
{
    [Test]
    public void FormatWeaponPower_ShowsDamageAndInterval()
    {
        Assert.AreEqual("Miecz 10 dmg / 0.80s", CombatHudCopy.FormatWeaponPower("Miecz", 10f, 0.8f));
    }

    [Test]
    public void FormatWeaponPower_UsesEffectiveDamageAfterTalents()
    {
        Assert.AreEqual("Miecz 11 dmg / 0.80s", CombatHudCopy.FormatWeaponPower("Miecz", 11f, 0.8f));
    }

    [Test]
    public void FormatWeaponPower_FallsBackWhenNameMissing()
    {
        Assert.AreEqual("- 5 dmg / 0.35s", CombatHudCopy.FormatWeaponPower(" ", 5f, 0.35f));
    }

    [Test]
    public void SkillSlotKeyHint_KeyboardUsesQERF()
    {
        Assert.AreEqual("Q", CombatHudCopy.GetSkillSlotKeyHint(0, true));
        Assert.AreEqual("E", CombatHudCopy.GetSkillSlotKeyHint(1, true));
        Assert.AreEqual("R", CombatHudCopy.GetSkillSlotKeyHint(2, true));
        Assert.AreEqual("F", CombatHudCopy.GetSkillSlotKeyHint(3, true));
    }

    [Test]
    public void SkillSlotKeyHint_GamepadUsesEastNorthLbRb()
    {
        Assert.AreEqual("East", CombatHudCopy.GetSkillSlotKeyHint(0, false));
        Assert.AreEqual("North", CombatHudCopy.GetSkillSlotKeyHint(1, false));
        Assert.AreEqual("LB", CombatHudCopy.GetSkillSlotKeyHint(2, false));
        Assert.AreEqual("RB", CombatHudCopy.GetSkillSlotKeyHint(3, false));
    }

    [Test]
    public void SkillSlotKeyHint_PassiveUltimateUsesPasyw()
    {
        Assert.AreEqual("PASYW", CombatHudCopy.GetSkillSlotKeyHint(3, true, true));
        Assert.AreEqual("PASYW", CombatHudCopy.GetSkillSlotKeyHint(3, false, true));
    }

    [Test]
    public void SkillSlotKeyHint_NonPassiveUltimateKeepsActionHint()
    {
        Assert.AreEqual("F", CombatHudCopy.GetSkillSlotKeyHint(3, true, false));
        Assert.AreEqual("RB", CombatHudCopy.GetSkillSlotKeyHint(3, false, false));
    }

    [Test]
    public void FormatHartPips_ShowsStacksAndReady()
    {
        Assert.AreEqual("Hart 0/5  [-----]", CombatHudCopy.FormatHartPips(0, 5, false));
        Assert.AreEqual("Hart 3/5  [###--]", CombatHudCopy.FormatHartPips(3, 5, false));
        Assert.AreEqual("Hart GOTOWY  [#####]", CombatHudCopy.FormatHartPips(5, 5, true));
    }

    [Test]
    public void FormatSkillCooldown_ShowsReadyWhenElapsed()
    {
        Assert.AreEqual("Trzaśnięcie gotowe", CombatHudCopy.FormatSkillCooldown("Trzaśnięcie", 0f, 8f));
        Assert.AreEqual("Trzaśnięcie 3.5s", CombatHudCopy.FormatSkillCooldown("Trzaśnięcie", 3.5f, 8f));
    }
}
