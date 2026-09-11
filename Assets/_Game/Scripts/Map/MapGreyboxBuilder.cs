using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buduje greybox mapy M2/M6.5: baza, 3 linie natarcia, 5 wież, podłoże i granice.
/// Zgodnie z FROZEN BASE_DEFENSE: 2 wieże przy bazie + 1 na każdej linii.
/// </summary>
[DisallowMultipleComponent]
[ExecuteAlways]
public class MapGreyboxBuilder : MonoBehaviour
{
    [Header("Rozmiar mapy")]
    [SerializeField] private Vector2 groundSize = new(50f, 50f);
    [SerializeField] private Vector2 playAreaX = new(-20f, 20f);
    [SerializeField] private Vector2 playAreaZ = new(-16f, 28f);

    [Header("Rdzeń bazy")]
    [SerializeField] private Vector3 baseCorePosition = new(0f, 0f, -10f);
    [SerializeField] private Vector3 baseCoreScale = new(4f, 2f, 3f);

    [Header("Wieże bazowe (2)")]
    [SerializeField] private Vector3 baseTowerLeftPosition = new(-6f, 0f, -6f);
    [SerializeField] private Vector3 baseTowerRightPosition = new(6f, 0f, -6f);

    [Header("Wieże liniowe (3)")]
    [SerializeField] private Vector3 centerLineTowerPosition = new(0f, 0f, 8f);
    [SerializeField] private Vector3 leftLineTowerPosition = new(-14f, 0f, 8f);
    [SerializeField] private Vector3 rightLineTowerPosition = new(14f, 0f, 8f);

    [Header("Końce linii (daleko od bazy)")]
    [SerializeField] private Vector3 centerLaneFarEnd = new(0f, 0f, 26f);
    [SerializeField] private Vector3 leftLaneFarEnd = new(-16f, 0f, 26f);
    [SerializeField] private Vector3 rightLaneFarEnd = new(16f, 0f, 26f);

    [Header("Przewężenia (choke)")]
    [SerializeField] private float chokeZ = 14f;
    [SerializeField] private Vector3 chokeScale = new(2.2f, 0.8f, 1.2f);
    [SerializeField] private Color chokeColor = new(0.5f, 0.5f, 0.55f, 0.85f);

    [Header("Korytarz linii")]
    [SerializeField] private float laneCorridorHalfWidth = 1.5f;
    [SerializeField] private float laneVisualWidth = 3f;

    [Header("Wizualne — kolory placeholderów")]
    [SerializeField] private Color groundColor = new(0.35f, 0.38f, 0.32f);
    [SerializeField] private Color baseColor = new(0.55f, 0.52f, 0.48f);
    [SerializeField] private Color lineTowerColor = new(0.45f, 0.55f, 0.7f);
    [SerializeField] private Color baseTowerColor = new(0.6f, 0.45f, 0.35f);
    [SerializeField] private Color laneColorLeft = new(0.7f, 0.35f, 0.35f, 0.35f);
    [SerializeField] private Color laneColorCenter = new(0.75f, 0.7f, 0.3f, 0.35f);
    [SerializeField] private Color laneColorRight = new(0.35f, 0.55f, 0.75f, 0.35f);
    [SerializeField] private Color boundaryColor = new(0.2f, 0.2f, 0.2f, 0.6f);

    [Header("Spawn graczy przy bazie")]
    [SerializeField] private Vector3[] playerSpawnPositions =
    {
        new(-4f, 1f, -12f),
        new( 4f, 1f, -12f),
        new(-7f, 1f, -10f),
        new( 7f, 1f, -10f)
    };

    public IReadOnlyList<Vector3> PlayerSpawnPositions => playerSpawnPositions;
    public Vector2 PlayAreaX => playAreaX;
    public Vector2 PlayAreaZ => playAreaZ;
    public Vector3 BaseCorePosition => baseCorePosition;
    public float LaneCorridorHalfWidth => laneCorridorHalfWidth;

    public bool HasVisualMapChild()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == "MapGreybox")
                return true;
        }

        return false;
    }

    /// <summary>Ścieżka linii: spawn → choke → wieża linii → rdzeń bazy (M6.5).</summary>
    public LanePath GetLanePath(AttackLineId line)
    {
        return LanePath.Create(
            GetFarEnd(line),
            GetChokePosition(line),
            GetLineTower(line),
            baseCorePosition,
            laneCorridorHalfWidth);
    }

    /// <summary>Pozycja spawnu wroga na końcu linii.</summary>
    public Vector3 GetLaneSpawnPosition(AttackLineId line, int offsetIndex = 0, float spread = 1.2f)
    {
        var farEnd = GetFarEnd(line);

        var lateral = line switch
        {
            AttackLineId.Left => Vector3.right,
            AttackLineId.Right => Vector3.left,
            _ => Vector3.right
        };

        return farEnd + lateral * (offsetIndex * spread) + Vector3.up;
    }

    private Transform _mapRoot;

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            BuildGreybox();
            return;
        }
