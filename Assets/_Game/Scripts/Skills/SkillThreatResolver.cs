using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Prowokacja po trafieniu skillem — osobna warstwa od obrażeń (M7.6-T6).
/// </summary>
public static class SkillThreatResolver
{
    public struct ApplyResult
    {
        public int TauntCount;
        public bool HadSuccessfulTaunt;
    }

    public static ApplyResult Apply(
        SkillDefinition definition,
        IReadOnlyList<SkillCircleOverlap.HitTarget> hits,
        PlayerCharacter caster,
        Color markerColor)
    {
        var result = new ApplyResult();
        if (definition == null || !definition.AppliesThreatOverride || hits == null || caster == null)
            return result;

        var duration = definition.ThreatDurationSeconds;
        if (duration <= 0f) return result;

        foreach (var hit in hits)
        {
            if (!SkillTargetFilter.IsValidTarget(hit.GameObject, definition, caster.gameObject))
                continue;

            if (!EnemyThreatService.TryTaunt(hit.GameObject, caster, duration))
                continue;

            TauntMarkerView.Show(hit.GameObject, markerColor, duration);
            result.TauntCount++;
        }

        result.HadSuccessfulTaunt = result.TauntCount > 0;
        return result;
    }
}
