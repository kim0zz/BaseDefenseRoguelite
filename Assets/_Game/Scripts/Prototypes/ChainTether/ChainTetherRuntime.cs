using UnityEngine;

/// <summary>Udostępnia aktywny preset i tuning liny w runtime (prototype).</summary>
[DisallowMultipleComponent]
public class ChainTetherRuntime : MonoBehaviour
{
    [SerializeField] private ChainTetherConfig config;
    [SerializeField] private ChainTetherHub hub;

    private ChainTetherPresetId _activePreset = ChainTetherPresetId.ExtraShort;
    private ChainTetherTopology _activeTopology = ChainTetherTopology.Ring;

    public ChainTetherConfig Config => config;
    public ChainTetherPresetId ActivePreset => _activePreset;
    public ChainTetherTopology ActiveTopology => _activeTopology;
    public ChainTetherHub Hub => hub;
    public ChainTetherBody HubBody => hub != null && hub.isActiveAndEnabled ? hub.Body : null;

    public ChainTetherTuning ActiveTuning =>
        config != null ? config.GetTuning(_activePreset) : ChainTetherTuning.ExtraShortDraft;

    public void AssignConfig(ChainTetherConfig newConfig)
    {
        if (newConfig != null)
            config = newConfig;
    }

    public ChainTetherSolver.LinkDebugState[] LastLinkDebug { get; private set; }

    private void Awake()
    {
        if (config == null)
            config = CreateFallbackConfig();
        _activePreset = config.DefaultPreset;
        ApplyTopologyVisibility();
    }

    public void AssignHub(ChainTetherHub newHub)
    {
        hub = newHub;
        ApplyTopologyVisibility();
    }

    public void SetTopology(ChainTetherTopology topology)
    {
        var changed = _activeTopology != topology;
        _activeTopology = topology;
        ApplyTopologyVisibility(snapHub: changed && topology == ChainTetherTopology.Hub);
    }

    private void ApplyTopologyVisibility(bool snapHub = false)
    {
        if (hub == null)
            return;

        var useHub = _activeTopology == ChainTetherTopology.Hub;
        hub.SetVisible(useHub);
        if (!useHub || !snapHub)
            return;

        var players = FindObjectsByType<PlayerCharacter>();
        if (players.Length == 0)
        {
            hub.SnapToCentroid(new Vector3(0f, 0.85f, 0f));
            return;
        }

        var sum = Vector3.zero;
        for (var i = 0; i < players.Length; i++)
            sum += players[i].transform.position;
        hub.SnapToCentroid(sum / players.Length);
    }

    public void SetPreset(ChainTetherPresetId presetId)
    {
        _activePreset = presetId;
    }

    internal void SetLastLinkDebug(ChainTetherSolver.LinkDebugState[] states)
    {
        LastLinkDebug = states;
    }

    private static ChainTetherConfig CreateFallbackConfig()
    {
        var runtime = ScriptableObject.CreateInstance<ChainTetherConfig>();
        runtime.EnsureDraftDefaults();
        return runtime;
    }
}
