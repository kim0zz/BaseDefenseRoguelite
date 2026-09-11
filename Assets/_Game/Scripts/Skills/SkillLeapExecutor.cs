using UnityEngine;

/// <summary>
/// Wykonuje skok XZ przez ForcedMovementStepper + kosmetyczny łuk Y (M7.6-T9).
/// </summary>
public sealed class SkillLeapExecutor
{
    public const float DefaultTravelSeconds = 0.30f;
    public const float DefaultArcHeight = 1.2f;

    private Transform _body;
    private Vector3 _origin;
    private Vector3 _destination;
    private float _baseY;
    private float _travelSeconds;
    private float _arcHeight;
    private float _elapsed;
    private bool _active;

    public bool IsActive => _active;
    public Vector3 Destination => _destination;
    public float Progress => _travelSeconds > 0f ? Mathf.Clamp01(_elapsed / _travelSeconds) : 1f;

    public void Begin(
        Transform body,
        Vector3 origin,
        Vector3 destination,
        float travelSeconds = DefaultTravelSeconds,
        float arcHeight = DefaultArcHeight)
    {
        _body = body;
        _origin = Flatten(origin);
        _destination = Flatten(destination);
        _baseY = body != null ? body.position.y : origin.y;
        _travelSeconds = Mathf.Max(0.01f, travelSeconds);
        _arcHeight = arcHeight;
        _elapsed = 0f;
        _active = body != null;
    }

    public bool Tick(float deltaTime)
    {
        if (!_active || _body == null)
            return true;

        _elapsed += Mathf.Max(0f, deltaTime);
        var t = Progress;

        var desired = Vector3.Lerp(_origin, _destination, t);
        desired.y = _baseY;
        var current = _body.position;
        current.y = _baseY;
        var delta = desired - current;
        delta.y = 0f;
        if (delta.sqrMagnitude > 0f)
            ForcedMovementStepper.TryMove(_body, delta, honorCollisions: true);

        var y = _baseY + Mathf.Sin(t * Mathf.PI) * _arcHeight;
        var pos = _body.position;
        pos.y = y;
        _body.position = pos;

        if (t >= 1f)
        {
            Complete();
            return true;
        }

        return false;
    }

    public void Complete()
    {
        if (_body != null)
        {
            var pos = _destination;
            pos.y = _baseY;
            _body.position = pos;
        }

        _active = false;
    }

    public void Cancel()
    {
        _active = false;
        _body = null;
    }

    private static Vector3 Flatten(Vector3 value)
    {
        value.y = 0f;
        return value;
    }
}
