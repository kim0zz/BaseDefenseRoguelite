using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Ładuje DRAFT z zasobu JSON, tworząc izolowane definicje bez modyfikowania klasycznego trybu.</summary>
public static class SiegeWaveFactory
{
    [Serializable] private sealed class Data
    {
        public EnemyData[] enemies;
        public WaveData[] waves;
    }
    [Serializable] private sealed class EnemyData
    {
        public EnemyKind enemyKind;
        public string displayName;
        public float maxHealth, damage, attackInterval, moveSpeed, attackRange, detectRange;
        public Vector3 bodyScale;
        public Color bodyColor;
        public int expReward, goldReward;
        public float structureDamage;
        public float playerDamageMultiplier = 1f;
        public float shieldProtectRadius;
    }
    [Serializable] private sealed class WaveData
    {
        public string title, description;
        public int activeCap;
        public GroupData[] groups;
    }
    [Serializable] private sealed class GroupData
    {
        public string label;
        public AttackLineId lane;
        public EnemyKind kind;
        public int count;
        public float startSeconds, telegraphSeconds, spawnInterval;
        public EliteModifier elite;
    }

    public static SiegeWaveSequenceDefinition CreateDefault(EnemyDefinition[] visualTemplates = null)
    {
        var text = Resources.Load<TextAsset>("Siege/SiegeWaves");
        if (text == null) throw new InvalidOperationException("Brak Resources/Siege/SiegeWaves.json.");
        return CreateFromJson(text.text, visualTemplates);
    }

    public static SiegeWaveSequenceDefinition CreateFromJson(string json, EnemyDefinition[] visualTemplates = null)
    {
        var data = JsonUtility.FromJson<Data>(json);
        if (data?.enemies == null || data.waves == null || data.waves.Length == 0)
            throw new ArgumentException("Niepoprawna konfiguracja Siege.");
        var enemies = new Dictionary<EnemyKind, EnemyDefinition>();
        foreach (var entry in data.enemies)
        {
            EnemyDefinition template = null;
            if (visualTemplates != null)
                foreach (var candidate in visualTemplates)
                    if (candidate != null && candidate.Kind == entry.enemyKind) { template = candidate; break; }
            var definition = template != null ? UnityEngine.Object.Instantiate(template)
                : ScriptableObject.CreateInstance<EnemyDefinition>();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(entry), definition);
            definition.name = "Siege_DRAFT_" + entry.displayName;
            enemies.Add(entry.enemyKind, definition);
        }
        var sequence = ScriptableObject.CreateInstance<SiegeWaveSequenceDefinition>();
        sequence.name = "Siege_DRAFT_5_fal";
        sequence.Waves = new SiegeWaveDefinition[data.waves.Length];
        for (var i = 0; i < data.waves.Length; i++)
        {
            var source = data.waves[i];
            var wave = ScriptableObject.CreateInstance<SiegeWaveDefinition>();
            wave.name = "Siege_" + (i + 1);
            wave.Title = source.title;
            wave.Description = source.description;
            wave.ActiveCap = Mathf.Max(1, source.activeCap);
            wave.Groups = new SiegeWaveGroup[source.groups.Length];
            for (var g = 0; g < source.groups.Length; g++)
            {
                var group = source.groups[g];
                wave.Groups[g] = new SiegeWaveGroup {
                    Label = group.label, Lane = group.lane, Enemy = enemies[group.kind],
                    Count = group.count, StartSeconds = group.startSeconds,
                    TelegraphSeconds = group.telegraphSeconds, SpawnInterval = group.spawnInterval,
                    Elite = group.elite
                };
            }
            sequence.Waves[i] = wave;
        }
        return sequence;
    }

    /// <summary>Wyłącznie sekwencje utworzone przez tę fabrykę; po usunięciu ich aktorów.</summary>
    public static void DestroyRuntimeSequence(SiegeWaveSequenceDefinition sequence)
    {
        if (sequence == null) return;
        var enemies = new HashSet<EnemyDefinition>();
        foreach (var wave in sequence.Waves)
        {
            if (wave == null) continue;
            foreach (var group in wave.Groups)
                if (group?.Enemy != null) enemies.Add(group.Enemy);
            Release(wave);
        }
        foreach (var enemy in enemies) Release(enemy);
        Release(sequence);
    }

    private static void Release(UnityEngine.Object target)
    {
        if (Application.isPlaying) UnityEngine.Object.Destroy(target);
        else UnityEngine.Object.DestroyImmediate(target);
    }
}
