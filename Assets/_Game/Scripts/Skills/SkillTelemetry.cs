using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Telemetria debug skilla — per skillId + kompatybilność overload bez id (M7.5-T7, M7.6-T8).
/// </summary>
public static class SkillTelemetry
{
    public const string UnknownSkillId = "unknown";
    public const int TrackedSlotCount = 3;

    private static string _activeSkillId = UnknownSkillId;
    private static readonly Dictionary<string, SkillStats> _bySkill = new();

    public static int Uses => GetStats(_activeSkillId).Uses;
    public static int UsesWithHit => GetStats(_activeSkillId).UsesWithHit;
    public static float HitRate => GetStats(_activeSkillId).HitRate;
    public static float AverageTargets => GetStats(_activeSkillId).AverageTargets;
    public static float TotalDamage => GetStats(_activeSkillId).TotalDamage;
    public static float TotalControlSeconds => GetStats(_activeSkillId).TotalControlSeconds;
    public static int CancelledUses => GetStats(_activeSkillId).CancelledUses;
    public static int MultiLaneUses => GetStats(_activeSkillId).MultiLaneUses;

    public static void SetActiveSkill(string skillId)
    {
        _activeSkillId = string.IsNullOrEmpty(skillId) ? UnknownSkillId : skillId;
        GetStats(_activeSkillId);
    }

    public static void RecordUse(SkillHitResolver.ResolveResult result, bool cancelled)
    {
        RecordUse(_activeSkillId, result, cancelled);
    }

    public static void RecordUse(string skillId, SkillHitResolver.ResolveResult result, bool cancelled)
    {
        var id = string.IsNullOrEmpty(skillId) ? UnknownSkillId : skillId;
        var stats = GetStats(id);

        if (cancelled)
        {
            stats.CancelledUses++;
            return;
        }

        stats.Uses++;
        if (result.HadHit)
        {
            stats.UsesWithHit++;
            stats.TotalTargets += result.HitCount;
            stats.TotalDamage += result.TotalDamage;
            stats.TotalControlSeconds += result.TotalControlSeconds;
            if (result.DistinctLaneCount >= 2)
                stats.MultiLaneUses++;
        }
    }

    public static void RecordCancelledWindup()
    {
        RecordUse(_activeSkillId, default, cancelled: true);
    }

    public static void RecordCancelledWindup(string skillId)
    {
        RecordUse(skillId, default, cancelled: true);
    }

    public static string BuildSummary(string skillId)
    {
        var id = string.IsNullOrEmpty(skillId) ? UnknownSkillId : skillId;
        var stats = GetStats(id);
        var sb = new StringBuilder();
        sb.AppendLine($"[SkillTelemetry] {id}");
        sb.AppendLine($"  uses={stats.Uses} hit%={stats.HitRate:P0} avgTargets={stats.AverageTargets:0.##}");
        sb.AppendLine($"  damage={stats.TotalDamage:0.#} control={stats.TotalControlSeconds:0.##}s");
        sb.AppendLine($"  cancelled={stats.CancelledUses} multiLane={stats.MultiLaneUses}");
        return sb.ToString();
    }

    public static string BuildAllSummaries(IReadOnlyList<string> skillIds)
    {
        var sb = new StringBuilder();
        var count = skillIds != null ? Mathf.Min(skillIds.Count, TrackedSlotCount) : 0;
        for (var i = 0; i < TrackedSlotCount; i++)
        {
            var id = i < count && !string.IsNullOrEmpty(skillIds[i])
                ? skillIds[i]
                : UnknownSkillId;
            sb.Append(BuildSummary(id));
            if (i < TrackedSlotCount - 1)
                sb.AppendLine();
        }

        return sb.ToString();
    }

    public static void LogSummary(string skillId)
    {
        Debug.Log(BuildSummary(skillId));
    }

    public static void LogAllSummaries(IReadOnlyList<string> skillIds)
    {
        Debug.Log(BuildAllSummaries(skillIds));
    }

    public static void Reset()
    {
        _activeSkillId = UnknownSkillId;
        _bySkill.Clear();
    }

    private static SkillStats GetStats(string skillId)
    {
        var id = string.IsNullOrEmpty(skillId) ? UnknownSkillId : skillId;
        if (!_bySkill.TryGetValue(id, out var stats))
        {
            stats = new SkillStats();
            _bySkill[id] = stats;
        }

        return stats;
    }

    private sealed class SkillStats
    {
        public int Uses;
        public int UsesWithHit;
        public int TotalTargets;
        public float TotalDamage;
        public float TotalControlSeconds;
        public int CancelledUses;
        public int MultiLaneUses;

        public float HitRate => Uses > 0 ? UsesWithHit / (float)Uses : 0f;
        public float AverageTargets => Uses > 0 ? TotalTargets / (float)Uses : 0f;
    }
}
