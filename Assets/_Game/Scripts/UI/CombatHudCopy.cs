using System;
using System.Globalization;
using System.Reflection;

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

    public static string FormatBombCap(int count, int cap)
    {
        if (cap <= 0)
            cap = 3;
        if (count < 0)
            count = 0;
        if (count > cap)
            count = cap;
        return $"Bomby {count}/{cap}";
    }

    public static string FormatOrbitalPips(int ready, int total)
    {
        if (total <= 0)
            return "";
        if (ready < 0)
            ready = 0;
        if (ready > total)
            ready = total;
        return $"Orb {ready}/{total}";
    }

    /// <summary>
    /// Timed or permanent AA interval override (Bomberman rapid / karabin).
    /// Returns false when runtime has not wired the effect yet.
    /// </summary>
    public static bool TryFormatAttackIntervalOverride(PlayerPersistentEffects persistents, out string label)
    {
        label = null;
        if (persistents == null)
            return false;

        if (TryReadOverrideRemaining(persistents, "TimedAttackIntervalOverride", out var timedRemaining))
        {
            label = timedRemaining > 0.05f
                ? string.Format(CultureInfo.InvariantCulture, "RAPID {0:0.#}s", timedRemaining)
                : "RAPID";
            return true;
        }

        if (HasPersistentKind(persistents, "PermanentAttackIntervalOverride"))
        {
            label = "RAPID";
            return true;
        }

        if (TryReadOverrideRemaining(persistents, out var genericRemaining))
        {
            label = genericRemaining > 0.05f
                ? string.Format(CultureInfo.InvariantCulture, "RAPID {0:0.#}s", genericRemaining)
                : "RAPID";
            return true;
        }

        return false;
    }

    private static bool TryReadOverrideRemaining(PlayerPersistentEffects persistents, out float remaining)
    {
        remaining = 0f;
        if (persistents == null)
            return false;

        var type = persistents.GetType();
        foreach (var name in new[]
                 {
                     "TimedAttackIntervalRemaining",
                     "AttackIntervalOverrideRemaining",
                     "RapidFireRemaining"
                 })
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop?.PropertyType == typeof(float) && prop.GetValue(persistents) is float value && value > 0f)
            {
                remaining = value;
                return true;
            }
        }

        foreach (var name in new[] { "TryGetAttackIntervalOverride", "TryGetTimedAttackIntervalOverride" })
        {
            var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
            if (method == null || method.ReturnType != typeof(bool))
                continue;

            var parameters = method.GetParameters();
            object[] args;
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(float).MakeByRefType())
            {
                args = new object[] { 0f };
                if (method.Invoke(persistents, args) is true)
                {
                    remaining = (float)args[0];
                    return remaining > 0f || HasPersistentKind(persistents, "TimedAttackIntervalOverride");
                }
            }
            else if (parameters.Length == 2 &&
                     parameters[0].ParameterType == typeof(float).MakeByRefType() &&
                     parameters[1].ParameterType == typeof(float).MakeByRefType())
            {
                args = new object[] { 0f, 0f };
                if (method.Invoke(persistents, args) is true)
                {
                    remaining = (float)args[1];
                    return remaining > 0f || HasPersistentKind(persistents, "TimedAttackIntervalOverride");
                }
            }
        }

        return false;
    }

    private static bool TryReadOverrideRemaining(
        PlayerPersistentEffects persistents,
        string kindName,
        out float remaining)
    {
        remaining = 0f;
        if (!HasPersistentKind(persistents, kindName))
            return false;

        if (TryReadOverrideRemaining(persistents, out remaining))
            return true;

        remaining = 0f;
        return true;
    }

    private static bool HasPersistentKind(PlayerPersistentEffects persistents, string kindName)
    {
        if (persistents == null || string.IsNullOrEmpty(kindName))
            return false;

        if (!Enum.TryParse(typeof(PersistentEffectKind), kindName, out var parsed))
            return false;

        return persistents.Has((PersistentEffectKind)parsed);
    }
}
