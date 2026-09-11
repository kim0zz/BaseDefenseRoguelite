using UnityEngine;

/// <summary>
/// Tworzy wrogów z definicji contentowej (M4/M6.5/M7).
/// </summary>
public static class EnemySpawner
{
    public static EnemyController Spawn(
        EnemyDefinition definition,
        Vector3 position,
        int spawnIndex,
        AttackLineId lane,
        EliteModifier eliteModifier = EliteModifier.None)
    {
        if (definition == null)
        {
            Debug.LogWarning("[EnemySpawner] Brak EnemyDefinition — pomijam spawn.");
            return null;
        }

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = BuildName(definition, spawnIndex, eliteModifier);
        go.transform.position = position;
        go.transform.localScale = definition.BodyScale;

        ApplyKindVisual(go, definition, eliteModifier);

        go.AddComponent<Health>();
        go.AddComponent<StatusEffectReceiver>();
        go.AddComponent<HitFlashFeedback>();
        go.AddComponent<StunFeedbackView>();
        go.AddComponent<KnockbackReceiver>();
        go.AddComponent<EnemyHealthBar>();
        go.AddComponent<EnemyLaneMotor>();
        var resistance = go.AddComponent<ForcedMovementResistanceProfile>();
        resistance.Configure(eliteModifier != EliteModifier.None
            ? ForcedMovementResistanceCategory.Elite
            : ForcedMovementResistanceCategory.Normal);
        var enemy = go.AddComponent<EnemyController>();
        enemy.Configure(definition, lane, eliteModifier);

        return enemy;
    }

    private static string BuildName(EnemyDefinition definition, int spawnIndex, EliteModifier eliteModifier)
    {
        var suffix = eliteModifier != EliteModifier.None ? $"_{eliteModifier}" : string.Empty;
        return $"{definition.DisplayName}{suffix}_{spawnIndex + 1}";
    }

    private static void ApplyKindVisual(GameObject go, EnemyDefinition definition, EliteModifier eliteModifier)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        var color = definition.BodyColor;
        switch (definition.Kind)
        {
            case EnemyKind.Rusher:
                color = new Color(0.95f, 0.45f, 0.15f, 1f);
                go.transform.localScale = definition.BodyScale * 0.9f;
                break;
            case EnemyKind.Carrier:
                color = new Color(0.95f, 0.85f, 0.2f, 1f);
                go.transform.localScale = definition.BodyScale * 1.05f;
                break;
            case EnemyKind.Siege:
                color = new Color(0.35f, 0.35f, 0.45f, 1f);
                go.transform.localScale = definition.BodyScale * 1.25f;
                break;
            case EnemyKind.Support:
                color = new Color(0.15f, 0.75f, 0.85f, 1f);
                go.transform.localScale = definition.BodyScale;
                SpawnSupportAuraRing(go, definition.SupportBuffRadius);
                break;
            case EnemyKind.Flanker:
                color = new Color(0.35f, 0.85f, 0.22f, 1f);
                go.transform.localScale = definition.BodyScale;
                break;
            case EnemyKind.Shielder:
                color = new Color(0.25f, 0.42f, 0.78f, 1f);
                go.transform.localScale = definition.BodyScale;
                break;
        }

        if (eliteModifier != EliteModifier.None)
        {
            color = Color.Lerp(color, Color.white, 0.15f);
            go.transform.localScale *= 1.15f;
        }

        var mat = new Material(renderer.sharedMaterial);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;
        renderer.material = mat;
    }

    private static void SpawnSupportAuraRing(GameObject parent, float buffRadius)
    {
        if (buffRadius <= 0f) return;

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "SupportAuraRing";
        ring.transform.SetParent(parent.transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.08f, 0f);

        var diameter = buffRadius * 2f;
        ring.transform.localScale = new Vector3(diameter, 0.02f, diameter);

        var collider = ring.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);

        var ringRenderer = ring.GetComponent<Renderer>();
        if (ringRenderer != null)
        {
            var ringMat = new Material(ringRenderer.sharedMaterial);
            var auraColor = new Color(0.15f, 0.75f, 0.85f, 0.25f);
            if (ringMat.HasProperty("_BaseColor"))
                ringMat.SetColor("_BaseColor", auraColor);
            else
                ringMat.color = auraColor;
            ringRenderer.material = ringMat;
        }
    }
}
