using UnityEngine;

/// <summary>
/// Tymczasowa odporność castera na knockback i stagger — stun nadal przechodzi (M7.6-T1).
/// </summary>
[DisallowMultipleComponent]
public class CrowdControlImmunity : MonoBehaviour
{
    private bool _blocksKnockback;
    private bool _blocksStagger;
    private float _remaining;

    public bool IsActive => _remaining > 0f;
    public bool BlocksKnockback => IsActive && _blocksKnockback;
    public bool BlocksStagger => IsActive && _blocksStagger;

    public void Enable(bool knockback, bool stagger, bool stun, float duration)
    {
        _blocksKnockback = knockback;
        _blocksStagger = stagger;
        // stun flag zapisywany na przyszłość — baseline nie blokuje stuna.
        _ = stun;
        _remaining = Mathf.Max(0f, duration);
    }

    public bool Blocks(ForcedMovementMode mode)
    {
        if (!BlocksKnockback) return false;

        // Charge to lokomocja castera (Byk), nie CC na ciele — immunity go nie blokuje.
        return mode switch
        {
            ForcedMovementMode.Knockback => true,
            ForcedMovementMode.Pull => true,
            ForcedMovementMode.Shove => true,
            ForcedMovementMode.Launch => true,
            _ => false
        };
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    /// <summary>Test helper — symuluje wygaśnięcie bez Update.</summary>
    public void Tick(float deltaTime)
    {
        if (_remaining <= 0f) return;
        _remaining -= deltaTime;
        if (_remaining < 0f)
            _remaining = 0f;
    }
}
