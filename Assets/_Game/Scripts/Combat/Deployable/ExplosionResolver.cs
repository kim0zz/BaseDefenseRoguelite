using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wspólny resolve wybuchu — overlap, dmg, burn, cluster, consume hooks (M8.4).
/// </summary>
public static class ExplosionResolver
{
    public static void Explode(
        Vector3 origin,
        float radius,
        float damage,
        GameObject owner,
        int maxTargets,
        float bossStagger,
        bool canApplyBurn,
        float? stunSeconds = null,
        int generation = 0,
        DeployableCategory category = DeployableCategory.Normal,
        bool applyPerTargetStacks = false)
    {
        if (owner == null || radius <= 0f) return;

        var persistents = owner.GetComponent<PlayerPersistentEffects>();
        var hits = SkillCircleOverlap.Query(origin, radius, maxTargets, ~0, 0);

        var uniqueTargets = new HashSet<GameObject>();
        foreach (var hit in hits)
        {
            if (hit.GameObject == null) continue;
            if (hit.GameObject.GetComponentInParent<PlayerCharacter>() != null) continue;

            var targetGo = hit.Damageable is Component c ? c.gameObject : hit.GameObject;
            uniqueTargets.Add(targetGo);

            var finalDamage = damage;
            if (applyPerTargetStacks && persistents != null)
                finalDamage = persistents.ApplyPerTargetHitMultiplier(targetGo, finalDamage);

            hit.Damageable?.TakeDamage(finalDamage, owner);
            hit.GameObject.GetComponentInParent<HitFlashFeedback>()?.PlayFlash();

            if (canApplyBurn && persistents != null && persistents.Has(PersistentEffectKind.ApplyBurnOnPlayerDamage))
            {
                var status = hit.GameObject.GetComponentInParent<StatusEffectReceiver>();
                if (status != null)
                {
                    status.Apply(
                        StatusEffectType.Burn,
                        persistents.GetBurnDuration(),
                        persistents.GetBurnTickDamage(),
                        owner);
                }
            }

            if (stunSeconds.HasValue && stunSeconds.Value > 0f)
            {
                var profile = hit.GameObject.GetComponentInParent<ForcedMovementResistanceProfile>();
                var cat = profile != null ? profile.Category : ForcedMovementResistanceCategory.Normal;
                var stun = cat switch
                {
                    ForcedMovementResistanceCategory.Elite => DeployableTuning.KickExplodeStunElite,
                    ForcedMovementResistanceCategory.Boss => DeployableTuning.KickExplodeStunBoss,
                    _ => stunSeconds.Value
                };
                if (stun > 0f)
                {
                    var req = ForcedMovementRequest.StunControl(owner, stun, bossStagger);
                    ForcedMovementResolver.Apply(req, hit.GameObject);
                }
                else if (cat == ForcedMovementResistanceCategory.Boss && bossStagger > 0f)
                {
                    hit.GameObject.GetComponentInParent<StatusEffectReceiver>()?.AddStagger(bossStagger);
                }
            }
            else if (bossStagger > 0f)
            {
                hit.GameObject.GetComponentInParent<StatusEffectReceiver>()?.AddStagger(bossStagger);
            }

            if (canApplyBurn && persistents != null)
                persistents.TryConsumeBurnOnHit(hit.GameObject, origin, radius);
        }

        if (persistents != null)
            persistents.TryPhoenixOnBurnKill(owner, origin, generation);

        if (generation == 0 && persistents != null && persistents.Has(PersistentEffectKind.ClusterOnExplode))
            SpawnClusterChildren(origin, owner, persistents, category);
    }

    private static void SpawnClusterChildren(
        Vector3 origin, GameObject owner, PlayerPersistentEffects persistents, DeployableCategory parentCategory)
    {
        var inheritsHoming = persistents.Has(PersistentEffectKind.ChildHoming);
        var orbitalCluster = parentCategory == DeployableCategory.Orbital
            && persistents.Has(PersistentEffectKind.OrbitalInheritsCluster);

        if (parentCategory == DeployableCategory.Orbital && !orbitalCluster)
            return;

        for (var i = 0; i < DeployableTuning.ClusterChildCount; i++)
        {
            var angle = (360f / DeployableTuning.ClusterChildCount) * i + Random.Range(0f, 45f);
            var dist = Random.Range(DeployableTuning.ClusterScatterMin, DeployableTuning.ClusterScatterMax);
            var offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * dist;
            var pos = origin + offset;
            pos.y = origin.y;

            var child = Deployable.SpawnPlaceholder(
                pos,
                owner,
                DeployableCategory.Child,
                DeployableTuning.ClusterChildDamage,
                DeployableTuning.ClusterChildRadius,
                0f,
                5,
                detonatable: true,
                generation: DeployableTuning.ClusterGeneration);

            child.SetFuse(DeployableTuning.ClusterChildFuse);
            if (inheritsHoming)
                child.Motor.BeginHoming();
        }
    }
}
