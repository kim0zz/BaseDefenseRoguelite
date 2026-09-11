using System.Collections;
using UnityEngine;

/// <summary>
/// Krótki flash przy trafieniu — pierwszy game-feel pass (M3).
/// </summary>
[DisallowMultipleComponent]
public class HitFlashFeedback : MonoBehaviour
{
    [SerializeField] private Color flashColor = new(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private float flashDuration = 0.12f;

    private Renderer _renderer;
    private Color _originalColor;
    private Coroutine _flashRoutine;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _originalColor = GetColor(_renderer);
    }

    public void PlayFlash()
    {
        if (_renderer == null) return;
        if (_flashRoutine != null)
            StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetColor(_renderer, flashColor);
        yield return new WaitForSeconds(flashDuration);
        SetColor(_renderer, _originalColor);
        _flashRoutine = null;
    }

    public void SetBaseColor(Color color)
    {
        _originalColor = color;
        if (_renderer != null)
            SetColor(_renderer, color);
    }

    private static Color GetColor(Renderer renderer)
    {
        var mat = renderer.material;
        return mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
    }

    private static void SetColor(Renderer renderer, Color color)
    {
        var mat = renderer.material;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;
    }
}
