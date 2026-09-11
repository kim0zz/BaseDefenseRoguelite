using UnityEngine;

/// <summary>
/// Runtime bootstrap Canvas HUD (M9.0) — tworzy CombatHudView gdy brak w scenie.
/// </summary>
[DisallowMultipleComponent]
public class HudCanvasBootstrap : MonoBehaviour
{
    public static void EnsureExists()
    {
        if (FindAnyObjectByType<CombatHudView>() != null)
            return;

        if (FindAnyObjectByType<HudCanvasBootstrap>() != null)
            return;

        var go = new GameObject("HudCanvas");
        go.AddComponent<HudCanvasBootstrap>();
    }

    private void Awake()
    {
        if (GetComponent<CombatHudView>() == null)
            gameObject.AddComponent<CombatHudView>();
    }
}
