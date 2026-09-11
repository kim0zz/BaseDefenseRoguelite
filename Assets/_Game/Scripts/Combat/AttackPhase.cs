/// <summary>
/// Fazy cyklu ataku. Idle = można zacząć nowy / wykonać swap.
/// </summary>
public enum AttackPhase
{
    Idle = 0,
    Windup = 1,
    Active = 2,
    Recovery = 3
}
