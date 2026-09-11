using UnityEngine;

/// <summary>
/// Trzy fale wzdłuż lane pathów (M8.1b Trzęsienie Świata).
/// </summary>
public static class SkillLaneWaveExecutor
{
    public const float WaveSpeed = 14f;
    public const float WaveWidth = 3.2f;

    public static int CountValidLanePaths()
    {
        var count = 0;
        foreach (AttackLineId line in System.Enum.GetValues(typeof(AttackLineId)))
        {
            var path = MapGreyboxLayout.GetLanePath(line);
            if (path.Waypoints != null && path.Waypoints.Length >= 2)
                count++;
        }

        return count;
    }

    public static void Execute(SkillDefinition skill, GameObject caster, LayerMask targetLayers)
    {
        if (skill == null || caster == null) return;

        var persistents = caster.GetComponent<PlayerPersistentEffects>();
        var castState = new LaneWaveCastState
        {
            HasKrwawaSejsmika = persistents != null && persistents.Has(PersistentEffectKind.KrwawaSejsmika),
            KrwawaPerHit = persistents != null
                ? persistents.GetTuningFloatPublic(PersistentEffectKind.KrwawaSejsmika, 0, 4f)
                : 4f,
            KrwawaCap = persistents != null
                ? persistents.GetTuningFloatPublic(PersistentEffectKind.KrwawaSejsmika, 1, 32f)
                : 32f
        };

        var hasAftershock = persistents != null && persistents.Has(PersistentEffectKind.AftershockWaves);
        var hasCracks = persistents != null && persistents.Has(PersistentEffectKind.GroundCracks);
        var afterDelay = persistents != null
            ? persistents.GetTuningFloatPublic(PersistentEffectKind.AftershockWaves, 0, 0.7f)
            : 0.7f;
        var afterDmgMul = persistents != null
            ? persistents.GetTuningFloatPublic(PersistentEffectKind.AftershockWaves, 1, 0.5f)
            : 0.5f;
        var afterStaggerMul = persistents != null
            ? persistents.GetTuningFloatPublic(PersistentEffectKind.AftershockWaves, 2, 0.7f)
            : 0.7f;
        var crackDuration = persistents != null
            ? persistents.GetTuningFloatPublic(PersistentEffectKind.GroundCracks, 0, 4f)
            : 4f;
        var crackPerTick = persistents != null
            ? persistents.GetTuningFloatPublic(PersistentEffectKind.GroundCracks, 1, 3f)
            : 3f;
        var crackTick = persistents != null
            ? persistents.GetTuningFloatPublic(PersistentEffectKind.GroundCracks, 2, 0.5f)
            : 0.5f;
        var crackDps = crackTick > 0f ? crackPerTick / crackTick : 6f;
        var staggerMul = 1f;
        var bossStaggerMul = 1f;
        if (persistents != null && persistents.Has(PersistentEffectKind.StaggerBoost))
        {
            staggerMul = persistents.GetTuningFloatPublic(PersistentEffectKind.StaggerBoost, 0, 1.6f);
            bossStaggerMul = persistents.GetTuningFloatPublic(PersistentEffectKind.StaggerBoost, 1, 1.5f);
        }

        foreach (AttackLineId line in System.Enum.GetValues(typeof(AttackLineId)))
        {
            var path = MapGreyboxLayout.GetLanePath(line);
            if (path.Waypoints == null || path.Waypoints.Length < 2) continue;

            SkillLaneWavePulse.Spawn(
                skill, caster, path, targetLayers, 0f, 1f, staggerMul,
                hasCracks, crackDuration, crackDps, castState, bossStaggerMul);

            if (!hasAftershock) continue;

            for (var wave = 0; wave < 2; wave++)
            {
                SkillLaneWavePulse.Spawn(
                    skill, caster, path, targetLayers,
                    afterDelay * (wave + 1),
                    afterDmgMul, afterStaggerMul,
                    false, 0f, 0f, castState);
            }
        }
    }
}
