using UnityEngine;

/// <summary>
/// Dane bossa The Ram — liczby DRAFT w SO (M7-T5).
/// </summary>
[CreateAssetMenu(fileName = "BossDefinition", menuName = "Game/Boss/Boss Definition")]
public class BossDefinition : ScriptableObject
{
    [SerializeField] private string bossId = "the_ram";
    [SerializeField] private string displayName = "The Ram";
    [SerializeField] private float maxHealth = 300f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float chargeSpeed = 14f;
    [SerializeField] private float telegraphSeconds = 1.8f;
    [SerializeField] private float telegraphSecondsEnraged = 1.0f;
    [SerializeField] private float recoverSeconds = 1.4f;
    [SerializeField] private float structureImpactDamage = 27f;
    [SerializeField] private float shockwaveDamage = 6f;
    [SerializeField] private float shockwaveRadius = 4f;
    [SerializeField] private float focusPlayerDamage = 5f;
    [SerializeField] private float focusAttackInterval = 2.2f;
    [SerializeField] private float focusDuration = 3.5f;
    [SerializeField] private float focusDurationEnraged = 2.2f;
    [SerializeField] private float focusContactGap = 0.55f;
    [SerializeField] private float playerBodyRadius = 0.5f;
    [SerializeField] private float staggerInterruptThreshold = 35f;
    [SerializeField] private float burstInterruptThreshold = 60f;
    [SerializeField] private float burstInterruptWindow = 2f;
    [SerializeField] private float interruptStunSeconds = 1.2f;
    [SerializeField] private float enrageHealthRatio = 0.5f;
    [SerializeField] private float chargeCooldown = 6f;
    [SerializeField] private float chargeCooldownEnraged = 3.5f;
    [SerializeField] private Color bodyColor = new(0.55f, 0.2f, 0.65f, 1f);
    [SerializeField] private Vector3 bodyScale = new(2.2f, 2.2f, 2.2f);

    public string BossId => bossId;
    public string DisplayName => displayName;
    public float MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float ChargeSpeed => chargeSpeed;
    public float TelegraphSeconds => telegraphSeconds;
    public float TelegraphSecondsEnraged => telegraphSecondsEnraged;
    public float RecoverSeconds => recoverSeconds;
    public float StructureImpactDamage => structureImpactDamage;
    public float ShockwaveDamage => shockwaveDamage;
    public float ShockwaveRadius => shockwaveRadius;
    public float FocusPlayerDamage => focusPlayerDamage;
    public float FocusAttackInterval => focusAttackInterval;
    public float FocusDuration => focusDuration;
    public float FocusDurationEnraged => focusDurationEnraged;
    public float FocusContactGap => focusContactGap;
    public float PlayerBodyRadius => playerBodyRadius;
    public float StaggerInterruptThreshold => staggerInterruptThreshold;
    public float BurstInterruptThreshold => burstInterruptThreshold;
    public float BurstInterruptWindow => burstInterruptWindow;
    public float InterruptStunSeconds => interruptStunSeconds;
    public float EnrageHealthRatio => enrageHealthRatio;
    public float ChargeCooldown => chargeCooldown;
    public float ChargeCooldownEnraged => chargeCooldownEnraged;
    public Color BodyColor => bodyColor;
    public Vector3 BodyScale => bodyScale;
}
