using UnityEngine;

/// <summary>
/// Placeholder zapowiedzi skilla — ciemny okrąg + błysk pierścienia (M7.5-T3).
/// Ring i linia szarży na osobnych child GO (jeden LineRenderer na obiekt).
/// </summary>
[DisallowMultipleComponent]
public class SkillTelegraphView : MonoBehaviour
{
    private const float RingHeight = 0.02f;

    private LineRenderer _ring;
    private LineRenderer _chargeLine;
    private Transform _ringRoot;
    private Transform _chargeLineRoot;
    private float _targetRadius;
    private float _growElapsed;
    private float _growDuration;
    private Color _playerColor = Color.white;
    private bool _alternateStyle;
    private bool _flashActive;
    private float _flashTimer;
    private bool _chargeLineActive;
    private float _chargeLineLength;
    private float _chargeLineWidth;
    private Vector3 _chargeLineDirection = Vector3.forward;
    private bool _useWorldCenter;
    private Vector3 _worldCenter;

    public void BeginWindup(float radius, float duration, Color playerColor, bool alternateStyle = false)
    {
        BeginWindupAtWorldPosition(transform.position, radius, duration, playerColor, alternateStyle);
    }

    public void BeginWindupAtWorldPosition(
        Vector3 worldCenter,
        float radius,
        float duration,
        Color playerColor,
        bool alternateStyle = false)
    {
        _chargeLineActive = false;
        if (_chargeLine != null)
            _chargeLine.enabled = false;

        _useWorldCenter = true;
        _worldCenter = worldCenter;
        _worldCenter.y = RingHeight;

        _targetRadius = radius;
        _growDuration = Mathf.Max(0.05f, duration);
        _growElapsed = 0f;
        _playerColor = playerColor;
        _alternateStyle = alternateStyle;
        _flashActive = false;
        EnsureRing();
        if (_ring == null) return;

        if (_alternateStyle)
        {
            _ring.startColor = new Color(playerColor.r, playerColor.g, playerColor.b, 0.95f);
            _ring.endColor = _ring.startColor;
            _ring.startWidth = 0.18f;
            _ring.endWidth = 0.18f;
        }
        else
        {
            _ring.startColor = new Color(1f, 0.85f, 0.2f, 0.95f);
            _ring.endColor = _ring.startColor;
            _ring.startWidth = 0.22f;
            _ring.endWidth = 0.22f;
        }

        UpdateRing(0.05f);
    }

    public bool BeginChargeLine(float lengthMeters, float widthMeters, Color playerColor)
    {
        _chargeLineActive = true;
        _chargeLineLength = Mathf.Max(0.5f, lengthMeters);
        _chargeLineWidth = Mathf.Max(0.2f, widthMeters);
        _playerColor = playerColor;
        _flashActive = false;

        if (_ring != null)
            _ring.enabled = false;

        EnsureChargeLine();
        if (_chargeLine == null)
        {
            _chargeLineActive = false;
            return false;
        }

        _chargeLine.enabled = true;
        _chargeLine.startColor = new Color(playerColor.r, playerColor.g, playerColor.b, 0.95f);
        _chargeLine.endColor = _chargeLine.startColor;
        UpdateChargeLine(_chargeLineDirection);
        return true;
    }

