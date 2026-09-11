using UnityEngine;

/// <summary>
/// Null-safe resolver slotów feedbacku (M9.1). Unity fake-null (puste pole w Inspectorze) = brak slotu.
/// </summary>
public static class SkillFeedbackResolver
{
    public static T Resolve<T>(T authored, T fallback) where T : class =>
        HasAuthored(authored) ? authored : fallback;

    public static bool HasAuthored(Object authored) => authored != null;

    public static bool HasAuthored(object authored)
    {
        if (authored == null)
            return false;
        if (authored is Object unityObject)
            return unityObject != null;
        return true;
    }
}
