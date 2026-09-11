using UnityEngine;

/// <summary>
/// Odtwarzanie SFX windup/impact/stun ze slotów (M9.1b).
/// </summary>
[DisallowMultipleComponent]
public class SkillFeedbackAudio : MonoBehaviour
{
    private AudioSource _source;

    public static SkillFeedbackAudio Ensure()
    {
        var feel = CombatFeelService.Ensure();
        var existing = feel.GetComponent<SkillFeedbackAudio>();
        if (existing != null) return existing;
        return feel.gameObject.AddComponent<SkillFeedbackAudio>();
    }

    public void PlayWindup(SkillDefinition skill)
    {
        var skin = CombatFeedbackSkin.GetOrDefault();
        var clip = SkillFeedbackResolver.Resolve(skill != null ? skill.WindupClip : null, skin.DefaultWindupClip)
            ?? PlaceholderSfx.GetWindup();
        PlayClip(clip);
    }

    public void PlayImpact(SkillDefinition skill)
    {
        var skin = CombatFeedbackSkin.GetOrDefault();
        var clip = SkillFeedbackResolver.Resolve(skill != null ? skill.ImpactClip : null, skin.DefaultImpactClip)
            ?? PlaceholderSfx.GetImpact();
        PlayClip(clip);
    }

    public void PlayStun()
    {
        var skin = CombatFeedbackSkin.GetOrDefault();
        var clip = SkillFeedbackResolver.Resolve(skin.StunClip, null) ?? PlaceholderSfx.GetStun();
        PlayClip(clip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;

        EnsureListener();
        EnsureSource();
        _source.ignoreListenerPause = true;
        _source.mute = false;
        _source.volume = 1f;
        _source.spatialBlend = 0f;
        _source.PlayOneShot(clip, 1f);
    }

    private void EnsureSource()
    {
        if (_source != null) return;
        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 0f;
        _source.ignoreListenerPause = true;
        _source.bypassListenerEffects = true;
    }

    private static void EnsureListener()
    {
        if (FindAnyObjectByType<AudioListener>() != null) return;

        var shared = FindAnyObjectByType<SharedCamera>();
        if (shared != null)
        {
            shared.gameObject.AddComponent<AudioListener>();
            return;
        }

        var camera = Camera.main;
        if (camera != null)
            camera.gameObject.AddComponent<AudioListener>();
    }
}
