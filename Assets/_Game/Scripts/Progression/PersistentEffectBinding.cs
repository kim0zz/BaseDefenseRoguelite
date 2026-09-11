using System;

/// <summary>
/// Persistent effect kind + per-card tuning (M8.1b).
/// </summary>
[Serializable]
public struct PersistentEffectBinding
{
    public PersistentEffectKind Kind;
    public EffectTuning Tuning;

    public PersistentEffectBinding(PersistentEffectKind kind, EffectTuning tuning = null)
    {
        Kind = kind;
        Tuning = tuning;
    }
}
