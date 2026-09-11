/// <summary>
/// Etykiety rarity i talentów do UI buildów (M6/M7).
/// </summary>
public static class LootRarityUi
{
    public static string FormatName(string displayName, LootRarity rarity, bool isBossUnique = false)
    {
        if (string.IsNullOrEmpty(displayName))
            return "-";

        if (isBossUnique || rarity == LootRarity.Unique)
        {
            return $"<color=#ffaa33>{displayName} (unikat bossa)</color>";
        }

        var color = rarity == LootRarity.Rare ? "#bb66ff" : "#88ccff";
        var tag = rarity == LootRarity.Rare ? "rzadka" : "zwykła";
        return $"<color={color}>{displayName} ({tag})</color>";
    }

    public static string FormatTalentOption(TalentDefinition talent)
    {
        return TalentCardCopy.FormatCard(talent);
    }
}
