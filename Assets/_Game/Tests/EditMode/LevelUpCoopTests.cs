using NUnit.Framework;
using UnityEngine;

public class LevelUpCoopTests
{
    [Test]
    public void PlayerTwo_CannotConfirmWithKeyboard()
    {
        var go = new GameObject("P2");
        var player = go.AddComponent<PlayerCharacter>();
        player.Initialize(1, PlayerInputMode.KeyboardMouse);
        Assert.IsFalse(player.TryReadLevelUpConfirmInput());
        Object.DestroyImmediate(go);
    }

    [Test]
    public void AllReady_RequiresEveryPendingPlayer()
    {
        Assert.IsFalse(LevelUpSelectionRules.AllPlayersReady(new[] { true, false }));
        Assert.IsTrue(LevelUpSelectionRules.AllPlayersReady(new[] { true, true }));
    }

    [Test]
    public void ClampOptionIndex_EmptyOptions_ReturnsZeroNotMinusOne()
    {
        Assert.AreEqual(0, LevelUpSelectionRules.ClampOptionIndex(0, 0));
        Assert.AreEqual(1, LevelUpSelectionRules.ClampOptionIndex(3, 2));
        Assert.AreEqual(1, LevelUpSelectionRules.ClampOptionIndex(1, 2));
    }

    [Test]
    public void StructSlot_OptionChangeMustBeWrittenBack()
    {
        var slots = new[] { new LevelUpChoice { SelectedIndex = 0 } };
        var slot = slots[0];
        slot.SelectedIndex = 1;
        Assert.AreEqual(0, slots[0].SelectedIndex, "Kopia structa nie zapisuje wyboru — trzeba write-back.");
        slots[0] = slot;
        Assert.AreEqual(1, slots[0].SelectedIndex);
    }

    [Test]
    public void AllReady_ResumesWorldWithoutExtraContinue()
    {
        Assert.IsFalse(LevelUpSelectionRules.ShouldResumeWorldWhenReady(false));
        Assert.IsTrue(LevelUpSelectionRules.ShouldResumeWorldWhenReady(true));
    }

    private struct LevelUpChoice
    {
        public int SelectedIndex;
    }
}
