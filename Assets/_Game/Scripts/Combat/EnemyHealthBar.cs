using UnityEngine;

/// <summary>
/// Prosty pasek HP nad wrogiem — czytelność walki M3.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new(0f, 1.35f, 0f);
    [SerializeField] private Vector3 barSize = new(1f, 0.12f, 0.05f);

    private Health _health;
    private Transform _fillTransform;
    private Transform _bgTransform;

    private void Awake()
    {
        _health = GetComponent<Health>();
        CreateBarVisuals();
        _health.HealthChanged += OnHealthChanged;
        OnHealthChanged(_health.CurrentHealth, _health.MaxHealth);
    }

    public void Hide()
    {
        if (_fillTransform != null)
            _fillTransform.parent.gameObject.SetActive(false);
    }

    public void ConfigureVisual(Vector3 worldOffset, Vector3 size)
    {
        offset = worldOffset;
        barSize = size;
        if (_bgTransform != null)
            _bgTransform.localScale = barSize;
        if (_health != null)
            OnHealthChanged(_health.CurrentHealth, _health.MaxHealth);
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.HealthChanged -= OnHealthChanged;
    }

    private void LateUpdate()
    {
        if (_fillTransform == null) return;
        var barRoot = _fillTransform.parent;
        barRoot.position = transform.position + offset;
        barRoot.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private void CreateBarVisuals()
    {
        var root = new GameObject("HPBar").transform;
        root.SetParent(transform, false);

        var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bg.name = "HPBar_BG";
        bg.transform.SetParent(root, false);
        bg.transform.localScale = barSize;
        SetColor(bg, new Color(0.15f, 0.15f, 0.15f, 0.9f));
        Destroy(bg.GetComponent<Collider>());
        _bgTransform = bg.transform;

        var fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "HPBar_Fill";
        fill.transform.SetParent(root, false);
        fill.transform.localScale = barSize;
        fill.transform.localPosition = Vector3.zero;
        SetColor(fill, new Color(0.9f, 0.2f, 0.15f, 1f));
        Destroy(fill.GetComponent<Collider>());
        _fillTransform = fill.transform;
    }

    private void OnHealthChanged(float current, float max)
    {
        if (_fillTransform == null || max <= 0f) return;

        var pct = Mathf.Clamp01(current / max);
        _fillTransform.localScale = new Vector3(barSize.x * pct, barSize.y, barSize.z);
        _fillTransform.localPosition = new Vector3(-barSize.x * (1f - pct) * 0.5f, 0f, -0.01f);
    }

    private static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;
        var mat = new Material(renderer.sharedMaterial);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;
        renderer.material = mat;
    }
}
