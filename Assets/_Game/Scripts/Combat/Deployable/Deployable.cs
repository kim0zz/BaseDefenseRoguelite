using System;
using UnityEngine;

/// <summary>
/// Pojedynczy ładunek właściciela — bomba, child, orbital, strike (M8.4).
/// </summary>
[DisallowMultipleComponent]
public class Deployable : MonoBehaviour
{
    private GameObject _owner;
    private DeployableCategory _category;
    private int _generation;
    private bool _detonatable;
    private float _damage;
    private float _radius;
    private float _bossStagger;
    private int _maxTargets;
    private float _fuseTimer = -1f;
    private bool _wasKicked;
    private DeployableMotor _motor;

    public GameObject Owner => _owner;
    public DeployableCategory Category => _category;
    public int Generation => _generation;
    public bool Detonatable => _detonatable;
    public float Damage => _damage;
    public float Radius => _radius;
    public float BossStagger => _bossStagger;
    public int MaxTargets => _maxTargets;
    public bool WasKicked => _wasKicked;
    public DeployableMotor Motor => _motor;
    public bool IsArmed => _motor != null && _motor.State == DeployableMotorState.Armed;
    public bool IsDetonated { get; private set; }

    public event Action<Deployable> Detonated;

    public void Initialize(
        GameObject owner,
        DeployableCategory category,
        float damage,
        float radius,
        float bossStagger,
        int maxTargets,
        bool detonatable,
        int generation = 0)
    {
        _owner = owner;
        _category = category;
        _damage = damage;
        _radius = radius;
        _bossStagger = bossStagger;
        _maxTargets = maxTargets;
        _detonatable = detonatable;
        _generation = generation;
        _motor = GetComponent<DeployableMotor>() ?? gameObject.AddComponent<DeployableMotor>();
        _motor.Initialize(this);
    }

    public void SetFuse(float seconds)
    {
        _fuseTimer = Mathf.Max(0f, seconds);
    }

    public void MarkKicked() => _wasKicked = true;

    public void Detonate(bool canApplyBurn = true, float? stunSeconds = null)
    {
        if (IsDetonated) return;
        IsDetonated = true;

        var origin = transform.position;
        ExplosionResolver.Explode(
            origin,
            _radius,
            _damage,
            _owner,
            _maxTargets,
            _bossStagger,
            canApplyBurn,
            stunSeconds,
            _generation,
            _category);

        Detonated?.Invoke(this);
        DeployableRegistry.Unregister(this);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (IsDetonated) return;

        if (_fuseTimer >= 0f)
        {
            _fuseTimer -= Time.deltaTime;
            if (_fuseTimer <= 0f)
                Detonate();
        }
    }

    public static Deployable SpawnPlaceholder(
        Vector3 position,
        GameObject owner,
        DeployableCategory category,
        float damage,
        float radius,
        float bossStagger,
        int maxTargets,
        bool detonatable,
        int generation = 0,
        Color? color = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = $"Deployable_{category}";
        go.transform.position = position;
        go.transform.localScale = Vector3.one * Mathf.Max(0.3f, radius * 0.35f);

        var col = go.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            var c = color ?? category switch
            {
                DeployableCategory.Child => new Color(1f, 0.6f, 0.2f),
                DeployableCategory.Orbital => new Color(0.3f, 0.7f, 1f),
                DeployableCategory.Strike => new Color(1f, 0.3f, 0.3f),
                _ => new Color(0.9f, 0.85f, 0.2f)
            };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            else mat.color = c;
            renderer.material = mat;
        }

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "RadiusRing";
        ring.transform.SetParent(go.transform, false);
        ring.transform.localPosition = Vector3.up * 0.02f;
        ring.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
        var ringCol = ring.GetComponent<Collider>();
        if (ringCol != null) Destroy(ringCol);

        var deployable = go.AddComponent<Deployable>();
        deployable.Initialize(owner, category, damage, radius, bossStagger, maxTargets, detonatable, generation);
        DeployableRegistry.Register(deployable);
        return deployable;
    }
}
