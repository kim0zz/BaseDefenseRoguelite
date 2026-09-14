using System.Collections.Generic;
using UnityEngine;

/// <summary>Runtime bootstrap areny prototypu liny + gracze + dummies.</summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-200)]
public class ChainMovementTestBootstrap : MonoBehaviour
{
    private const string ConfigAssetPath = "Assets/_Game/Config/Prototypes/ChainTetherConfig.asset";

    [SerializeField] private ChainTetherConfig tetherConfig;
    [SerializeField] private PlayerJoinManager joinManager;
    [SerializeField] private int dummyCount = 20;
    [SerializeField] private bool buildArenaOnAwake = true;

    private readonly HashSet<PlayerCharacter> _ccApplied = new();
    private Transform _arenaRoot;

    private void Awake()
    {
        if (buildArenaOnAwake)
            BuildArena();

        EnsureTetherStack();
    }

    private void Update()
    {
        EnsureCharacterControllersOnPlayers();
    }

    public void BuildArena()
    {
        if (_arenaRoot != null) return;

        _arenaRoot = new GameObject("ChainPrototypeArena").transform;
        BuildFloor(_arenaRoot);
        BuildWalls(_arenaRoot);
        BuildObstacles(_arenaRoot);
        EnsureMapPlayArea();
        SpawnDummies(_arenaRoot);
    }

    private void EnsureTetherStack()
    {
        var runtimeGo = GameObject.Find("ChainTether");
        if (runtimeGo == null)
            runtimeGo = new GameObject("ChainTether");

        if (runtimeGo.GetComponent<ChainTetherRuntime>() == null)
            runtimeGo.AddComponent<ChainTetherRuntime>();
        if (runtimeGo.GetComponent<ChainTetherSolver>() == null)
            runtimeGo.AddComponent<ChainTetherSolver>();
        if (runtimeGo.GetComponent<ChainTetherVisual>() == null)
            runtimeGo.AddComponent<ChainTetherVisual>();
        if (runtimeGo.GetComponent<ChainTetherDebugHud>() == null)
            runtimeGo.AddComponent<ChainTetherDebugHud>();
        if (runtimeGo.GetComponent<ChainTetherPresetSwitcher>() == null)
            runtimeGo.AddComponent<ChainTetherPresetSwitcher>();

        var runtime = runtimeGo.GetComponent<ChainTetherRuntime>();
        if (tetherConfig == null)
            tetherConfig = LoadConfigAsset();
        runtime.AssignConfig(tetherConfig);
        var hub = ChainTetherHub.Ensure(new Vector3(0f, 1f, 0f));
        runtime.AssignHub(hub);
    }

    private void EnsureCharacterControllersOnPlayers()
    {
        if (joinManager == null)
            joinManager = FindAnyObjectByType<PlayerJoinManager>();
        if (joinManager == null) return;

        foreach (var player in joinManager.Players)
        {
            if (player == null || _ccApplied.Contains(player)) continue;

            var go = player.gameObject;
            if (go.GetComponent<CharacterController>() == null)
            {
                var cc = go.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.radius = 0.4f;
                cc.center = new Vector3(0f, 1f, 0f);
            }

            if (go.GetComponent<ChainTetherBody>() == null)
                go.AddComponent<ChainTetherBody>();

            _ccApplied.Add(player);
        }
    }

    private void SpawnDummies(Transform root)
    {
        var random = new System.Random(90210);
        var palette = new[]
        {
            new Color(0.55f, 0.35f, 0.65f),
            new Color(0.4f, 0.55f, 0.7f),
            new Color(0.65f, 0.5f, 0.35f)
        };

        for (var i = 0; i < dummyCount; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = $"ChainDummy_{i + 1}";
            go.transform.SetParent(root, false);
            var x = RandomRange(random, -24f, 24f);
            var z = RandomRange(random, -24f, 24f);
            go.transform.position = new Vector3(x, 1f, z);
            go.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

            var collider = go.GetComponent<CapsuleCollider>();
            if (collider != null)
                collider.enabled = true;

            var color = palette[i % palette.Length];
            var fodder = go.AddComponent<ChainPrototypeDummyFodder>();
            var hp = 30f + (i % 3) * 10f;
            fodder.Initialize(go.transform.position, color, hp, enableChase: i % 5 == 0);
        }
    }

    private static void BuildFloor(Transform root)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(root, false);
        floor.transform.localScale = new Vector3(6f, 1f, 6f);
        floor.transform.position = Vector3.zero;
    }

    private static void BuildWalls(Transform root)
    {
        CreateWall(root, "Wall_North", new Vector3(0f, 2f, 30f), new Vector3(62f, 4f, 1f));
        CreateWall(root, "Wall_South", new Vector3(0f, 2f, -30f), new Vector3(62f, 4f, 1f));
        CreateWall(root, "Wall_East", new Vector3(30f, 2f, 0f), new Vector3(1f, 4f, 62f));
        CreateWall(root, "Wall_West", new Vector3(-30f, 2f, 0f), new Vector3(1f, 4f, 62f));
    }

    private static void BuildObstacles(Transform root)
    {
        CreateWall(root, "Corridor_Left", new Vector3(-8f, 2f, 10f), new Vector3(1f, 4f, 18f));
        CreateWall(root, "Corridor_Right", new Vector3(-2f, 2f, 10f), new Vector3(1f, 4f, 18f));

        CreateWall(root, "CentralPillar", new Vector3(6f, 2f, -4f), new Vector3(2.5f, 4f, 2.5f));

        CreateWall(root, "L_Wall_A", new Vector3(14f, 2f, -12f), new Vector3(10f, 4f, 1f));
        CreateWall(root, "L_Wall_B", new Vector3(19f, 2f, -8f), new Vector3(1f, 4f, 10f));
    }

    private static void CreateWall(Transform root, string name, Vector3 position, Vector3 size)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(root, false);
        wall.transform.position = position;
        wall.transform.localScale = size;
        var renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(0.35f, 0.35f, 0.38f));
            renderer.material = mat;
        }
    }

    private static void EnsureMapPlayArea()
    {
        var playArea = FindAnyObjectByType<MapPlayArea>();
        if (playArea == null)
        {
            var go = new GameObject("MapPlayArea");
            playArea = go.AddComponent<MapPlayArea>();
        }

        playArea.SetBounds(new Vector2(-28f, 28f), new Vector2(-28f, 28f));
    }

    private ChainTetherConfig LoadConfigAsset()
    {
#if UNITY_EDITOR
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ChainTetherConfig>(ConfigAssetPath);
        if (asset != null) return asset;
#endif
        var runtime = ScriptableObject.CreateInstance<ChainTetherConfig>();
        runtime.EnsureDraftDefaults();
        return runtime;
    }

    private static float RandomRange(System.Random random, float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }
}
