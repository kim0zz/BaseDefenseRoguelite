using NUnit.Framework;
using UnityEngine;

public class BossUniqueDropTests
{
    [Test]
    public void RollBossUnique_AlwaysReturnsUniqueWeapon()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        for (var i = 0; i < 20; i++)
        {
            var weapon = catalog.RollBossUniqueWeapon();
            Assert.NotNull(weapon);
            Assert.IsTrue(weapon.IsBossUnique || weapon.Rarity == LootRarity.Unique);
        }
    }
}
