using UnityEngine;

/// <summary>
/// Strefa spowolnienia / DoT — placeholder (M7.5-T6, M8.1b).
/// </summary>
[DisallowMultipleComponent]
public class SlowZone : MonoBehaviour
{
    public const float SlowMultiplier = 0.4f;
    public const float DurationSeconds = 2.5f;

    [SerializeField] private float radius = 3f;
    [SerializeField] private float duration = DurationSeconds;
    [SerializeField] private float damagePerSecond;

    private float _remaining;
    private float _damageTickTimer;
    private GameObject _caster;
    private LineRenderer _ring;
    private bool _applySlow = true;
    private bool _trackCapacity;

    public static SlowZone Spawn(
        Vector3 position, float radiusMeters, GameObject caster,
        float damagePerSecond = 0f, float durationSeconds = DurationSeconds,
        bool applySlow = true, bool trackCapacity = false)
    {
        var go = new GameObject("SlowZone_Crack");
        go.transform.position = new Vector3(position.x, 0.05f, position.z);
        var zone = go.AddComponent<SlowZone>();
        zone.Initialize(radiusMeters, caster, damagePerSecond, durationSeconds, applySlow, trackCapacity);
        return zone;
    }

    public void Initialize(
        float radiusMeters, GameObject caster, float dps = 0f,
        float durationSeconds = DurationSeconds, bool applySlow = true, bool trackCapacity = false)
    {
        radius = radiusMeters;
        _caster = caster;
        damagePerSecond = dps;
        duration = durationSeconds;
        _remaining = duration;
        _applySlow = applySlow;
        _trackCapacity = trackCapacity;
        BuildVisual();
    }

    private void BuildVisual()
    {
        _ring = gameObject.AddComponent<LineRenderer>();
        _ring.useWorldSpace = false;
        _ring.loop = true;
        _ring.positionCount = 32;
        _ring.startWidth = damagePerSecond > 0f ? 0.28f : 0.12f;
        _ring.endWidth = _ring.startWidth;
        _ring.material = new Material(Shader.Find("Sprites/Default"));
        _ring.startColor = damagePerSecond > 0f
            ? new Color(1f, 0.35f, 0.08f, 0.95f)
            : new Color(0.2f, 0.15f, 0.1f, 0.65f);
        _ring.endColor = _ring.startColor;

        for (var i = 0; i < _ring.positionCount; i++)
        {
            var angle = i / (float)_ring.positionCount * Mathf.PI * 2f;
            _ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.05f, Mathf.Sin(angle) * radius));
        }

        if (damagePerSecond > 0f)
            SpawnCraterDisc();
    }

    private void SpawnCraterDisc()
    {
        var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "CrackDisc";
        disc.transform.SetParent(transform, false);
        disc.transform.localPosition = Vector3.up * 0.04f;
        disc.transform.localScale = new Vector3(radius * 2f, 0.06f, radius * 2f);
        var collider = disc.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);

        var renderer = disc.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = new Color(0.95f, 0.22f, 0.05f, 0.72f);
        }
    }

    private void Update()
    {
        _remaining -= Time.deltaTime;
        if (_remaining <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        ApplyEffectsToOccupants();
    }

    private void ApplyEffectsToOccupants()
    {
        var center = transform.position;
        var overlaps = Physics.OverlapSphere(center + Vector3.up * 0.5f, radius);
        foreach (var collider in overlaps)
        {
            if (collider == null) continue;
            if (collider.GetComponentInParent<PlayerCharacter>() != null) continue;

            var status = collider.GetComponentInParent<StatusEffectReceiver>();
            if (status != null && _applySlow)
                status.Apply(StatusEffectType.MoveSpeedBuff, 0.2f, SlowMultiplier - 1f, _caster);
        }

        if (damagePerSecond <= 0f) return;

        _damageTickTimer += Time.deltaTime;
        if (_damageTickTimer < 0.5f) return;
        _damageTickTimer = 0f;

        foreach (var collider in overlaps)
        {
            if (collider == null) continue;
            if (collider.GetComponentInParent<PlayerCharacter>() != null) continue;
            var health = collider.GetComponentInParent<Health>();
            health?.TakeDamage(damagePerSecond * 0.5f, _caster);
        }
    }

    private void OnDestroy()
    {
        if (_trackCapacity)
            SkillEffectApplier.NotifyDamageZoneDestroyed();
    }
}
