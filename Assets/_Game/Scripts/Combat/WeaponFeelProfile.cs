/// <summary>
/// DRAFT parametry feel per rodzina — docs/technical/GAME_FEEL.md.
/// Frakcje faz sumują się do 1 i skalują AttackInterval.
/// </summary>
public readonly struct WeaponFeelProfile
{
    public readonly WeaponFamily Family;
    public readonly float WindupFraction;
    public readonly float ActiveFraction;
    public readonly float RecoveryFraction;
    public readonly float MoveMultiplierDuringSwing;
    public readonly float HitStopSeconds;
    public readonly float Knockback;
    public readonly float Shake;
    public readonly float ProjectileSpeed;

    public WeaponFeelProfile(
        WeaponFamily family,
        float windupFraction,
        float activeFraction,
        float recoveryFraction,
        float moveMultiplierDuringSwing,
        float hitStopSeconds,
        float knockback,
        float shake,
        float projectileSpeed)
    {
        Family = family;
        WindupFraction = windupFraction;
        ActiveFraction = activeFraction;
        RecoveryFraction = recoveryFraction;
        MoveMultiplierDuringSwing = moveMultiplierDuringSwing;
        HitStopSeconds = hitStopSeconds;
        Knockback = knockback;
        Shake = shake;
        ProjectileSpeed = projectileSpeed;
    }

    public float FractionSum => WindupFraction + ActiveFraction + RecoveryFraction;

    public bool IsRanged => Family == WeaponFamily.Bow;

    public static WeaponFeelProfile For(WeaponFamily family)
    {
        switch (family)
        {
            case WeaponFamily.Dagger:
                return new WeaponFeelProfile(WeaponFamily.Dagger, 0.08f, 0.12f, 0.80f, 1.00f, 0.00f, 0.15f, 0.00f, 0f);
            case WeaponFamily.Axe:
                return new WeaponFeelProfile(WeaponFamily.Axe, 0.38f, 0.10f, 0.52f, 0.25f, 0.10f, 1.80f, 0.12f, 0f);
            case WeaponFamily.Bow:
                return new WeaponFeelProfile(WeaponFamily.Bow, 0.28f, 0.00f, 0.72f, 0.40f, 0.03f, 0.20f, 0.05f, 20f);
            default:
                return new WeaponFeelProfile(WeaponFamily.Sword, 0.18f, 0.12f, 0.70f, 0.55f, 0.04f, 0.60f, 0.04f, 0f);
        }
    }
}
