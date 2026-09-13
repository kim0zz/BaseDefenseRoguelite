using UnityEngine;

/// <summary>
/// Szerokie kopnięcie przed casterem — odpycha wrogów i wystrzeliwuje ładunki w tym samym obszarze.
/// </summary>
public static class DeployableKickAreaResolver
{
    public static bool Contains(Vector3 origin, Vector3 direction, float width, float length, Vector3 point)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.forward;
        direction.Normalize();

        var local = point - origin;
        local.y = 0f;
        var along = Vector3.Dot(local, direction);
        if (along < -0.25f || along > length + 0.5f) return false;

        var lateral = (local - direction * along).magnitude;
        return lateral <= width * 0.5f + 0.35f;
    }

    public static int Execute(
        SkillDefinition skill,
        GameObject caster,
        Vector3 origin,
        Vector3 aim,
        LayerMask targetLayers)
    {
        if (skill == null || caster == null) return 0;

        aim.y = 0f;
        if (aim.sqrMagnitude < 0.01f) aim = Vector3.forward;
        aim = aim.normalized;

        var width = skill.CapsuleWidthMeters > 0f ? skill.CapsuleWidthMeters : DeployableTuning.KickCapsuleWidth;
        var length = skill.CapsuleLengthMeters > 0f ? skill.CapsuleLengthMeters : DeployableTuning.KickCapsuleLength;
        var kickSpeed = skill.ChargeSpeed > 0f ? skill.ChargeSpeed : DeployableTuning.KickSpeed;
        var kickDistance = skill.ChargeRangeMeters > 0f ? skill.ChargeRangeMeters : DeployableTuning.KickMaxDistance;

        var enemyHits = SkillChargeCapsuleOverlap.Query(
            origin, aim, width, length, skill.MaxTargets > 0 ? skill.MaxTargets : 8, targetLayers);

        foreach (var hit in enemyHits)
        {
            if (hit.GameObject == null) continue;
            if (hit.GameObject.GetComponentInParent<PlayerCharacter>() != null) continue;

            var shove = ForcedMovementRequest.ShoveAlong(
                caster,
                aim,
                DeployableTuning.KickShoveSpeed,
                DeployableTuning.KickShoveDuration,
                honorCollisions: true);
            ForcedMovementResolver.Apply(shove, hit.GameObject);
        }

        var persistents = caster.GetComponent<PlayerPersistentEffects>();
        var searchRange = Mathf.Max(width, length) + 1.5f;
        var bombs = DeployableRegistry.FindAllInRange(
            caster,
            origin,
            searchRange,
            IsKickable);

        var kickedIds = new System.Collections.Generic.HashSet<Deployable>();
        var host = caster.GetComponent<OrbitingDeployableHost>();
        foreach (var deployable in bombs)
        {
            if (deployable == null || deployable.IsDetonated) continue;
            if (!Contains(origin, aim, width, length, deployable.transform.position)) continue;
            Launch(deployable, aim, kickSpeed, kickDistance, host);
            kickedIds.Add(deployable);
        }

        if (persistents != null && persistents.Has(PersistentEffectKind.MultiLaunchOwned))
        {
            var extra = DeployableRegistry.FindAllInRange(
                caster,
                origin,
                DeployableTuning.MultiLaunchPickRange,
                IsKickable);
            foreach (var deployable in extra)
            {
                if (deployable == null || deployable.IsDetonated || kickedIds.Contains(deployable)) continue;
                Launch(deployable, aim, kickSpeed, kickDistance, host);
                kickedIds.Add(deployable);
            }
        }

        return kickedIds.Count + enemyHits.Count;
    }

    private static bool IsKickable(Deployable d)
    {
        if (d == null || d.IsDetonated) return false;
        return d.Category == DeployableCategory.Normal
            || d.Category == DeployableCategory.Child
            || d.Category == DeployableCategory.Orbital
            || d.Category == DeployableCategory.DashCharge;
    }

    private static void Launch(
        Deployable deployable,
        Vector3 aim,
        float speed,
        float distance,
        OrbitingDeployableHost host)
    {
        if (host != null && host.TryKick(deployable, aim))
            return;
        deployable.Motor.Kick(aim, speed, distance);
    }
}
