/// <summary>
/// Mapowanie weaponId → rodzina. Źródło prawdy dla factory i testów.
/// </summary>
public static class WeaponFamilyCatalog
{
    public static WeaponFamily FromId(string weaponId)
    {
        switch (weaponId)
        {
            case "sztylet":
            case "viper_fang":
                return WeaponFamily.Dagger;
            case "topor":
            case "mlot":
                return WeaponFamily.Axe;
            case "luk":
                return WeaponFamily.Bow;
            default:
                return WeaponFamily.Sword;
        }
    }
}
