using UnityEngine;

/// <summary>
/// Placeholder znacznika tauntu nad głową wroga — czytelny w 4P (M7.6-T6).
/// </summary>
[DisallowMultipleComponent]
public class TauntMarkerView : MonoBehaviour
{
    private GameObject _marker;
    private float _remaining;
    private Color _color;

    public static void Show(GameObject enemy, Color playerColor, float duration)
    {
        if (enemy == null || duration <= 0f) return;

        var root = enemy.transform.root.gameObject;
        var view = root.GetComponent<TauntMarkerView>();
        if (view == null)
            view = root.AddComponent<TauntMarkerView>();

        view.Play(playerColor, duration);
    }

    private void Play(Color playerColor, float duration)
    {
        _color = playerColor;
        _remaining = duration;
        EnsureMarker();
        UpdateMarkerVisual();
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (_remaining <= 0f) return;
        _remaining -= deltaTime;
        if (_remaining <= 0f)
            HideMarker();
    }

    private void EnsureMarker()
    {
        if (_marker != null)
        {
            _marker.SetActive(true);
            return;
        }

        _marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _marker.name = "TauntMarker";
        _marker.transform.SetParent(transform, false);
        _marker.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        _marker.transform.localScale = Vector3.one * 0.35f;

        var collider = _marker.GetComponent<Collider>();
        if (collider != null)
        {
            if (Application.isPlaying)
                Destroy(collider);
            else
                DestroyImmediate(collider);
        }
    }

    private void UpdateMarkerVisual()
    {
        if (_marker == null) return;
        var renderer = _marker.GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;
        var tint = new Color(_color.r, _color.g, _color.b, 0.95f);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", tint);
        else
            mat.color = tint;
    }

    private void HideMarker()
    {
        if (_marker != null)
            _marker.SetActive(false);
        _remaining = 0f;
    }
}
