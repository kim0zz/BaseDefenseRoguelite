using UnityEngine;

/// <summary>Konfiguracja presetów liny — ScriptableObject (prototype v0.1).</summary>
[CreateAssetMenu(fileName = "ChainTetherConfig", menuName = "Game/Prototypes/Chain Tether Config")]
public class ChainTetherConfig : ScriptableObject
{
    [SerializeField] private ChainTetherTuning baseline = ChainTetherTuning.BaselineDraft;
    [SerializeField] private ChainTetherTuning shortTune = ChainTetherTuning.ShortDraft;
    [SerializeField] private ChainTetherTuning extraShort = ChainTetherTuning.ExtraShortDraft;
    [SerializeField] private ChainTetherPresetId defaultPreset = ChainTetherPresetId.ExtraShort;

    public ChainTetherPresetId DefaultPreset => defaultPreset;

    public ChainTetherTuning GetTuning(ChainTetherPresetId presetId)
    {
        return presetId switch
        {
            ChainTetherPresetId.Short => shortTune,
            ChainTetherPresetId.ExtraShort => extraShort,
            _ => baseline
        };
    }

    public void EnsureDraftDefaults()
    {
        baseline = ChainTetherTuning.BaselineDraft;
        shortTune = ChainTetherTuning.ShortDraft;
        extraShort = ChainTetherTuning.ExtraShortDraft;
        defaultPreset = ChainTetherPresetId.ExtraShort;
    }
}
