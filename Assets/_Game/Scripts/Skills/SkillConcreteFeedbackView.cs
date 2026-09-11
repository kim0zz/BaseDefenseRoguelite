using UnityEngine;

/// <summary>
/// Placeholder outline „beton” na casterze po udanym tauncie (M7.6-T6).
/// </summary>
[DisallowMultipleComponent]
public class SkillConcreteFeedbackView : MonoBehaviour
{
    private Renderer _renderer;
    private Color _originalColor;
    private float _remaining;
    private bool _hasOriginal;

    public void ShowOutline(Color playerColor, float duration)
    {
        if (duration <= 0f) return;
        EnsureRenderer();
        if (_renderer == null) return;

        if (!_hasOriginal)
        {
            _originalColor = ReadColor(_renderer);
            _hasOriginal = true;
        }

        _remaining = duration;
        var tint = Color.Lerp(_originalColor, playerColor, 0.55f);
        tint.a = 1f;
        WriteColor(_renderer, tint);
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
            RestoreOriginal();
    }

    private void EnsureRenderer()
    {
        if (_renderer != null) return;
        _renderer = GetComponentInChildren<Renderer>();
    }

    private void RestoreOriginal()
    {
        if (_renderer != null && _hasOriginal)
            WriteColor(_renderer, _originalColor);
        _remaining = 0f;
    }

    private static Color ReadColor(Renderer renderer)
    {
        var mat = renderer.material;
        return mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
    }

    private static void WriteColor(Renderer renderer, Color color)
    {
        var mat = renderer.material;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;
    }
}
