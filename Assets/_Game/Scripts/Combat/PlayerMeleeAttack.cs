using UnityEngine;

/// <summary>
/// Legacy M3 melee — timing przeniesiony do PlayerAttackController.
/// Komponent zostaje, żeby nie psuć starych referencji; nie zadaje obrażeń.
/// </summary>
[DisallowMultipleComponent]
public class PlayerMeleeAttack : MonoBehaviour
{
    [SerializeField] private MeleeWeaponDefinition weapon;

    public void Configure(MeleeWeaponDefinition weaponDefinition)
    {
        weapon = weaponDefinition;
    }

    public void ApplyEffectiveStats(EffectiveCombatStats stats, MeleeWeaponDefinition fallbackProfile)
    {
        if (fallbackProfile != null)
            weapon = fallbackProfile;

        var controller = GetComponent<PlayerAttackController>();
        var build = GetComponent<PlayerBuildState>();
        if (controller == null || build == null) return;
        controller.ApplyEffectiveStats(stats, build.GetWeapon(build.ActiveWeaponIndex));
    }
}
