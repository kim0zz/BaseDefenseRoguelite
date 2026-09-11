using UnityEngine;

/// <summary>
/// Spawn bossa z definicji SO (M7).
/// </summary>
public static class BossSpawner
{
    public static BossRamController Spawn(BossDefinition definition, Vector3 position)
    {
        if (definition == null)
        {
            Debug.LogWarning("[BossSpawner] Brak BossDefinition.");
            return null;
        }

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = definition.DisplayName;
        go.transform.position = position;
        go.transform.localScale = definition.BodyScale;

        go.AddComponent<Health>();
        go.AddComponent<StatusEffectReceiver>();
        go.AddComponent<HitFlashFeedback>();
        go.AddComponent<StunFeedbackView>();
        go.AddComponent<KnockbackReceiver>();
        var resistance = go.AddComponent<ForcedMovementResistanceProfile>();
        resistance.Configure(ForcedMovementResistanceCategory.Boss);
        var hpBar = go.AddComponent<EnemyHealthBar>();

        var boss = go.AddComponent<BossRamController>();
        boss.Configure(definition);
        hpBar.ConfigureVisual(new Vector3(0f, 2.8f, 0f), new Vector3(2.6f, 0.2f, 0.08f));

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", definition.BodyColor);
            else
                mat.color = definition.BodyColor;
            renderer.material = mat;
        }

        Debug.Log($"[BossSpawner] {definition.DisplayName} @ {position}");
        return boss;
    }

    public static BossWardenController SpawnWarden(WardenDefinition definition, Vector3 position)
    {
        if (definition == null)
        {
            Debug.LogWarning("[BossSpawner] Brak WardenDefinition.");
            return null;
        }

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = definition.DisplayName;
        go.transform.position = position;
        go.transform.localScale = definition.BodyScale;

        go.AddComponent<Health>();
        go.AddComponent<DamageTakenMultiplier>();
        go.AddComponent<HitFlashFeedback>();
        go.AddComponent<StunFeedbackView>();
        go.AddComponent<KnockbackReceiver>();
        var resistance = go.AddComponent<ForcedMovementResistanceProfile>();
        resistance.Configure(ForcedMovementResistanceCategory.Boss);
        var hpBar = go.AddComponent<EnemyHealthBar>();

        var warden = go.AddComponent<BossWardenController>();
        warden.Configure(definition);
        hpBar.ConfigureVisual(new Vector3(0f, 3f, 0f), new Vector3(2.8f, 0.22f, 0.08f));

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", definition.BodyColor);
            else
                mat.color = definition.BodyColor;
            renderer.material = mat;
        }

        Debug.Log($"[BossSpawner] {definition.DisplayName} @ {position}");
        return warden;
    }
}
