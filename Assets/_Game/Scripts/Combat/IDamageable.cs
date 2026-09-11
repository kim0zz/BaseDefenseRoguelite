using UnityEngine;

/// <summary>
/// Cel mogący otrzymać obrażenia (gracz, wróg, struktura).
/// </summary>
public interface IDamageable
{
    bool IsAlive { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }
    void TakeDamage(float amount, GameObject source = null);
}
