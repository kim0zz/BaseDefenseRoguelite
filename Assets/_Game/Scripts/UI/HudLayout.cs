using UnityEngine;

/// <summary>
/// Jedna skala całego Canvas HUD (M9.0). Niższa referencja = większy HUD.
/// </summary>
public static class HudLayout
{
    public const float ReferenceWidth = 1920f;
    public const float ReferenceHeight = 1080f;
    public const float MinUiScale = 0.8f;
    public const float MaxUiScale = 2.5f;
    public const float DefaultUiScale = 1.45f;
    public const float MinTopBarScale = 0.55f;
    public const float MaxTopBarScale = 1.2f;
    public const float DefaultTopBarScale = 0.78f;

    public static float ClampUiScale(float uiScale)
    {
        return Mathf.Clamp(uiScale, MinUiScale, MaxUiScale);
    }

    public static float ClampTopBarScale(float topBarScale)
    {
        return Mathf.Clamp(topBarScale, MinTopBarScale, MaxTopBarScale);
    }

    public static Vector2 ScaledReferenceResolution(float uiScale)
    {
        var scale = ClampUiScale(uiScale);
        return new Vector2(ReferenceWidth / scale, ReferenceHeight / scale);
    }
}
