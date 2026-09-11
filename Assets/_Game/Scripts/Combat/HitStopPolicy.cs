using UnityEngine;

/// <summary>
/// Hit-stop vs pauza level-up. Nie odpauzowuje Time.timeScale == 0.
/// </summary>
public static class HitStopPolicy
{
    public const float HitStopTimeScale = 0.12f;

    public static bool CanStart(float currentTimeScale)
    {
        return Mathf.Approximately(currentTimeScale, 1f);
    }

    public static float ExtendRemaining(float remainingUnscaled, float requestedUnscaled)
    {
        return Mathf.Max(remainingUnscaled, requestedUnscaled);
    }

    public static float ResolveRestoreScale(float currentTimeScale, float scaleBeforeHitStop)
    {
        if (currentTimeScale <= 0f)
            return 0f;
        return scaleBeforeHitStop;
    }
}
