using UnityEngine;

/// <summary>Animacja i błysk modelu; nie steruje obrażeniami ani ruchem wroga.</summary>
[DisallowMultipleComponent]
public sealed class EnemyModelView : MonoBehaviour
{
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private Color hitColor = new(1f, 0.3f, 0.3f, 1f);
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private EnemyController _enemy;
    private Health _health;
    private StatusEffectReceiver _status;
    private Animator _animator;
    private Renderer[] _renderers;
    private MaterialPropertyBlock _block;
    private Vector3 _previousPosition;
    private float _flashRemaining;
    private bool _dead;

    public void Initialize(EnemyController enemy)
    {
        _enemy = enemy;
        _health = enemy.GetComponent<Health>();
        _status = enemy.GetComponent<StatusEffectReceiver>();
        _animator = GetComponent<Animator>();
        _renderers = GetComponentsInChildren<Renderer>();
        _block = new MaterialPropertyBlock();
        _previousPosition = enemy.transform.position;
        _enemy.Attacked += OnAttack;
        _health.Damaged += OnDamaged;
        _health.Died += OnDied;
    }

    private void LateUpdate()
    {
        if (_enemy == null) return;
        var delta = _enemy.transform.position - _previousPosition;
        delta.y = 0f;
        _previousPosition = _enemy.transform.position;
        if (_animator != null && !_dead)
        {
            var blocked = _status != null && _status.BlocksMovement && _status.BlocksAttack;
            _animator.speed = blocked ? 0f : 1f;
            var speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            _animator.SetFloat(Speed, speed > movementThreshold ? speed : 0f);
        }
        if (_flashRemaining <= 0f) return;
        _flashRemaining -= Time.deltaTime;
        foreach (var renderer in _renderers)
        {
            if (renderer == null) continue;
            renderer.GetPropertyBlock(_block);
            var material = renderer.sharedMaterial;
            if (material != null && material.HasProperty(BaseColor))
                _block.SetColor(BaseColor, _flashRemaining > 0f ? hitColor : material.GetColor(BaseColor));
            renderer.SetPropertyBlock(_block);
        }
    }

    private void OnAttack()
    {
        if (_animator != null && !_dead) _animator.SetTrigger(Attack);
    }

    private void OnDamaged(float amount, GameObject source) => _flashRemaining = flashDuration;

    private void OnDied()
    {
        _dead = true;
        if (_animator == null) return;
        // Mieści upadek w istniejącym znikaniu przeciwnika (0,35 s).
        _animator.speed = 0.9f / 0.35f;
        _animator.Play("Death", 0, 0f);
    }

    private void OnDestroy()
    {
        if (_enemy != null) _enemy.Attacked -= OnAttack;
        if (_health == null) return;
        _health.Damaged -= OnDamaged;
        _health.Died -= OnDied;
    }
}
