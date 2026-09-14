using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Widoczna gwarantowana nagroda fali; podniesienie przez proximity lub Enter.</summary>
[DisallowMultipleComponent]
public sealed class SiegeRewardDrop : MonoBehaviour
{
    public int Wave { get; private set; }
    public bool Collected { get; private set; }
    private float _radius = 1.8f;
    public static SiegeRewardDrop Spawn(Vector3 position, int wave)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = $"Siege_GwarantowanaNagroda_{wave + 1}";
        go.transform.position = position + Vector3.up * .65f;
        go.transform.localScale = Vector3.one * .65f;
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial) { color = new Color(1f, .72f, .16f) };
            renderer.material = mat;
        }
        Destroy(go.GetComponent<Collider>());
        var drop = go.AddComponent<SiegeRewardDrop>();
        drop.Wave = wave;
        return drop;
    }
    private void Update()
    {
        if (Collected) return;
        transform.Rotate(0f, 90f * Time.unscaledDeltaTime, 0f, Space.World);
        foreach (var player in FindObjectsByType<PlayerCharacter>())
        {
            if (player == null || !player.IsCombatEnabled) continue;
            if (Vector3.Distance(player.transform.position, transform.position) <= _radius || Keyboard.current?.enterKey.wasPressedThisFrame == true)
            { Collect(); return; }
        }
    }
    public void Collect()
    {
        if (Collected) return;
        Collected = true;
        SharedRunState.Instance?.AddSiegeReward(12 + Wave * 4, 20 + Wave * 10);
        Debug.Log($"[Siege] Pobrano gwarantowaną nagrodę fali {Wave + 1}.");
        Destroy(gameObject);
    }
}
