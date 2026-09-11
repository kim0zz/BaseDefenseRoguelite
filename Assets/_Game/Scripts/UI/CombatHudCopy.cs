using System.Globalization;

/// <summary>
/// Teksty HUD walki — osobno od OnGUI, żeby dało się testować format.
/// </summary>
public static class CombatHudCopy
{
    public static string FormatWeaponPower(string displayName, float damage, float attackInterval)
    {
        var name = string.IsNullOrWhiteSpace(displayName) ? "-" : displayName;
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} {1:0.#} dmg / {2:0.00}s",
            name,
            damage,
            attackInterval);
    }

    public static string FormatBasicAttackPower(float damage, float attackInterval)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "Atak {0:0.#} dmg / {1:0.00}s",
            damage,
            attackInterval);
    }

    public static string FormatHartMeter(int stacks, int maxStacks, bool ready)
    {
        if (maxStacks <= 0)
            maxStacks = 5;
        if (ready)
            return "Hart GOTOWY";
        if (stacks < 0)
            stacks = 0;
        if (stacks > maxStacks)
            stacks = maxStacks;
        return $"Hart {stacks}/{maxStacks}";
    }

    public static string FormatHartPips(int stacks, int maxStacks, bool ready)
    {
        if (maxStacks <= 0)
            maxStacks = 5;
        if (stacks < 0)
            stacks = 0;
        if (stacks > maxStacks)
            stacks = maxStacks;

        var filled = new string('#', stacks);
        var empty = new string('-', maxStacks - stacks);
        var pips = $"[{filled}{empty}]";
        return ready
            ? $"Hart GOTOWY  {pips}"
            : $"{FormatHartMeter(stacks, maxStacks, false)}  {pips}";
    }

    public static string FormatSkillCooldown(string skillName, float remaining, float total)
    {
        if (total <= 0f || remaining <= 0f)
            return $"{skillName} gotowe";
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} {1:0.#}s",
            skillName,
            remaining);
    }

    public static string GetSkillSlotKeyHint(int slotIndex, bool keyboardMouse)
    {
        return GetSkillSlotKeyHint(slotIndex, keyboardMouse, isPassive: false);
    }

    public static string GetSkillSlotKeyHint(int slotIndex, bool keyboardMouse, bool isPassive)
    {
        if (isPassive && slotIndex == PlayerSkillController.UltimateSlot)
            return "PASYW";

        if (keyboardMouse)
        {
            return slotIndex switch
            {
                0 => "Q",
                1 => "E",
                2 => "R",
                3 => "F",
                _ => "?"
            };
        }

        return slotIndex switch
        {
            0 => "East",
            1 => "North",
            2 => "LB",
            3 => "RB",
            _ => "?"
        };
    }
}
