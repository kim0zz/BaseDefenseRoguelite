using UnityEngine;

/// <summary>
/// Falloff wstrząsu kamery od odległości zdarzenia (M7.5-T3).
/// </summary>
public static class SkillShakeMath
{
    public const float FullStrengthDistance = 6f;
    public const float ZeroStrengthDistance = 24f;

    public static float FalloffFromDistance(float distance)
    {
        if (distance <= FullStrengthDistance) return 1f;
        if (distance >= ZeroStrengthDistance) return 0f;
        return 1f - (distance - FullStrengthDistance) / (ZeroStrengthDistance - FullStrengthDistance);
    }
}
