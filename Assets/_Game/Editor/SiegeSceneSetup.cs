#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Tworzy osobną scenę Siege z BootScene, bez zapisywania ani mutowania BootScene.</summary>
public static class SiegeSceneSetup
{
    private const string Source = "Assets/Scenes/BootScene.unity";
    private const string Target = "Assets/Scenes/SiegeDefense.unity";
    [MenuItem("Game/Siege/Create SiegeDefense Scene")]
    public static void CreateScene()
    {
        if (!System.IO.File.Exists(Target) && !AssetDatabase.CopyAsset(Source, Target))
        {
            if (!System.IO.File.Exists(Target)) { Debug.LogError("[Siege] Nie można skopiować BootScene."); return; }
        }
        AssetDatabase.SaveAssets();
        var scene = EditorSceneManager.OpenScene(Target, OpenSceneMode.Single);
        RemoveLegacyObjects(scene);
        var arena = New<SiegeArena>("Siege Arena");
        var config = AssetDatabase.LoadAssetAtPath<SiegeArenaConfig>("Assets/_Game/Config/SiegeArenaConfig.asset");
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<SiegeArenaConfig>();
            AssetDatabase.CreateAsset(config, "Assets/_Game/Config/SiegeArenaConfig.asset");
        }
        arena.Config = config;
        var director = New<SiegeWaveDirector>("Siege Wave Director");
        var reward = New<SiegeRewardController>("Siege Rewards");
        var run = New<SiegeRunController>("Siege Run Controller");
        var templateIds = AssetDatabase.FindAssets("t:EnemyDefinition", new[] { "Assets/_Game/Config" });
        var templates = new EnemyDefinition[templateIds.Length];
        for (var i = 0; i < templateIds.Length; i++) templates[i] = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(templateIds[i]));
        var runSerialized = new SerializedObject(run);
        runSerialized.FindProperty("visualTemplates").arraySize = templates.Length;
        for (var i = 0; i < templates.Length; i++) runSerialized.FindProperty("visualTemplates").GetArrayElementAtIndex(i).objectReferenceValue = templates[i];
        runSerialized.ApplyModifiedPropertiesWithoutUndo();
        var build = Object.FindAnyObjectByType<BuildSystemBootstrap>() ?? New<BuildSystemBootstrap>("Build System Bootstrap");
        var buildCatalog = AssetDatabase.LoadAssetAtPath<BuildContentCatalog>("Assets/_Game/Config/M6_BuildContentCatalog.asset");
        if (buildCatalog != null)
        {
            var buildSerialized = new SerializedObject(build);
            buildSerialized.FindProperty("catalog").objectReferenceValue = buildCatalog;
            buildSerialized.ApplyModifiedPropertiesWithoutUndo();
        }
        var shared = Object.FindAnyObjectByType<SharedRunState>();
        var progress = AssetDatabase.LoadAssetAtPath<ProgressionConfig>("Assets/_Game/Config/M5_ProgressionConfig.asset");
        if (shared != null && progress != null)
        {
            var sharedSerialized = new SerializedObject(shared);
            sharedSerialized.FindProperty("config").objectReferenceValue = progress;
            sharedSerialized.ApplyModifiedPropertiesWithoutUndo();
        }
        New<SiegeHudView>("Siege HUD");
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<SiegeCamera>() == null) cam.gameObject.AddComponent<SiegeCamera>();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!buildScenes.Exists(s => s.path == Target)) { buildScenes.Add(new EditorBuildSettingsScene(Target, true)); EditorBuildSettings.scenes = buildScenes.ToArray(); }
        Debug.Log("[Siege] Utworzono Assets/Scenes/SiegeDefense.unity. Zagraj przez Play.");
    }
    private static T New<T>(string name) where T : Component
    {
        var existing = Object.FindAnyObjectByType<T>();
        if (existing != null) return existing;
        var go = new GameObject(name); SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene()); return go.AddComponent<T>();
    }
    private static void RemoveLegacyObjects(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var components = root.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (component == null) continue;
                var type = component.GetType().Name;
                if (type == "MapGreyboxBuilder" || type == "GameFlowManager" || type == "WaveManager" || type == "CombatTestSpawner" || type == "SharedCamera")
                    Object.DestroyImmediate(component);
            }
            if (root.GetComponent<PlayerCharacter>() == null && (root.name == "Caster" || root.name == "Capsule" || root.name.StartsWith("Deployable_") || root.name.Contains("MapGreybox") || root.name == "GeneratedMap"))
                Object.DestroyImmediate(root);
        }
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "Map" || go.name == "GeneratedMap") Object.DestroyImmediate(go);
    }
}
#endif
