using System;
using UnityEngine;

/// <summary>Czysta matematyka liny — testowalna bez sceny Unity.</summary>
public static class ChainTetherMath
{
    public readonly struct EvaluateResult
    {
        public EvaluateResult(
            ChainTetherZone zone,
            float springAcceleration,
            float dampedAcceleration)
        {
            Zone = zone;
            SpringAcceleration = springAcceleration;
            DampedAcceleration = dampedAcceleration;
        }

        public ChainTetherZone Zone { get; }
        public float SpringAcceleration { get; }
        public float DampedAcceleration { get; }
    }

    public static ChainTetherZone ResolveZone(float distance, ChainTetherTuning tuning)
    {
        if (distance <= tuning.softTensionStart) return ChainTetherZone.Slack;
        if (distance <= tuning.hardTensionStart) return ChainTetherZone.Soft;
        return ChainTetherZone.Hard;
    }

    public static float ComputeSpringAcceleration(float distance, ChainTetherTuning tuning)
    {
        if (distance <= tuning.softTensionStart)
            return tuning.slackSpring;

        var softSpan = Mathf.Max(0.001f, tuning.hardTensionStart - tuning.softTensionStart);
        var softFactor = Mathf.Clamp01((distance - tuning.softTensionStart) / softSpan);
        var softAccel = tuning.softSpring * softFactor;

        if (distance <= tuning.hardTensionStart)
            return softAccel;

        var hardSpan = Mathf.Max(0.001f, tuning.maxStretch - tuning.hardTensionStart);
        var hardFactor = (distance - tuning.hardTensionStart) / hardSpan;
        var hardAccel = tuning.hardSpring * hardFactor;

        if (distance > tuning.maxStretch)
        {
            var beyond = distance - tuning.maxStretch;
            hardAccel += tuning.hardSpring * (beyond / hardSpan);
        }

        return softAccel + hardAccel;
    }

    public static float ComputeDampingAcceleration(float relativeSpeedAlongRope, ChainTetherTuning tuning)
    {
        return -tuning.damping * relativeSpeedAlongRope;
    }

    public static float ApplyAgency(float accelerationMagnitude, float inputAway01, ChainTetherTuning tuning)
    {
        var away = Mathf.Clamp01(inputAway01);
        var retention = Mathf.Clamp01(tuning.playerAgencyRetention);
        var scale = Mathf.Lerp(1f, retention, away);
        return accelerationMagnitude * scale;
    }

    public static EvaluateResult Evaluate(
        float distance,
        float relativeSpeedAlongRope,
        float inputAway01,
        ChainTetherTuning tuning)
    {
        if (float.IsNaN(distance) || float.IsInfinity(distance))
            throw new ArgumentException("distance must be finite", nameof(distance));

        var zone = ResolveZone(distance, tuning);
        var spring = ComputeSpringAcceleration(distance, tuning);
        var damp = ComputeDampingAcceleration(relativeSpeedAlongRope, tuning);
        var raw = spring + damp;

        if (distance > tuning.restLength && raw < 0f)
            raw = 0f;

        var withAgency = ApplyAgency(Mathf.Max(0f, raw), inputAway01, tuning);
        var clamped = Mathf.Clamp(withAgency, 0f, tuning.maxPullAcceleration);

        if (zone == ChainTetherZone.Slack)
            clamped = 0f;

        return new EvaluateResult(zone, spring, clamped);
    }

    public static float ComputeInputAway01(Vector3 moveDirection, Vector3 ropeDirectionFromAtoB)
    {
        moveDirection.y = 0f;
        ropeDirectionFromAtoB.y = 0f;
        if (moveDirection.sqrMagnitude < 0.0001f) return 0f;
        if (ropeDirectionFromAtoB.sqrMagnitude < 0.0001f) return 0f;

        var move = moveDirection.normalized;
        var rope = ropeDirectionFromAtoB.normalized;
        var away = Vector3.Dot(move, rope);
        return Mathf.Clamp01(away);
    }

    /// <summary>
    /// Integracja pędu liny: prędkość zostaje między klatkami, damping gasi oscylacje.
    /// </summary>
    public static Vector3 IntegrateTetherVelocity(
        Vector3 velocity,
        Vector3 acceleration,
        float deltaTime,
        float velocityDampingPerSecond,
        float maxSpeed)
    {
        if (deltaTime <= 0f)
            return velocity;

        velocity.y = 0f;
        acceleration.y = 0f;
        velocity += acceleration * deltaTime;
        velocity *= 1f / (1f + Mathf.Max(0f, velocityDampingPerSecond) * deltaTime);

        var cap = Mathf.Max(0.01f, maxSpeed);
        if (velocity.sqrMagnitude > cap * cap)
            velocity = velocity.normalized * cap;

        velocity.y = 0f;
        return velocity;
    }
}
