/// <summary>
/// Operacja karty talentu — zamknięta lista, nie DSL.
/// </summary>
public enum TalentEffectOp
{
    None,
    MutateSkill,
    AddSkillEffect,
    GrantSkill,
    AttachPersistent
}

/// <summary>
/// Efekt na resolve skilla (per slot).
/// </summary>
public enum SkillEffectKind
{
    None,
    SpawnSlowZone,
    SpawnDamageZone,
    HealCasterOnHit,
    PulseRingOnWallStop
}

/// <summary>
/// Efekt trwały na graczu.
/// </summary>
public enum PersistentEffectKind
{
    None,
    Berserker,
    Hart,
    DamageReflect,
    LastStand,
    HellAura,
    BodyKolos,
    VampireAura,
    OverheatAura,
    FuryMeter,
    HartStacks,
    TimedLastChance,
    KolosForm,
    HellAuraRamping,
    MoveShockwave,
    OnKillFormExtend,
    PeriodicTauntInForm,
    AftershockWaves,
    GroundCracks,
    OverheatAuraRamping,
    FireTrail,
    ChainIgnite,
    HealAmpInLastChance,
    AgonyPulses,
    SecondWind,
    AutoFuryOnLastChance,
    ConvertTauntHitsToHeal,
    WkurwionyKolos,
    StaggerBoost,
    KrwawaSejsmika,
    ClusterOnExplode,
    BasicSplashBonus,
    KickExplodeStun,
    ArmToProximityMine,
    ApplyBurnOnPlayerDamage,
    ChildHoming,
    PerTargetHitStacks,
    BilliardOnKick,
    TimedAttackIntervalOverride,
    PermanentAttackIntervalOverride,
    ConsumeBurnOnHit,
    PhoenixOnBurnKill,
    PackHuntFocus,
    SpreadBurnOnSplash,
    MarkedDelayedBlast,
    MultiLaunchOwned,
    CarryBurningOnKick,
    BilliardBreakUpgrade,
    CollisionScalingExplosion,
    RapidSplashDuringOverride,
    OrbitalRechargeOnExplode,
    OrbitalInheritsCluster,
    PlanetaryOrbitUpgrade,
    AirstrikeLeavesNormals,
    AirstrikeFinisher,
    DeployableCapBonus
}
