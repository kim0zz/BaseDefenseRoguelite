using UnityEngine;

/// <summary>
/// Definicja klasy gracza MVP (M6).
/// </summary>
[CreateAssetMenu(fileName = "ClassDefinition", menuName = "Game/Build/Class Definition")]
public class ClassDefinition : ScriptableObject
{
    [SerializeField] private PlayerClassId classId = PlayerClassId.Jamie;
    [SerializeField] private string displayName = "Jamie";
    [SerializeField] private WeaponDefinition startingWeaponPrimary;
    [SerializeField] private WeaponDefinition startingWeaponSecondary;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float maxHealthPerTeamLevel = 8f;
    [SerializeField] private float moveSpeedPerTeamLevel = 0.05f;

    public PlayerClassId ClassId => classId;
    public string DisplayName => displayName;
    public WeaponDefinition StartingWeaponPrimary => startingWeaponPrimary;
    public WeaponDefinition StartingWeaponSecondary => startingWeaponSecondary;
    public float BaseMaxHealth => baseMaxHealth;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float MaxHealthPerTeamLevel => maxHealthPerTeamLevel;
    public float MoveSpeedPerTeamLevel => moveSpeedPerTeamLevel;
}
