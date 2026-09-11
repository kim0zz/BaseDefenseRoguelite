using UnityEngine;

/// <summary>
/// Placeholder broni + strefa ataku (windup słaba, active mocna). Zero FBX.
/// </summary>
[DisallowMultipleComponent]
public class WeaponPlaceholderView : MonoBehaviour
{
    private PlayerBuildState _build;
    private PlayerAttackController _attack;
    private Animator _animator;
    private GameObject _weaponRoot;
    private GameObject _arcRoot;
    private Mesh _arcMesh;
    private MeshFilter _arcFilter;
    private WeaponFamily _shownFamily;
    private float _swingT;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int WeaponFamilyHash = Animator.StringToHash("WeaponFamily");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private void Awake()
    {
        _build = GetComponent<PlayerBuildState>();
        _attack = GetComponent<PlayerAttackController>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (_build != null)
            _build.BuildChanged += RebuildWeapon;
        RebuildWeapon();
    }

    private void OnDisable()
    {
        if (_build != null)
            _build.BuildChanged -= RebuildWeapon;
    }

    private void OnDestroy()
    {
        if (_arcRoot != null)
            Destroy(_arcRoot);
        if (_arcMesh != null)
            Destroy(_arcMesh);
    }

    private void Update()
    {
        UpdateAnimator();
        UpdateSwingAndArc();
    }

    public void RebuildWeapon()
    {
        var family = ResolveFamily();
        if (_weaponRoot != null && _shownFamily == family) return;

        if (_weaponRoot != null)
            Destroy(_weaponRoot);
        if (_arcRoot != null)
            Destroy(_arcRoot);
        if (_arcMesh != null)
            Destroy(_arcMesh);
        _weaponRoot = null;
        _arcRoot = null;
        _arcMesh = null;

        _shownFamily = family;
        _weaponRoot = BuildWeaponMesh(family);
        _arcRoot = BuildArcMesh();
        SetArcVisible(false, false, idleAim: false);
    }

    public void HideSwing()
    {
        _swingT = 0f;
        if (_weaponRoot != null)
            _weaponRoot.transform.localRotation = Quaternion.identity;
        SetArcVisible(false, false, idleAim: false);
    }

    private WeaponFamily ResolveFamily()
    {
        if (_attack != null && _attack.IsAttacking)
            return _attack.CycleFamily;

        var weapon = _build != null ? _build.GetWeapon(_build.ActiveWeaponIndex) : null;
        return weapon != null ? weapon.Family : WeaponFamily.Sword;
    }

