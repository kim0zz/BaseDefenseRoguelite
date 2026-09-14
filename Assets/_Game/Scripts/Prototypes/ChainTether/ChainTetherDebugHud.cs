using UnityEngine;

/// <summary>Prosty HUD debug liny (OnGUI) — tylko scena prototypu.</summary>
[DisallowMultipleComponent]
public class ChainTetherDebugHud : MonoBehaviour
{
    [SerializeField] private ChainTetherRuntime runtime;

    private void Awake()
    {
        if (runtime == null)
            runtime = GetComponent<ChainTetherRuntime>();
    }

    private void OnGUI()
    {
        if (runtime == null) return;

        var tuning = runtime.ActiveTuning;
        var style = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
        var y = 10f;
        GUI.Label(new Rect(10f, y, 900f, 24f),
            $"Chain preset: {runtime.ActivePreset} (1=Baseline  2=Short  3=ExtraShort)", style);
        y += 22f;
        GUI.Label(new Rect(10f, y, 900f, 24f),
            $"Topology: {runtime.ActiveTopology}  (7=Pierścień/trójkąt/kwadrat  8=Hub/kupsko)  pad D-pad lewo/prawo",
            style);
        y += 22f;
        if (runtime.ActiveTopology == ChainTetherTopology.Hub && runtime.Hub != null)
        {
            GUI.Label(new Rect(10f, y, 900f, 24f),
                $"Hub mass={runtime.Hub.CurrentMass:0.00}  (0=lżejsze  9=cięższe)",
                style);
            y += 22f;
        }
        GUI.Label(new Rect(10f, y, 720f, 24f),
            $"rest={tuning.restLength:0.#} softStart={tuning.softTensionStart:0.#} hardStart={tuning.hardTensionStart:0.#} maxStretch={tuning.maxStretch:0.#}",
            style);
        y += 22f;
        GUI.Label(new Rect(10f, y, 720f, 24f),
            $"softK={tuning.softSpring:0.#} hardK={tuning.hardSpring:0.#} damp={tuning.damping:0.#} maxAccel={tuning.maxPullAcceleration:0.#} agency={tuning.playerAgencyRetention:0.##}",
            style);
        y += 28f;

        var links = runtime.LastLinkDebug;
        if (links == null) return;

        for (var i = 0; i < links.Length; i++)
        {
            var link = links[i];
            GUI.Label(new Rect(10f, y, 720f, 22f),
                $"Link {link.LinkIndex + 1}: d={link.Distance:0.##}m zone={link.Zone} spring={link.SpringAcceleration:0.#} pull={link.AppliedAcceleration:0.#}",
                style);
            y += 20f;
        }
    }
}
