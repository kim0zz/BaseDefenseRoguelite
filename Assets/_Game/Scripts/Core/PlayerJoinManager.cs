using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerJoinManager — sloty graczy 1–4 zgodnie z FROZEN input.
/// P1: domyślnie klawiatura; może przełączyć na pad (South). Powrót: Tab.
/// P2–4: dołączają wyłącznie przez South na wolnym padzie.
/// </summary>
[DisallowMultipleComponent]
public class PlayerJoinManager : MonoBehaviour
{
    private const int Player1Slot = 0;
    private const int MaxSlots = 4;

    [SerializeField] private int maxPlayers = MaxSlots;

    [Tooltip("Gdy true — na Start przypisuje podłączone pady do slotów P1..Pn zamiast wymuszać klawiaturę P1.")]
    [SerializeField] private bool autoJoinConnectedGamepads;

    [Tooltip("Opcjonalne Transform'y punktów spawnu. Jeśli puste — używa FallbackSpawnPositions.")]
    [SerializeField] private Transform[] spawnPoints;

    [Tooltip("Pozycje spawnu używane gdy SpawnPoints i MapGreyboxBuilder nie są dostępne.")]
    [SerializeField] private Vector3[] fallbackSpawnPositions =
    {
        new(-4f, 1f, -12f),
        new( 4f, 1f, -12f),
        new(-7f, 1f, -10f),
        new( 7f, 1f, -10f)
    };

    private readonly PlayerCharacter[] _playerSlots = new PlayerCharacter[MaxSlots];
    private readonly HashSet<Gamepad> _assignedGamepads = new();

    public IReadOnlyList<PlayerCharacter> Players
    {
        get
        {
            var list = new List<PlayerCharacter>();
            for (var i = 0; i < MaxSlots; i++)
            {
                if (_playerSlots[i] != null)
                    list.Add(_playerSlots[i]);
            }
            return list;
        }
    }

    private void Start()
    {
        if (autoJoinConnectedGamepads && TryAutoJoinConnectedGamepadsOnStart())
        {
            Debug.Log("[PlayerJoinManager] Auto-join padów aktywny (P1..Pn). Tab: P1 → klawiatura. South: kolejni gracze.");
            return;
        }

        if (Keyboard.current == null)
        {
            Debug.LogWarning("[PlayerJoinManager] Brak klawiatury — Player 1 nie zostanie utworzony automatycznie.");
            return;
        }

        SpawnPlayer(Player1Slot, PlayerInputMode.KeyboardMouse);
        Debug.Log("[PlayerJoinManager] Player 1 przypisany do klawiatury (WASD). " +
                  "P2–4: South na padzie. P1 → pad: South. P1 → klawiatura: Tab.");
    }

