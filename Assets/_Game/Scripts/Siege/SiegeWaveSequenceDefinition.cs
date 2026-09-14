using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SiegeSequence", menuName = "Game/Siege/Sekwencja")]
public sealed class SiegeWaveSequenceDefinition : ScriptableObject
{
    public SiegeWaveDefinition[] Waves = Array.Empty<SiegeWaveDefinition>();
}
