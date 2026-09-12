using UnityEngine;

/// <summary>
/// Read-only deployable counts for combat HUD (Bomberman cap / orbitals).
/// Implemented by DeployableRegistry when runtime is ready.
/// </summary>
public interface IDeployableHudInfo
{
    int GetNormalCount(GameObject owner);
    int GetCap(GameObject owner);
    int GetOrbitalReadyCount(GameObject owner);
    int GetOrbitalTotal(GameObject owner);
}
