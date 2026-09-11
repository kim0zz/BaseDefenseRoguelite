using UnityEngine;

/// <summary>
/// Dane umiejętności aktywnej — data-driven (M7.5-T2).
/// </summary>
[CreateAssetMenu(fileName = "SkillDefinition", menuName = "Game/Skills/Skill Definition")]
public class SkillDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string skillId = "pudzian_trzasniecie";
    [SerializeField] private string displayName = "Trzaśnięcie";
    [SerializeField] private Sprite icon;
    [SerializeField] private PlayerClassId ownerClass = PlayerClassId.Pudzian;
    [SerializeField] private int skillSlotIndex = 0;
    [SerializeField] private SkillActivationMode activationMode = SkillActivationMode.Active;
    [SerializeField] private ChargeContactMode chargeContactMode = ChargeContactMode.ForwardShove;

    [Header("Timing")]
    [SerializeField] private float windupSeconds = 0.35f;
    [SerializeField] private float activeSeconds = 0.12f;
    [SerializeField] private float recoverySeconds = 0.45f;
    [SerializeField] private float windupMoveMultiplier = 0f;
    [SerializeField] private float recoveryMoveMultiplier = 0.5f;
    [SerializeField] private bool locksFacingInActive = true;

    [Header("Cooldown")]
    [SerializeField] private float cooldownSeconds = 8f;
    [SerializeField] private int maxCharges = 1;
    [SerializeField] private bool cooldownStartsOnActive = true;
    [SerializeField] private float windupInterruptCooldownRefund = 0.5f;

    [Header("Targeting")]
    [SerializeField] private SkillAimMode aimMode = SkillAimMode.Self;
    [SerializeField] private SkillLocomotionMode locomotion = SkillLocomotionMode.Root;
    [SerializeField] private SkillHitPolicy hitPolicy = SkillHitPolicy.OnEnteredActive;

    [Header("Shape")]
    [SerializeField] private SkillShapeType shapeType = SkillShapeType.Circle;
    [SerializeField] private float radiusMeters = 3f;
    [SerializeField] private float chargeSpeed = 12.7f;
    [SerializeField] private float chargeRangeMeters = 7f;
    [SerializeField] private float capsuleWidthMeters = 1.6f;
    [SerializeField] private float capsuleLengthMeters = 1.8f;
    [SerializeField] private int maxTargets = 8;

    [Header("Effects")]
    [SerializeField] private float damage = 16f;
    [SerializeField] private SkillControlMode controlMode = SkillControlMode.Stun;
    [SerializeField] private float controlDurationSeconds = 1.2f;
    [SerializeField] private float knockbackForce = 0f;
    [SerializeField] private float bossStaggerContribution = 12f;
    [SerializeField] private SkillTargetMask targetMask = SkillTargetMask.Enemy | SkillTargetMask.Elite | SkillTargetMask.Boss;
    [SerializeField] private bool appliesThreatOverride;
    [SerializeField] private float threatDurationSeconds = 3.5f;

    [Header("Feel")]
    [SerializeField] private float hitStopSeconds = 0.08f;
    [SerializeField] private float shakeStrength = 0.35f;
    [SerializeField] private float rumbleLow = 0.4f;
    [SerializeField] private float rumbleHigh = 0.6f;

    [Header("Feedback slots — null = placeholder")]
    [SerializeField] private GameObject telegraphPrefab;
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private AudioClip windupClip;
    [SerializeField] private AudioClip impactClip;

    public string SkillId => skillId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public PlayerClassId OwnerClass => ownerClass;
    public int SkillSlotIndex => skillSlotIndex;
    public SkillActivationMode ActivationMode => activationMode;
    public ChargeContactMode ChargeContactMode => chargeContactMode;
    public float WindupSeconds => windupSeconds;
    public float ActiveSeconds => activeSeconds;
    public float RecoverySeconds => recoverySeconds;
    public float WindupMoveMultiplier => windupMoveMultiplier;
    public float RecoveryMoveMultiplier => recoveryMoveMultiplier;
    public bool LocksFacingInActive => locksFacingInActive;
    public float CooldownSeconds => cooldownSeconds;
    public int MaxCharges => maxCharges;
    public bool CooldownStartsOnActive => cooldownStartsOnActive;
    public float WindupInterruptCooldownRefund => windupInterruptCooldownRefund;
    public SkillAimMode AimMode => aimMode;
    public SkillLocomotionMode Locomotion => locomotion;
    public SkillHitPolicy HitPolicy => hitPolicy;
    public SkillShapeType ShapeType => shapeType;
    public float RadiusMeters => radiusMeters;
    public float ChargeSpeed => chargeSpeed;
    public float ChargeRangeMeters => chargeRangeMeters;
    public float CapsuleWidthMeters => capsuleWidthMeters;
    public float CapsuleLengthMeters => capsuleLengthMeters;
    public int MaxTargets => maxTargets;
    public float Damage => damage;
    public SkillControlMode ControlMode => controlMode;
    public float ControlDurationSeconds => controlDurationSeconds;
    public float KnockbackForce => knockbackForce;
    public float BossStaggerContribution => bossStaggerContribution;
    public SkillTargetMask TargetMask => targetMask;
    public bool AppliesThreatOverride => appliesThreatOverride;
    public float ThreatDurationSeconds => threatDurationSeconds;
    public float HitStopSeconds => hitStopSeconds;
    public float ShakeStrength => shakeStrength;
    public float RumbleLow => rumbleLow;
    public float RumbleHigh => rumbleHigh;
    public GameObject TelegraphPrefab => telegraphPrefab;
    public GameObject ImpactPrefab => impactPrefab;
    public AudioClip WindupClip => windupClip;
    public AudioClip ImpactClip => impactClip;
}
