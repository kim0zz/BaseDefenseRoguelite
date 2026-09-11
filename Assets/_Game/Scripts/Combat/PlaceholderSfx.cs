using UnityEngine;

/// <summary>
/// Krótkie tony sine jako placeholder SFX skilli (M9.1b). Cache statyczny.
/// </summary>
public static class PlaceholderSfx
{
    public const float WindupFrequencyHz = 420f;
    public const float ImpactFrequencyHz = 180f;
    public const float StunFrequencyHz = 880f;
    public const float WindupDurationSeconds = 0.22f;
    public const float ImpactDurationSeconds = 0.22f;
    public const float StunDurationSeconds = 0.20f;

    private static AudioClip _windup;
    private static AudioClip _impact;
    private static AudioClip _stun;

    public static AudioClip GetWindup() =>
        _windup ??= CreateSineClip("PlaceholderSfx_Windup", WindupFrequencyHz, WindupDurationSeconds);

    public static AudioClip GetImpact() =>
        _impact ??= CreateSineClip("PlaceholderSfx_Impact", ImpactFrequencyHz, ImpactDurationSeconds);

    public static AudioClip GetStun() =>
        _stun ??= CreateSineClip("PlaceholderSfx_Stun", StunFrequencyHz, StunDurationSeconds);

    private static AudioClip CreateSineClip(string name, float frequencyHz, float durationSeconds)
    {
        const int sampleRate = 44100;
        var sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
        var samples = new float[sampleCount];
        var fadeSamples = Mathf.Max(1, sampleCount / 8);

        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)sampleRate;
            var envelope = 1f;
            if (i < fadeSamples)
                envelope = i / (float)fadeSamples;
            else if (i >= sampleCount - fadeSamples)
                envelope = (sampleCount - i) / (float)fadeSamples;

            samples[i] = Mathf.Sin(2f * Mathf.PI * frequencyHz * t) * envelope * 0.85f;
        }

        var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
