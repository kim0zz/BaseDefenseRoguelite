using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Przełączanie presetów liny w runtime (klawiatura 1/2/3, pad Select).</summary>
[DisallowMultipleComponent]
public class ChainTetherPresetSwitcher : MonoBehaviour
{
    [SerializeField] private ChainTetherRuntime runtime;

    private void Awake()
    {
        if (runtime == null)
            runtime = GetComponent<ChainTetherRuntime>();
    }

    private void Update()
    {
        if (runtime == null) return;

        if (ReadKeyboardPreset(out var preset))
        {
            runtime.SetPreset(preset);
            return;
        }

        if (ReadKeyboardTopology(out var topology))
        {
            runtime.SetTopology(topology);
            return;
        }

        if (ReadGamepadPreset(out preset))
        {
            runtime.SetPreset(preset);
            return;
        }

        if (ReadGamepadTopology(out topology))
            runtime.SetTopology(topology);

        NudgeHubMass();
    }

    private void NudgeHubMass()
    {
        var hub = runtime.Hub;
        if (hub == null || !hub.isActiveAndEnabled)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.minusKey.wasPressedThisFrame || keyboard.digit0Key.wasPressedThisFrame)
            hub.NudgeMass(-0.2f);
        else if (keyboard.equalsKey.wasPressedThisFrame || keyboard.digit9Key.wasPressedThisFrame)
            hub.NudgeMass(0.2f);
    }

    private static bool ReadKeyboardPreset(out ChainTetherPresetId preset)
    {
        preset = ChainTetherPresetId.Baseline;
        var keyboard = Keyboard.current;
        if (keyboard == null) return false;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            preset = ChainTetherPresetId.Baseline;
            return true;
        }

        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            preset = ChainTetherPresetId.Short;
            return true;
        }

        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            preset = ChainTetherPresetId.ExtraShort;
            return true;
        }

        return false;
    }

    private static bool ReadGamepadPreset(out ChainTetherPresetId preset)
    {
        preset = ChainTetherPresetId.Baseline;
        foreach (var pad in Gamepad.all)
        {
            if (pad == null || !pad.enabled) continue;
            if (pad.selectButton.wasPressedThisFrame)
            {
                preset = ChainTetherPresetId.Short;
                return true;
            }

            if (pad.startButton.wasPressedThisFrame)
            {
                preset = ChainTetherPresetId.ExtraShort;
                return true;
            }
        }

        return false;
    }

    private static bool ReadKeyboardTopology(out ChainTetherTopology topology)
    {
        topology = ChainTetherTopology.Ring;
        var keyboard = Keyboard.current;
        if (keyboard == null) return false;

        if (keyboard.digit7Key.wasPressedThisFrame)
        {
            topology = ChainTetherTopology.Ring;
            return true;
        }

        if (keyboard.digit8Key.wasPressedThisFrame)
        {
            topology = ChainTetherTopology.Hub;
            return true;
        }

        return false;
    }

    private static bool ReadGamepadTopology(out ChainTetherTopology topology)
    {
        topology = ChainTetherTopology.Ring;
        foreach (var pad in Gamepad.all)
        {
            if (pad == null || !pad.enabled) continue;
            if (pad.dpad.left.wasPressedThisFrame)
            {
                topology = ChainTetherTopology.Ring;
                return true;
            }

            if (pad.dpad.right.wasPressedThisFrame)
            {
                topology = ChainTetherTopology.Hub;
                return true;
            }
        }

        return false;
    }
}
