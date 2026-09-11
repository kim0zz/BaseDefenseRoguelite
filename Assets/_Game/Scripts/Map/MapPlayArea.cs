using UnityEngine;

/// <summary>
/// Granice mapy greybox — ogranicza ruch graczy do obszaru grywalnego (M2).
/// </summary>
[DisallowMultipleComponent]
public class MapPlayArea : MonoBehaviour
{
    public static MapPlayArea Instance { get; private set; }

    [SerializeField] private Vector2 xBounds = new(-20f, 20f);
    [SerializeField] private Vector2 zBounds = new(-16f, 28f);

    public Vector2 XBounds => xBounds;
    public Vector2 ZBounds => zBounds;

    private void Awake()
    {
        EnsureInstance();
    }

    private void OnEnable()
    {
        EnsureInstance();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void LateUpdate()
    {
        ClampAllPlayers();
    }

    public void SetBounds(Vector2 newXBounds, Vector2 newZBounds)
    {
        xBounds = newXBounds;
        zBounds = newZBounds;
        EnsureInstance();
    }

    /// <summary>
    /// Rejestruje singleton — także poza Play Mode (EditMode testy nie wołają Awake).
    /// </summary>
    private void EnsureInstance()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[MapPlayArea] Więcej niż jeden obszar gry — używam pierwszego.");
            return;
        }

        Instance = this;
    }

    public bool Contains(Vector3 worldPosition)
    {
        return worldPosition.x >= xBounds.x && worldPosition.x <= xBounds.y
            && worldPosition.z >= zBounds.x && worldPosition.z <= zBounds.y;
    }

    public Vector3 Clamp(Vector3 worldPosition)
    {
        return new Vector3(
            Mathf.Clamp(worldPosition.x, xBounds.x, xBounds.y),
            worldPosition.y,
            Mathf.Clamp(worldPosition.z, zBounds.x, zBounds.y));
    }

    private void ClampAllPlayers()
    {
        var players = FindObjectsByType<PlayerCharacter>();
        foreach (var player in players)
        {
            var pos = player.transform.position;
            var clamped = Clamp(pos);
            if ((clamped - pos).sqrMagnitude > 0.0001f)
                player.transform.position = clamped;
        }
    }
}
