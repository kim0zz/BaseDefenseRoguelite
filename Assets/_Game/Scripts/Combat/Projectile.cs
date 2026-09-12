using UnityEngine;

/// <summary>
/// Pocisk — liniowy (łuk) lub wybuchowy z łukiem (M8.4).
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
    private ProjectileImpactMode _impactMode;
    private float _splashRadius;
    private float _arcHeight;
    private int _maxTargets;
    private float _bossStagger;
    private bool _consumed;
    private float _arcProgress;

    public static Projectile Spawn(
        Vector3 origin,
        Vector3 direction,
        float speed,
        float maxDistance,
        float damage,
        GameObject owner,
        WeaponFeelProfile feel)
    {
        return Spawn(origin, direction, speed, maxDistance, damage, owner, feel, ProjectileProfile.Direct);
    }

    public static Projectile Spawn(
        Vector3 origin,
        Vector3 direction,
        float speed,
        float maxDistance,
        float damage,
        GameObject owner,
        WeaponFeelProfile feel,
        ProjectileProfile profile)
    {
        var isExplosive = profile.ImpactMode == ProjectileImpactMode.ExplodeOnFirstHitOrObstacle;
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = isExplosive ? "ThrownExplosive" : "BowProjectile";
        go.transform.position = origin;
        go.transform.localScale = Vector3.one * (isExplosive ? 0.28f : 0.22f);

        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            var color = isExplosive
                ? new Color(1f, 0.55f, 0.15f, 1f)
                : new Color(0.95f, 0.85f, 0.35f, 1f);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else
                mat.color = color;
            renderer.material = mat;
        }

        var projectile = go.AddComponent<Projectile>();
        projectile._direction = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;
        projectile._direction.y = 0f;
        projectile._speed = speed > 0f ? speed : ProjectileRules.ProjectileSpeed;
        projectile._maxDistance = Mathf.Max(0.1f, maxDistance);
        projectile._damage = damage;
        projectile._owner = owner;
        projectile._feel = feel;
        projectile._impactMode = profile.ImpactMode;
        projectile._splashRadius = profile.SplashRadius;
        projectile._arcHeight = profile.ArcHeight;
        projectile._maxTargets = profile.MaxTargets;
        projectile._bossStagger = profile.BossStagger;
        go.transform.rotation = Quaternion.LookRotation(projectile._direction, Vector3.up);
        return projectile;
    }

    private void Update()
    {
        if (_consumed) return;

        var step = _speed * Time.deltaTime;
        _travelled += step;
        _arcProgress = _maxDistance > 0f ? Mathf.Clamp01(_travelled / _maxDistance) : 1f;

        var basePos = transform.position;
        var nextFlat = basePos + _direction * step;
        if (_arcHeight > 0f)
            nextFlat.y = Mathf.Sin(_arcProgress * Mathf.PI) * _arcHeight;
        transform.position = nextFlat;

        if (_impactMode == ProjectileImpactMode.Direct)
        {
            if (TryDirectHit())
            {
                Consume();
                return;
            }
        }
        else
        {
            if (TryExplosiveImpact())
            {
                Consume();
                return;
            }
        }

        if (ProjectileRules.HasReachedMaxDistance(_travelled, _maxDistance))
        {
            if (_impactMode == ProjectileImpactMode.ExplodeOnFirstHitOrObstacle)
                ExplodeAt(transform.position);
            Consume();
        }
    }

    private bool TryDirectHit()
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

    private bool TryExplosiveImpact()
    {
        if (ProjectileRules.IsObstacleAt(transform.position, _owner))
        {
            ExplodeAt(transform.position);
            return true;
        }

        var hits = Physics.OverlapSphere(transform.position, ProjectileRules.OverlapRadius);
        foreach (var hit in hits)
        {
            if (!ProjectileRules.IsValidTarget(hit, _owner)) continue;
            ExplodeAt(transform.position);
            return true;
        }

        return false;
    }

    private void ExplodeAt(Vector3 position)
    {
        var splash = _splashRadius > 0f ? _splashRadius : DeployableTuning.ThrownExplosiveDefaultSplash;
        var persistents = _owner != null ? _owner.GetComponent<PlayerPersistentEffects>() : null;
        var damage = _damage;
        if (persistents != null)
            damage *= persistents.GetBasicDamageMultiplier();

        if (persistents != null)
            splash = persistents.GetBasicSplashRadius(splash > 0f ? splash : DeployableTuning.BasicSplashRadius);

        ExplosionResolver.Explode(
            position,
            splash,
            damage,
            _owner,
            _maxTargets > 0 ? _maxTargets : 5,
            _bossStagger,
            canApplyBurn: true,
            applyPerTargetStacks: true);

        if (persistents != null)
            persistents.TrySpawnRapidScorch(position, splash);

        var feel = CombatFeelService.Ensure();
        feel.RequestHitStop(_feel.HitStopSeconds);
        feel.RequestShake(_feel.Shake);
    }

    private void Consume()
    {
        if (_consumed) return;
        _consumed = true;
        Destroy(gameObject);
    }
}

public readonly struct ProjectileProfile
{
    public readonly ProjectileImpactMode ImpactMode;
    public readonly float SplashRadius;
    public readonly float ArcHeight;
    public readonly int MaxTargets;
    public readonly float BossStagger;

    public ProjectileProfile(
        ProjectileImpactMode impactMode,
        float splashRadius = 0f,
        float arcHeight = 0f,
        int maxTargets = 5,
        float bossStagger = 0f)
    {
        ImpactMode = impactMode;
        SplashRadius = splashRadius;
        ArcHeight = arcHeight;
        MaxTargets = maxTargets;
        BossStagger = bossStagger;
    }

    public static ProjectileProfile Direct => new(ProjectileImpactMode.Direct);
}
