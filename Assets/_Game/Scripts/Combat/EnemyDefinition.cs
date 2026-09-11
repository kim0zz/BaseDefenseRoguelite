using UnityEngine;

/// <summary>
/// Typ przeciwnika — M4 visual slice: Grunt + Hunter.
/// </summary>
public enum EnemyKind
{
    Grunt,
    Hunter,
    Rusher,
    Carrier,
    Siege,
    Support,
    Flanker,
    Shielder
}

/// <summary>
/// Dane przeciwnika (data-driven). Wartości z BASELINE_VALUES — benchmark moba fala 1.
/// </summary>
[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Game/Combat/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [SerializeField] private EnemyKind enemyKind = EnemyKind.Grunt;
    [SerializeField] private string displayName = "Grunt";
    [SerializeField] private float maxHealth = 18f;
    [SerializeField] private float damage = 8f;
    [SerializeField] private float attackInterval = 1.4f;
    [SerializeField] private float moveSpeed = 4.1f;
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float detectRange = 30f;
    [SerializeField] private Vector3 bodyScale = new(0.85f, 0.85f, 0.85f);
    [SerializeField] private Color bodyColor = new(0.75f, 0.15f, 0.15f, 1f);
    [SerializeField] private int expReward = 8;
    [SerializeField] private int goldReward = 5;
    [SerializeField] private float structureDamage;
    [SerializeField] private float playerDamageMultiplier = 1f;
    [SerializeField] private float supportBuffRadius;
    [SerializeField] private float supportMoveSpeedMultiplier = 1f;
    [SerializeField] private float supportDamageMultiplier = 1f;
    [SerializeField] private float shieldProtectRadius;

    public EnemyKind Kind => enemyKind;
    public string DisplayName => displayName;
    public float MaxHealth => maxHealth;
    public float Damage => damage;
    public float AttackInterval => attackInterval;
    public float MoveSpeed => moveSpeed;
    public float AttackRange => attackRange;
    public float DetectRange => detectRange;
    public Vector3 BodyScale => bodyScale;
    public Color BodyColor => bodyColor;
    public int ExpReward => expReward;
    public int GoldReward => goldReward;
    public float StructureDamage => structureDamage > 0f ? structureDamage : damage;
    public float PlayerDamageMultiplier => playerDamageMultiplier;
    public float SupportBuffRadius => supportBuffRadius;
    public float SupportMoveSpeedMultiplier => supportMoveSpeedMultiplier;
    public float SupportDamageMultiplier => supportDamageMultiplier;
    public float ShieldProtectRadius => shieldProtectRadius;

    /// <summary>Tylko testy EditMode — ustawia kind bez assetu.</summary>
    public void ConfigureForTest(EnemyKind kind)
    {
        enemyKind = kind;
    }

    /// <summary>Tylko testy EditMode — parametry Support.</summary>
    public void ConfigureSupportForTest(float radius, float moveMul, float dmgMul)
    {
        supportBuffRadius = radius;
        supportMoveSpeedMultiplier = moveMul;
        supportDamageMultiplier = dmgMul;
    }

    /// <summary>Tylko testy EditMode — parametry Shielder.</summary>
    public void ConfigureShielderForTest(float radius)
    {
        shieldProtectRadius = radius;
    }
}
