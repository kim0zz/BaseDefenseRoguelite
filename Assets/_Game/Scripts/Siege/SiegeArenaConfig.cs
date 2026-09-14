using UnityEngine;

[CreateAssetMenu(menuName = "Game/Siege/Arena", fileName = "SiegeArenaConfig")]
public class SiegeArenaConfig : ScriptableObject
{
    public float HalfWidth = 13f;
    public Vector2 OuterDepth = new(-0.5f, 16f);
    public Vector2 InnerDepth = new(-14f, 1f);
    public float GateZ = -2f;
    public float GateHealth = 950f;
    public float CoreHealth = 750f;
    public float RetreatSeconds = 3f;
    public float IntermissionSeconds = 8f;
    public float CameraPadding = 1f;
    public float CameraTilt = 65f;
    public float CameraTransitionSpeed = 4f;
}