#endif
        if (Application.isPlaying && !HasVisualMapChild())
            BuildGreybox();
    }

    private void Start()
    {
        if (Application.isPlaying && !HasVisualMapChild())
            BuildGreybox();
    }

    public void BuildGreybox()
    {
        ClearMapChildren();

        _mapRoot = new GameObject("MapGreybox").transform;
        _mapRoot.SetParent(transform, false);

        CreateGround();
        CreateBaseCore();
        CreateTower("Wieza_Linia_Lewa", leftLineTowerPosition, AttackLineId.Left, TowerRole.LineTower, lineTowerColor);
        CreateTower("Wieza_Linia_Srodek", centerLineTowerPosition, AttackLineId.Center, TowerRole.LineTower,
            new Color(0.5f, 0.48f, 0.42f), new Vector3(2.2f, 2f, 2.2f));
        CreateTower("Wieza_Linia_Prawa", rightLineTowerPosition, AttackLineId.Right, TowerRole.LineTower, lineTowerColor);
        CreateTower("Wieza_Bazowa_L", baseTowerLeftPosition, AttackLineId.Left, TowerRole.BaseTower, baseTowerColor);
        CreateTower("Wieza_Bazowa_P", baseTowerRightPosition, AttackLineId.Right, TowerRole.BaseTower, baseTowerColor);
        CreateLanePolyline("Linia_Lewa", AttackLineId.Left, laneColorLeft);
        CreateLanePolyline("Linia_Srodek", AttackLineId.Center, laneColorCenter);
        CreateLanePolyline("Linia_Prawa", AttackLineId.Right, laneColorRight);
        CreateChokePlaceholder("Choke_Lewa", AttackLineId.Left);
        CreateChokePlaceholder("Choke_Srodek", AttackLineId.Center);
        CreateChokePlaceholder("Choke_Prawa", AttackLineId.Right);
        CreateBoundaryMarkers();
        ConfigurePlayArea();

        Debug.Log("[MapGreyboxBuilder] Greybox M6.5 zbudowany: baza, 5 wież, 3 linie, choke, granice mapy.");
    }

    private Vector3 GetFarEnd(AttackLineId line) => line switch
    {
        AttackLineId.Left => leftLaneFarEnd,
        AttackLineId.Right => rightLaneFarEnd,
        _ => centerLaneFarEnd
    };

    private Vector3 GetLineTower(AttackLineId line) => line switch
    {
        AttackLineId.Left => leftLineTowerPosition,
        AttackLineId.Right => rightLineTowerPosition,
        _ => centerLineTowerPosition
    };

    private Vector3 GetChokePosition(AttackLineId line)
    {
        var farEnd = GetFarEnd(line);
        return new Vector3(farEnd.x, 0f, chokeZ);
    }

    private void ClearMapChildren()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name != "MapGreybox") continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
