using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nalot pasem wzdłuż aim — telegraph + sekwencja eksplozji (M8.4).
/// </summary>
public static class SkillAimStripBurstExecutor
{
    public struct StripResult
    {
        public int ExplosionCount;
        public int UniqueTargetsHit;
        public bool IncludedPlayer;
    }

    public static StripResult Execute(
        SkillDefinition skill,
        GameObject owner,
        Vector3 origin,
        Vector3 aimDirection,
        LayerMask targetLayers)
    {
        var result = new StripResult();
        if (skill == null || owner == null) return result;

        var dir = aimDirection.sqrMagnitude > 0.01f ? aimDirection.normalized : Vector3.forward;
        dir.y = 0f;
        var count = skill.MaxTargets > 0 ? skill.MaxTargets : DeployableTuning.AirstrikeBurstCount;
        var spacing = DeployableTuning.AirstrikeSpacing;
        var damage = skill.Damage > 0f ? skill.Damage : DeployableTuning.AirstrikeDamage;
        var radius = skill.RadiusMeters > 0f ? skill.RadiusMeters : DeployableTuning.AirstrikeBlastRadius;
        var persistents = owner.GetComponent<PlayerPersistentEffects>();
        var uniqueTargets = new HashSet<GameObject>();
        var startOffset = DeployableTuning.PlaceOffsetMeters;

        for (var i = 0; i < count; i++)
        {
            var pos = origin + dir * (startOffset + spacing * i);
            pos.y = origin.y;

            var hits = SkillCircleOverlap.Query(pos, radius, 32, targetLayers, 0);
            foreach (var hit in hits)
            {
                if (hit.GameObject.GetComponentInParent<PlayerCharacter>() != null)
                {
                    result.IncludedPlayer = true;
                    continue;
                }

                uniqueTargets.Add(hit.GameObject);
            }

            var delayedPlace = persistents != null
                && persistents.Has(PersistentEffectKind.AirstrikeLeavesNormals)
                && IsDelayedPlaceIndex(i);

            if (delayedPlace)
            {
                Deployable.SpawnPlaceholder(
                    pos,
                    owner,
                    DeployableCategory.Normal,
                    DeployableTuning.BombDamage,
                    DeployableTuning.BombRadius,
                    DeployableTuning.BombBossStagger,
                    DeployableTuning.BombMaxTargets,
                    detonatable: true);
            }
            else
            {
                ExplosionResolver.Explode(
                    pos,
                    radius,
                    damage,
                    owner,
                    skill.MaxTargets > 0 ? skill.MaxTargets : 8,
                    skill.BossStaggerContribution,
                    canApplyBurn: true,
                    generation: 0,
                    category: DeployableCategory.Strike);
            }

            result.ExplosionCount++;
        }

        result.UniqueTargetsHit = uniqueTargets.Count;

        if (persistents != null && persistents.Has(PersistentEffectKind.AirstrikeFinisher))
            SpawnFinisher(owner, origin, dir, spacing, count, uniqueTargets.Count, persistents);

        return result;
    }

    private static bool IsDelayedPlaceIndex(int index)
    {
        return index == 1 || index == 3 || index == 5;
    }

    private static void SpawnFinisher(
        GameObject owner,
        Vector3 origin,
        Vector3 dir,
        float spacing,
        int count,
        int uniqueHitCount,
        PlayerPersistentEffects persistents)
    {
        var capped = Mathf.Min(uniqueHitCount, DeployableTuning.AirstrikeFinisherUniqueCap);
        var damage = DeployableTuning.AirstrikeFinisherBaseDamage
            + capped * DeployableTuning.AirstrikeFinisherBonusDamagePerUnique;
        var radius = DeployableTuning.AirstrikeFinisherBaseRadius
            + capped * DeployableTuning.AirstrikeFinisherBonusRadiusPerUnique;

        var pos = origin + dir * (DeployableTuning.PlaceOffsetMeters + spacing * count);
        ExplosionResolver.Explode(
            pos,
            radius,
            damage,
            owner,
            16,
            0f,
            canApplyBurn: true,
            generation: 0,
            category: DeployableCategory.Strike);
    }
}
