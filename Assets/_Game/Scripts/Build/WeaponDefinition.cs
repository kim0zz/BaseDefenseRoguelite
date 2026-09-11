using UnityEngine;

/// <summary>
/// Definicja broni MVP (M6) — profil melee + metadane + stat boosty rare.
/// </summary>
[CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Game/Build/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    [SerializeField] private string weaponId = "miecz";
    [SerializeField] private string displayName = "Miecz";
    [SerializeField] private LootRarity rarity = LootRarity.Common;
    [SerializeField] private MeleeWeaponDefinition meleeProfile;
    [SerializeField] private StatModifiers bonusStats;
    [SerializeField] private WeaponFamily family = WeaponFamily.Sword;
    [SerializeField] private bool isBossUnique;

    public string WeaponId => weaponId;
    public string DisplayName => displayName;
    public LootRarity Rarity => rarity;
    public MeleeWeaponDefinition MeleeProfile => meleeProfile;
    public StatModifiers BonusStats => bonusStats;
    public WeaponFamily Family => family;
    public WeaponFeelProfile Feel => WeaponFeelProfile.For(family);
    public bool IsBossUnique => isBossUnique;
}
