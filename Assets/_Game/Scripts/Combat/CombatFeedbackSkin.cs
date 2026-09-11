using UnityEngine;

/// <summary>
/// Sloty globalnego feedbacku walki (M9.1) — null = wbudowany placeholder.
/// </summary>
[CreateAssetMenu(fileName = "CombatFeedbackSkin", menuName = "Game/Combat/Combat Feedback Skin")]
public class CombatFeedbackSkin : ScriptableObject
{
    [Header("VFX prefabs — null = LineRenderer placeholder")]
    [SerializeField] private GameObject stunPrefab;
    [SerializeField] private GameObject auraPrefab;
    [SerializeField] private GameObject kolosPrefab;

    [Header("SFX — null = generated sine placeholder")]
    [SerializeField] private AudioClip defaultWindupClip;
    [SerializeField] private AudioClip defaultImpactClip;
    [SerializeField] private AudioClip stunClip;

    [Header("Placeholder colors")]
    [SerializeField] private Color stunColor = new(1f, 0.92f, 0.2f, 0.85f);
    [SerializeField] private Color auraColor = new(0.95f, 0.35f, 0.12f, 0.55f);
    [SerializeField] private Color kolosColor = new(0.85f, 0.55f, 0.15f, 0.65f);

    public GameObject StunPrefab => stunPrefab;
    public GameObject AuraPrefab => auraPrefab;
    public GameObject KolosPrefab => kolosPrefab;
    public AudioClip DefaultWindupClip => defaultWindupClip;
    public AudioClip DefaultImpactClip => defaultImpactClip;
    public AudioClip StunClip => stunClip;
    public Color StunColor => stunColor;
    public Color AuraColor => auraColor;
    public Color KolosColor => kolosColor;

    private static CombatFeedbackSkin _runtimeDefault;

    public static CombatFeedbackSkin GetOrDefault()
    {
#if UNITY_EDITOR
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CombatFeedbackSkin>(
            "Assets/_Game/Config/CombatFeedbackSkin.asset");
        if (asset != null)
            return asset;
#endif
        if (_runtimeDefault == null)
        {
            _runtimeDefault = CreateInstance<CombatFeedbackSkin>();
            _runtimeDefault.hideFlags = HideFlags.HideAndDontSave;
        }

        return _runtimeDefault;
    }
}
