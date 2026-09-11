using UnityEngine;

/// <summary>
/// Jeden owner hit-stop + shake. Nie rusza timeScale gdy gra jest spauzowana.
/// </summary>
[DisallowMultipleComponent]
public class CombatFeelService : MonoBehaviour
{
    private float _hitStopRemaining;
    private float _scaleBeforeHitStop = 1f;
    private bool _hitStopActive;

    public static CombatFeelService Ensure()
    {
        var existing = FindAnyObjectByType<CombatFeelService>();
        if (existing != null) return existing;

        var go = new GameObject("CombatFeelService");
        return go.AddComponent<CombatFeelService>();
    }

    public void RequestHitStop(float durationUnscaledSeconds)
    {
        if (durationUnscaledSeconds <= 0f) return;

        if (_hitStopActive)
        {
            _hitStopRemaining = HitStopPolicy.ExtendRemaining(_hitStopRemaining, durationUnscaledSeconds);
            return;
        }

        if (!HitStopPolicy.CanStart(Time.timeScale)) return;

        _scaleBeforeHitStop = Time.timeScale;
        Time.timeScale = HitStopPolicy.HitStopTimeScale;
        _hitStopRemaining = durationUnscaledSeconds;
        _hitStopActive = true;
    }

    public void RequestShake(float amplitude)
    {
        if (amplitude <= 0f) return;
        var camera = FindAnyObjectByType<SharedCamera>();
        camera?.AddShake(amplitude);
    }

    /// <summary>
    /// Wstrząs od siły × odległości zdarzenia od centroidu drużyny (M7.5, FROZEN co-op).
    /// </summary>
    public void RequestShakeAtEvent(float baseStrength, Vector3 eventPosition)
    {
        if (baseStrength <= 0f) return;

        var players = FindObjectsByType<PlayerCharacter>();
        if (players.Length == 0)
        {
            RequestShake(baseStrength);
            return;
        }

        var positions = new Vector3[players.Length];
        for (var i = 0; i < players.Length; i++)
            positions[i] = players[i].transform.position;

        var centroid = SharedCameraMath.CalculateCentroid(positions);
        var distance = Vector3.Distance(
            new Vector3(eventPosition.x, 0f, eventPosition.z),
            new Vector3(centroid.x, 0f, centroid.z));
        var falloff = SkillShakeMath.FalloffFromDistance(distance);
        RequestShake(baseStrength * falloff);
    }

    public void PlayHitFeel(WeaponFeelProfile feel, Vector3 hitOrigin, KnockbackReceiver knockbackTarget)
    {
        RequestHitStop(feel.HitStopSeconds);
        RequestShake(feel.Shake);
        knockbackTarget?.AddFromOrigin(hitOrigin, feel.Knockback);
    }

    private void Update()
    {
        if (!_hitStopActive) return;

        _hitStopRemaining -= Time.unscaledDeltaTime;
        if (_hitStopRemaining > 0f) return;

        Time.timeScale = HitStopPolicy.ResolveRestoreScale(Time.timeScale, _scaleBeforeHitStop);
        _hitStopActive = false;
        _hitStopRemaining = 0f;
    }
}
