using UnityEngine;

/// <summary>
/// Mnożnik obrażeń przyjmowanych — używany przez Wardena (DR totemów, M8.3).
/// </summary>
[DisallowMultipleComponent]
public class DamageTakenMultiplier : MonoBehaviour
{
    private float _multiplier = 1f;

    public float Multiplier => _multiplier;

    public void SetMultiplier(float multiplier) =>
        _multiplier = Mathf.Max(0f, multiplier);
}
