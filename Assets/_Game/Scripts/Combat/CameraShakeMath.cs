using UnityEngine;

/// <summary>
/// Apply-then-decay: amplituda z tabeli feel musi przeżyć więcej niż 1 klatkę.
/// </summary>
public static class CameraShakeMath
{
    public const float VisualScale = 4f;
    public const float DecayPerSecond = 1.6f;

    public static float StoreAmplitude(float current, float requested)
    {
        if (requested <= 0f) return current;
        return Mathf.Max(current, requested * VisualScale);
    }

    public static float DecayAfterApply(float amplitude, float unscaledDeltaTime)
    {
        return Mathf.MoveTowards(amplitude, 0f, DecayPerSecond * Mathf.Max(0f, unscaledDeltaTime));
    }
}
