using UnityEngine;

/// <summary>
/// HP wieży greybox — struktura do ataku wrogów/bossa (M7-T2).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TowerMarker))]
public class TowerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private Color healthyColor = new(0.45f, 0.55f, 0.7f, 1f);
    [SerializeField] private Color damagedColor = new(0.85f, 0.55f, 0.2f, 1f);
    [SerializeField] private Color destroyedColor = new(0.15f, 0.12f, 0.12f, 1f);
    [SerializeField] [Range(0f, 1f)] private float lowHealthThreshold = 0.35f;

    private Health _health;
    private TowerMarker _marker;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private Color _configuredHealthyColor;
    private bool _destroyed;
    private bool _disabledByBoss;

    public AttackLineId AttackLine => _marker != null ? _marker.AttackLine : AttackLineId.Center;
    public TowerRole Role => _marker != null ? _marker.Role : TowerRole.LineTower;
    public bool IsDisabledByBoss => _disabledByBoss;
    public bool IsOperational => _health != null && _health.IsAlive && !_destroyed && !_disabledByBoss;
    public bool IsDestroyed => _destroyed;
    public bool IsAlive => IsOperational;
    public float CurrentHealth => _health != null ? _health.CurrentHealth : 0f;
    public float MaxHealth => _health != null ? _health.MaxHealth : 0f;

    private void Awake()
    {
        EnsureComponents();
    }

    private void EnsureComponents()
    {
        if (_marker == null)
            _marker = GetComponent<TowerMarker>();
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();
        if (_health != null) return;

        _health = GetComponent<Health>();
        if (_health == null)
            _health = gameObject.AddComponent<Health>();
        _health.HealthChanged += OnHealthChanged;
        _health.Died += OnTowerDestroyed;
    }

    private void OnEnable()
    {
        TowerRegistry.Register(this);
    }

    private void OnDisable()
    {
        TowerRegistry.Unregister(this);
    }

    public void InitializeFromConfig(ProgressionConfig config, Color baseColor)
    {
        EnsureComponents();

        _configuredHealthyColor = baseColor;
        healthyColor = baseColor;
        var maxHp = config != null ? config.TowerMaxHealth : 120f;
        _destroyed = false;
        _disabledByBoss = false;
        _health.Configure(maxHp);
        ApplyVisual();
    }

    public void SetDisabledByBoss(bool disabled)
    {
        _disabledByBoss = disabled;
        ApplyVisual();
    }

    public void TakeDamage(float amount, GameObject source = null)
    {
        if (_destroyed || amount <= 0f) return;
        _health.TakeDamage(amount, source);
    }

    private void OnHealthChanged(float current, float max)
    {
        ApplyVisual();
    }

    private void OnTowerDestroyed()
    {
        _destroyed = true;
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (_renderer == null) return;

        Color color;
        if (_destroyed || (_health != null && !_health.IsAlive))
            color = destroyedColor;
        else if (_disabledByBoss)
        {
            var baseColor = _configuredHealthyColor.a > 0f ? _configuredHealthyColor : healthyColor;
            color = new Color(baseColor.r * 0.35f, baseColor.g * 0.35f, baseColor.b * 0.35f, baseColor.a);
        }
        else if (_health != null && _health.CurrentHealth / Mathf.Max(1f, _health.MaxHealth) <= lowHealthThreshold)
            color = damagedColor;
        else
            color = _configuredHealthyColor.a > 0f ? _configuredHealthyColor : healthyColor;

        _propertyBlock ??= new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor("_BaseColor", color);
        _propertyBlock.SetColor("_Color", color);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
