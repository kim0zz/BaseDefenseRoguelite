using UnityEngine;

/// <summary>Widoczne „kupsko” — ciało z masą, do którego idą liny w topologii Hub.</summary>
[DisallowMultipleComponent]
public class ChainTetherHub : MonoBehaviour
{
    [SerializeField] private float mass = 1.85f;
    [SerializeField] private float radius = 0.7f;

    public ChainTetherBody Body { get; private set; }

    public static ChainTetherHub Ensure(Vector3 spawnPosition)
    {
        var existing = FindAnyObjectByType<ChainTetherHub>();
        if (existing != null)
            return existing;

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "ChainHub";
        go.transform.position = spawnPosition;
        go.transform.localScale = Vector3.one * 1.45f;

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(0.55f, 0.32f, 0.12f));
            else
                mat.color = new Color(0.55f, 0.32f, 0.12f);
            renderer.material = mat;
        }

        var sphere = go.GetComponent<SphereCollider>();
        if (sphere != null)
            Object.DestroyImmediate(sphere);

        var hub = go.AddComponent<ChainTetherHub>();
        hub.ConfigureBody();
        return hub;
    }

    private void Awake()
    {
        ConfigureBody();
    }

    private void ConfigureBody()
    {
        Body = GetComponent<ChainTetherBody>();
        if (Body == null)
            Body = gameObject.AddComponent<ChainTetherBody>();
        Body.Mass = mass;

        if (GetComponent<CharacterController>() == null)
        {
            var cc = gameObject.AddComponent<CharacterController>();
            cc.height = radius * 2f;
            cc.radius = radius;
            cc.center = Vector3.zero;
        }
    }

    public void SetVisible(bool visible)
    {
        if (gameObject.activeSelf == visible)
            return;
        gameObject.SetActive(visible);
        if (visible)
            ConfigureBody();
    }

    public void SnapToCentroid(Vector3 centroid)
    {
        centroid.y = Mathf.Max(0.85f, transform.position.y);
        transform.position = centroid;
        if (Body == null)
            ConfigureBody();
        Body.ClearVelocity();
        Body.MarkPoseAfterTether();
    }

    public float CurrentMass => Body != null ? Body.Mass : mass;

    public void NudgeMass(float delta)
    {
        if (Body == null)
            ConfigureBody();
        mass = Mathf.Clamp(mass + delta, 0.7f, 4.5f);
        Body.Mass = mass;
    }
}
