using UnityEngine;

/// <summary>Tani dummy do testów walki i tłoku — bez tether damage.</summary>
[DisallowMultipleComponent]
public class ChainPrototypeDummyFodder : MonoBehaviour
{
    [SerializeField] private float wanderSpeed = 0.6f;
    [SerializeField] private float wanderRadius = 2.5f;
    [SerializeField] private float retargetInterval = 2.5f;
    [SerializeField] private bool chaseNearestPlayer;
    [SerializeField] private float chaseSpeed = 0.35f;

    private Vector3 _origin;
    private Vector3 _target;
    private float _retargetTimer;

    public void Initialize(Vector3 spawnPosition, Color color, float maxHealth, bool enableChase = false)
    {
        transform.position = spawnPosition;
        _origin = spawnPosition;
        chaseNearestPlayer = enableChase;
        PickNewTarget();

        var health = GetComponent<Health>();
        if (health == null)
            health = gameObject.AddComponent<Health>();
        health.Configure(maxHealth);

        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else
                mat.color = color;
            renderer.material = mat;
        }
    }

    private void Update()
    {
        _retargetTimer -= Time.deltaTime;
        if (_retargetTimer <= 0f)
        {
            PickNewTarget();
            _retargetTimer = retargetInterval;
        }

        var direction = Vector3.zero;
        if (chaseNearestPlayer)
        {
            var nearest = FindNearestPlayer();
            if (nearest != null)
            {
                direction = nearest.position - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.01f)
                    direction = direction.normalized * chaseSpeed;
            }
        }
        else if ((_target - transform.position).sqrMagnitude > 0.05f)
        {
            direction = (_target - transform.position);
            direction.y = 0f;
            direction = direction.normalized * wanderSpeed;
        }

        if (direction.sqrMagnitude < 0.0001f) return;
        transform.position += direction * Time.deltaTime;
    }

    private void PickNewTarget()
    {
        var offset = Random.insideUnitCircle * wanderRadius;
        _target = _origin + new Vector3(offset.x, 0f, offset.y);
    }

    private Transform FindNearestPlayer()
    {
        var players = FindObjectsByType<PlayerCharacter>();
        Transform best = null;
        var bestDist = float.MaxValue;
        foreach (var player in players)
        {
            var d = (player.transform.position - transform.position).sqrMagnitude;
            if (d >= bestDist) continue;
            bestDist = d;
            best = player.transform;
        }

        return best;
    }
}