    private void UpdateAnimator()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null) return;

        var health = GetComponent<Health>();
        _animator.SetInteger(WeaponFamilyHash, (int)ResolveFamily());
        _animator.SetBool(DeadHash, health != null && !health.IsAlive);

        var character = GetComponent<PlayerCharacter>();
        var speed = character != null && character.IsCombatEnabled ? 1f : 0f;
        _animator.SetFloat(SpeedHash, speed);
    }

    public void NotifyAttackStarted()
    {
        if (_animator != null && _animator.runtimeAnimatorController != null)
            _animator.SetTrigger(AttackHash);
    }

    private void UpdateSwingAndArc()
    {
        if (_attack == null)
        {
            HideSwing();
            return;
        }

        var phase = _attack.Phase;
        var inSwing = phase == AttackPhase.Windup || phase == AttackPhase.Active;
        if (inSwing)
            _swingT = Mathf.Clamp01(_swingT + Time.deltaTime * 6f);
        else
            _swingT = Mathf.Clamp01(_swingT - Time.deltaTime * 8f);

        if (_weaponRoot != null)
        {
            var yaw = Mathf.Lerp(50f, -55f, _swingT);
            _weaponRoot.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        var family = _attack.CycleFamily;
        var player = GetComponent<PlayerCharacter>();
        var facing = inSwing
            ? _attack.AttackFacing
            : (player != null ? player.AimDirection : _attack.AttackFacing);

        if (!inSwing)
        {
            SetArcVisible(true, false, idleAim: true);
            AlignArc(facing, Mathf.Min(1.35f, Mathf.Max(0.8f, _attack.EffectiveRange * 0.45f)),
                _attack.EffectiveArc, family, aimTick: true);
            return;
        }

        SetArcVisible(true, phase == AttackPhase.Active, idleAim: false);
        AlignArc(facing, _attack.EffectiveRange, _attack.EffectiveArc, family, aimTick: family == WeaponFamily.Bow);
    }

    private GameObject BuildWeaponMesh(WeaponFamily family)
    {
        var root = new GameObject("WeaponPlaceholder");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = new Vector3(0.38f, 0.35f, 0.28f);
        root.transform.localRotation = Quaternion.identity;

        switch (family)
        {
            case WeaponFamily.Dagger:
                AddBox(root.transform, "Blade", new Vector3(0.08f, 0.08f, 0.55f), new Vector3(0f, 0.1f, 0.35f),
                    new Color(0.35f, 0.36f, 0.4f));
                break;
            case WeaponFamily.Axe:
                AddBox(root.transform, "Haft", new Vector3(0.12f, 0.12f, 1.15f), new Vector3(0f, 0.1f, 0.5f),
                    new Color(0.45f, 0.28f, 0.14f));
                AddBox(root.transform, "Head", new Vector3(0.55f, 0.18f, 0.35f), new Vector3(0.15f, 0.1f, 1.05f),
                    new Color(0.25f, 0.28f, 0.32f));
                break;
            case WeaponFamily.Bow:
                AddBox(root.transform, "Limb", new Vector3(0.08f, 0.85f, 0.12f), new Vector3(0f, 0.2f, 0.15f),
                    new Color(0.4f, 0.22f, 0.1f));
                AddBox(root.transform, "String", new Vector3(0.03f, 0.7f, 0.03f), new Vector3(0.18f, 0.2f, 0.15f),
                    new Color(0.85f, 0.8f, 0.7f));
                break;
            default:
                AddBox(root.transform, "Blade", new Vector3(0.1f, 0.08f, 1.15f), new Vector3(0f, 0.1f, 0.6f),
                    new Color(0.72f, 0.75f, 0.8f));
                break;
        }

        return root;
    }

    private GameObject BuildArcMesh()
    {
        var go = new GameObject("AttackArcPlaceholder");
        _arcFilter = go.AddComponent<MeshFilter>();
        var renderer = go.AddComponent<MeshRenderer>();
        _arcMesh = new Mesh { name = "AttackArcWedge" };
        _arcFilter.mesh = _arcMesh;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.SetActive(false);
        renderer.sharedMaterial = cube.GetComponent<Renderer>().sharedMaterial;
        Object.Destroy(cube);

        return go;
    }

    private void AlignArc(Vector3 facing, float range, float arcDegrees, WeaponFamily family, bool aimTick)
    {
        if (_arcRoot == null || _arcMesh == null) return;

        facing.y = 0f;
        if (facing.sqrMagnitude < 0.01f)
            facing = Vector3.forward;
        facing.Normalize();

        _arcRoot.transform.position = transform.position + Vector3.up * 0.14f;
        _arcRoot.transform.rotation = Quaternion.LookRotation(facing, Vector3.up);

        if (aimTick || family == WeaponFamily.Bow)
            BuildAimLineMesh(range);
        else
            BuildWedgeMesh(range, arcDegrees);
    }

    private void BuildWedgeMesh(float range, float arcDegrees)
    {
        AttackArcGeometry.WedgePoints(Vector3.forward, range, arcDegrees, out var start, out var left, out var right);
        _arcMesh.Clear();
        _arcMesh.vertices = new[] { start, left, right };
        _arcMesh.triangles = new[] { 0, 1, 2, 0, 2, 1 };
        _arcMesh.RecalculateNormals();
        _arcMesh.RecalculateBounds();
    }

    private void BuildAimLineMesh(float range)
    {
        var half = 0.08f;
        var z0 = 0.2f;
        var z1 = Mathf.Max(range, z0 + 0.1f);
        _arcMesh.Clear();
        _arcMesh.vertices = new[]
        {
            new Vector3(-half, 0f, z0),
            new Vector3(half, 0f, z0),
            new Vector3(half, 0f, z1),
            new Vector3(-half, 0f, z1)
        };
        _arcMesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 3, 2, 0, 2, 1 };
        _arcMesh.RecalculateNormals();
        _arcMesh.RecalculateBounds();
    }

    private void SetArcVisible(bool visible, bool activeWindow, bool idleAim)
    {
        if (_arcRoot == null) return;
        _arcRoot.SetActive(visible);
        if (!visible) return;

        Color color;
        if (idleAim)
            color = new Color(0.55f, 0.9f, 1f, 1f);
        else if (activeWindow)
            color = new Color(1f, 0.95f, 0.15f, 1f);
        else
            color = new Color(0.2f, 0.75f, 1f, 1f);
        ApplyColor(_arcRoot, color);
    }

    private static void AddBox(Transform parent, string name, Vector3 scale, Vector3 localPos, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPos;
        box.transform.localScale = scale;
        StripCollider(box);
        ApplyColor(box, color);
    }

    private static void StripCollider(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }

    private static void ApplyColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;
        var mat = new Material(renderer.sharedMaterial);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;
        renderer.material = mat;
    }
}
