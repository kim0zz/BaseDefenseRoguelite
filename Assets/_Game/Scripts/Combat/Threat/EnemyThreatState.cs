using UnityEngine;

/// <summary>
/// Stan prowokacji na wrogu — last-write-wins, wygasa po czasie lub utracie tauntera.
/// </summary>
[DisallowMultipleComponent]
public class EnemyThreatState : MonoBehaviour
{
    private PlayerCharacter _taunter;
    private float _remaining;

    public bool IsActive => _remaining > 0f && IsTaunterValid();
    public PlayerCharacter Taunter => _taunter;

    public bool TryApply(PlayerCharacter taunter, float duration)
    {
        if (taunter == null || duration <= 0f) return false;

        _taunter = taunter;
        _remaining = duration;
        return true;
    }

    public void Tick(float deltaTime)
    {
        if (_remaining <= 0f) return;

        if (!IsTaunterValid())
        {
            Clear();
            return;
        }

        _remaining -= deltaTime;
        if (_remaining <= 0f)
            Clear();
    }

    private bool IsTaunterValid()
    {
        return _taunter != null && _taunter.IsCombatEnabled;
    }

    private void Clear()
    {
        _remaining = 0f;
        _taunter = null;
    }
}
