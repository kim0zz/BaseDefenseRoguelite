using UnityEngine;

/// <summary>
/// SharedCamera — wspólna kamera śledząca centroid wszystkich dołączonych graczy.
/// Ortograficzna, widok z góry (top-down XZ). M6.5: SharedCameraMath + clamp do mapy.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class SharedCamera : MonoBehaviour
{
    [Header("Podążanie za graczami")]
    [SerializeField] private float followSmoothSpeed = 5f;
    [SerializeField] private float cameraHeight = 20f;

    [Header("Zoom ortograficzny")]
    [SerializeField] private float minOrthoSize = 10f;
    [SerializeField] private float maxOrthoSize = 28f;
    [Tooltip("Dodatkowy margines poza skrajnymi graczami.")]
    [SerializeField] private float zoomPadding = 4f;
    [SerializeField] private float zoomSmoothSpeed = 3f;

    private Camera _cam;
    private Vector3 _shakeOffset;
    private float _shakeAmplitude;
    private Vector3 _followPosition;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        _cam.orthographicSize = minOrthoSize;
        if (GetComponent<AudioListener>() == null)
            gameObject.AddComponent<AudioListener>();

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        transform.position = new Vector3(0f, cameraHeight, 0f);
        _followPosition = transform.position;
    }

    public void AddShake(float amplitude)
    {
        if (amplitude <= 0f) return;
        _shakeAmplitude = CameraShakeMath.StoreAmplitude(_shakeAmplitude, amplitude);
    }

    private void LateUpdate()
    {
        var players = FindObjectsByType<PlayerCharacter>();
        if (players.Length == 0) return;

        var positions = new Vector3[players.Length];
        for (var i = 0; i < players.Length; i++)
            positions[i] = players[i].transform.position;

        var centroid = SharedCameraMath.CalculateCentroid(positions);
        var aspect = _cam.aspect;

        var targetSize = SharedCameraMath.CalculateOrthoSize(
            positions, centroid, aspect, zoomPadding, minOrthoSize, maxOrthoSize);

        var mapBounds = ResolveMapBounds();
        var clampedCentroid = SharedCameraMath.ClampFollowPosition(
            centroid, targetSize, aspect, mapBounds.x, mapBounds.z);

        var targetPos = new Vector3(clampedCentroid.x, cameraHeight, clampedCentroid.z);
        _followPosition = Vector3.Lerp(_followPosition, targetPos, followSmoothSpeed * Time.deltaTime);

        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetSize,
            zoomSmoothSpeed * Time.unscaledDeltaTime);

        if (_shakeAmplitude > 0.001f)
        {
            _shakeOffset = new Vector3(
                (Random.value * 2f - 1f) * _shakeAmplitude,
                0f,
                (Random.value * 2f - 1f) * _shakeAmplitude);
        }
        else
        {
            _shakeOffset = Vector3.zero;
        }

        transform.position = _followPosition + _shakeOffset;
        _shakeAmplitude = CameraShakeMath.DecayAfterApply(_shakeAmplitude, Time.unscaledDeltaTime);
    }

    private (Vector2 x, Vector2 z) ResolveMapBounds()
    {
        var playArea = MapPlayArea.Instance;
        if (playArea != null)
            return (playArea.XBounds, playArea.ZBounds);

        return (MapGreyboxLayout.PlayAreaX, MapGreyboxLayout.PlayAreaZ);
    }
}
