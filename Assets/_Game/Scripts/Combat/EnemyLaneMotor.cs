using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ruch wroga wzdłuż przypisanej linii — polyline + clamp boczny + separacja (M6.5).
/// </summary>
[DisallowMultipleComponent]
public class EnemyLaneMotor : MonoBehaviour
{
    [SerializeField] private float separationStrength = 0.65f;
    [Tooltip("Mnożnik szerokości korytarza przy sprawdzaniu agro Grunta na gracza.")]
    [SerializeField] private float laneLeashMultiplier = 1f;

    private AttackLineId _assignedLane;
    private LanePath _lanePath;
    private EnemyDefinition _definition;
    private MapGreyboxBuilder _mapBuilder;
    private KnockbackReceiver _knockback;
    private EnemyController _controller;

    private static readonly List<Vector3> SeparationPositions = new();
    private static readonly List<float> SeparationRadii = new();
    private static readonly Dictionary<EnemyController, int> SiegeSeparationIndices = new();
    private static int _siegeSeparationFrame = -1;
    private static SiegeArena _separationArena;

    public AttackLineId AssignedLane => _assignedLane;
    public LanePath LanePath => _lanePath;
    public float LaneLeashMultiplier => laneLeashMultiplier;

    public float LaneCorridorHalfWidthExtra()
    {
        return _lanePath.HalfWidth * Mathf.Max(0f, laneLeashMultiplier - 1f);
    }

    public void Initialize(AttackLineId lane, EnemyDefinition definition)
    {
        _assignedLane = lane;
        _definition = definition;
        RefreshLanePath();
    }

    private void Awake()
    {
        _knockback = GetComponent<KnockbackReceiver>();
        _controller = GetComponent<EnemyController>();
    }

    public void RefreshLanePath()
    {
        if (_mapBuilder == null)
            _mapBuilder = FindAnyObjectByType<MapGreyboxBuilder>();

        if (_definition != null && _definition.Kind == EnemyKind.Flanker)
        {
            _lanePath = MapGreyboxLayout.GetBypassLanePath(_assignedLane);
            return;
        }

        _lanePath = _mapBuilder != null
            ? _mapBuilder.GetLanePath(_assignedLane)
            : MapGreyboxLayout.GetLanePath(_assignedLane);
    }

    public bool IsInLaneCorridor(Vector3 worldPos, float extraLeash = 0f)
    {
        return _lanePath.IsInCorridor(worldPos, extraLeash);
    }

    public void ClampToLane()
    {
        if (SiegeArena.Instance != null)
        {
            transform.position = SiegeArena.Instance.ClampEnemy(transform.position);
            return;
        }
        var pos = transform.position;
        pos = _lanePath.ClampToCorridor(pos);
        transform.position = pos;
    }

    public void AdvanceAlongLane(float distance)
    {
        if (SiegeArena.Instance != null)
        {
            MoveDirect(SiegeArena.Instance.GetEnemyDestination(transform.position), distance);
            return;
        }
        var previousPos = transform.position;
        var pos = _lanePath.AdvanceAlongPath(previousPos, distance);
        pos = ApplySeparationAndReclamp(pos);
        FaceDirection(pos - previousPos);
        transform.position = pos;
    }

    public void MoveTowardAlongCorridor(Vector3 target, float maxDistance)
    {
        if (SiegeArena.Instance != null)
        {
            MoveDirect(target, maxDistance);
            return;
        }
        var pos = _lanePath.MoveTowardAlongCorridor(transform.position, target, maxDistance);
        pos = ApplySeparationAndReclamp(pos);
        transform.position = pos;
        FaceDirection(target - transform.position);
    }

    public void FaceToward(Vector3 worldTarget)
    {
        FaceDirection(worldTarget - transform.position);
    }

