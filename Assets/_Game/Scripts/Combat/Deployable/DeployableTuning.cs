/// <summary>
/// Baseline v0.1 z M84_BOMBERMAN_PLAN sekcja D — bez magic numbers w logice.
/// </summary>
public static class DeployableTuning
{
    public const int DefaultNormalCap = 3;
    public const int CapBonusCap = 10;

    public const float PlaceOffsetMeters = 0.60f;
    public const float BombDamage = 16f;
    public const float BombRadius = 2.40f;
    public const float BombBossStagger = 8f;
    public const int BombMaxTargets = 8;
    public const float MainBombFuseSeconds = 1.60f;

    public const float KickSpeed = 16f;
    public const float KickMaxDistance = 6.5f;
    public const float KickPickRange = 3.5f;
    public const float KickCapsuleWidth = 3.40f;
    public const float KickCapsuleLength = 3.80f;
    public const float KickShoveSpeed = 11f;
    public const float KickShoveDuration = 0.30f;
    public const float ShoveDetonateRadius = 0.55f;
    public const float MultiLaunchPickRange = 4.5f;

    public const float DashChargeDamage = 10f;
    public const float DashChargeRadius = 1.80f;
    public const float DashChargeFuseSeconds = 0.70f;
    public const float DashChargeBossStagger = 4f;
    public const int DashChargeMaxTargets = 6;
    public const float DashSpeed = 20f;
    public const float DashActiveSeconds = 0.20f;

    public const float SaperArmSeconds = 1.00f;
    public const float SaperTriggerRadius = 1.60f;

    public const float ClusterChildDamage = 7f;
    public const float ClusterChildRadius = 1.40f;
    public const float ClusterChildFuse = 0.80f;
    public const float ClusterScatterMin = 1.20f;
    public const float ClusterScatterMax = 1.80f;
    public const int ClusterChildCount = 3;
    public const int ClusterGeneration = 1;

    public const float HomingSpeed = 8f;
    public const float HomingContactRadius = 0.45f;
    public const float HomingSplitRadius = 8f;
    public const int HomingSplitThreshold = 3;
    public const float HomingNoTargetFuse = 0.80f;

    public const int OrbitalSlotCount = 4;
    public const float OrbitalRadius = 2.20f;
    public const float OrbitalDegreesPerSecond = 140f;
    public const float OrbitalDamage = 12f;
    public const float OrbitalBlastRadius = 1.80f;
    public const float OrbitalContactRadius = 0.50f;
    public const float OrbitalRechargeSeconds = 5f;

    public const int PlanetarySlotCount = 6;
    public const float PlanetaryRadius = 3.00f;
    public const float PlanetaryDegreesPerSecond = 200f;

    public const float OrbitalRechargeReductionSeconds = 1.00f;

    public const float BilliardSpeed = 11f;
    public const int BilliardMaxCollisions = 3;
    public const float BilliardMaxTime = 0.85f;
    public const float BilliardMaxDistance = 6.5f;
    public const int BilliardEliteCollisionCap = 1;
    public const float BilliardEliteSpeedScale = 0.5f;

    public const float BilliardBreakSpeed = 16f;
    public const int BilliardBreakMaxCollisions = 6;
    public const float BilliardBreakDamagePerCollision = 6f;
    public const float BilliardBreakFinalRadiusMul = 1.35f;

    public const float BurnDuration = 2.00f;
    public const float BurnTickDamage = 2f;
    public const float BurnTickInterval = 0.50f;

    public const float BasicSplashRadius = 1.10f;
    public const float HukSplashRadius = 2.20f;
    public const float RapidSplashOverride = 2.40f;
    public const float RapidSplashWithHuk = 3.20f;

    public const float TimedOverrideDuration = 6f;
    public const float TimedOverrideInterval = 0.22f;
    public const float PermanentOverrideInterval = 0.22f;
    public const float PermanentDamageMultiplier = 0.45f;

    public const float StackResetTimeout = 1.75f;
    public const int StackMax = 4;
    public static readonly float[] StackMultipliers = { 1.00f, 1.20f, 1.40f, 1.60f };

    public const float KickExplodeStunNormal = 1.0f;
    public const float KickExplodeStunElite = 0.5f;
    public const float KickExplodeStunBoss = 0f;
    public const float KickExplodeBossStaggerExtra = 8f;

    public const int AirstrikeBurstCount = 7;
    public const float AirstrikeTelegraphSeconds = 1.00f;
    public const float AirstrikeSequenceSeconds = 1.70f;
    public const float AirstrikeSpacing = 2.20f;
    public const float AirstrikeBlastRadius = 2.00f;
    public const float AirstrikeDamage = 14f;
    public const float AirstrikeStripHalfWidth = 1.00f;

    public const float AirstrikeFinisherBaseDamage = 18f;
    public const float AirstrikeFinisherBaseRadius = 2.60f;
    public const float AirstrikeFinisherBonusDamagePerUnique = 4f;
    public const float AirstrikeFinisherBonusRadiusPerUnique = 0.20f;
    public const int AirstrikeFinisherUniqueCap = 8;

    public const float ThrownExplosiveSpeed = 14f;
    public const float ThrownExplosiveArcPeak = 0.55f;
    public const float ThrownExplosiveMaxRange = 8f;
    public const float ThrownExplosiveDefaultSplash = 1.10f;
}
