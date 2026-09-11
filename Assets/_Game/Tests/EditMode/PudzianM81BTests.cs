using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PudzianM81BTests
{
    [Test]
    public void Combo_HoldContinuesToNextHit()
    {
        var combo = AttackComboDefinition.CreateBulawaBaseline();
        Assert.AreEqual(3, combo.HitCount);
        Assert.AreEqual(10f, combo.Hits[0].Damage, 0.01f);
        Assert.AreEqual(16f, combo.Hits[2].Damage, 0.01f);

        var cycle = new ComboAttackCycle();
        Assert.IsTrue(cycle.TryStart(combo));
        cycle.Tick(combo.Hits[0].TotalSeconds, holdInput: true);
        Assert.AreEqual(1, cycle.CurrentHitIndex);
    }

    [Test]
    public void Combo_Reset125_AfterIdleRestartsAtHit0()
    {
        var combo = AttackComboDefinition.CreateBulawaBaseline();
        var cycle = new ComboAttackCycle();
        Assert.IsTrue(cycle.TryStart(combo));
        FinishComboHitToIdle(cycle, combo, 0);
        Assert.AreEqual(1, cycle.CurrentHitIndex);
        cycle.Tick(1.26f);
        Assert.IsTrue(cycle.TryStart(combo));
        Assert.AreEqual(0, cycle.CurrentHitIndex);
    }

    [Test]
    public void Combo_WithinResetWindow_ContinuesToNextHit()
    {
        var combo = AttackComboDefinition.CreateBulawaBaseline();
        var cycle = new ComboAttackCycle();
        Assert.IsTrue(cycle.TryStart(combo));
        FinishComboHitToIdle(cycle, combo, 0);
        cycle.Tick(0.5f);
        Assert.IsTrue(cycle.TryStart(combo));
        Assert.AreEqual(1, cycle.CurrentHitIndex);
    }

    [Test]
    public void RequiresAnyTags_OrBehavior()
    {
        var req = TalentRequirement.Create(requiresAnyTags: new[] { TalentTag.Berserker, TalentTag.Risk });
        var talent = ScriptableObject.CreateInstance<TalentDefinition>();
        talent.ConfigureRuntime("test", "t", "", PlayerClassId.Pudzian, 5,
            new[] { TalentTag.Aura }, req, TalentEffect.Persistent(PersistentEffectKind.OverheatAuraRamping, "cap"));

        var riskOnly = new ProgressionSnapshot(PlayerClassId.Pudzian, 5,
            new[] { "pudzian_piekielna_aura" }, "pudzian_piekielna_aura",
            new Dictionary<TalentTag, int> { { TalentTag.Risk, 1 }, { TalentTag.Aura, 1 } });
        Assert.IsTrue(TalentEligibility.IsEligible(talent, riskOnly, AlwaysUnlockedMetaQuery.Instance));
    }

    [Test]
    public void SingletonL5_AllFourBossPaths_OfferOnlyKazdyKrok()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var builds = new[]
        {
            new[] { "pudzian_skok", "pudzian_hart", "pudzian_ja_jestem_boss" },
            new[] { "pudzian_skok", "pudzian_spalona_ziemia", "pudzian_ja_jestem_boss" },
            new[] { "pudzian_spychacz", "pudzian_hart", "pudzian_ja_jestem_boss" },
            new[] { "pudzian_spychacz", "pudzian_w_sciane", "pudzian_ja_jestem_boss" }
        };

        foreach (var chosen in builds)
        {
            var talents = FindAll(catalog, chosen);
            var snapshot = new ProgressionSnapshot(
                PlayerClassId.Pudzian, 5, chosen, "pudzian_ja_jestem_boss",
                TalentTagCounter.Compute(talents));
            var offer = catalog.BuildLevelOffer(snapshot);
            Assert.AreEqual(1, offer.Count, string.Join("+", chosen));
            Assert.AreEqual("pudzian_kazdy_krok", offer[0].TalentId, string.Join("+", chosen));
        }
    }

    [Test]
    public void RequiresAnyTags_Przegrzanie_NotOfferedWithoutBerserkerOrRisk()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var chosen = new[] { "pudzian_skok", "pudzian_spalona_ziemia", "pudzian_piekielna_aura" };
        var talents = FindAll(catalog, chosen);
        var snapshot = new ProgressionSnapshot(
            PlayerClassId.Pudzian, 5, chosen, "pudzian_piekielna_aura",
            TalentTagCounter.Compute(talents));
        Assert.IsFalse(snapshot.GetTagCount(TalentTag.Berserker) > 0 || snapshot.GetTagCount(TalentTag.Risk) > 0);
        Assert.Greater(snapshot.GetTagCount(TalentTag.Aura), 0);
        Assert.Greater(snapshot.GetTagCount(TalentTag.Zone), 0);

        var offer = catalog.BuildLevelOffer(snapshot);
        CollectionAssert.DoesNotContain(Ids(offer), "pudzian_przegrzanie");
    }

    [Test]
    public void Fury_MeterActivatesThenExpiresWithoutRefillDuringActive()
    {
        var go = new GameObject("P");
        var fx = go.AddComponent<PlayerPersistentEffects>();
        fx.ReplaceAll(new[]
        {
            new PersistentEffectBinding(
                PersistentEffectKind.FuryMeter,
                EffectTuning.Create(45f, 0.25f, 0.20f, 0.15f, 0.4f, 1.5f, 0.10f, 6f))
        });

        for (var i = 0; i < 5; i++)
            fx.ProcessIncoming(10f, null);
        Assert.GreaterOrEqual(fx.FuryMeter, fx.FuryThreshold);

        fx.TickFuryForTests(0f);
        Assert.IsTrue(fx.FuryActive);
        var meterDuring = fx.FuryMeter;
        fx.ProcessIncoming(20f, null);
        Assert.AreEqual(meterDuring, fx.FuryMeter, 0.001f);

        fx.TickFuryForTests(6f);
        Assert.IsFalse(fx.FuryActive);
        Assert.AreEqual(0f, fx.FuryMeter, 0.001f);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Hart_FifthHitArms_SixthNegatedAndStacksReset()
    {
        var go = new GameObject("P");
        var fx = go.AddComponent<PlayerPersistentEffects>();
        fx.ReplaceAll(new[]
        {
            new PersistentEffectBinding(
                PersistentEffectKind.HartStacks,
                EffectTuning.Create(2f, 1f, 0.5f, 0f, 0f, 0f, i0: 5))
        });

        for (var i = 0; i < 5; i++)
        {
            var hit = fx.ProcessIncoming(10f, null);
            Assert.IsFalse(hit.Negated);
        }

        Assert.AreEqual(5, fx.HartStacks);
        Assert.IsTrue(fx.HartReady);

        var sixth = fx.ProcessIncoming(10f, null);
        Assert.IsTrue(sixth.Negated);
        Assert.AreEqual(0, fx.HartStacks);
        Assert.IsFalse(fx.HartReady);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void LeapDamageImmunity_BlocksThenAllowsDamage()
    {
        var go = new GameObject("P");
        var health = go.AddComponent<Health>();
        health.Configure(100f);
        var immunity = go.AddComponent<DamageImmunity>();

        immunity.Push();
        health.TakeDamage(50f);
        Assert.AreEqual(100f, health.CurrentHealth, 0.001f);

        immunity.Pop();
        health.TakeDamage(50f);
        Assert.AreEqual(50f, health.CurrentHealth, 0.001f);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Zryj_ConvertsThreeTauntHitsThenStops()
    {
        var playerGo = new GameObject("Player");
        var player = playerGo.AddComponent<PlayerCharacter>();
        var fx = playerGo.AddComponent<PlayerPersistentEffects>();
        fx.ReplaceAll(new[]
        {
            new PersistentEffectBinding(
                PersistentEffectKind.ConvertTauntHitsToHeal,
                EffectTuning.Create(0f, 0f, 0f, 0f, 0f, 0f, i0: 3))
        });

        var enemyGo = new GameObject("Enemy");
        enemyGo.AddComponent<EnemyThreatState>().TryApply(player, 4f);
        fx.NotifyTauntPulseApplied();

        for (var i = 0; i < 3; i++)
        {
            var hit = fx.ProcessIncoming(10f, enemyGo);
            Assert.IsTrue(hit.ConvertedToHeal, $"convert {i}");
        }

        var fourth = fx.ProcessIncoming(10f, enemyGo);
        Assert.IsFalse(fourth.ConvertedToHeal);

        Object.DestroyImmediate(enemyGo);
        Object.DestroyImmediate(playerGo);
    }

    [Test]
    public void AuraRamp_IncreasesExtraTicks_ThenResetsAfterGrace()
    {
        var ramp = new PersistentEffectRuntime.AuraRampSimState();
        const int maxExtra = 5;
        const float grace = 1.5f;

        PersistentEffectRuntime.ApplyAuraEnemyTick(ref ramp, 1, maxExtra);
        Assert.AreEqual(1, ramp.ExtraTicks);
        Assert.AreEqual(4f, PersistentEffectRuntime.AuraDamageForTick(3f, ramp.ExtraTicks), 0.001f);

        PersistentEffectRuntime.ApplyAuraEnemyTick(ref ramp, 1, maxExtra);
        Assert.AreEqual(2, ramp.ExtraTicks);

        PersistentEffectRuntime.ApplyAuraGraceTick(ref ramp, 0.5f);
        Assert.IsFalse(PersistentEffectRuntime.ShouldClearAuraRamp(ramp, grace));

        PersistentEffectRuntime.ApplyAuraGraceTick(ref ramp, 1.0f);
        Assert.IsTrue(PersistentEffectRuntime.ShouldClearAuraRamp(ramp, grace));
    }

    [Test]
    public void LaneWaves_TrzesienieUsesShapeAndThreeLanePaths()
    {
        var skill = SkillContentFactory.CreateTrzesienieSwiata();
        Assert.AreEqual(SkillShapeType.LaneWaves, skill.ShapeType);
        Assert.AreEqual(3, SkillLaneWaveExecutor.CountValidLanePaths());
    }

    [Test]
    public void EveryLegalBuild_HasOfferInRecipeRange()
    {
        var catalog = BuildContentFactory.CreateDefaultCatalog();
        var table = catalog.GetProgression(PlayerClassId.Pudzian);
        var l2 = new[] { "pudzian_skok", "pudzian_zryj_mnie", "pudzian_spychacz" };
        var l3Options = new Dictionary<string, string[]>
        {
            { "pudzian_skok", new[] { "pudzian_wkurw", "pudzian_hart", "pudzian_spalona_ziemia" } },
            { "pudzian_zryj_mnie", new[] { "pudzian_wkurw", "pudzian_hart", "pudzian_najezony" } },
            { "pudzian_spychacz", new[] { "pudzian_wkurw", "pudzian_hart", "pudzian_w_sciane" } }
        };
        var ultis = new[]
        {
            "pudzian_ja_jestem_boss", "pudzian_trzesienie",
            "pudzian_piekielna_aura", "pudzian_nie_zabijecie_mnie"
        };

        foreach (var pick2 in l2)
        foreach (var pick3 in l3Options[pick2])
        foreach (var ulti in ultis)
        {
            var chosen = new[] { pick2, pick3, ulti };
            var talents = FindAll(catalog, chosen);
            var snapshot = new ProgressionSnapshot(
                PlayerClassId.Pudzian, 5, chosen,
                FindSkillId(catalog, ulti),
                TalentTagCounter.Compute(talents));
            var offer = catalog.BuildLevelOffer(snapshot);
            var rule = table.GetRule(5);
            Assert.GreaterOrEqual(offer.Count, rule.MinCount);
            Assert.LessOrEqual(offer.Count, rule.MaxCount);
        }
    }

    [Test]
    public void Fury_TuningUsesFloat6And7()
    {
        var tuning = EffectTuning.Create(45f, 0.25f, 0.20f, 0.15f, 0.4f, 1.5f, 0.10f, 6f);
        Assert.AreEqual(0.10f, tuning.Float6, 0.001f);
        Assert.AreEqual(6f, tuning.Float7, 0.001f);
    }

    [Test]
    public void LastChance_FailDiesEvenWithHpRemaining()
    {
        var go = new GameObject("P");
        var health = go.AddComponent<Health>();
        health.Configure(135f);
        var fx = go.AddComponent<PlayerPersistentEffects>();
        fx.ReplaceAll(new[] { new PersistentEffectBinding(PersistentEffectKind.TimedLastChance,
            EffectTuning.Create(6f, 0.30f)) });

        health.TakeDamage(200f, null);
        Assert.IsTrue(fx.LastChanceActive);
        health.Heal(20f);
        Assert.Greater(health.CurrentHealth, 0f);

        fx.AdvanceLastChanceTimer(6f);
        Assert.IsTrue(health.IsDead);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void LastChance_HealAtZeroHpWorksWithAmp()
    {
        var go = new GameObject("P");
        var health = go.AddComponent<Health>();
        health.Configure(135f);
        var fx = go.AddComponent<PlayerPersistentEffects>();
        fx.ReplaceAll(new[]
        {
            new PersistentEffectBinding(PersistentEffectKind.TimedLastChance, EffectTuning.Create(6f, 0.30f)),
            new PersistentEffectBinding(PersistentEffectKind.HealAmpInLastChance, EffectTuning.Create(1.75f, 0.20f))
        });

        health.TakeDamage(200f, null);
        health.Heal(10f);
        Assert.Greater(fx.LastChanceHealAccum, 10f);
        Assert.IsTrue(health.IsAlive);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Pozeracz_ExtendCappedAtFourSeconds()
    {
        var extra = 0f;
        for (var i = 0; i < 20; i++)
            extra += PersistentEffectRuntime.ApplyPozeraczExtend(extra, 0.4f, 4f);
        Assert.AreEqual(4f, extra, 0.001f);
    }

    [Test]
    public void KrwawaHeal_CappedAt32()
    {
        var accum = 0f;
        for (var i = 0; i < 20; i++)
        {
            var heal = PersistentEffectRuntime.ComputeKrwawaHeal(accum, 4f, 32f);
            accum += heal;
        }

        Assert.AreEqual(32f, accum, 0.001f);
    }

    [Test]
    public void ChainIgnite_DoesNotSpreadFromIgnited()
    {
        var ignited = new HashSet<int> { 42 };
        Assert.IsFalse(PersistentEffectRuntime.CanChainIgniteTarget(ignited, 42));
        Assert.IsTrue(PersistentEffectRuntime.CanChainIgniteTarget(ignited, 99));
    }

    [Test]
    public void DamageZone_CapDecrementsOnDestroy()
    {
        var caster = new GameObject("Caster");
        Assert.IsTrue(SkillEffectApplier.SpawnTrackedDamageZone(Vector3.zero, 3f, caster, 6f, 0.1f));
        Assert.IsTrue(SkillEffectApplier.SpawnTrackedDamageZone(Vector3.one, 3f, caster, 6f, 0.1f));
        Assert.AreEqual(2, SkillEffectApplier.ActiveDamageZoneCount);
        for (var i = 2; i < SkillEffectApplier.MaxDamageZones; i++)
            Assert.IsTrue(SkillEffectApplier.SpawnTrackedDamageZone(Vector3.right * i, 3f, caster, 6f, 0.1f));
        Assert.AreEqual(SkillEffectApplier.MaxDamageZones, SkillEffectApplier.ActiveDamageZoneCount);
        Assert.IsFalse(SkillEffectApplier.SpawnTrackedDamageZone(Vector3.up, 3f, caster, 6f, 0.1f));

        var zones = Object.FindObjectsByType<SlowZone>();
        Object.DestroyImmediate(zones[0].gameObject);
        Assert.AreEqual(SkillEffectApplier.MaxDamageZones - 1, SkillEffectApplier.ActiveDamageZoneCount);

        Object.DestroyImmediate(caster);
        foreach (var z in Object.FindObjectsByType<SlowZone>())
            Object.DestroyImmediate(z.gameObject);
    }

    [Test]
    public void MoveShockwave_TriggersOnDistanceOrTime()
    {
        Assert.IsTrue(PersistentEffectRuntime.ShouldSpawnMoveShockwave(1.2f, 0f, 1.2f, 0.45f));
        Assert.IsTrue(PersistentEffectRuntime.ShouldSpawnMoveShockwave(0f, 0.45f, 1.2f, 0.45f));
        Assert.IsFalse(PersistentEffectRuntime.ShouldSpawnMoveShockwave(0.5f, 0.2f, 1.2f, 0.45f));
    }

    [Test]
    public void SpawnDamageZone_DoesNotRequireHit()
    {
        var caster = new GameObject("Caster");
        SkillEffectApplier.ApplyOnResolve(
            SkillContentFactory.CreateTrzasniecie(),
            new[] { SkillEffectKind.SpawnDamageZone },
            caster,
            Vector3.zero,
            0,
            false);
        Assert.AreEqual(1, SkillEffectApplier.ActiveDamageZoneCount);
        Object.DestroyImmediate(caster);
        foreach (var z in Object.FindObjectsByType<SlowZone>())
            Object.DestroyImmediate(z.gameObject);
    }

    [Test]
    public void GroundCrack_SpacingRejectsNearbyAndAllowsSeparated()
    {
        var existing = new List<Vector3> { Vector3.zero };
        Assert.IsFalse(SkillEffectApplier.IsFarEnoughFromExisting(new Vector3(0.5f, 0f, 0f), existing, 1.6f));
        Assert.IsTrue(SkillEffectApplier.IsFarEnoughFromExisting(new Vector3(2f, 0f, 0f), existing, 1.6f));
        Assert.IsTrue(SkillEffectApplier.IsFarEnoughFromExisting(Vector3.forward * 3f, existing, 1.6f));
    }

    [Test]
    public void GroundCrack_SpacedSpawnRecordsOrigin()
    {
        var caster = new GameObject("Caster");
        var origins = new List<Vector3>();
        Assert.IsTrue(SkillEffectApplier.TrySpawnSpacedDamageZone(
            Vector3.zero, 1.6f, caster, 6f, 0.1f, origins, 1.6f));
        Assert.IsFalse(SkillEffectApplier.TrySpawnSpacedDamageZone(
            new Vector3(0.4f, 0f, 0f), 1.6f, caster, 6f, 0.1f, origins, 1.6f));
        Assert.AreEqual(1, origins.Count);
        Assert.AreEqual(1, SkillEffectApplier.ActiveDamageZoneCount);

        Object.DestroyImmediate(caster);
        foreach (var z in Object.FindObjectsByType<SlowZone>())
            Object.DestroyImmediate(z.gameObject);
    }

    private static void FinishComboHitToIdle(ComboAttackCycle cycle, AttackComboDefinition combo, int hitIndex)
    {
        var hit = combo.Hits[hitIndex];
        var tick = cycle.Tick(hit.TotalSeconds, holdInput: false);
        Assert.IsTrue(tick.ReturnedToIdle);
        Assert.AreEqual(AttackPhase.Idle, cycle.Phase);
    }

    private static string[] Ids(IReadOnlyList<TalentDefinition> offer)
    {
        var ids = new string[offer.Count];
        for (var i = 0; i < offer.Count; i++)
            ids[i] = offer[i].TalentId;
        return ids;
    }

    private static string FindSkillId(BuildContentCatalog catalog, string talentId)
    {
        foreach (var t in catalog.AllTalents)
        {
            if (t.TalentId != talentId) continue;
            return t.Effect?.GrantedSkill?.SkillId ?? talentId;
        }

        return talentId;
    }

    private static TalentDefinition[] FindAll(BuildContentCatalog catalog, string[] ids)
    {
        var list = new List<TalentDefinition>();
        foreach (var id in ids)
        {
            foreach (var t in catalog.AllTalents)
            {
                if (t.TalentId == id) list.Add(t);
            }
        }

        return list.ToArray();
    }
}
