using UnityEngine;

/// <summary>Wizualizacja linków liny (LineRenderer, kolor strefy).</summary>
[DisallowMultipleComponent]
public class ChainTetherVisual : MonoBehaviour
{
    [SerializeField] private ChainTetherRuntime runtime;
    [SerializeField] private PlayerJoinManager joinManager;
    [SerializeField] private float baseWidth = 0.12f;
    [SerializeField] private float hardWidthBonus = 0.08f;

    private readonly ChainTetherGraph _graph = new();
    private LineRenderer[] _lines = System.Array.Empty<LineRenderer>();

    private static readonly Color SlackColor = new(0.35f, 0.85f, 0.45f, 1f);
    private static readonly Color SoftColor = new(0.95f, 0.85f, 0.2f, 1f);
    private static readonly Color HardColor = new(0.95f, 0.25f, 0.2f, 1f);

    private void Awake()
    {
        if (runtime == null)
            runtime = GetComponent<ChainTetherRuntime>();
        if (joinManager == null)
            joinManager = FindAnyObjectByType<PlayerJoinManager>();
    }

    private void LateUpdate()
    {
        if (runtime == null) return;

        var players = joinManager != null
            ? joinManager.Players
            : new System.Collections.Generic.List<PlayerCharacter>(FindObjectsByType<PlayerCharacter>());

        _graph.Rebuild(runtime.ActiveTopology, players, runtime.HubBody);
        EnsureLineCount(_graph.Links.Count);

        var debug = runtime.LastLinkDebug;
        for (var i = 0; i < _graph.Links.Count; i++)
        {
            var link = _graph.Links[i];
            if (link.BodyA == null || link.BodyB == null) continue;

            var line = _lines[i];
            var aPos = link.BodyA.transform.position + Vector3.up * 0.9f;
            var bPos = link.BodyB.transform.position + Vector3.up * 0.9f;
            line.SetPosition(0, aPos);
            line.SetPosition(1, bPos);

            var zone = ChainTetherZone.Slack;
            if (debug != null && i < debug.Length)
                zone = debug[i].Zone;

            line.startColor = line.endColor = ColorForZone(zone);
            var width = baseWidth + (zone == ChainTetherZone.Hard ? hardWidthBonus : zone == ChainTetherZone.Soft ? hardWidthBonus * 0.5f : 0f);
            line.startWidth = line.endWidth = width;
        }
    }

    private void EnsureLineCount(int count)
    {
        if (_lines.Length == count) return;

        for (var i = 0; i < _lines.Length; i++)
        {
            if (_lines[i] != null)
                Destroy(_lines[i].gameObject);
        }

        _lines = new LineRenderer[count];
        for (var i = 0; i < count; i++)
        {
            var go = new GameObject($"ChainTetherLine_{i + 1}");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            _lines[i] = line;
        }
    }

    private static Color ColorForZone(ChainTetherZone zone)
    {
        return zone switch
        {
            ChainTetherZone.Soft => SoftColor,
            ChainTetherZone.Hard => HardColor,
            _ => SlackColor
        };
    }
}
