using UnityEngine;

/// <summary>Stan prędkości gracza wynikający z sił liny (XZ).</summary>
[DisallowMultipleComponent]
public class ChainTetherBody : MonoBehaviour
{
    [SerializeField] private PlayerCharacter player;
    [SerializeField] private float mass = 1f;

    private Vector3 _velocity;
    private Vector3 _poseAfterLastTether;
    private bool _hasPoseAfterLastTether;

    public float Mass
    {
        get => Mathf.Max(0.2f, mass);
        set => mass = Mathf.Max(0.2f, value);
    }

    public PlayerCharacter Player => player != null ? player : player = GetComponent<PlayerCharacter>();
    public Vector3 Velocity
    {
        get => _velocity;
        set
        {
            value.y = 0f;
            _velocity = value;
        }
    }

    public Vector3 Position
    {
        get => transform.position;
        set
        {
            var p = value;
            p.y = transform.position.y;
            transform.position = p;
        }
    }

    private void Awake()
    {
        if (player == null)
            player = GetComponent<PlayerCharacter>();
    }

    public void ClearVelocity()
    {
        _velocity = Vector3.zero;
    }

    public Vector3 SampleLocomotionVelocity(float deltaTime)
    {
        if (!_hasPoseAfterLastTether || deltaTime <= 0.0001f)
            return Vector3.zero;

        var delta = transform.position - _poseAfterLastTether;
        delta.y = 0f;
        return delta / deltaTime;
    }

    public void MarkPoseAfterTether()
    {
        _poseAfterLastTether = transform.position;
        _hasPoseAfterLastTether = true;
    }
}
