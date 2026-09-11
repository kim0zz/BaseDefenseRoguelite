/// <summary>
/// Combat Playability: lock rotacji tylko w oknie hitu.
/// </summary>
public static class AttackFacingPolicy
{
    public static bool LocksFacing(AttackPhase phase) => phase == AttackPhase.Active;
}
