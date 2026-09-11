using UnityEngine;

/// <summary>
/// Pickup lootu na ziemi (M6).
/// </summary>
[DisallowMultipleComponent]
public class LootPickup : MonoBehaviour
{
    [SerializeField] private WeaponDefinition weapon;
    [SerializeField] private ItemDefinition item;
    [SerializeField] private float pickupRadius = 1.4f;

    public WeaponDefinition Weapon => weapon;
    public ItemDefinition Item => item;

    public static LootPickup Spawn(Vector3 position, WeaponDefinition weaponDrop, ItemDefinition itemDrop)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = weaponDrop != null ? $"Loot_{weaponDrop.DisplayName}" : $"Loot_{itemDrop.DisplayName}";
        go.transform.position = position + Vector3.up * 0.35f;
        go.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);

        var collider = go.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            var rarity = weaponDrop != null ? weaponDrop.Rarity : itemDrop.Rarity;
            mat.color = rarity == LootRarity.Rare
                ? new Color(0.75f, 0.35f, 1f, 0.95f)
                : new Color(0.35f, 0.85f, 1f, 0.9f);
            renderer.material = mat;
        }

        var pickup = go.AddComponent<LootPickup>();
        pickup.weapon = weaponDrop;
        pickup.item = itemDrop;
        return pickup;
    }

    private void Update()
    {
        var players = FindObjectsByType<PlayerCharacter>();
        foreach (var player in players)
        {
            if (!player.IsCombatEnabled) continue;
            var dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist > pickupRadius) continue;

            var build = player.GetComponent<PlayerBuildState>();
            if (build == null) continue;

            var pickedUp = weapon != null
                ? build.TryPickupWeapon(weapon)
                : build.TryPickupItem(item);

            if (pickedUp)
            {
                Debug.Log($"[LootPickup] P{player.PlayerIndex + 1} podniósł {(weapon != null ? weapon.DisplayName : item.DisplayName)}.");
                Destroy(gameObject);
                return;
            }
        }
    }
}