    public void MoveDirect(Vector3 target, float maxDistance)
    {
        var toTarget = target - transform.position;
        toTarget.y = 0f;
        var dist = toTarget.magnitude;
        if (dist < 0.0001f) return;

        var step = Mathf.Min(maxDistance, dist);
        var pos = transform.position + toTarget.normalized * step;
        pos = ApplySeparation(pos, clampToLane: false);
        if (SiegeArena.Instance != null) pos = SiegeArena.Instance.ClampEnemy(pos);
        transform.position = pos;
        FaceDirection(toTarget);
    }

    private Vector3 ApplySeparationAndReclamp(Vector3 position)
    {
        return ApplySeparation(position, clampToLane: true);
    }

    private Vector3 ApplySeparation(Vector3 position, bool clampToLane)
    {
        if (SiegeArena.Instance != null)
            return ApplySiegeSeparation(position);
        if (_knockback != null && _knockback.IsActive)
            return position;

        if (separationStrength <= 0f)
            return clampToLane ? _lanePath.ClampToCorridor(position) : position;

        BuildSeparationBuffers();
        var selfIndex = FindLivingSelfIndex();
        if (selfIndex < 0 || selfIndex >= SeparationPositions.Count)
            return clampToLane ? _lanePath.ClampToCorridor(position) : position;

        var radius = _definition != null
            ? EnemySeparation.RadiusFromBodyScale(_definition.BodyScale)
            : 0.5f;

        SeparationRadii[selfIndex] = radius;
        SeparationPositions[selfIndex] = position;

        var separated = EnemySeparation.Apply(
            position, selfIndex, SeparationPositions, SeparationRadii, separationStrength);

        return clampToLane ? _lanePath.ClampToCorridor(separated) : separated;
    }

    private void BuildSeparationBuffers()
    {
        SeparationPositions.Clear();
        SeparationRadii.Clear();

        foreach (var enemy in EnemyRegistry.Active)
        {
            if (enemy == null) continue;

            var health = enemy.GetComponent<Health>();
            if (health != null && !health.IsAlive) continue;

            var def = enemy.Definition;
            var scale = def != null ? def.BodyScale : Vector3.one * 0.85f;
            SeparationPositions.Add(enemy.transform.position);
            SeparationRadii.Add(EnemySeparation.RadiusFromBodyScale(scale));
        }
    }

    private Vector3 ApplySiegeSeparation(Vector3 position)
    {
        if (separationStrength <= 0f || (_knockback != null && _knockback.IsActive)) return position;
        if (_siegeSeparationFrame != Time.frameCount || _separationArena != SiegeArena.Instance)
        {
            _siegeSeparationFrame = Time.frameCount;
            _separationArena = SiegeArena.Instance;
            SeparationPositions.Clear();
            SeparationRadii.Clear();
            SiegeSeparationIndices.Clear();
            foreach (var enemy in EnemyRegistry.Active)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                SiegeSeparationIndices[enemy] = SeparationPositions.Count;
                SeparationPositions.Add(enemy.transform.position);
                var scale = enemy.Definition != null ? enemy.Definition.BodyScale : Vector3.one * 0.85f;
                SeparationRadii.Add(EnemySeparation.RadiusFromBodyScale(scale));
            }
        }
        if (_controller == null) _controller = GetComponent<EnemyController>();
        if (_controller == null || !SiegeSeparationIndices.TryGetValue(_controller, out var index)) return position;
        var separated = EnemySeparation.Apply(position, index, SeparationPositions, SeparationRadii, separationStrength);
        // Bounded displacement prevents a packed spawn from explosively ejecting agents.
        return position + Vector3.ClampMagnitude(separated - position, Time.deltaTime * 2f);
    }

    /// <summary>
    /// Index in the living-only separation buffers — not EnemyRegistry.Active.
    /// Dead enemies stay registered during DeathRoutine; mixing those indices
    /// throws ArgumentOutOfRangeException every frame (level-up pause freeze).
    /// </summary>
    private int FindLivingSelfIndex()
    {
        var controller = GetComponent<EnemyController>();
        return EnemyLivingBuffer.IndexOf(EnemyRegistry.Active, controller);
    }

    private void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}