#endif
                Destroy(child.gameObject);
        }

        _mapRoot = null;
    }

    private void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Podloze";
        ground.transform.SetParent(_mapRoot, false);
        ground.transform.localScale = new Vector3(groundSize.x / 10f, 1f, groundSize.y / 10f);
        ApplyColor(ground, groundColor);
        DestroyCollider(ground);
    }

    private void CreateBaseCore()
    {
        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGo.name = "Baza_Rdzen";
        baseGo.transform.SetParent(_mapRoot, false);
        baseGo.transform.position = baseCorePosition + Vector3.up * (baseCoreScale.y * 0.5f);
        baseGo.transform.localScale = baseCoreScale;
        baseGo.AddComponent<BaseCore>();
        var baseHealth = baseGo.AddComponent<BaseHealth>();
        ApplyColor(baseGo, baseColor);
        ConfigureStructureCollider(baseGo);

        var runState = SharedRunState.Instance;
        if (runState != null && runState.Config != null)
            baseHealth.InitializeFromConfig(runState.Config);
    }

    private void CreateTower(string name, Vector3 position, AttackLineId line, TowerRole role, Color color,
        Vector3? scaleOverride = null)
    {
        var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tower.name = name;
        tower.transform.SetParent(_mapRoot, false);
        var scale = scaleOverride ?? new Vector3(1.8f, 1.5f, 1.8f);
        tower.transform.position = position + Vector3.up * (scale.y * 0.5f);
        tower.transform.localScale = scale;
        var marker = tower.AddComponent<TowerMarker>();
        marker.Configure(line, role);
        var towerHealth = tower.AddComponent<TowerHealth>();
        var runState = SharedRunState.Instance;
        var config = runState != null ? runState.Config : null;
        towerHealth.InitializeFromConfig(config, color);
        ApplyColor(tower, color);
        ConfigureStructureCollider(tower);
    }

    private void CreateLanePolyline(string name, AttackLineId line, Color color)
    {
        var path = GetLanePath(line);
        var laneRoot = new GameObject(name).transform;
        laneRoot.SetParent(_mapRoot, false);

        for (var i = 0; i < path.SegmentCount; i++)
        {
            var a = path.Waypoints[i];
            var b = path.Waypoints[i + 1];
            CreateLaneSegment(laneRoot, $"{name}_Seg{i + 1}", a, b, color, line);
        }
    }

    private void CreateLaneSegment(Transform parent, string name, Vector3 from, Vector3 to, Color color, AttackLineId line)
    {
        var midpoint = (from + to) * 0.5f;
        var direction = to - from;
        var length = direction.magnitude;
        if (length < 0.01f) return;

        var lane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lane.name = name;
        lane.transform.SetParent(parent, false);
        lane.transform.position = midpoint + Vector3.up * 0.05f;
        lane.transform.localScale = new Vector3(laneVisualWidth, 0.1f, length);
        lane.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        lane.AddComponent<LaneMarker>().Configure(line);
        ApplyColor(lane, color, transparent: true);
        DestroyCollider(lane);
    }

    private void CreateChokePlaceholder(string name, AttackLineId line)
    {
        var chokePos = GetChokePosition(line);
        var choke = GameObject.CreatePrimitive(PrimitiveType.Cube);
        choke.name = name;
        choke.transform.SetParent(_mapRoot, false);
        choke.transform.position = chokePos + Vector3.up * (chokeScale.y * 0.5f);
        choke.transform.localScale = chokeScale;
        ApplyColor(choke, chokeColor);
        DestroyCollider(choke);
    }

    private void CreateBoundaryMarkers()
    {
        var boundaryRoot = new GameObject("GraniceMapy").transform;
        boundaryRoot.SetParent(_mapRoot, false);

        var minX = playAreaX.x;
        var maxX = playAreaX.y;
        var minZ = playAreaZ.x;
        var maxZ = playAreaZ.y;
        var wallHeight = 0.5f;
        var wallThickness = 0.3f;

        CreateBoundaryWall(boundaryRoot, "Granica_N", new Vector3(0f, wallHeight * 0.5f, maxZ),
            new Vector3(maxX - minX + wallThickness, wallHeight, wallThickness));
        CreateBoundaryWall(boundaryRoot, "Granica_S", new Vector3(0f, wallHeight * 0.5f, minZ),
            new Vector3(maxX - minX + wallThickness, wallHeight, wallThickness));
        CreateBoundaryWall(boundaryRoot, "Granica_W", new Vector3(minX, wallHeight * 0.5f, (minZ + maxZ) * 0.5f),
            new Vector3(wallThickness, wallHeight, maxZ - minZ + wallThickness));
        CreateBoundaryWall(boundaryRoot, "Granica_E", new Vector3(maxX, wallHeight * 0.5f, (minZ + maxZ) * 0.5f),
            new Vector3(wallThickness, wallHeight, maxZ - minZ + wallThickness));
    }

    private void CreateBoundaryWall(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        ApplyColor(wall, boundaryColor, transparent: true);
        DestroyCollider(wall);
    }

    private void ConfigurePlayArea()
    {
        var playArea = GetComponent<MapPlayArea>();
        if (playArea == null)
            playArea = gameObject.AddComponent<MapPlayArea>();

        playArea.SetBounds(playAreaX, playAreaZ);
    }

    private static void ApplyColor(GameObject go, Color color, bool transparent = false)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = new Material(renderer.sharedMaterial);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;

        if (transparent)
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
        }

        renderer.sharedMaterial = mat;
    }

    private static void DestroyCollider(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(col);
        else
#endif
            Destroy(col);
    }

    private static void ConfigureStructureCollider(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col == null)
            col = go.AddComponent<BoxCollider>();

        col.isTrigger = false;

        var profile = go.GetComponent<ForcedMovementResistanceProfile>();
        if (profile == null)
            profile = go.AddComponent<ForcedMovementResistanceProfile>();
        profile.Configure(ForcedMovementResistanceCategory.Structure);
    }
}
