using UnityEngine;

/// <summary>
/// Wspólna ścieżka przemieszczenia ciała — jedyny punkt ustawiania pozycji (M7.6-T1).
/// </summary>
public static class ForcedMovementStepper
{
    public static ForcedMovementStepResult TryMove(Transform body, Vector3 delta, bool honorCollisions)
    {
        if (body == null || delta.sqrMagnitude <= 0f)
            return new ForcedMovementStepResult(Vector3.zero, ForcedMovementStepHit.None);

        var current = body.position;
        var desired = current + delta;

        if (!honorCollisions)
        {
            body.position = desired;
            return new ForcedMovementStepResult(delta, ForcedMovementStepHit.None);
        }

        var playArea = ResolvePlayArea();
        if (playArea == null)
        {
            body.position = desired;
            return new ForcedMovementStepResult(delta, ForcedMovementStepHit.None);
        }

        if (playArea.Contains(desired))
        {
            body.position = desired;
            return new ForcedMovementStepResult(delta, ForcedMovementStepHit.None);
        }

        var clamped = playArea.Clamp(desired);
        body.position = clamped;
        var moved = clamped - current;
        return new ForcedMovementStepResult(moved, ForcedMovementStepHit.PlayAreaEdge);
    }

    private static MapPlayArea ResolvePlayArea()
    {
        if (MapPlayArea.Instance != null)
            return MapPlayArea.Instance;

        // EditMode: AddComponent nie woła Awake — fallback gdy Instance jeszcze nie ustawione.
        return Object.FindAnyObjectByType<MapPlayArea>();
    }
}
