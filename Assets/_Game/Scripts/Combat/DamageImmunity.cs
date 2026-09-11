using UnityEngine;

/// <summary>
/// Blocks HP loss while active (e.g. leap i-frames). Generic, not class-specific.
/// </summary>
[DisallowMultipleComponent]
public class DamageImmunity : MonoBehaviour
{
    private int _activeCount;

    public bool IsActive => _activeCount > 0;

    public void Push() => _activeCount++;
    public void Pop() => _activeCount = Mathf.Max(0, _activeCount - 1);

    public void SetActive(bool active)
    {
        _activeCount = active ? 1 : 0;
    }
}
