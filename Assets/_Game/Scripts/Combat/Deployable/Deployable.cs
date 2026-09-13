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
    private float _fuseDuration;
    private bool _wasKicked;
    private DeployableMotor _motor;
    private Renderer _bodyRenderer;
    private Material _fuseMaterial;
    private Color _baseColor = new(0.9f, 0.85f, 0.2f);

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
    public bool HasFuse => _fuseTimer >= 0f;
    public float RemainingFuse => _fuseTimer;
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
        _fuseDuration = Mathf.Max(0f, seconds);
        _fuseTimer = _fuseDuration;
    }

    public void MarkKicked() => _wasKicked = true;

    public void TickFuseForTests(float deltaTime) => TickFuse(deltaTime);

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
        SafeDestroy(gameObject);
    }

    private void Update() => TickFuse(Time.deltaTime);

    private void TickFuse(float deltaTime)
    {
        if (IsDetonated) return;
        if (_fuseTimer < 0f) return;

        _fuseTimer -= deltaTime;
        UpdateFuseVisual();
        if (_fuseTimer <= 0f)
            Detonate();
    }

    private void UpdateFuseVisual()
    {
        if (_fuseMaterial == null || _fuseDuration <= 0f) return;

        var t = 1f - Mathf.Clamp01(_fuseTimer / _fuseDuration);
        var pulse = 0.85f + 0.25f * Mathf.Sin(Time.time * (4f + t * 10f));
        var warn = Color.Lerp(_baseColor, new Color(1f, 0.15f, 0.05f), t);
        warn *= pulse;
        if (_fuseMaterial.HasProperty("_BaseColor"))
            _fuseMaterial.SetColor("_BaseColor", warn);
        else
            _fuseMaterial.color = warn;
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

        var bodyColor = color ?? category switch
        {
            DeployableCategory.Child => new Color(1f, 0.6f, 0.2f),
            DeployableCategory.Orbital => new Color(0.3f, 0.7f, 1f),
            DeployableCategory.Strike => new Color(1f, 0.3f, 0.3f),
            DeployableCategory.DashCharge => new Color(1f, 0.35f, 0.55f),
            _ => new Color(0.9f, 0.85f, 0.2f)
        };

        var renderer = go.GetComponent<Renderer>();
        Material fuseMat = null;
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", bodyColor);
            else mat.color = bodyColor;
            renderer.sharedMaterial = mat;
            fuseMat = mat;
        }

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "RadiusRing";
        ring.transform.SetParent(go.transform, false);
        ring.transform.localPosition = Vector3.up * 0.02f;
        ring.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
        var ringCol = ring.GetComponent<Collider>();
        if (ringCol != null) SafeDestroy(ringCol);

        var deployable = go.AddComponent<Deployable>();
        deployable.Initialize(owner, category, damage, radius, bossStagger, maxTargets, detonatable, generation);
        deployable._bodyRenderer = renderer;
        deployable._fuseMaterial = fuseMat;
        deployable._baseColor = bodyColor;
        DeployableRegistry.Register(deployable);
        return deployable;
    }

    private static void SafeDestroy(UnityEngine.Object obj)
    {
        if (obj == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEngine.Object.DestroyImmediate(obj);
            return;
        }
#endif
        UnityEngine.Object.Destroy(obj);
    }
}
