using UnityEngine;

/// <summary>
/// Domyślne wymiary greyboxu M6.5 — wspólne dla testów EditMode i MapGreyboxBuilder.
/// </summary>
public static class MapGreyboxLayout
{
    public const float LaneVisualWidth = 3f;
    public static float LaneHalfWidth => LaneVisualWidth * 0.5f;

    public static Vector2 PlayAreaX => new(-20f, 20f);
    public static Vector2 PlayAreaZ => new(-16f, 28f);
    public static Vector2 GroundSize => new(50f, 50f);

    public static Vector3 BaseCore => new(0f, 0f, -10f);
    public static Vector3 BaseTowerLeft => new(-6f, 0f, -6f);
    public static Vector3 BaseTowerRight => new(6f, 0f, -6f);

    public static Vector3 CenterLineTower => new(0f, 0f, 8f);
    public static Vector3 LeftLineTower => new(-14f, 0f, 8f);
    public static Vector3 RightLineTower => new(14f, 0f, 8f);

    public static Vector3 CenterLaneFarEnd => new(0f, 0f, 26f);
    public static Vector3 LeftLaneFarEnd => new(-16f, 0f, 26f);
    public static Vector3 RightLaneFarEnd => new(16f, 0f, 26f);

    public static float ChokeZ => 14f;
    public const float FlankerBypassLateralOffset = 3.5f;
    public const float FlankerBypassHalfWidth = 2.2f;

    public static Vector3 GetChokePosition(AttackLineId line)
    {
        var x = line switch
        {
            AttackLineId.Left => LeftLaneFarEnd.x,
            AttackLineId.Right => RightLaneFarEnd.x,
            _ => CenterLaneFarEnd.x
        };
        return new Vector3(x, 0f, ChokeZ);
    }

    public static Vector3 GetFarEnd(AttackLineId line) => line switch
    {
        AttackLineId.Left => LeftLaneFarEnd,
        AttackLineId.Right => RightLaneFarEnd,
        _ => CenterLaneFarEnd
    };

    public static Vector3 GetLineTower(AttackLineId line) => line switch
    {
        AttackLineId.Left => LeftLineTower,
        AttackLineId.Right => RightLineTower,
        _ => CenterLineTower
    };

    public static LanePath GetLanePath(AttackLineId line)
    {
        return LanePath.Create(
            GetFarEnd(line),
            GetChokePosition(line),
            GetLineTower(line),
            BaseCore,
            LaneHalfWidth);
    }

    public static Vector3 GetBypassWaypoint(AttackLineId line)
    {
        var choke = GetChokePosition(line);
        var sign = line == AttackLineId.Left ? -1f : 1f;
        return new Vector3(choke.x + sign * FlankerBypassLateralOffset, choke.y, ChokeZ);
    }

    public static LanePath GetBypassLanePath(AttackLineId line)
    {
        return LanePath.Create(
            GetFarEnd(line),
            GetBypassWaypoint(line),
            GetLineTower(line),
            BaseCore,
            FlankerBypassHalfWidth);
    }

    public static bool PlayAreaContains(Vector3 position)
    {
        return position.x >= PlayAreaX.x && position.x <= PlayAreaX.y
            && position.z >= PlayAreaZ.x && position.z <= PlayAreaZ.y;
    }

    public static bool PlayAreaContainsAllSpawnsAndCore()
    {
        return PlayAreaContains(BaseCore)
            && PlayAreaContains(CenterLaneFarEnd)
            && PlayAreaContains(LeftLaneFarEnd)
            && PlayAreaContains(RightLaneFarEnd);
    }
}
