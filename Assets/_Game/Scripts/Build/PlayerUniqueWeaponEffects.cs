using UnityEngine;

/// <summary>
/// Efekty unikatów bossa na aktywnej broni (M7-T6).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerCharacter))]
[RequireComponent(typeof(PlayerAttackController))]
[RequireComponent(typeof(PlayerBuildState))]
public class PlayerUniqueWeaponEffects : MonoBehaviour
{
    private const float HornKnockbackThreshold = 3.5f;
    private const float HornBuffDuration = 2.5f;
    private const float HornBuffMagnitude = 0.25f;
    private const float PikeMoveSecondsRequired = 2f;
    private const float PikeDamageBonus = 1.35f;
    private const float RamhammerHitThreshold = 5;
    private const float RamhammerShockwaveRadius = 3f;
    private const float RamhammerShockwaveDamage = 10f;

    private PlayerCharacter _player;
    private PlayerBuildState _build;
    private PlayerAttackController _attack;
    private StatusEffectReceiver _status;
    private float _continuousMoveTime;
    private Vector3 _lastMoveCheckPosition;

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _build = GetComponent<PlayerBuildState>();
        _attack = GetComponent<PlayerAttackController>();
        _status = GetComponent<StatusEffectReceiver>();
        if (_status == null)
            _status = gameObject.AddComponent<StatusEffectReceiver>();
    }

    private void Update()
    {
        TrackContinuousMovement();
    }

    public float GetDamageMultiplier()
    {
        var weapon = _build?.GetWeapon(_build.ActiveWeaponIndex);
        if (weapon == null || !weapon.IsBossUnique) return 1f;
        if (weapon.WeaponId != "charging_pike") return 1f;
        return _continuousMoveTime >= PikeMoveSecondsRequired ? PikeDamageBonus : 1f;
    }

    public void OnMeleeHitResolved(System.Collections.Generic.List<IDamageable> targets, float knockbackForce)
    {
        var weapon = _build?.GetWeapon(_build.ActiveWeaponIndex);
        if (weapon == null || !weapon.IsBossUnique) return;

        switch (weapon.WeaponId)
        {
            case "ramhammer":
                if (targets.Count >= RamhammerHitThreshold)
                    TriggerRamhammerShockwave();
                break;
            case "horn_of_momentum":
                if (knockbackForce >= HornKnockbackThreshold)
                    _status?.Apply(StatusEffectType.MoveSpeedBuff, HornBuffDuration, HornBuffMagnitude, gameObject);
                break;
        }
    }

    private void TrackContinuousMovement()
    {
        if (!_player.IsCombatEnabled)
        {
            _continuousMoveTime = 0f;
            return;
        }

        var delta = transform.position - _lastMoveCheckPosition;
        delta.y = 0f;
        _lastMoveCheckPosition = transform.position;

        if (delta.sqrMagnitude > 0.0025f * Time.deltaTime)
            _continuousMoveTime += Time.deltaTime;
        else
            _continuousMoveTime = 0f;
    }

    private void TriggerRamhammerShockwave()
    {
        var hits = Physics.OverlapSphere(transform.position, RamhammerShockwaveRadius);
        foreach (var hit in hits)
        {
            var damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) continue;
            if (hit.GetComponentInParent<PlayerCharacter>() != null) continue;
            damageable.TakeDamage(RamhammerShockwaveDamage, gameObject);
            hit.GetComponentInParent<KnockbackReceiver>()?.AddFromOrigin(transform.position, 2f);
        }

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.position = transform.position + Vector3.up * 0.05f;
        ring.transform.localScale = new Vector3(RamhammerShockwaveRadius * 2f, 0.05f, RamhammerShockwaveRadius * 2f);
        var col = ring.GetComponent<Collider>();
        if (col != null) Destroy(col);
        Destroy(ring, 0.35f);
    }
}
