#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Testy okablowania BootScene — łapie zerwane referencje M5 (GUID, missing script).
/// </summary>
public class BootSceneWiringTests
{
    private const string BootScenePath = "Assets/Scenes/BootScene.unity";

    [Test]
    public void BootScene_ProgressionConfigAsset_LoadsCorrectType()
    {
        var config = AssetDatabase.LoadAssetAtPath<ProgressionConfig>("Assets/_Game/Config/M5_ProgressionConfig.asset");
        Assert.NotNull(config, "M5_ProgressionConfig.asset musi ładować się jako ProgressionConfig.");
        Assert.AreEqual(30, config.RepairGoldCost);
    }

    [Test]
    public void BootScene_WaveSequenceAsset_LoadsCorrectType()
    {
        var sequence = AssetDatabase.LoadAssetAtPath<WaveSequenceDefinition>("Assets/_Game/Config/M7_WaveSequence.asset");
        Assert.NotNull(sequence, "M7_WaveSequence.asset musi ładować się jako WaveSequenceDefinition.");
        Assert.AreEqual(10, sequence.WaveCount);
    }

    [Test]
    public void BootScene_WaveSequence_HasTenWavesIncludingRamOnFive()
    {
        var sequence = AssetDatabase.LoadAssetAtPath<WaveSequenceDefinition>("Assets/_Game/Config/M7_WaveSequence.asset");
        Assert.NotNull(sequence);
        Assert.AreEqual(10, sequence.WaveCount);

        var wave5 = sequence.GetWave(4);
        Assert.NotNull(wave5);
        Assert.AreEqual(5, wave5.WaveNumber);
        Assert.IsTrue(wave5.IsBossWave);
        Assert.NotNull(wave5.BossDefinition);

        var wave10 = sequence.GetWave(9);
        Assert.NotNull(wave10);
        Assert.AreEqual(10, wave10.WaveNumber);
        Assert.IsTrue(wave10.IsBossWave);
        Assert.NotNull(wave10.WardenDefinition);
        Assert.IsNull(wave10.BossDefinition);
    }

    [Test]
    public void BootScene_HasSharedRunStateWithConfig()
    {
        var scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Additive);
        try
        {
            var runState = Object.FindAnyObjectByType<SharedRunState>();
            Assert.NotNull(runState, "BootScene musi mieć SharedRunState (RunProgression).");

            var serialized = new SerializedObject(runState);
            var scriptProp = serialized.FindProperty("m_Script");
            Assert.NotNull(scriptProp?.objectReferenceValue,
                "SharedRunState ma Missing Script — kasa/EXP/HUD nie zadziałają.");

            var configProp = serialized.FindProperty("config");
            Assert.NotNull(configProp, "SharedRunState.config property missing.");
            Assert.NotNull(configProp.objectReferenceValue,
                "SharedRunState.config jest null — kasa/EXP/HUD nie zadziałają.");

            var gameFlow = Object.FindAnyObjectByType<GameFlowManager>();
            Assert.NotNull(gameFlow, "BootScene musi mieć GameFlowManager.");

            var m7Sequence = AssetDatabase.LoadAssetAtPath<WaveSequenceDefinition>(
                "Assets/_Game/Config/M7_WaveSequence.asset");
            Assert.NotNull(m7Sequence, "M7_WaveSequence.asset musi istnieć.");
            Assert.AreEqual(10, m7Sequence.WaveCount, "M7_WaveSequence musi mieć 10 fal.");

            var gameFlowSerialized = new SerializedObject(gameFlow);
            var waveSequenceProp = gameFlowSerialized.FindProperty("waveSequence");
            Assert.NotNull(waveSequenceProp, "GameFlowManager.waveSequence property missing.");
            Assert.AreEqual(m7Sequence, waveSequenceProp.objectReferenceValue,
                "GameFlowManager.waveSequence musi wskazywać M7_WaveSequence (10 fal).");

            var hud = Object.FindAnyObjectByType<CombatHudPlaceholder>();
            Assert.NotNull(hud, "BootScene musi mieć CombatHudPlaceholder.");
            Assert.IsNull(Object.FindAnyObjectByType<CombatHudView>(),
                "CombatHudView powinien powstać w runtime przez bootstrap, nie w BootScene.");

            var mapBuilder = Object.FindAnyObjectByType<MapGreyboxBuilder>();
            Assert.NotNull(mapBuilder, "BootScene musi mieć MapGreyboxBuilder (MapGreybox).");

            var mapBuilderSerialized = new SerializedObject(mapBuilder);
            var mapScriptProp = mapBuilderSerialized.FindProperty("m_Script");
            Assert.NotNull(mapScriptProp?.objectReferenceValue,
                "MapGreyboxBuilder ma Missing Script — mapa nie zostanie zbudowana.");
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}
#endif
