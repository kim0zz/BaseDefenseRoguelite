using System.Collections.Generic;
using UnityEngine;

/// <summary>Integracja sił sprężystej liny — LateUpdate po ruchu gracza.</summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(50)]
public class ChainTetherSolver : MonoBehaviour
{
    public struct LinkDebugState
    {
        public int LinkIndex;
        public float Distance;
        public ChainTetherZone Zone;
        public float SpringAcceleration;
        public float AppliedAcceleration;
        public Vector3 RopeDirection;
    }

    [SerializeField] private ChainTetherRuntime runtime;
    [SerializeField] private PlayerJoinManager joinManager;
    [SerializeField] private float velocityDampingPerSecond = 6f;
    [SerializeField] private float hubVelocityDampingPerSecond = 3.2f;
    [SerializeField] private float maxTetherSpeed = 18f;
    [SerializeField] private float maxHubSpeed = 24f;

    private readonly ChainTetherGraph _graph = new();
    private readonly Dictionary<ChainTetherBody, Vector3> _accumulatedAccel = new();
    private readonly Dictionary<ChainTetherBody, Vector3> _combinedVelocity = new();
    private LinkDebugState[] _debugStates = System.Array.Empty<LinkDebugState>();

    private void Awake()
    {
        if (runtime == null)
            runtime = GetComponent<ChainTetherRuntime>();
        if (joinManager == null)
            joinManager = FindAnyObjectByType<PlayerJoinManager>();
    }

    private void LateUpdate()
    {
        if (runtime == null) return;

        var players = joinManager != null
            ? joinManager.Players
            : new List<PlayerCharacter>(FindObjectsByType<PlayerCharacter>());

        var hubBody = runtime.HubBody;
        _graph.Rebuild(runtime.ActiveTopology, players, hubBody);
        var dt = Time.deltaTime;
        var bodies = CollectBodies(players, hubBody);
        CacheCombinedVelocities(bodies, dt);

        _accumulatedAccel.Clear();
        var tuning = runtime.ActiveTuning;
        _debugStates = _graph.Links.Count == 0
            ? System.Array.Empty<LinkDebugState>()
            : new LinkDebugState[_graph.Links.Count];

        for (var i = 0; i < _graph.Links.Count; i++)
            SolveLink(_graph.Links[i], tuning, i);

        IntegrateBodies(bodies, dt);
        runtime.SetLastLinkDebug(_debugStates);
    }

    private static List<ChainTetherBody> CollectBodies(
        IReadOnlyList<PlayerCharacter> players,
        ChainTetherBody hubBody)
    {
        var bodies = new List<ChainTetherBody>();
        for (var i = 0; i < players.Count; i++)
        {
            var body = players[i] != null ? players[i].GetComponent<ChainTetherBody>() : null;
            if (body != null)
                bodies.Add(body);
        }

        if (hubBody != null && !bodies.Contains(hubBody))
            bodies.Add(hubBody);
        return bodies;
    }

    private void CacheCombinedVelocities(List<ChainTetherBody> bodies, float dt)
    {
        _combinedVelocity.Clear();
        for (var i = 0; i < bodies.Count; i++)
        {
            var body = bodies[i];
            if (body == null) continue;
            _combinedVelocity[body] = body.SampleLocomotionVelocity(dt) + body.Velocity;
        }
    }

    private void IntegrateBodies(List<ChainTetherBody> bodies, float dt)
    {
        for (var i = 0; i < bodies.Count; i++)
        {
            var body = bodies[i];
            if (body == null) continue;

            _accumulatedAccel.TryGetValue(body, out var accel);
            var isHub = body.Player == null;
            body.Velocity = ChainTetherMath.IntegrateTetherVelocity(
                body.Velocity,
                accel,
                dt,
                isHub ? hubVelocityDampingPerSecond : velocityDampingPerSecond,
                isHub ? maxHubSpeed : maxTetherSpeed);

            ApplyDisplacement(body, body.Velocity * dt);
            body.MarkPoseAfterTether();
        }
    }

    private void SolveLink(ChainTetherGraph.Link link, ChainTetherTuning tuning, int debugIndex)
    {
        var a = link.BodyA;
        var b = link.BodyB;
        if (a == null || b == null) return;

        var delta = b.Position - a.Position;
        delta.y = 0f;
        var distance = delta.magnitude;
        if (distance < 0.001f) return;

        var ropeDir = delta / distance;
        var velA = _combinedVelocity.TryGetValue(a, out var cachedA) ? cachedA : a.Velocity;
        var velB = _combinedVelocity.TryGetValue(b, out var cachedB) ? cachedB : b.Velocity;
        var relSpeed = Vector3.Dot(velB - velA, ropeDir);

        var awayA = ChainTetherMath.ComputeInputAway01(a.Player != null ? a.Player.CurrentMoveDirection : Vector3.zero, ropeDir);
        var awayB = ChainTetherMath.ComputeInputAway01(b.Player != null ? b.Player.CurrentMoveDirection : Vector3.zero, -ropeDir);

        var evalA = ChainTetherMath.Evaluate(distance, relSpeed, awayA, tuning);
        var evalB = ChainTetherMath.Evaluate(distance, relSpeed, awayB, tuning);
        var pullScalar = (evalA.DampedAcceleration + evalB.DampedAcceleration) * 0.5f;

        if (pullScalar <= 0f)
        {
            _debugStates[debugIndex] = new LinkDebugState
            {
                LinkIndex = link.Index,
                Distance = distance,
                Zone = evalA.Zone,
                SpringAcceleration = evalA.SpringAcceleration,
                AppliedAcceleration = 0f,
                RopeDirection = ropeDir
            };
            return;
        }

        var accelOnA = ropeDir * (pullScalar / a.Mass);
        var accelOnB = -ropeDir * (pullScalar / b.Mass);

        Accumulate(a, accelOnA);
        Accumulate(b, accelOnB);

        _debugStates[debugIndex] = new LinkDebugState
        {
            LinkIndex = link.Index,
            Distance = distance,
            Zone = evalA.Zone,
            SpringAcceleration = evalA.SpringAcceleration,
            AppliedAcceleration = pullScalar,
            RopeDirection = ropeDir
        };
    }

    private void Accumulate(ChainTetherBody body, Vector3 accel)
    {
        accel.y = 0f;
        if (body == null) return;
        if (_accumulatedAccel.TryGetValue(body, out var existing))
            _accumulatedAccel[body] = existing + accel;
        else
            _accumulatedAccel[body] = accel;
    }

    private static void ApplyDisplacement(ChainTetherBody body, Vector3 displacement)
    {
        displacement.y = 0f;
        if (displacement.sqrMagnitude < 0.0000001f) return;

        var cc = body.GetComponent<CharacterController>();
        if (cc != null && cc.enabled)
        {
            cc.Move(displacement);
            return;
        }

        body.transform.Translate(displacement, Space.World);
    }
}
