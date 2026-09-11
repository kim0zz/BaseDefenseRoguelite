using UnityEngine;

/// <summary>
/// Kolory i opcjonalne sprite HUD (M9.0) — podmiana packa bez ifów skillId.
/// </summary>
[CreateAssetMenu(fileName = "HudSkin", menuName = "Game/UI/Hud Skin")]
public class HudSkin : ScriptableObject
{
    [SerializeField] private Color panelColor = new(0.06f, 0.07f, 0.10f, 0.94f);
    [SerializeField] private Color chromeColor = new(0.06f, 0.07f, 0.10f, 0.38f);
    [SerializeField] private Color accentColor = new(1f, 0.87f, 0.40f, 1f);
    [SerializeField] private Color hpFillColor = new(0.90f, 0.20f, 0.20f, 1f);
    [SerializeField] private Color slotEmptyColor = new(0.20f, 0.20f, 0.24f, 0.70f);
    [SerializeField] private Sprite panelSprite;
    [SerializeField] private Sprite barSprite;
    [SerializeField, Range(HudLayout.MinUiScale, HudLayout.MaxUiScale)]
    private float uiScale = HudLayout.DefaultUiScale;
    [SerializeField, Range(HudLayout.MinTopBarScale, HudLayout.MaxTopBarScale)]
    private float topBarScale = HudLayout.DefaultTopBarScale;

    public Color PanelColor => panelColor;
    public Color ChromeColor => chromeColor;
    public Color AccentColor => accentColor;
    public Color HpFillColor => hpFillColor;
    public Color SlotEmptyColor => slotEmptyColor;
    public Sprite PanelSprite => panelSprite;
    public Sprite BarSprite => barSprite;
    public float UiScale => HudLayout.ClampUiScale(uiScale);
    public float TopBarScale => HudLayout.ClampTopBarScale(topBarScale);

    private static HudSkin _runtimeDefault;

    public static HudSkin GetOrDefault()
    {
#if UNITY_EDITOR
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<HudSkin>("Assets/_Game/Config/HudSkin.asset");
        if (asset != null)
            return asset;
#endif
        if (_runtimeDefault == null)
        {
            _runtimeDefault = CreateInstance<HudSkin>();
            _runtimeDefault.hideFlags = HideFlags.HideAndDontSave;
        }

        return _runtimeDefault;
    }
}
