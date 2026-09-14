#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Tworzy scenę prototypu ruchu na linie (ChainMovementTest).</summary>
public static class ChainMovementTestSceneSetup
{
    private const string SceneFolder = "Assets/Scenes/Prototypes";
    private const string ScenePath = SceneFolder + "/ChainMovementTest.unity";
    private const string ConfigFolder = "Assets/_Game/Config/Prototypes";
    private const string ConfigPath = ConfigFolder + "/ChainTetherConfig.asset";
    private const string CatalogPath = "Assets/_Game/Config/M6_BuildContentCatalog.asset";

    [MenuItem("Game/Create ChainMovementTest Scene")]
    [MenuItem("Game/Prototypes/Create ChainMovementTest Scene")]
    public static void CreateScene()
    {
        EnsureFolders();
        EnsureConfigAsset();

        Scene scene;
        if (File.Exists(ScenePath))
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClearSceneRoots(scene);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        var cameraGo = new GameObject("Main Camera");
        SceneManager.MoveGameObjectToScene(cameraGo, scene);
        var cam = cameraGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 14f;
        cameraGo.AddComponent<AudioListener>();
        cameraGo.AddComponent<SharedCamera>();

        var lightGo = new GameObject("Directional Light");
        SceneManager.MoveGameObjectToScene(lightGo, scene);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var join = New<PlayerJoinManager>("Player Join Manager");
        var joinSerialized = new SerializedObject(join);
        joinSerialized.FindProperty("maxPlayers").intValue = 4;
        joinSerialized.FindProperty("autoJoinConnectedGamepads").boolValue = true;
        joinSerialized.FindProperty("fallbackSpawnPositions").arraySize = 4;
        SetSpawn(joinSerialized.FindProperty("fallbackSpawnPositions"), 0, new Vector3(-1.5f, 1f, 0f));
        SetSpawn(joinSerialized.FindProperty("fallbackSpawnPositions"), 1, new Vector3(1.5f, 1f, 0f));
        SetSpawn(joinSerialized.FindProperty("fallbackSpawnPositions"), 2, new Vector3(-3f, 1f, 1.5f));
        SetSpawn(joinSerialized.FindProperty("fallbackSpawnPositions"), 3, new Vector3(3f, 1f, 1.5f));
        joinSerialized.ApplyModifiedPropertiesWithoutUndo();

        var build = New<BuildSystemBootstrap>("Build System Bootstrap");
        var catalog = AssetDatabase.LoadAssetAtPath<BuildContentCatalog>(CatalogPath);
        if (catalog != null)
        {
            var buildSerialized = new SerializedObject(build);
            buildSerialized.FindProperty("catalog").objectReferenceValue = catalog;
            buildSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        New<CombatBootstrap>("Combat Bootstrap");

        var playArea = New<MapPlayArea>("MapPlayArea");
        playArea.SetBounds(new Vector2(-28f, 28f), new Vector2(-28f, 28f));

        var bootstrap = New<ChainMovementTestBootstrap>("Chain Movement Test Bootstrap");
        var config = AssetDatabase.LoadAssetAtPath<ChainTetherConfig>(ConfigPath);
        var bootstrapSerialized = new SerializedObject(bootstrap);
        bootstrapSerialized.FindProperty("tetherConfig").objectReferenceValue = config;
        bootstrapSerialized.FindProperty("joinManager").objectReferenceValue = join;
        bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        if (!Directory.Exists(SceneFolder))
            Directory.CreateDirectory(SceneFolder);
        EditorSceneManager.SaveScene(scene, ScenePath);

        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ChainPrototype] Utworzono {ScenePath}. Play: 2+ padów auto-join lub P1 klawiatura + South na P2.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder(SceneFolder))
            AssetDatabase.CreateFolder("Assets/Scenes", "Prototypes");
        if (!AssetDatabase.IsValidFolder("Assets/_Game/Config/Prototypes"))
            AssetDatabase.CreateFolder("Assets/_Game/Config", "Prototypes");
    }

    private static void EnsureConfigAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<ChainTetherConfig>(ConfigPath);
        if (existing != null) return;

        var config = ScriptableObject.CreateInstance<ChainTetherConfig>();
        config.EnsureDraftDefaults();
        AssetDatabase.CreateAsset(config, ConfigPath);
    }

    private static void SetSpawn(SerializedProperty array, int index, Vector3 value)
    {
        array.GetArrayElementAtIndex(index).vector3Value = value;
    }

    private static T New<T>(string name) where T : Component
    {
        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
        return go.AddComponent<T>();
    }

    private static void ClearSceneRoots(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            Object.DestroyImmediate(root);
    }

    private static void AddSceneToBuildSettings(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
