using NUnit.Framework;
using UnityEngine;

public class PlayerBuildStateTests
{
    [Test]
    public void TryApplyTalent_UsesEligibilityNotBranchTree()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var go = new GameObject("TestPlayer");
        go.AddComponent<PlayerCharacter>();
        go.AddComponent<Health>();
        var build = go.AddComponent<PlayerBuildState>();

        build.Initialize(catalog.GetClass(PlayerClassId.Pudzian), catalog, 0, 2);

        var optionsLvl2 = build.GetAvailableTalents(catalog, 2);
        Assert.AreEqual(3, optionsLvl2.Count);

        TalentDefinition skok = null;
        foreach (var option in optionsLvl2)
        {
            if (option.TalentId == "pudzian_skok")
                skok = option;
        }

        Assert.NotNull(skok);
        Assert.IsTrue(build.TryApplyTalent(skok, 2));

        var optionsLvl3 = build.GetAvailableTalents(catalog, 3);
        Assert.AreEqual(3, optionsLvl3.Count);
        var hasFollowup = false;
        foreach (var option in optionsLvl3)
        {
            if (option.TalentId == "pudzian_spalona_ziemia")
                hasFollowup = true;
        }

        Assert.IsTrue(hasFollowup);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void TrySwapWeaponSlotWith_ExchangesWeapons()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var goA = CreatePlayer("A", 0);
        var goB = CreatePlayer("B", 1);
        var buildA = goA.GetComponent<PlayerBuildState>();
        var buildB = goB.GetComponent<PlayerBuildState>();

        buildA.Initialize(catalog.GetClass(PlayerClassId.Jamie), catalog, 0, 1);
        buildB.Initialize(catalog.GetClass(PlayerClassId.Cwel), catalog, 1, 1);

        var aWeapon = buildA.GetWeapon(0);
        var bWeapon = buildB.GetWeapon(0);

        Assert.IsTrue(buildA.TrySwapWeaponSlotWith(buildB, 0, 0));
        Assert.AreEqual(bWeapon, buildA.GetWeapon(0));
        Assert.AreEqual(aWeapon, buildB.GetWeapon(0));

        Object.DestroyImmediate(goA);
        Object.DestroyImmediate(goB);
    }

    [Test]
    public void TryPickupItem_FillsFirstFreeSlot()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var go = CreatePlayer("P", 0);
        var build = go.GetComponent<PlayerBuildState>();
        build.Initialize(catalog.GetClass(PlayerClassId.Jamie), catalog, 0, 1);

        var item = catalog.GetItemById("blood_charm");
        Assert.IsTrue(build.TryPickupItem(item));
        Assert.AreEqual(item, build.GetItem(0));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void TryPickupItem_RejectsWhenInventoryFull()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var go = CreatePlayer("P", 0);
        var build = go.GetComponent<PlayerBuildState>();
        build.Initialize(catalog.GetClass(PlayerClassId.Jamie), catalog, 0, 1);

        for (var i = 0; i < BuildStatCalculator.ItemSlotCount; i++)
            Assert.IsTrue(build.TryPickupItem(catalog.GetItemById("blood_charm")));

        Assert.IsFalse(build.TryPickupItem(catalog.GetItemById("spiked_boots")));

        Object.DestroyImmediate(go);
    }

    private static GameObject CreatePlayer(string name, int slot)
    {
        var go = new GameObject(name);
        var character = go.AddComponent<PlayerCharacter>();
        character.Initialize(slot, PlayerInputMode.KeyboardMouse);
        go.AddComponent<Health>();
        go.AddComponent<PlayerBuildState>();
        return go;
    }
}