    public void SetChargeLineDirection(Vector3 worldDirection)
    {
        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.01f) return;
        _chargeLineDirection = worldDirection.normalized;
        if (_chargeLineActive && _chargeLine != null && _chargeLine.enabled)
            UpdateChargeLine(_chargeLineDirection);
    }

    public void FlashActiveRing()
    {
        _flashActive = true;
        _flashTimer = 0.28f;
        if (_ring != null)
        {
            _ring.startColor = _playerColor;
            _ring.endColor = _playerColor;
        }

        UpdateRing(_targetRadius);
    }

    public void Hide()
    {
        _chargeLineActive = false;
        _useWorldCenter = false;
        _worldCenter = Vector3.zero;
        if (_ring != null)
            _ring.enabled = false;
        if (_chargeLine != null)
            _chargeLine.enabled = false;
    }

    public void SetWorldCenter(Vector3 worldCenter)
    {
        if (!_useWorldCenter) return;
        _worldCenter = worldCenter;
        _worldCenter.y = RingHeight;
        UpdateRing(_targetRadius);
    }

    private void Update()
    {
        if (_chargeLineActive && _chargeLine != null && _chargeLine.enabled)
        {
            UpdateChargeLine(_chargeLineDirection);
            return;
        }

        if (_ring == null || !_ring.enabled) return;

        if (_flashActive)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
            {
                _flashActive = false;
                Hide();
            }
            return;
        }

        _growElapsed += Time.deltaTime;
        var t = Mathf.Clamp01(_growElapsed / _growDuration);
        UpdateRing(Mathf.Lerp(0.05f, _targetRadius, t));
    }

    private void EnsureRingRoot()
    {
        if (_ringRoot != null) return;

        var existing = transform.Find("TelegraphRing");
        if (existing != null)
        {
            _ringRoot = existing;
            return;
        }

        var go = new GameObject("TelegraphRing");
        go.transform.SetParent(transform, false);
        _ringRoot = go.transform;
    }

    private void EnsureChargeLineRoot()
    {
        if (_chargeLineRoot != null) return;

        var existing = transform.Find("TelegraphChargeLine");
        if (existing != null)
        {
            _chargeLineRoot = existing;
            return;
        }

        var go = new GameObject("TelegraphChargeLine");
        go.transform.SetParent(transform, false);
        _chargeLineRoot = go.transform;
    }

    private void EnsureRing()
    {
        EnsureRingRoot();
        if (_ringRoot == null) return;

        if (_ring == null)
            _ring = _ringRoot.GetComponent<LineRenderer>();
        if (_ring == null)
            _ring = _ringRoot.gameObject.AddComponent<LineRenderer>();
        if (_ring == null) return;

        _ring.useWorldSpace = false;
        _ring.loop = true;
        _ring.positionCount = 40;
        _ring.startWidth = 0.1f;
        _ring.endWidth = 0.1f;
        if (_ring.sharedMaterial == null)
            _ring.material = new Material(Shader.Find("Sprites/Default"));
        _ring.enabled = true;
    }

    private void EnsureChargeLine()
    {
        EnsureChargeLineRoot();
        if (_chargeLineRoot == null) return;

        if (_chargeLine == null)
            _chargeLine = _chargeLineRoot.GetComponent<LineRenderer>();
        if (_chargeLine == null)
            _chargeLine = _chargeLineRoot.gameObject.AddComponent<LineRenderer>();
        if (_chargeLine == null) return;

        _chargeLine.useWorldSpace = false;
        _chargeLine.loop = true;
        _chargeLine.positionCount = 5;
        _chargeLine.startWidth = 0.08f;
        _chargeLine.endWidth = 0.08f;
        if (_chargeLine.sharedMaterial == null)
            _chargeLine.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void UpdateChargeLine(Vector3 worldDirection)
    {
        if (_chargeLine == null) return;

        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.01f)
            worldDirection = Vector3.forward;
        worldDirection.Normalize();

        var right = Vector3.Cross(Vector3.up, worldDirection).normalized;
        var halfWidth = _chargeLineWidth * 0.5f;
        var localForward = transform.InverseTransformDirection(worldDirection);
        var localRight = transform.InverseTransformDirection(right);
        var origin = Vector3.zero;
        origin.y = RingHeight;

        var far = origin + localForward * _chargeLineLength;
        var nearLeft = origin - localRight * halfWidth;
        var nearRight = origin + localRight * halfWidth;
        var farLeft = far - localRight * halfWidth;
        var farRight = far + localRight * halfWidth;

        _chargeLine.SetPosition(0, nearLeft);
        _chargeLine.SetPosition(1, farLeft);
        _chargeLine.SetPosition(2, farRight);
        _chargeLine.SetPosition(3, nearRight);
        _chargeLine.SetPosition(4, nearLeft);
    }

    private void UpdateRing(float currentRadius)
    {
        if (_ring == null) return;

        var centerLocal = _useWorldCenter
            ? transform.InverseTransformPoint(_worldCenter)
            : Vector3.zero;
        centerLocal.y = RingHeight;

        for (var i = 0; i < _ring.positionCount; i++)
        {
            var angle = i / (float)_ring.positionCount * Mathf.PI * 2f;
            var radius = currentRadius;
            if (_alternateStyle && i % 4 >= 2)
                radius *= 0.35f;
            _ring.SetPosition(
                i,
                centerLocal + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
    }
}
