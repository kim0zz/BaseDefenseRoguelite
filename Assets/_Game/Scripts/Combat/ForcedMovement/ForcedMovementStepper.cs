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

        var enemyHit = TryDetectEnemyCollision(body, current, desired, out var enemy);
        if (enemyHit != null)
        {
            body.position = enemyHit.Value;
            var enemyMoved = enemyHit.Value - current;
            return new ForcedMovementStepResult(enemyMoved, ForcedMovementStepHit.Enemy, enemy);
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

    private static Vector3? TryDetectEnemyCollision(Transform body, Vector3 from, Vector3 to, out GameObject enemy)
    {
        enemy = null;
        if (body == null) return null;

        var direction = to - from;
        var dist = direction.magnitude;
        if (dist <= 0.001f) return null;

        var hits = Physics.RaycastAll(from + Vector3.up * 0.5f, direction.normalized, dist);
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.transform == body || hit.collider.transform.IsChildOf(body)) continue;
            if (hit.collider.GetComponentInParent<PlayerCharacter>() != null) continue;
            var ec = hit.collider.GetComponentInParent<EnemyController>();
            if (ec == null) continue;
            var dmg = hit.collider.GetComponentInParent<IDamageable>();
            if (dmg == null || !dmg.IsAlive) continue;
            enemy = ec.gameObject;
            return hit.point;
        }

        return null;
    }

    private static MapPlayArea ResolvePlayArea()
    {
        if (MapPlayArea.Instance != null)
            return MapPlayArea.Instance;

        // EditMode: AddComponent nie woła Awake — fallback gdy Instance jeszcze nie ustawione.
        return Object.FindAnyObjectByType<MapPlayArea>();
    }
}
