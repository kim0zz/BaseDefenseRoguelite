using UnityEngine;

/// <summary>
/// Kolejność fal w runie (data-driven, M5).
/// </summary>
[CreateAssetMenu(fileName = "WaveSequence", menuName = "Game/Waves/Wave Sequence")]
public class WaveSequenceDefinition : ScriptableObject
{
    [SerializeField] private WaveDefinition[] waves;

    public WaveDefinition[] Waves => waves;
    public int WaveCount => waves != null ? waves.Length : 0;

    public WaveDefinition GetWave(int index)
    {
        if (waves == null || index < 0 || index >= waves.Length)
            return null;
        return waves[index];
    }
}
