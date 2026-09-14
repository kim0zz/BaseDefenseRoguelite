#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>Nieblokujące sondy runtime wywoływane przez edytor/MCP.</summary>
public static class SiegeRuntimeValidation
{
    private static readonly List<float> FrameTimes = new();
    private static int _frames, _limit, _lastFrame = -1;
    private static string _label;
    private static bool _running;

    public static void StartPerformanceProbe(int frames = 100, string label = "stationary")
    {
        _limit = Mathf.Clamp(frames, 25, 1000);
        _label = string.IsNullOrEmpty(label) ? "stationary" : label;
        _frames = 0; _lastFrame = -1; FrameTimes.Clear(); _running = true;
        EditorApplication.update -= PollPerformance;
        EditorApplication.update += PollPerformance;
        Debug.Log($"[SiegeValidation] Performance probe started: {_label}, {_limit} frames.");
    }

    /// <summary>Confirms the currently visible first option for every player without input.</summary>
    public static int AutoConfirmOpenRewards()
    {
        var count = 0;
        foreach (var rewards in UnityEngine.Object.FindObjectsByType<SiegeRewardController>())
        {
            var choicesField = typeof(SiegeRewardController).GetField("_choices", BindingFlags.Instance | BindingFlags.NonPublic);
            var confirm = typeof(SiegeRewardController).GetMethod("Confirm", BindingFlags.Instance | BindingFlags.NonPublic);
            var choices = choicesField?.GetValue(rewards) as IEnumerable;
            if (choices == null || confirm == null) continue;
            foreach (var choice in choices)
            {
                var type = choice.GetType();
                var ready = (bool)(type.GetField("Ready", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(choice) ?? true);
                if (ready) continue;
                type.GetField("Selected", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(choice, 0);
                confirm.Invoke(rewards, new[] { choice });
                count++;
            }
        }
        Debug.Log($"[SiegeValidation] Auto-confirmed reward choices: {count}.");
        return count;
    }

    private static void PollPerformance()
    {
        if (!_running || !Application.isPlaying || Time.frameCount == _lastFrame) return;
        _lastFrame = Time.frameCount;
        FrameTimes.Add(Time.unscaledDeltaTime);
        _frames++;
        if (_frames >= _limit) Finish();
    }

    private static void Finish()
    {
        _running = false;
        EditorApplication.update -= PollPerformance;
        var total = 0f;
        foreach (var value in FrameTimes) total += value;
        var average = total / Mathf.Max(1, FrameTimes.Count);
        var fps = average > 0f ? 1f / average : 0f;
        var path = Path.GetFullPath(Path.Combine("docs", "plans", "siege-performance.csv"));
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.AppendAllText(path, $"{DateTime.UtcNow:O},{_label},{FrameTimes.Count},{fps:0.00},{QualitySettings.vSyncCount}\n");
        AssetDatabase.Refresh();
        Debug.Log($"[SiegeValidation] Performance probe finished: {_label}, avg FPS {fps:0.00}. Results: {path}");
    }

    [MenuItem("Game/Siege/Auto-confirm Open Rewards")]
    private static void MenuConfirm() => AutoConfirmOpenRewards();

    [MenuItem("Game/Siege/Probe Performance (100 frames)")]
    private static void MenuProbe() => StartPerformanceProbe(100, "stationary");
}
#endif
