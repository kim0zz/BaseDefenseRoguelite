using System;
using UnityEngine;

/// <summary>
/// Niezależne sloty orbitalne wokół właściciela (M8.4).
/// </summary>
public class OrbitingSlotSet
{
    private readonly GameObject _owner;
    private readonly PlayerPersistentEffects _persistents;
    private readonly Deployable[] _slots;
    private readonly float[] _rechargeTimers;
    private readonly float[] _angles;
    private float _orbitRadius;
    private float _degreesPerSecond;
    private int _slotCount;

    public OrbitingSlotSet(GameObject owner, PlayerPersistentEffects persistents)
    {
        _owner = owner;
        _persistents = persistents;
        _slotCount = DeployableTuning.OrbitalSlotCount;
        _orbitRadius = DeployableTuning.OrbitalRadius;
        _degreesPerSecond = DeployableTuning.OrbitalDegreesPerSecond;

        if (persistents != null && persistents.Has(PersistentEffectKind.PlanetaryOrbitUpgrade))
        {
            _slotCount = DeployableTuning.PlanetarySlotCount;
            _orbitRadius = DeployableTuning.PlanetaryRadius;
            _degreesPerSecond = DeployableTuning.PlanetaryDegreesPerSecond;
        }

        _slots = new Deployable[_slotCount];
        _rechargeTimers = new float[_slotCount];
        _angles = new float[_slotCount];
        for (var i = 0; i < _slotCount; i++)
        {
            _angles[i] = (360f / _slotCount) * i;
            SpawnOrbital(i);
        }
    }

    public int ReadyCount
    {
        get
        {
            var ready = 0;
            for (var i = 0; i < _slotCount; i++)
            {
                if (_rechargeTimers[i] <= 0f && _slots[i] != null && !_slots[i].IsDetonated)
                    ready++;
            }

            return ready;
        }
    }

    public int SlotCount => _slotCount;
    public float GetRechargeTimer(int index) => _rechargeTimers[index];

    public void Tick(float deltaTime)
    {
        if (_owner == null) return;

        var center = _owner.transform.position;
        center.y = 0.5f;

        for (var i = 0; i < _slotCount; i++)
        {
            if (_rechargeTimers[i] > 0f)
            {
                _rechargeTimers[i] -= deltaTime;
                if (_rechargeTimers[i] <= 0f)
                    SpawnOrbital(i);
                continue;
            }

            if (_slots[i] == null || _slots[i].IsDetonated)
            {
                _rechargeTimers[i] = DeployableTuning.OrbitalRechargeSeconds;
                continue;
            }

            _angles[i] += _degreesPerSecond * deltaTime;
            var rad = _angles[i] * Mathf.Deg2Rad;
            var offset = new Vector3(Mathf.Sin(rad) * _orbitRadius, 0f, Mathf.Cos(rad) * _orbitRadius);
            _slots[i].transform.position = center + offset;

            if (TryOrbitalContact(_slots[i]))
            {
                var explodedIndex = i;
                _slots[i].Detonated += OnOrbitalDetonated;
                _slots[i].Detonate();
                _slots[i] = null;
                _rechargeTimers[explodedIndex] = DeployableTuning.OrbitalRechargeSeconds;
                ApplyRechargeReduction(explodedIndex);
            }
        }
    }

    private void SpawnOrbital(int index)
    {
        if (_owner == null) return;
        var center = _owner.transform.position;
        var rad = _angles[index] * Mathf.Deg2Rad;
        var pos = center + new Vector3(Mathf.Sin(rad) * _orbitRadius, 0.5f, Mathf.Cos(rad) * _orbitRadius);

        _slots[index] = Deployable.SpawnPlaceholder(
            pos,
            _owner,
            DeployableCategory.Orbital,
            DeployableTuning.OrbitalDamage,
            DeployableTuning.OrbitalBlastRadius,
            0f,
            5,
            detonatable: false,
            color: new Color(0.3f, 0.7f, 1f));
        _slots[index].Motor.SetOrbiting();
        _rechargeTimers[index] = 0f;
    }

    private bool TryOrbitalContact(Deployable orbital)
    {
        var hits = Physics.OverlapSphere(orbital.transform.position, DeployableTuning.OrbitalContactRadius);
        foreach (var hit in hits)
        {
            if (hit.GetComponentInParent<PlayerCharacter>() != null) continue;
            var enemy = hit.GetComponentInParent<EnemyController>();
            if (enemy == null) continue;
            var dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg != null && dmg.IsAlive) return true;
        }

        return false;
    }

    private void OnOrbitalDetonated(Deployable d) { }

    private void ApplyRechargeReduction(int explodedIndex)
    {
        if (_persistents == null || !_persistents.Has(PersistentEffectKind.OrbitalRechargeOnExplode))
            return;

        for (var i = 0; i < _slotCount; i++)
        {
            if (i == explodedIndex) continue;
            if (_rechargeTimers[i] <= 0f) continue;
            _rechargeTimers[i] = Mathf.Max(
                0f,
                _rechargeTimers[i] - DeployableTuning.OrbitalRechargeReductionSeconds);
        }
    }

    public Deployable GetSlot(int index) =>
        index >= 0 && index < _slotCount ? _slots[index] : null;

    public void SetRechargeTimerForTests(int index, float seconds)
    {
        if (index >= 0 && index < _slotCount)
            _rechargeTimers[index] = seconds;
    }

    public void TriggerRechargeReductionForTests(int explodedIndex) =>
        ApplyRechargeReduction(explodedIndex);

    public void KickOrbital(int index, Vector3 direction)
    {
        if (index < 0 || index >= _slotCount || _slots[index] == null) return;
        _slots[index].Motor.Kick(direction);
        _slots[index] = null;
        _rechargeTimers[index] = DeployableTuning.OrbitalRechargeSeconds;
    }

    public bool TryKickDeployable(Deployable deployable, Vector3 direction)
    {
        if (deployable == null) return false;
        for (var i = 0; i < _slotCount; i++)
        {
            if (_slots[i] != deployable) continue;
            KickOrbital(i, direction);
            return true;
        }

        return false;
    }
}
