using UnityEngine;

/// <summary>
/// Definicja przedmiotu MVP (M6) — efekt stat/modifier.
/// </summary>
[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Game/Build/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] private string itemId = "blood_charm";
    [SerializeField] private string displayName = "Blood Charm";
    [SerializeField] private LootRarity rarity = LootRarity.Common;
    [SerializeField] private StatModifiers statModifiers;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public LootRarity Rarity => rarity;
    public StatModifiers StatModifiers => statModifiers;
}
