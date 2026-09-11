using System.Text;

/// <summary>
/// Teksty kart talentów — testowalne formatowanie (M9.0).
/// </summary>
public static class TalentCardCopy
{
    public static string FormatTags(TalentTag[] tags)
    {
        if (tags == null || tags.Length == 0)
            return string.Empty;

        var parts = new StringBuilder();
        foreach (var tag in tags)
        {
            var label = TagLabel(tag);
            if (string.IsNullOrEmpty(label))
                continue;

            if (parts.Length > 0)
                parts.Append(" · ");
            parts.Append(label);
        }

        return parts.ToString();
    }

    public static string FormatCard(TalentDefinition talent)
    {
        if (talent == null)
            return string.Empty;

        var granted = talent.Effect != null && talent.Effect.Operation == TalentEffectOp.GrantSkill
            ? talent.Effect.GrantedSkill
            : null;
        var activation = FormatActivationLabel(talent, granted);
        var tagsLine = FormatTags(talent.Tags);
        var body = talent.Description ?? string.Empty;

        if (string.IsNullOrEmpty(activation))
        {
            if (string.IsNullOrEmpty(tagsLine))
                return $"{talent.DisplayName}\n{body}".Trim();

            return $"{talent.DisplayName}\n{tagsLine}\n{body}".Trim();
        }

        if (string.IsNullOrEmpty(tagsLine))
            return $"{talent.DisplayName}\n{body}\n{activation}".Trim();

        return $"{talent.DisplayName}\n{tagsLine}\n{body}\n{activation}".Trim();
    }

    public static string FormatActivationLabel(TalentDefinition talent, SkillDefinition grantedOrNull)
    {
        if (talent == null)
            return string.Empty;

        if (talent.TalentId == "pudzian_piekielna_aura" || talent.TalentId == "pudzian_nie_zabijecie_mnie")
            return "Pasywne";

        if (grantedOrNull != null)
        {
            if (grantedOrNull.ActivationMode == SkillActivationMode.Passive)
                return "Pasywne";
            if (talent.RequiredTeamLevel >= 4)
                return "Aktywne";
            return string.Empty;
        }

        if (talent.Effect != null && talent.Effect.Operation == TalentEffectOp.GrantSkill)
        {
            var skill = talent.Effect.GrantedSkill;
            if (skill == null)
                return string.Empty;
            if (skill.ActivationMode == SkillActivationMode.Passive)
                return "Pasywne";
            if (talent.RequiredTeamLevel >= 4)
                return "Aktywne";
        }

        return string.Empty;
    }

    private static string TagLabel(TalentTag tag)
    {
        return tag switch
        {
            TalentTag.Aoe => "AOE",
            TalentTag.Mobility => "MOBILITY",
            TalentTag.Taunt => "TAUNT",
            TalentTag.Heal => "HEAL",
            TalentTag.Control => "CONTROL",
            TalentTag.Collision => "COLLISION",
            TalentTag.Berserker => "BERSERKER",
            TalentTag.Risk => "RISK",
            TalentTag.Tank => "TANK",
            TalentTag.Defence => "DEFENCE",
            TalentTag.Zone => "ZONE",
            TalentTag.Damage => "DAMAGE",
            TalentTag.Reflect => "REFLECT",
            TalentTag.Kolos => "KOLOS",
            TalentTag.Trzesienie => "TRZĘSIENIE",
            TalentTag.Aura => "AURA",
            TalentTag.OstatniaSzansa => "OSTATNIA SZANSA",
            _ => string.Empty
        };
    }
}
