using UnityEngine;

/// <summary>
/// Pocisk-placeholder łuku (CR-T6).
/// </summary>
[DisallowMultipleComponent]
public class Projectile : MonoBehaviour
{
    private Vector3 _direction;
    private float _speed;
    private float _maxDistance;
    private float _damage;
    private float _travelled;
    private GameObject _owner;
    private WeaponFeelProfile _feel;
    private bool _consumed;

    public static Projectile Spawn(
        Vector3 origin,
        Vector3 direction,
        float speed,
        float maxDistance,
        float damage,
        GameObject owner,
        WeaponFeelProfile feel)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "BowProjectile";
        go.transform.position = origin;
        go.transform.localScale = Vector3.one * 0.22f;

        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            var color = new Color(0.95f, 0.85f, 0.35f, 1f);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else
                mat.color = color;
            renderer.material = mat;
        }

        var projectile = go.AddComponent<Projectile>();
        projectile._direction = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;
        projectile._speed = speed > 0f ? speed : ProjectileRules.ProjectileSpeed;
        projectile._maxDistance = Mathf.Max(0.1f, maxDistance);
        projectile._damage = damage;
        projectile._owner = owner;
        projectile._feel = feel;
        go.transform.rotation = Quaternion.LookRotation(projectile._direction, Vector3.up);
        return projectile;
    }

    private void Update()
    {
        if (_consumed) return;

        var step = _speed * Time.deltaTime;
        transform.position += _direction * step;
        _travelled += step;

        if (TryHit())
        {
            Consume();
            return;
        }

        if (ProjectileRules.HasReachedMaxDistance(_travelled, _maxDistance))
            Consume();
    }

    private bool TryHit()
    {
        var hits = Physics.OverlapSphere(transform.position, ProjectileRules.OverlapRadius);
        foreach (var hit in hits)
        {
            if (!ProjectileRules.IsValidTarget(hit, _owner)) continue;

            var damageable = hit.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage, _owner);

            var flash = hit.GetComponentInParent<HitFlashFeedback>();
            flash?.PlayFlash();

            var knockback = hit.GetComponentInParent<KnockbackReceiver>();
            CombatFeelService.Ensure().PlayHitFeel(_feel, transform.position, knockback);
            return true;
        }

        return false;
    }

    private void Consume()
    {
        if (_consumed) return;
        _consumed = true;
        Destroy(gameObject);
    }
}
