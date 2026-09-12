using UnityEngine;

/// <summary>
/// Tickuje orbitujące ładunki na graczu po wzięciu ulti Orbitale (M8.4).
/// </summary>
[DisallowMultipleComponent]
public class OrbitingDeployableHost : MonoBehaviour
{
    private OrbitingSlotSet _set;
    private PlayerPersistentEffects _persistents;

    public int ReadyCount => _set != null ? _set.ReadyCount : 0;
    public int TotalCount => _set != null ? _set.SlotCount : 0;

    public void EnsureStarted()
    {
        if (_persistents == null)
            _persistents = GetComponent<PlayerPersistentEffects>();
        _set ??= new OrbitingSlotSet(gameObject, _persistents);
        enabled = true;
    }

    public void StopAndClear()
    {
        _set = null;
        enabled = false;
    }

    public bool TryKick(Deployable deployable, Vector3 direction) =>
        _set != null && _set.TryKickDeployable(deployable, direction);

    private void Update()
    {
        _set?.Tick(Time.deltaTime);
    }
}
