using System;
using UnityEngine;

/// <summary>Parametry sprężystej liny — data-driven, bez logiki.</summary>
[Serializable]
public struct ChainTetherTuning
{
    public float restLength;
    public float softTensionStart;
    public float hardTensionStart;
    public float maxStretch;
    public float slackSpring;
    public float softSpring;
    public float hardSpring;
    public float damping;
    public float maxPullAcceleration;
    [Range(0f, 1f)] public float playerAgencyRetention;

    /// <summary>Dawny Chaotic — punkt odniesienia po playteście.</summary>
    public static ChainTetherTuning BaselineDraft => new()
    {
        restLength = 4f,
        softTensionStart = 5f,
        hardTensionStart = 8f,
        maxStretch = 12f,
        slackSpring = 0f,
        softSpring = 16f,
        hardSpring = 40f,
        damping = 9f,
        maxPullAcceleration = 42f,
        playerAgencyRetention = 0.4f
    };

    public static ChainTetherTuning ShortDraft => new()
    {
        restLength = 3.2f,
        softTensionStart = 4f,
        hardTensionStart = 6.5f,
        maxStretch = 10f,
        slackSpring = 0f,
        softSpring = 18f,
        hardSpring = 44f,
        damping = 10f,
        maxPullAcceleration = 44f,
        playerAgencyRetention = 0.4f
    };

    public static ChainTetherTuning ExtraShortDraft => new()
    {
        restLength = 2.6f,
        softTensionStart = 3.2f,
        hardTensionStart = 5.2f,
        maxStretch = 8f,
        slackSpring = 0f,
        softSpring = 20f,
        hardSpring = 48f,
        damping = 11f,
        maxPullAcceleration = 46f,
        playerAgencyRetention = 0.38f
    };
}
