using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Poruszająca się fala wzdłuż lane path (Trzęsienie Świata).
/// </summary>
[DisallowMultipleComponent]
public class SkillLaneWavePulse : MonoBehaviour
{
    private SkillDefinition _skill;
    private GameObject _caster;
    private Vector3[] _waypoints;
    private float _speed;
    private float _width;
    private float _damageMul;
    private float _staggerMul;
    private float _bossStaggerMul;
    private LayerMask _targetLayers;
    private float _segmentProgress;
    private int _segmentIndex;
    private float _delayRemaining;
    private bool _spawnGroundCracks;
    private float _crackDuration;
    private float _crackDps;
    private LaneWaveCastState _castState;
    private readonly HashSet<Health> _hitTargets = new();
    private LineRenderer _visual;

    public static void Spawn(
        SkillDefinition skill,
        GameObject caster,
        LanePath path,
        LayerMask targetLayers,
        float delaySeconds,
        float damageMultiplier,
        float staggerMultiplier,
        bool spawnGroundCracks,
        float crackDuration,
        float crackDps,
        LaneWaveCastState castState,
        float bossStaggerMultiplier = -1f)
    {
        if (skill == null || caster == null || path.Waypoints == null || path.Waypoints.Length < 2) return;

        var go = new GameObject("LaneWavePulse");
        var pulse = go.AddComponent<SkillLaneWavePulse>();
        pulse.Initialize(
            skill, caster, path.Waypoints, targetLayers,
            SkillLaneWaveExecutor.WaveSpeed, SkillLaneWaveExecutor.WaveWidth,
            delaySeconds, damageMultiplier, staggerMultiplier,
            spawnGroundCracks, crackDuration, crackDps, castState,
            bossStaggerMultiplier);
    }

    private void Initialize(
        SkillDefinition skill,
        GameObject caster,
        Vector3[] waypoints,
        LayerMask targetLayers,
        float speed,
        float width,
        float delaySeconds,
        float damageMul,
        float staggerMul,
        bool spawnGroundCracks,
        float crackDuration,
        float crackDps,
        LaneWaveCastState castState,
        float bossStaggerMul = -1f)
    {
        _skill = skill;
        _caster = caster;
        _waypoints = waypoints;
        _targetLayers = targetLayers;
        _speed = speed;
        _width = width;
        _delayRemaining = delaySeconds;
        _damageMul = damageMul;
        _staggerMul = staggerMul;
        _bossStaggerMul = bossStaggerMul >= 0f ? bossStaggerMul : staggerMul;
        _spawnGroundCracks = spawnGroundCracks;
        _crackDuration = crackDuration;
        _crackDps = crackDps;
        _castState = castState;
        transform.position = waypoints[0];
        BuildVisual();
    }

    private void BuildVisual()
    {
        if (_skill != null && _skill.ImpactPrefab != null)
        {
            var instance = Instantiate(_skill.ImpactPrefab, transform);
            instance.transform.localPosition = Vector3.zero;
            return;
        }

        _visual = gameObject.AddComponent<LineRenderer>();
        _visual.useWorldSpace = false;
        _visual.loop = true;
        _visual.positionCount = 24;
        _visual.startWidth = 0.28f;
        _visual.endWidth = 0.28f;
        _visual.material = new Material(Shader.Find("Sprites/Default"));
        _visual.startColor = new Color(1f, 0.72f, 0.12f, 0.95f);
        _visual.endColor = _visual.startColor;
        RefreshRing(_width * 0.5f);
    }

    private void RefreshRing(float radius)
    {
        if (_visual == null) return;

        for (var i = 0; i < _visual.positionCount; i++)
        {
            var angle = i / (float)_visual.positionCount * Mathf.PI * 2f;
            _visual.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
    }

    private void Update()
    {
        if (_delayRemaining > 0f)
        {
            _delayRemaining -= Time.deltaTime;
            return;
        }

        if (_waypoints == null || _segmentIndex >= _waypoints.Length - 1)
        {
            Destroy(gameObject);
            return;
        }

        var from = _waypoints[_segmentIndex];
        var to = _waypoints[_segmentIndex + 1];
        var segmentLength = Vector3.Distance(from, to);
        if (segmentLength <= 0.01f)
        {
            _segmentIndex++;
            _segmentProgress = 0f;
            return;
        }

        _segmentProgress += _speed * Time.deltaTime;
        while (_segmentProgress >= segmentLength && _segmentIndex < _waypoints.Length - 1)
        {
            _segmentProgress -= segmentLength;
            _segmentIndex++;
            if (_segmentIndex >= _waypoints.Length - 1) break;
            from = _waypoints[_segmentIndex];
            to = _waypoints[_segmentIndex + 1];
            segmentLength = Vector3.Distance(from, to);
            if (segmentLength <= 0.01f) continue;
        }

        if (_segmentIndex >= _waypoints.Length - 1)
        {
            transform.position = _waypoints[_waypoints.Length - 1];
            ResolveHits();
            Destroy(gameObject);
            return;
        }

        var t = segmentLength > 0.01f ? _segmentProgress / segmentLength : 0f;
        transform.position = Vector3.Lerp(from, to, t);
        ResolveHits();
    }

    private void ResolveHits()
    {
        var center = transform.position;
        var hits = Physics.OverlapSphere(center + Vector3.up * 0.5f, _width * 0.5f, _targetLayers);
        foreach (var col in hits)
        {
            if (col == null) continue;
            if (col.GetComponentInParent<PlayerCharacter>() != null) continue;
            if (!SkillTargetFilter.IsValidTarget(col.gameObject, _skill, _caster)) continue;

            var health = col.GetComponentInParent<Health>();
            if (health == null || !health.IsAlive) continue;
            if (!_hitTargets.Add(health)) continue;

            var dmg = PlayerPersistentEffects.ModifyOutgoingDamage(_caster, _skill.Damage * _damageMul);
            health.TakeDamage(dmg, _caster);

            if (_skill.ControlDurationSeconds > 0f)
            {
                var stun = ForcedMovementRequest.StunControl(
                    _caster,
                    _skill.ControlDurationSeconds * _staggerMul,
                    _skill.BossStaggerContribution * _bossStaggerMul);
                ForcedMovementResolver.Apply(stun, col.gameObject);
            }

            if (_castState != null && _castState.HasKrwawaSejsmika)
            {
                var heal = PersistentEffectRuntime.ComputeKrwawaHeal(
                    _castState.KrwawaHealAccum,
                    _castState.KrwawaPerHit,
                    _castState.KrwawaCap);
                if (heal > 0f)
                {
                    _castState.KrwawaHealAccum += heal;
                    _caster.GetComponent<Health>()?.Heal(heal);
                }
            }

            if (_spawnGroundCracks)
                TrySpawnGroundCrackAt(health.transform.position);
        }
    }

    private void TrySpawnGroundCrackAt(Vector3 position)
    {
        var spawned = SkillEffectApplier.TrySpawnSpacedDamageZone(
            position,
            _width * 0.5f,
            _caster,
            _crackDps,
            _crackDuration,
            _castState != null ? _castState.CrackOrigins : null,
            _width * 0.55f);
        if (spawned)
            Debug.Log($"[Rozpadlina] krater @ trafienie {position}.");
    }
}

public sealed class LaneWaveCastState
{
    public bool HasKrwawaSejsmika;
    public float KrwawaPerHit = 4f;
    public float KrwawaCap = 32f;
    public float KrwawaHealAccum;
    public readonly List<Vector3> CrackOrigins = new();
}
