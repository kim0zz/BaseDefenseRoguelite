using UnityEngine;

/// <summary>
/// Odrzut XZ — EnemyController honoruje, gdy velocity jest aktywne.
/// M7.5: deleguje do ForcedMovementResolver; zachowany dla kompatybilności AI (IsActive).
/// </summary>
[DisallowMultipleComponent]
public class KnockbackReceiver : MonoBehaviour
{
    private ForcedMovementReceiver _forcedMovement;

    public bool IsActive
    {
        get
        {
            EnsureForcedMovement();
            return _forcedMovement != null && _forcedMovement.IsActive;
        }
    }

    public void AddFromOrigin(Vector3 origin, float force, GameObject source = null, bool restoreToLaneAfter = true)
    {
        if (force <= 0f) return;

        // TECH DEBT (M7.5): melee atak podstawowy nadal woła KnockbackReceiver.AddFromOrigin;
        // odrzut idzie przez wspólny resolver. Pełna migracja AI na ForcedMovementReceiver bezpośrednio — osobny ticket.
        var request = ForcedMovementRequest.KnockbackFromOrigin(
            source != null ? source : gameObject,
            origin,
            force,
            restoreToLaneAfter);
        ForcedMovementResolver.Apply(request, gameObject);
    }

    private void Awake()
    {
        EnsureForcedMovement();
    }

    private void EnsureForcedMovement()
    {
        if (_forcedMovement != null) return;
        _forcedMovement = GetComponent<ForcedMovementReceiver>();
        if (_forcedMovement == null)
            _forcedMovement = gameObject.AddComponent<ForcedMovementReceiver>();
    }
}
