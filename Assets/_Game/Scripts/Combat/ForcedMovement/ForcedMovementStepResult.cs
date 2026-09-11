using UnityEngine;

/// <summary>
/// Wynik pojedynczego kroku wymuszonego ruchu (M7.6-T1).
/// </summary>
public readonly struct ForcedMovementStepResult
{
    public Vector3 MovedDelta { get; }
    public ForcedMovementStepHit Hit { get; }

    public ForcedMovementStepResult(Vector3 movedDelta, ForcedMovementStepHit hit)
    {
        MovedDelta = movedDelta;
        Hit = hit;
    }
}
