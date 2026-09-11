using UnityEngine;

/// <summary>
/// Debug overlay telemetrii skilli — 3 bloki slotów 0–2 (M7.5-T7, M7.6-T8).
/// </summary>
[DisallowMultipleComponent]
public class SkillTelemetryDebugOverlay : MonoBehaviour
{
    [SerializeField] private bool showOverlay = true;

    private void OnGUI()
    {
        if (!showOverlay) return;
        var flow = FindAnyObjectByType<GameFlowManager>();
        if (flow != null && flow.State == GameFlowState.LevelUpPause)
            return;

        var skillIds = ResolveTrackedSkillIds();
        var rect = new Rect(Screen.width - 340f, 8f, 320f, 360f);
        GUI.Box(rect, "Skill Telemetry");
        GUI.Label(
            new Rect(rect.x + 8f, rect.y + 24f, rect.width - 16f, rect.height - 32f),
            SkillTelemetry.BuildAllSummaries(skillIds));
    }

    private void OnApplicationQuit()
    {
        SkillTelemetry.LogAllSummaries(ResolveTrackedSkillIds());
    }

    private static string[] ResolveTrackedSkillIds()
    {
        var ids = new string[SkillTelemetry.TrackedSlotCount];
        var controllers = FindObjectsByType<PlayerSkillController>();
        foreach (var controller in controllers)
        {
            if (controller == null) continue;
            var player = controller.GetComponent<PlayerCharacter>();
            if (player != null && player.PlayerIndex != 0) continue;

            for (var slot = 0; slot < SkillTelemetry.TrackedSlotCount; slot++)
            {
                var skill = controller.GetSkill(slot);
                ids[slot] = skill != null ? skill.SkillId : SkillTelemetry.UnknownSkillId;
            }

            return ids;
        }

        for (var i = 0; i < ids.Length; i++)
            ids[i] = SkillTelemetry.UnknownSkillId;
        return ids;
    }
}