    private bool TryAutoJoinConnectedGamepadsOnStart()
    {
        var pads = new List<Gamepad>();
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad != null)
                pads.Add(gamepad);
        }

        if (pads.Count == 0)
            return false;

        var slot = 0;
        for (var i = 0; i < pads.Count && slot < maxPlayers; i++, slot++)
            SpawnPlayer(slot, PlayerInputMode.Gamepad, pads[i]);

        return slot > 0;
    }

    private void Update()
    {
        HandlePlayer1SwitchToKeyboard();
        HandleGamepadJoinAndSwitch();
    }

    private void HandlePlayer1SwitchToKeyboard()
    {
        var player1 = _playerSlots[Player1Slot];
        if (player1 == null || player1.InputMode != PlayerInputMode.Gamepad) return;

        var keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.tabKey.wasPressedThisFrame) return;

        ReleaseGamepad(player1.AssignedGamepad);
        player1.SetInputMode(PlayerInputMode.KeyboardMouse);
        Debug.Log("[PlayerJoinManager] Player 1 przełączony na klawiaturę (Tab).");
    }

    private void HandleGamepadJoinAndSwitch()
    {
        foreach (var gamepad in Gamepad.all)
        {
            if (!gamepad.buttonSouth.wasPressedThisFrame) continue;
            if (_assignedGamepads.Contains(gamepad)) continue;

            if (TrySwitchPlayer1ToGamepad(gamepad))
                continue;

            TryJoinGamepadPlayer(gamepad);
        }
    }

    private bool TrySwitchPlayer1ToGamepad(Gamepad gamepad)
    {
        var player1 = _playerSlots[Player1Slot];
        if (player1 == null || player1.InputMode != PlayerInputMode.KeyboardMouse) return false;

        player1.SetInputMode(PlayerInputMode.Gamepad, gamepad);
        _assignedGamepads.Add(gamepad);
        Debug.Log($"[PlayerJoinManager] Player 1 przełączony na pad ({gamepad.displayName}). " +
                  "Powrót do klawiatury: Tab.");
        return true;
    }

    private void TryJoinGamepadPlayer(Gamepad gamepad)
    {
        if (CountActivePlayers() >= maxPlayers) return;

        var slot = FindFirstFreeGamepadSlot();
        if (slot < 0) return;

        SpawnPlayer(slot, PlayerInputMode.Gamepad, gamepad);
    }

    private int FindFirstFreeGamepadSlot()
    {
        for (var slot = 1; slot < MaxSlots; slot++)
        {
            if (_playerSlots[slot] == null)
                return slot;
        }
        return -1;
    }

    private int CountActivePlayers()
    {
        var count = 0;
        for (var i = 0; i < MaxSlots; i++)
        {
            if (_playerSlots[i] != null)
                count++;
        }
        return count;
    }

    private void SpawnPlayer(int slotIndex, PlayerInputMode mode, Gamepad gamepad = null)
    {
        if (_playerSlots[slotIndex] != null)
        {
            Debug.LogWarning($"[PlayerJoinManager] Slot {slotIndex + 1} jest już zajęty.");
            return;
        }

        if (mode == PlayerInputMode.Gamepad)
        {
            if (gamepad == null || _assignedGamepads.Contains(gamepad))
                return;

            _assignedGamepads.Add(gamepad);
        }

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = $"Gracz_{slotIndex + 1}";
        go.transform.position = GetSpawnPosition(slotIndex);
        Destroy(go.GetComponent<CapsuleCollider>());

        var character = go.AddComponent<PlayerCharacter>();
        character.Initialize(slotIndex, mode, gamepad);

        var bootstrap = FindAnyObjectByType<CombatBootstrap>();
        bootstrap?.SetupPlayer(go, slotIndex);

        _playerSlots[slotIndex] = character;

        Debug.Log($"[PlayerJoinManager] Gracz {slotIndex + 1} dołączył " +
                  $"(tryb: {mode}, urządzenie: {GetDeviceLabel(mode, gamepad)}).");
    }

    private void ReleaseGamepad(Gamepad gamepad)
    {
        if (gamepad != null)
            _assignedGamepads.Remove(gamepad);
    }

    private Vector3 GetSpawnPosition(int index)
    {
        var siege = SiegeArena.Instance;
        if (siege != null)
            return siege.GetPlayerSpawn(index);
        if (spawnPoints != null && index < spawnPoints.Length && spawnPoints[index] != null)
            return spawnPoints[index].position;

        var mapBuilder = FindAnyObjectByType<MapGreyboxBuilder>();
        if (mapBuilder != null)
        {
            var mapSpawns = mapBuilder.PlayerSpawnPositions;
            if (index < mapSpawns.Count)
                return mapSpawns[index];
        }

        if (index < fallbackSpawnPositions.Length)
            return fallbackSpawnPositions[index];

        return Vector3.zero;
    }

    private static string GetDeviceLabel(PlayerInputMode mode, Gamepad gamepad)
    {
        if (mode == PlayerInputMode.KeyboardMouse)
            return "Keyboard&Mouse";

        return gamepad != null ? gamepad.displayName : "brak pada";
    }
}
