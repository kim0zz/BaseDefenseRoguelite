using System.Collections.Generic;
using UnityEngine;

public enum DeployableMotorState
{
    Idle,
    Armed,
    Kicked,
    Homing,
    Orbiting,
    Recharging
}

/// <summary>
/// Ruch i stany ładunku — kick, mine, homing, orbit (M8.4).
/// </summary>
[DisallowMultipleComponent]
public class DeployableMotor : MonoBehaviour
{
    private Deployable _deployable;
    private DeployableMotorState _state = DeployableMotorState.Idle;
    private Vector3 _kickDirection;
    private float _kickDistanceRemaining;
    private float _kickSpeed;
    private float _armTimer;
    private GameObject _homingTarget;
    private CollisionChainMotor _chainMotor;

    public DeployableMotorState State => _state;
    public GameObject HomingTargetForTests => _homingTarget;

    public void Initialize(Deployable deployable)
    {
        _deployable = deployable;
    }

    public void BeginArming()
    {
        if (_deployable == null) return;
        _state = DeployableMotorState.Armed;
        _armTimer = DeployableTuning.SaperArmSeconds;
    }

    public void Kick(Vector3 direction, float speed = -1f, float maxDistance = -1f)
    {
        if (_deployable == null) return;
        _deployable.MarkKicked();
        _state = DeployableMotorState.Kicked;
        _kickDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;
        _kickDirection.y = 0f;
        _kickSpeed = speed > 0f ? speed : DeployableTuning.KickSpeed;
        _kickDistanceRemaining = maxDistance > 0f ? maxDistance : DeployableTuning.KickMaxDistance;
    }

    public void BeginHoming()
    {
        _state = DeployableMotorState.Homing;
        RetargetHoming();
    }

    public void SetOrbiting() => _state = DeployableMotorState.Orbiting;

    public void SetRecharging() => _state = DeployableMotorState.Recharging;

    private void Update() => TickMotor(Time.deltaTime);

    public void TickMotorForTests(float deltaTime) => TickMotor(deltaTime);

    private void TickMotor(float deltaTime)
    {
        if (_deployable == null || _deployable.IsDetonated) return;

        switch (_state)
        {
            case DeployableMotorState.Idle:
                TickShoveContact();
                break;
            case DeployableMotorState.Armed:
                TickArmed(deltaTime);
                TickShoveContact();
                break;
            case DeployableMotorState.Kicked:
                TickKicked(deltaTime);
                break;
            case DeployableMotorState.Homing:
                TickHoming(deltaTime);
                break;
        }

        if (_chainMotor != null && _chainMotor.IsActive)
            _chainMotor.Tick(deltaTime);
    }

