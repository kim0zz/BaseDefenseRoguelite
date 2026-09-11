using UnityEngine;

/// <summary>
/// Dane bossa Warden — liczby DRAFT w SO (M8.3).
/// </summary>
[CreateAssetMenu(fileName = "WardenDefinition", menuName = "Game/Boss/Warden Definition")]
public class WardenDefinition : ScriptableObject
{
    [SerializeField] private string bossId = "warden";
    [SerializeField] private string displayName = "Warden";
    [SerializeField] private float maxHealth = 700f;
    [SerializeField] private Vector3 bodyScale = new(2.4f, 2.4f, 2.4f);
    [SerializeField] private Color bodyColor = new(0.15f, 0.35f, 0.32f, 1f);
    [SerializeField] private float slamDamage = 16f;
    [SerializeField] private float slamRadius = 4f;
    [SerializeField] private float slamTelegraph = 1.1f;
    [SerializeField] private float slamInterval = 3.4f;
    [SerializeField] private float slamIntervalFrenzy = 2f;
    [SerializeField] private float totemHpRatio = 0.7f;
    [SerializeField] private float totemMaxHealth = 45f;
    [SerializeField] private float damageTakenWhileTotemsAlive = 0.25f;
    [SerializeField] private float towerDisableHpRatio = 0.4f;
    [SerializeField] private int siegeSummonCount = 2;
    [SerializeField] private EnemyDefinition siegeEnemyDefinition;
    [SerializeField] private float frenzyHpRatio = 0.2f;
    [SerializeField] private float zoneTelegraph = 0.6f;
    [SerializeField] private float zoneRadius = 3.5f;
    [SerializeField] private float zoneDuration = 3.5f;
    [SerializeField] private float zoneTickDamage = 8f;
    [SerializeField] private float zoneTickInterval = 0.5f;
    [SerializeField] private float zoneInterval = 5f;

    public string BossId => bossId;
    public string DisplayName => displayName;
    public float MaxHealth => maxHealth;
    public Vector3 BodyScale => bodyScale;
    public Color BodyColor => bodyColor;
    public float SlamDamage => slamDamage;
    public float SlamRadius => slamRadius;
    public float SlamTelegraph => slamTelegraph;
    public float SlamInterval => slamInterval;
    public float SlamIntervalFrenzy => slamIntervalFrenzy;
    public float TotemHpRatio => totemHpRatio;
    public float TotemMaxHealth => totemMaxHealth;
    public float DamageTakenWhileTotemsAlive => damageTakenWhileTotemsAlive;
    public float TowerDisableHpRatio => towerDisableHpRatio;
    public int SiegeSummonCount => siegeSummonCount;
    public EnemyDefinition SiegeEnemyDefinition => siegeEnemyDefinition;
    public float FrenzyHpRatio => frenzyHpRatio;
    public float ZoneTelegraph => zoneTelegraph;
    public float ZoneRadius => zoneRadius;
    public float ZoneDuration => zoneDuration;
    public float ZoneTickDamage => zoneTickDamage;
    public float ZoneTickInterval => zoneTickInterval;
    public float ZoneInterval => zoneInterval;
}
