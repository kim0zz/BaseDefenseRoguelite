using UnityEngine;

/// <summary>
/// Wykonuje wymuszony ruch na ciele (odrzut, shove, szarża) — M7.5-T1, M7.6-T1.
/// </summary>
[DisallowMultipleComponent]
public class ForcedMovementReceiver : MonoBehaviour
{
    private const float ChargeProbeRadius = 0.45f;

    [SerializeField] private float damping = KnockbackMath.Damping;
    [SerializeField] private float activeThreshold = 0.05f;

    private Vector3 _velocity;
    private bool _restoreToLaneAfter;
    private EnemyLaneMotor _laneMotor;

    private bool _shoveActive;
    private float _shoveRemaining;
    private bool _shoveHonorCollisions;

    private bool _chargeActive;
    private float _chargeRemaining;
    private Vector3 _chargeDirection;
    private float _chargeSpeed;
    private bool _chargeHonorCollisions;
    private ChargeStopReason _chargeStopReason = ChargeStopReason.None;

    public bool IsActive =>
        _shoveActive
        || _chargeActive
        || _velocity.sqrMagnitude > activeThreshold * activeThreshold;

    public Vector3 Velocity => _velocity;
    public bool IsShoveActive => _shoveActive;
    public float ShoveRemaining => _shoveRemaining;
    public bool IsChargeActive => _chargeActive;
    public ChargeStopReason LastChargeStopReason => _chargeStopReason;

    private void Awake()
    {
        _laneMotor = GetComponent<EnemyLaneMotor>();
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    /// <summary>Symulacja ruchu — używana przez Update i testy EditMode.</summary>
    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f) return;

        if (_chargeActive)
        {
            TickCharge(deltaTime);
            return;
        }

        if (_shoveActive)
        {
            TickShove(deltaTime);
            return;
        }

        TickImpulse(deltaTime);
    }

    public void ApplyImpulse(Vector3 impulse, bool restoreToLaneAfter)
    {
        if (impulse.sqrMagnitude <= 0f) return;

        CancelCharge();
        CancelShove();
        _velocity += impulse;
        if (restoreToLaneAfter)
            _restoreToLaneAfter = true;
    }

    public void ApplyShove(Vector3 direction, float speed, float duration, bool honorCollisions = true)
    {
        if (speed <= 0f || duration <= 0f) return;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        CancelCharge();
        _shoveActive = true;
        _shoveRemaining = duration;
        _shoveHonorCollisions = honorCollisions;
        _velocity = direction.normalized * speed;
    }

    public void StartCharge(Vector3 direction, float speed, float duration, bool honorCollisions = true)
    {
        if (speed <= 0f || duration <= 0f) return;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        CancelShove();
        _chargeActive = true;
        _chargeRemaining = duration;
        _chargeDirection = direction.normalized;
        _chargeSpeed = speed;
        _chargeHonorCollisions = honorCollisions;
        _chargeStopReason = ChargeStopReason.None;
        _velocity = _chargeDirection * speed;
    }

    public void ApplySpeedMultiplier(float multiplier)
    {
        if (!_chargeActive || multiplier <= 0f) return;
        _chargeSpeed *= multiplier;
        _velocity = _chargeDirection * _chargeSpeed;
    }

    private void TickShove(float deltaTime)
    {
        var delta = _velocity * deltaTime;
        ForcedMovementStepper.TryMove(transform, delta, _shoveHonorCollisions);

        _shoveRemaining -= deltaTime;
        if (_shoveRemaining <= 0f)
            EndShove();
    }

    private void TickCharge(float deltaTime)
    {
        var stepDistance = _chargeSpeed * deltaTime;
        var origin = transform.position;
        var obstacle = ProbeResistanceObstacle(origin, _chargeDirection, stepDistance);
        if (obstacle != ChargeStopReason.None)
        {
            StopCharge(obstacle);
            return;
        }

        var step = _chargeDirection * stepDistance;
        var result = ForcedMovementStepper.TryMove(transform, step, _chargeHonorCollisions);
        if (result.Hit == ForcedMovementStepHit.PlayAreaEdge)
        {
            StopCharge(ChargeStopReason.PlayAreaEdge);
            return;
        }

        _chargeRemaining -= deltaTime;
        if (_chargeRemaining <= 0f)
            StopCharge(ChargeStopReason.Completed);
    }

    private void TickImpulse(float deltaTime)
    {
        if (_velocity.sqrMagnitude <= activeThreshold * activeThreshold)
        {
            _velocity = Vector3.zero;
            TryRestoreToLane();
            return;
        }

        var delta = _velocity * deltaTime;
        ForcedMovementStepper.TryMove(transform, delta, false);
        _velocity = Vector3.Lerp(_velocity, Vector3.zero, damping * deltaTime);

        if (_velocity.sqrMagnitude < activeThreshold * activeThreshold)
        {
            _velocity = Vector3.zero;
            TryRestoreToLane();
        }
    }

    private ChargeStopReason ProbeResistanceObstacle(Vector3 origin, Vector3 direction, float distance)
    {
        if (distance <= 0f) return ChargeStopReason.None;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return ChargeStopReason.None;
        direction.Normalize();

        var end = origin + direction * distance;
        var hits = Physics.OverlapCapsule(origin, end, ChargeProbeRadius, ~0, QueryTriggerInteraction.Ignore);
        for (var i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit == null || hit.transform == transform) continue;

            var profile = hit.GetComponentInParent<ForcedMovementResistanceProfile>();
            if (profile == null) continue;

            if (profile.Category == ForcedMovementResistanceCategory.Boss)
                return ChargeStopReason.Boss;
            if (profile.Category == ForcedMovementResistanceCategory.Structure)
                return ChargeStopReason.Structure;
        }

        if (Physics.SphereCast(origin, ChargeProbeRadius, direction, out var castHit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (castHit.collider != null && castHit.collider.transform != transform)
            {
                var profile = castHit.collider.GetComponentInParent<ForcedMovementResistanceProfile>();
                if (profile != null)
                {
                    if (profile.Category == ForcedMovementResistanceCategory.Boss)
                        return ChargeStopReason.Boss;
                    if (profile.Category == ForcedMovementResistanceCategory.Structure)
                        return ChargeStopReason.Structure;
                }
            }
        }

        return ChargeStopReason.None;
    }

    private void EndShove()
    {
        _shoveActive = false;
        _shoveRemaining = 0f;
        _velocity = Vector3.zero;
    }

    private void CancelShove()
    {
        _shoveActive = false;
        _shoveRemaining = 0f;
    }

    private void StopCharge(ChargeStopReason reason)
    {
        _chargeActive = false;
        _chargeRemaining = 0f;
        _velocity = Vector3.zero;
        _chargeStopReason = reason;
    }

    private void CancelCharge()
    {
        _chargeActive = false;
        _chargeRemaining = 0f;
    }

    private void TryRestoreToLane()
    {
        if (!_restoreToLaneAfter) return;
        _restoreToLaneAfter = false;
        _laneMotor?.ClampToLane();
    }
}
