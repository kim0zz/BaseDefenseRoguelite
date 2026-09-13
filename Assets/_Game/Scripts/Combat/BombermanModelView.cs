using UnityEngine;

/// <summary>Doctor presentation only. Samples action clips against the existing combat clock.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class BombermanModelView : MonoBehaviour
{
    private Animator _animator;
    private PlayerCharacter _player;
    private PlayerAttackController _attack;
    private PlayerSkillController _skills;
    private Health _health;
    private Renderer _capsule;
    private Renderer[] _renderers;
    private MaterialPropertyBlock _block;
    private Vector3 _lastPosition;
    private float _locomotionTime;
    private float _deathTime;
    private float _flash;
    private bool _wasDead;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    public string CurrentVisualAction { get; private set; } = "Idle";

    public void Initialize(GameObject player)
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        _animator.speed = 0f;
        _player = player.GetComponent<PlayerCharacter>();
        _attack = player.GetComponent<PlayerAttackController>();
        _skills = player.GetComponent<PlayerSkillController>();
        _health = player.GetComponent<Health>();
        _capsule = player.GetComponent<Renderer>();
        if (_capsule != null) _capsule.enabled = false;
        _renderers = GetComponentsInChildren<Renderer>();
        _block = new MaterialPropertyBlock();
        _lastPosition = player.transform.position;
        _health.Damaged += OnDamaged;
        HidePlaceholder();
    }

    private void LateUpdate()
    {
        if (_player == null || _animator.runtimeAnimatorController == null) return;
        var delta = _player.transform.position - _lastPosition;
        delta.y = 0f;
        _lastPosition = _player.transform.position;
        var speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
        var dead = !_health.IsAlive;
        // Leave the existing aim arc in place; hide only the old hand-held primitive.
        HidePlaceholder();
        if (_capsule != null) _capsule.enabled = false;

        var moving = speed > .12f && !dead;
        _locomotionTime += Time.deltaTime * (moving ? Mathf.Clamp(speed / 5f, .5f, 1.8f) / .533333f : .5f);
        _animator.Play(moving ? "Run" : "Idle", 0, _locomotionTime % 1f);
        _animator.SetLayerWeight(1, 0f);
        _animator.SetLayerWeight(2, 0f);
        CurrentVisualAction = moving ? "Run" : "Idle";

        if (dead)
        {
            _deathTime = _wasDead ? _deathTime + Time.deltaTime : 0f;
            SampleAction("Death", Mathf.Min(_deathTime / .75f, .999f), true);
        }
        else if (_skills != null && _skills.IsCasting)
        {
            var slot = _skills.ActiveSlot;
            var skill = _skills.GetSkill(slot);
            if (skill != null)
            {
                var action = slot == 0 ? "Place" : slot == 1 ? "Detonate" : slot == 2 ? "Kick" : "Cast";
                var progress = ActionProgress(_skills.CastElapsed, skill.WindupSeconds,
                    skill.ActiveSeconds + skill.RecoverySeconds);
                SampleAction(action, progress, slot == 0 || slot == 2);
            }
        }
        else if (_attack != null && _attack.IsAttacking && _attack.CycleFamily == WeaponFamily.ThrownExplosive)
        {
            var windup = _attack.SingleAttackInterval * WeaponFeelProfile.For(WeaponFamily.ThrownExplosive).WindupFraction;
            SampleAction("Throw", ActionProgress(_attack.SingleAttackElapsed, windup,
                _attack.SingleAttackInterval - windup), false);
        }
        _wasDead = dead;
        _animator.Update(0f);

        _flash = Mathf.Max(0f, _flash - Time.deltaTime);
        var tint = _flash > 0f ? new Color(1f, .32f, .32f) : dead ? new Color(.35f, .5f, .6f) : Color.white;
        foreach (var r in _renderers)
        {
            r.GetPropertyBlock(_block);
            _block.SetColor(BaseColor, tint);
            r.SetPropertyBlock(_block);
        }
    }

    // Blender contact is at frame 16/31 (.5); actual game timings can vary with stats.
    public static float ActionProgress(float elapsed, float windup, float remainder)
    {
        if (elapsed < windup && windup > 0f) return .5f * Mathf.Clamp01(elapsed / windup);
        return .5f + .499f * Mathf.Clamp01((elapsed - windup) / Mathf.Max(.001f, remainder));
    }

    private void SampleAction(string action, float progress, bool fullBody)
    {
        var layer = fullBody ? 2 : 1;
        _animator.SetLayerWeight(layer, 1f);
        _animator.Play(action, layer, progress);
        CurrentVisualAction = action;
    }

    private void HidePlaceholder()
    {
        // RebuildWeapon can leave an old child pending Destroy until the end of the frame.
        foreach (Transform child in _player.transform)
            if (child.name == "WeaponPlaceholder" && child.gameObject.activeSelf)
                child.gameObject.SetActive(false);
    }

    private void OnDamaged(float amount, GameObject source) => _flash = .12f;
    private void OnDestroy()
    {
        if (_health != null) _health.Damaged -= OnDamaged;
        if (_capsule != null) _capsule.enabled = true;
    }
}
