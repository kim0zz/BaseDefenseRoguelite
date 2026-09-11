/// <summary>
/// Kiedy rozwiązywać trafienia (M7.6-T3 — T3 tylko OnEnteredActive).
/// </summary>
public enum SkillHitPolicy
{
    OnEnteredActive,
    OnLand,
    EveryTickWhileActiveOncePerTarget
}
