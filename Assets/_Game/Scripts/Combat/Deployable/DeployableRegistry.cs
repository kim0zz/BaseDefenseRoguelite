using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rejestr owned deployables — cap FIFO, detonacja, place API (M8.4).
/// </summary>
[DisallowMultipleComponent]
public class DeployableRegistry : MonoBehaviour, IDeployableHudInfo
{
    private static DeployableRegistry _instance;

    private readonly Dictionary<GameObject, OwnerState> _owners = new();

    private class OwnerState
    {
        public readonly List<Deployable> Normals = new();
        public readonly List<Deployable> All = new();
        public int Cap = DeployableTuning.DefaultNormalCap;
        public bool SubscribedDeath;
    }

    public static DeployableRegistry Ensure()
    {
        if (_instance != null) return _instance;
        _instance = FindAnyObjectByType<DeployableRegistry>();
        if (_instance != null) return _instance;

        var go = new GameObject("DeployableRegistry");
        _instance = go.AddComponent<DeployableRegistry>();
        return _instance;
    }

    public static void ResetForTests()
    {
        if (_instance != null)
            DestroyImmediate(_instance.gameObject);
        _instance = null;
    }

    public static void Register(Deployable d)
    {
        if (d == null || d.Owner == null) return;
        Ensure();
        var state = _instance.GetOrCreateState(d.Owner);

        if (d.Category == DeployableCategory.Normal)
        {
            while (state.Normals.Count >= state.Cap)
            {
                var oldest = state.Normals[0];
                state.Normals.RemoveAt(0);
                if (oldest != null && !oldest.IsDetonated)
                    oldest.Detonate();
            }

            state.Normals.Add(d);
        }

        state.All.Add(d);
        _instance.EnsureDeathSubscription(d.Owner, state);
    }

    public static void Unregister(Deployable d)
    {
        if (_instance == null || d == null || d.Owner == null) return;
        if (!_instance._owners.TryGetValue(d.Owner, out var state)) return;

        state.All.Remove(d);
        state.Normals.Remove(d);
    }

    public static void SetCap(GameObject owner, int cap)
    {
        if (owner == null) return;
        Ensure();
        var state = _instance.GetOrCreateState(owner);
        state.Cap = Mathf.Max(1, cap);
    }

    public static int GetCap(GameObject owner)
    {
        if (owner == null) return DeployableTuning.DefaultNormalCap;
        Ensure();
        return _instance.GetOrCreateState(owner).Cap;
    }

    public static int GetNormalCount(GameObject owner)
    {
        if (owner == null) return 0;
        Ensure();
        return _instance.GetOrCreateState(owner).Normals.Count;
    }

    int IDeployableHudInfo.GetNormalCount(GameObject owner) => GetNormalCount(owner);

    int IDeployableHudInfo.GetCap(GameObject owner) => GetCap(owner);

    int IDeployableHudInfo.GetOrbitalReadyCount(GameObject owner)
    {
        if (owner == null) return 0;
        var host = owner.GetComponent<OrbitingDeployableHost>();
        return host != null ? host.ReadyCount : 0;
    }

    int IDeployableHudInfo.GetOrbitalTotal(GameObject owner)
    {
        if (owner == null) return 0;
        var host = owner.GetComponent<OrbitingDeployableHost>();
        return host != null ? host.TotalCount : 0;
    }

    public static void DetonateAllDetonatable(GameObject owner)
    {
        if (owner == null) return;
        Ensure();
        if (!_instance._owners.TryGetValue(owner, out var state)) return;

        var toDetonate = new List<Deployable>();
        foreach (var d in state.All)
        {
            if (d != null && d.Detonatable && !d.IsDetonated)
                toDetonate.Add(d);
        }

        foreach (var d in toDetonate)
            d.Detonate();
    }

    public static Deployable FindNearest(
        GameObject owner,
        Vector3 position,
        float range,
        Func<Deployable, bool> predicate = null)
    {
        if (owner == null) return null;
        Ensure();
        if (!_instance._owners.TryGetValue(owner, out var state)) return null;

        Deployable best = null;
        var bestDist = range * range;
        foreach (var d in state.All)
        {
            if (d == null || d.IsDetonated) continue;
            if (predicate != null && !predicate(d)) continue;
            var dist = (d.transform.position - position).sqrMagnitude;
            if (dist > bestDist) continue;
            bestDist = dist;
            best = d;
        }

        return best;
    }

    public static List<Deployable> FindAllInRange(
        GameObject owner,
        Vector3 position,
        float range,
        Func<Deployable, bool> predicate = null,
        int maxCount = int.MaxValue)
    {
        var results = new List<Deployable>();
        if (owner == null) return results;
        Ensure();
        if (!_instance._owners.TryGetValue(owner, out var state)) return results;

        var rangeSq = range * range;
        foreach (var d in state.All)
        {
            if (d == null || d.IsDetonated) continue;
            if (predicate != null && !predicate(d)) continue;
            if ((d.transform.position - position).sqrMagnitude > rangeSq) continue;
            results.Add(d);
            if (results.Count >= maxCount) break;
        }

        return results;
    }

    public static Deployable Place(GameObject owner, Vector3 position, SkillDefinition skill)
    {
        if (owner == null || skill == null) return null;

        var cap = owner.GetComponent<PlayerPersistentEffects>()?.GetDeployableCap()
            ?? DeployableTuning.DefaultNormalCap;
        SetCap(owner, cap);

        return Deployable.SpawnPlaceholder(
            position,
            owner,
            DeployableCategory.Normal,
            skill.Damage,
            skill.RadiusMeters,
            skill.BossStaggerContribution,
            skill.MaxTargets,
            detonatable: true);
    }

    public static void DespawnAllOwned(GameObject owner)
    {
        if (owner == null) return;
        Ensure();
        if (!_instance._owners.TryGetValue(owner, out var state)) return;

        foreach (var d in state.All)
        {
            if (d != null && d.gameObject != null)
                Destroy(d.gameObject);
        }

        state.All.Clear();
        state.Normals.Clear();
    }

    private OwnerState GetOrCreateState(GameObject owner)
    {
        if (!_owners.TryGetValue(owner, out var state))
        {
            state = new OwnerState();
            _owners[owner] = state;
        }

        return state;
    }

    private void EnsureDeathSubscription(GameObject owner, OwnerState state)
    {
        if (state.SubscribedDeath) return;
        var health = owner.GetComponent<Health>();
        if (health == null) return;
        health.Died += () => DespawnAllOwned(owner);
        state.SubscribedDeath = true;
    }
}
