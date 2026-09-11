using UnityEngine;

/// <summary>
/// Spawn prefabów telegraph/impact ze slotów SkillDefinition (M9.1).
/// </summary>
public static class SkillVfxSpawner
{
    public static GameObject SpawnTelegraph(GameObject prefab, Vector3 worldPosition, float lifetimeSeconds) =>
        Spawn(prefab, worldPosition, lifetimeSeconds);

    public static GameObject SpawnImpact(GameObject prefab, Vector3 worldPosition, float lifetimeSeconds) =>
        Spawn(prefab, worldPosition, lifetimeSeconds);

    public static GameObject Spawn(GameObject prefab, Vector3 worldPosition, float lifetimeSeconds)
    {
        if (prefab == null) return null;

        var position = worldPosition;
        position.y = 0.02f;
        var instance = Object.Instantiate(prefab, position, Quaternion.identity);
        if (lifetimeSeconds > 0f)
            Object.Destroy(instance, lifetimeSeconds);
        return instance;
    }
}
