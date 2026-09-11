using System.Collections.Generic;

/// <summary>
/// Trzy sloty umiejętności z niezależnymi CD i buforem per slot (M7.6-T3).
/// </summary>
public sealed class SkillLoadout
{
    public const int ActiveSlotCount = 3;
    public const int UltimateSlot = 3;
    public const int SlotCount = 4;

    private readonly SkillDefinition[] _skills = new SkillDefinition[SlotCount];
    private readonly SkillCooldownTracker[] _cooldowns = new SkillCooldownTracker[SlotCount];
    private readonly bool[] _buffered = new bool[SlotCount];

    public SkillLoadout()
    {
        for (var i = 0; i < SlotCount; i++)
            _cooldowns[i] = new SkillCooldownTracker();
    }

    public SkillDefinition GetSkill(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return null;
        return _skills[slot];
    }

    public SkillCooldownTracker GetCooldown(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return _cooldowns[0];
        return _cooldowns[slot];
    }

    public void ConfigureSlot(int slot, SkillDefinition skill)
    {
        if (slot < 0 || slot >= SlotCount) return;
        _skills[slot] = skill;
        _cooldowns[slot].Configure(skill != null ? skill.CooldownSeconds : 0f);
    }

    public void Configure(IReadOnlyList<SkillDefinition> skills)
    {
        for (var i = 0; i < SlotCount; i++)
        {
            var skill = skills != null && i < skills.Count ? skills[i] : null;
            ConfigureSlot(i, skill);
        }
    }

    public void SetAllPaused(bool paused)
    {
        for (var i = 0; i < SlotCount; i++)
            _cooldowns[i].SetPaused(paused);
    }

    public void TickAll(float deltaTime)
    {
        for (var i = 0; i < SlotCount; i++)
            _cooldowns[i].Tick(deltaTime);
    }

    public void BufferSlot(int slot)
    {
        if (slot < 0 || slot >= SlotCount) return;
        _buffered[slot] = true;
    }

    public int? ConsumeBufferedSlot()
    {
        for (var i = 0; i < SlotCount; i++)
        {
            if (!_buffered[i]) continue;
            _buffered[i] = false;
            return i;
        }

        return null;
    }

    public void ClearBuffers()
    {
        for (var i = 0; i < SlotCount; i++)
            _buffered[i] = false;
    }

    public void ResetActiveCooldowns()
    {
        for (var i = 0; i < ActiveSlotCount; i++)
            _cooldowns[i].ResetCooldown();
    }
}
