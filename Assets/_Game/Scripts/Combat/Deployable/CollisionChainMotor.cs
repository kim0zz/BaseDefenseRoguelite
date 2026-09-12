using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Billiard chain — kolizje wróg–wróg z limitami czasu/dystansu (M8.4).
/// </summary>
[DisallowMultipleComponent]
public class CollisionChainMotor : MonoBehaviour
{
    private Deployable _deployable;
    private GameObject _carrier;
    private float _speed;
    private int _maxCollisions;
    private float _maxTime;
    private float _maxDistance;
    private int _collisionCount;
    private float _elapsed;
    private float _travelled;
    private Vector3 _direction;
    private Vector3 _startPosition;
    private Action _onComplete;
    private PlayerPersistentEffects _persistents;
    private readonly HashSet<GameObject> _hitIds = new();

    public bool IsActive { get; private set; }
    public int CollisionCount => _collisionCount;
    public float Elapsed => _elapsed;
    public float Travelled => _travelled;

    public void Begin(
        Deployable deployable,
        GameObject carrier,
        ForcedMovementResistanceCategory category,
        bool breakUpgrade,
        PlayerPersistentEffects persistents,
        Action onComplete)
    {
        _deployable = deployable;
        _carrier = carrier;
        _persistents = persistents;
        _onComplete = onComplete;
        _collisionCount = 0;
        _elapsed = 0f;
        _travelled = 0f;
        _hitIds.Clear();
        _startPosition = transform.position;

        _speed = breakUpgrade ? DeployableTuning.BilliardBreakSpeed : DeployableTuning.BilliardSpeed;
        _maxCollisions = breakUpgrade
            ? DeployableTuning.BilliardBreakMaxCollisions
            : DeployableTuning.BilliardMaxCollisions;
        _maxTime = DeployableTuning.BilliardMaxTime;
        _maxDistance = DeployableTuning.BilliardMaxDistance;

        if (category == ForcedMovementResistanceCategory.Elite)
        {
            _maxCollisions = Mathf.Min(_maxCollisions, DeployableTuning.BilliardEliteCollisionCap);
            _speed *= DeployableTuning.BilliardEliteSpeedScale;
        }

        var toCarrier = carrier.transform.position - transform.position;
        toCarrier.y = 0f;
        _direction = toCarrier.sqrMagnitude > 0.01f ? toCarrier.normalized : Vector3.forward;

        if (category != ForcedMovementResistanceCategory.Boss)
        {
            var shove = ForcedMovementRequest.ShoveAlong(
                deployable.Owner,
                _direction,
                _speed,
                _maxTime,
                honorCollisions: true);
            ForcedMovementResolver.Apply(shove, carrier);
        }

        IsActive = true;
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive) return;

        _elapsed += deltaTime;
        var step = _speed * deltaTime;
        _travelled += step;

        var delta = _direction * step;
        var result = ForcedMovementStepper.TryMove(transform, delta, honorCollisions: true);
        if (result.Hit != ForcedMovementStepHit.None && result.Hit != ForcedMovementStepHit.Enemy)
        {
            Complete();
            return;
        }

        if (result.Hit == ForcedMovementStepHit.Enemy && result.OtherEnemy != null)
            RegisterCollision(result.OtherEnemy);

        TryDetectEnemyOverlap();

        if (_collisionCount >= _maxCollisions || _elapsed >= _maxTime || _travelled >= _maxDistance)
            Complete();
    }

    private void TryDetectEnemyOverlap()
    {
        var hits = Physics.OverlapSphere(transform.position, 0.45f);
        foreach (var hit in hits)
        {
            if (hit.GetComponentInParent<PlayerCharacter>() != null) continue;
            var enemy = hit.GetComponentInParent<EnemyController>();
            if (enemy == null) continue;
            RegisterCollision(enemy.gameObject);
        }
    }

    private void RegisterCollision(GameObject enemy)
    {
        if (_hitIds.Contains(enemy)) return;
        _hitIds.Add(enemy);
        _collisionCount++;

        if (_persistents != null && _persistents.Has(PersistentEffectKind.CollisionScalingExplosion))
            _persistents.RegisterBilliardCollision();

        var profile = enemy.GetComponentInParent<ForcedMovementResistanceProfile>();
        var category = profile != null ? profile.Category : ForcedMovementResistanceCategory.Normal;
        if (category == ForcedMovementResistanceCategory.Boss)
        {
            Complete();
            return;
        }

        if (category != ForcedMovementResistanceCategory.Boss)
        {
            var shove = ForcedMovementRequest.ShoveAlong(
                _deployable.Owner,
                _direction,
                _speed,
                _maxTime - _elapsed,
                honorCollisions: true);
            ForcedMovementResolver.Apply(shove, enemy);
        }
    }

    private void Complete()
    {
        if (!IsActive) return;
        IsActive = false;
        transform.position = _carrier != null ? _carrier.transform.position : transform.position;
        _onComplete?.Invoke();
    }

    public static bool WouldExceedLimits(int collisions, float elapsed, float travelled, bool elite)
    {
        var maxColl = elite
            ? DeployableTuning.BilliardEliteCollisionCap
            : DeployableTuning.BilliardMaxCollisions;
        return collisions >= maxColl
            || elapsed >= DeployableTuning.BilliardMaxTime
            || travelled >= DeployableTuning.BilliardMaxDistance;
    }
}
