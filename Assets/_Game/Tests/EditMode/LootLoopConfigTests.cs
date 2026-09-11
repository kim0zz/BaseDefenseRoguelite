using NUnit.Framework;
using UnityEngine;

public class LootLoopConfigTests
{
    [Test]
    public void LootDisabled_BlocksBossUniqueDropCall()
    {
        LootLoopConfig.LootEnabledInRun = false;
        LootDropService.DropBossUnique(Vector3.zero);
        Assert.IsNull(Object.FindAnyObjectByType<LootPickup>());
    }
}
