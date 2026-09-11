using UnityEngine;

/// <summary>
/// Zarządza dwoma slotami broni i przełączaniem aktywnej (M6).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerCharacter))]
[RequireComponent(typeof(PlayerBuildState))]
public class PlayerWeaponController : MonoBehaviour
{
    private PlayerCharacter _player;
    private PlayerBuildState _build;

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _build = GetComponent<PlayerBuildState>();
    }

    private void Update()
    {
        if (_build == null || _player == null) return;
        if (!_player.IsCombatEnabled) return;
        if (!LootLoopConfig.LootEnabledInRun) return;
        if (!_player.TryReadWeaponSwapInput()) return;

        var attack = GetComponent<PlayerAttackController>();
        if (attack != null)
            attack.QueueWeaponSwap();
        else
            _build.CycleActiveWeapon();

        Debug.Log($"[PlayerWeaponController] P{_player.PlayerIndex + 1} — żądanie zmiany aktywnej broni.");
    }
}
