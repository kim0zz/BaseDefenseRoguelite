using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spychacz — zbiera wrogów przed casterem (M8.1b).
/// </summary>
public class ChargeCarryTracker : MonoBehaviour
{
    public const float DumpDamage = 6f;
    public const float DumpRadius = 1.8f;

    private static readonly Dictionary<GameObject, ChargeCarryTracker> Active = new();
    private readonly List<GameObject> _carried = new();
    private Vector3 _chargeDirection;
    private GameObject _caster;

    public static void Track(GameObject caster, GameObject enemy)
    {
        if (caster == null || enemy == null) return;
        if (!Active.TryGetValue(caster, out var tracker))
        {
            tracker = caster.GetComponent<ChargeCarryTracker>();
            if (tracker == null) tracker = caster.AddComponent<ChargeCarryTracker>();
            Active[caster] = tracker;
        }

        tracker._caster = caster;
        if (!tracker._carried.Contains(enemy))
            tracker._carried.Add(enemy);
    }

    public static void BeginCharge(GameObject caster, Vector3 direction)
    {
        if (caster == null) return;
        var tracker = caster.GetComponent<ChargeCarryTracker>();
        if (tracker == null) tracker = caster.AddComponent<ChargeCarryTracker>();
        tracker._caster = caster;
        tracker._chargeDirection = direction.normalized;
        Active[caster] = tracker;
    }

    public static void EndCharge(GameObject caster)
    {
        if (caster == null) return;
        if (!Active.TryGetValue(caster, out var tracker)) return;
        tracker.Dump();
        Active.Remove(caster);
    }

    private void LateUpdate()
    {
        if (_caster == null || _carried.Count == 0) return;
        var front = _caster.transform.position + _chargeDirection * 1.2f;
        foreach (var enemy in _carried)
        {
            if (enemy == null) continue;
            var receiver = enemy.GetComponent<ForcedMovementReceiver>();
            if (receiver == null) continue;
            var step = ForcedMovementRequest.ShoveAlong(_caster, _chargeDirection, 20f, 0.05f, honorCollisions: true);
            ForcedMovementResolver.Apply(step, enemy);
        }
    }

    private void Dump()
    {
        var center = _caster != null ? _caster.transform.position : transform.position;
        var hits = Physics.OverlapSphere(center + Vector3.up * 0.5f, DumpRadius);
        foreach (var collider in hits)
        {
            if (collider == null) continue;
            var health = collider.GetComponentInParent<Health>();
            if (health == null || !health.IsAlive) continue;
            if (collider.GetComponentInParent<PlayerCharacter>() != null) continue;
            health.TakeDamage(DumpDamage, _caster);
        }

        _carried.Clear();
    }
}