    private void TickArmed(float deltaTime)
    {
        _armTimer -= deltaTime;
        if (_armTimer > 0f) return;

        var owner = _deployable.Owner;
        if (owner == null) return;
        var persistents = owner.GetComponent<PlayerPersistentEffects>();
        if (persistents == null || !persistents.Has(PersistentEffectKind.ArmToProximityMine)) return;

        var hits = Physics.OverlapSphere(transform.position, DeployableTuning.SaperTriggerRadius);
        foreach (var hit in hits)
        {
            if (hit.GetComponentInParent<PlayerCharacter>() != null) continue;
            var enemy = hit.GetComponentInParent<EnemyController>();
            if (enemy == null) continue;
            var dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg == null || !dmg.IsAlive) continue;
            _deployable.Detonate();
            return;
        }
    }

    private void TickShoveContact()
    {
        if (_deployable == null || _deployable.IsDetonated) return;

        var hits = Physics.OverlapSphere(transform.position, DeployableTuning.ShoveDetonateRadius);
        foreach (var hit in hits)
        {
            if (hit.GetComponentInParent<PlayerCharacter>() != null) continue;
            var enemy = hit.GetComponentInParent<EnemyController>();
            if (enemy == null) continue;
            var dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg == null || !dmg.IsAlive) continue;
            var receiver = hit.GetComponentInParent<ForcedMovementReceiver>();
            if (receiver == null || !receiver.IsShoveActive) continue;

            _deployable.Detonate();
            return;
        }
    }

    private void TickKicked(float deltaTime)
    {
        var step = _kickSpeed * deltaTime;
        if (step >= _kickDistanceRemaining)
        {
            step = _kickDistanceRemaining;
            _kickDistanceRemaining = 0f;
        }
        else
        {
            _kickDistanceRemaining -= step;
        }

        var delta = _kickDirection * step;
        var result = ForcedMovementStepper.TryMove(transform, delta, honorCollisions: true);
        if (result.Hit == ForcedMovementStepHit.Enemy)
        {
            TryEnemyContactOnKick();
            return;
        }

        if (result.Hit != ForcedMovementStepHit.None)
        {
            _state = DeployableMotorState.Idle;
            return;
        }

        if (TryEnemyContactOnKick()) return;

        if (_kickDistanceRemaining <= 0f)
            _state = DeployableMotorState.Idle;
    }

    private bool TryEnemyContactOnKick()
    {
        var hits = Physics.OverlapSphere(transform.position, 0.4f);
        foreach (var hit in hits)
        {
            if (hit.GetComponentInParent<PlayerCharacter>() != null) continue;
            var enemy = hit.GetComponentInParent<EnemyController>();
            if (enemy == null) continue;
            var dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg == null || !dmg.IsAlive) continue;

            var owner = _deployable.Owner;
            var persistents = owner != null ? owner.GetComponent<PlayerPersistentEffects>() : null;
            var profile = hit.GetComponentInParent<ForcedMovementResistanceProfile>();
            var category = profile != null ? profile.Category : ForcedMovementResistanceCategory.Normal;

            if (category == ForcedMovementResistanceCategory.Boss)
            {
                _deployable.Detonate();
                return true;
            }

            if (persistents != null && persistents.Has(PersistentEffectKind.BilliardOnKick))
            {
                StartBilliard(hit.gameObject, category);
                return true;
            }

            var stun = persistents != null && persistents.Has(PersistentEffectKind.KickExplodeStun)
                ? DeployableTuning.KickExplodeStunNormal
                : (float?)null;
            _deployable.Detonate(stunSeconds: stun);
            return true;
        }

        return false;
    }

    private void StartBilliard(GameObject enemy, ForcedMovementResistanceCategory category)
    {
        _chainMotor = GetComponent<CollisionChainMotor>() ?? gameObject.AddComponent<CollisionChainMotor>();
        var persistents = _deployable.Owner?.GetComponent<PlayerPersistentEffects>();
        var hasBreak = persistents != null && persistents.Has(PersistentEffectKind.BilliardBreakUpgrade);
        _chainMotor.Begin(
            _deployable,
            enemy,
            category,
            hasBreak,
            persistents,
            () => _deployable.Detonate());
        _state = DeployableMotorState.Kicked;
    }

    private void TickHoming(float deltaTime)
    {
        if (_homingTarget == null || !IsTargetAlive(_homingTarget))
            RetargetHoming();

        if (_homingTarget == null)
        {
            _deployable.SetFuse(DeployableTuning.HomingNoTargetFuse);
            _state = DeployableMotorState.Idle;
            return;
        }

        var targetPos = _homingTarget.transform.position;
        targetPos.y = transform.position.y;
        var toTarget = targetPos - transform.position;
        var dist = toTarget.magnitude;
        if (dist <= DeployableTuning.HomingContactRadius)
        {
            _deployable.Detonate();
            return;
        }

        var step = Mathf.Min(DeployableTuning.HomingSpeed * deltaTime, dist);
        transform.position += toTarget.normalized * step;
    }

    public void RetargetHoming()
    {
        var owner = _deployable?.Owner;
        if (owner == null) return;

        var candidates = new List<GameObject>();
        var overlaps = Physics.OverlapSphere(transform.position, DeployableTuning.HomingSplitRadius);
        foreach (var hit in overlaps)
        {
            if (hit.GetComponentInParent<PlayerCharacter>() != null) continue;
            var enemy = hit.GetComponentInParent<EnemyController>();
            if (enemy == null) continue;
            var dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg == null || !dmg.IsAlive) continue;
            var root = enemy.gameObject;
            if (!candidates.Contains(root))
                candidates.Add(root);
        }

        if (candidates.Count >= DeployableTuning.HomingSplitThreshold)
        {
            candidates.Sort((a, b) =>
                (a.transform.position - transform.position).sqrMagnitude
                    .CompareTo((b.transform.position - transform.position).sqrMagnitude));
            var slot = Mathf.Abs(_deployable.GetHashCode()) % candidates.Count;
            _homingTarget = candidates[slot];
            return;
        }

        _homingTarget = candidates.Count > 0 ? FindNearestTarget(candidates) : null;
    }

    private GameObject FindNearestTarget(List<GameObject> candidates)
    {
        GameObject best = null;
        var bestDist = float.MaxValue;
        foreach (var c in candidates)
        {
            if (c == null) continue;
            var dist = (c.transform.position - transform.position).sqrMagnitude;
            if (dist >= bestDist) continue;
            bestDist = dist;
            best = c;
        }

        return best;
    }

    private static bool IsTargetAlive(GameObject target)
    {
        if (target == null) return false;
        var dmg = target.GetComponentInParent<IDamageable>();
        return dmg != null && dmg.IsAlive;
    }
}
