using UnityEngine;

/// <summary>
/// Dane placeholderowej broni melee (M3 — jeden miecz dla wszystkich graczy).
/// Wartości z docs/balance/BASELINE_VALUES.md (benchmark miecza).
/// </summary>
[CreateAssetMenu(fileName = "MeleeWeapon", menuName = "Game/Combat/Melee Weapon")]
public class MeleeWeaponDefinition : ScriptableObject
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackInterval = 0.8f;
    [SerializeField] private float range = 2f;
    [SerializeField] private float arcDegrees = 120f;
    [SerializeField] private int maxTargets = 3;
    [SerializeField] private AttackComboDefinition comboProfile;
    [SerializeField] private float splashRadius;
    [SerializeField] private float arcHeight;
    [SerializeField] private ProjectileImpactMode impactMode = ProjectileImpactMode.Direct;

    public float Damage => damage;
    public float AttackInterval => attackInterval;
    public float Range => range;
    public float ArcDegrees => arcDegrees;
    public int MaxTargets => maxTargets;
    public AttackComboDefinition ComboProfile => comboProfile;
    public bool HasCombo => comboProfile != null && comboProfile.HitCount > 0;
    public float SplashRadius => splashRadius;
    public float ArcHeight => arcHeight;
    public ProjectileImpactMode ImpactMode => impactMode;
}
