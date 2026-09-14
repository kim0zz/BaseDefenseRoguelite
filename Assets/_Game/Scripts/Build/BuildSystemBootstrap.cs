using UnityEngine;

/// <summary>
/// Bootstrap build systemu M6 — zapewnia katalog contentu w scenie.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-90)]
public class BuildSystemBootstrap : MonoBehaviour
{
    [SerializeField] private BuildContentCatalog catalog;

    public static BuildSystemBootstrap Instance { get; private set; }
    public BuildContentCatalog Catalog => catalog;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BuildSystemBootstrap] Więcej niż jedna instancja — używam pierwszej.");
            return;
        }

        Instance = this;
        ResolveCatalogReference();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void ResolveCatalogReference()
    {
        if (catalog != null) return;

#if UNITY_EDITOR
        catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildContentCatalog>(
            "Assets/_Game/Config/M6_BuildContentCatalog.asset");
        if (catalog != null)
        {
            Debug.LogWarning("[BuildSystemBootstrap] Przywrócono catalog z assetu (zerwana referencja sceny).");
            return;
        }
#endif

        catalog = BuildContentFactory.CreateDefaultCatalog();
        // Runtime fallback is a supported path for prototype scenes (including
        // Siege), so it should not look like a gameplay error in the console.
        Debug.Log("[BuildSystemBootstrap] Używam runtime BuildContentFactory (brak assetu katalogu).");
    }

    public static BuildContentCatalog GetCatalogOrDefault()
    {
        if (Instance != null && Instance.catalog != null)
            return Instance.catalog;

        return BuildContentFactory.CreateDefaultCatalog();
    }
}
