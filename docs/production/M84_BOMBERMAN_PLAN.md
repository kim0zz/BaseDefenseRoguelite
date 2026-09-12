# M8.4 — Bomberman kompletny (drugi pełny test frameworku progresji)

## Status
**NEEDS PLAYTEST** — 2026-09-11. Kod + Verifier: AC 1–12 PASS, Unity Console 0 Error. Game feel = człowiek. L5 stuby (5 kart) FLAG, nie blokują L1. Balance Agent: nie.

Cel: Bomberman lvl 1–5 grywalny ręcznie, przez **generyczny** framework M8.1 (eligibility / tagi / ulti / `ClassProgressionTable`). To drugi test frameworku po Pudzianie — inne czasowniki (stawianie, detonacja, kopnięcie, burn, homing, orbity, nalot), bez `if (class == Bomberman)` w logice.

## FROZEN — bez zmian
- Pętla: `PLAYER_PROGRESSION.md` (4 decyzje, pauza, full HP, wspólny EXP, max 5).
- Zasady skilli: `SKILLS.md` (wspólna fizyka, brak gołego `+%`, brak `SkillModifierId` per karta).
- Loot PARKED. Struktura kart FROZEN. `VISUAL_FEEDBACK_CHECKLIST` obowiązuje.
- Receptura klasy **może być inna niż Pudzian** bez zmiany silnika (lock PO: „Inna klasa może mieć inną tabelę”).

## ⚠️ ESKALACJA FROZEN — slot klasy (nie blocker designu kitu)

`03_MVP_SCOPE` i `CLASSES.md`: MVP ma **4 klasy** o tożsamościach Cwel / Pudzian / Cipak / Jamie. M8 plan: Bomberman **poza** M8; kolejność M8.4 Jamie.

Ten canvas PO wprowadza Bombermana jako **drugą kompletną postać**.

| Opcja | Skutek |
|---|---|
| **A (lock Lead, jeśli PO nie sprzeciwi się)** | Dodać `PlayerClassId.Bomberman` jako 5. klasę playable. Czwórka MVP zostaje w enumie (Jamie/Cwel/Cipak = stuby). Slice = tylko Bomberman. `CLASSES.md`: sekcja additive DRAFT, bez kasowania tożsamości czwórki. |
| B | Bomberman zastępuje Jamie — narusza tożsamość Jamie. |
| C | Bomberman = kit Cipaka — fałsz (Cipak = łuk/pułapki, nie detonator). |

Założenie planu: **A**. P1 zostaje Pudzian (regresja). Playtest Bombermana: `CombatBootstrap.usePlaytestClassOverride` + `playtestClassOverride = Bomberman` w Inspectorze (bez edycji kodu).

## Decyzje Leada (lock na implementację)

| ID | Decyzja |
|---|---|
| D1 | Skill ID: `bomberman_bomba`, `bomberman_detonator`, `bomberman_kopniak`. Ulti talent id = skill id (jak M8.1b D3): `bomberman_szybkostrzelnosc`, `bomberman_orbitale`, `bomberman_nalot`. |
| D2 | Licznik tagów = suma **wszystkich** wybranych kart, w tym ulti (FROZEN). |
| D3 | L2 **nie** używa `OneMutationPerActive` (Większy Huk mutuje AA, Detonator nie ma mutacji L2). Nowa receptura: `OfferRecipeKind.AllEligibleInGroup` + `independentGroup = mutation`. |
| D4 | L3 = istniejące `MixIndependentAndFollowup` (Saper + Piroman + 1 follow-up 1:1). L4 = `AllEligibleGrantUltimate` (3 ulti, bez filtra L2/L3). L5 = `AllEligible` capstone. |
| D5 | Detonator jest skillem L1 (slot 1), **nie** gałęzią talentów. Detonuje wszystkie `Detonatable` deployable właściciela (zwykłe + miny + child). Orbitale i bomby nalotu: detonatable = false, chyba że talent to włączy. |
| D6 | Modyfikatory czytane **w momencie zdarzenia** z persistents właściciela (żywy snapshot), nie kopiowane na zawsze przy spawnie. Flagi instancji: kategoria, generacja child, czy jest kopnięta, czy orbitalna. |
| D7 | Wszystkie liczby z sekcji D — Implementer **nie** wymyśla wartości. Tuning w `EffectTuning` / `SkillDefinition` / profilu broni / `DeployableTuning`. |
| D8 | Burn: tick **nie** nakłada burnu. Consume-AoE **nie** nakłada burnu. 1 generacja spreadu (Rozniecanie). Feniks: child generation ≥ 1 nie spawnuje kolejnego Feniksa. |
| D9 | Karabin maszynowy: **jedna** karta `bomberman_karabin_maszynowy`, `requiresTagCounts: Basic ≥ 2`, **bez** `requiresUltimateId`. |
| D10 | 10 BOMB: `requiresAnyTags: Demolition, Trapper`. Nie fallback UI. |
| D11 | Brak `BombermanController`. Brak `if (ClassId == Bomberman)` w combat/skill/progression. ClassId tylko w factory contentu i bootstrapie kitu (jak Pudzian). |
| D12 | Placeholdery VFX OK. Telegraph + HUD bez konsoli obowiązkowe. Animacje = M9. |
| D13 | Forced movement wrogów tylko przez warstwę `ForcedMovement*` (Kula / BREAK / taran). Bombę kopniętą rusza `DeployableMotor`, nie `transform` wroga. |
| D14 | `ProgressionApplier` nadal **nie** dokleja dwóch bindingów tego samego `PersistentEffectKind` — talenty mutex L2 nie dzielą kindu z talentami, które mogą współistnieć. |

Brak blokera mechaniki. Slot klasy = A, dopóki PO nie każe inaczej.

---

# A. Diagnoza repo

Framework M8.1 **działa** i jest generyczny: `TalentEligibility` → `OfferGenerator` → `ProgressionApplier` → `SkillLoadout` + `PlayerPersistentEffects`. Content runtime w `BuildContentFactory` / `SkillContentFactory`. Wzorzec: Pudzian M8.1b.

### Jest i reuse bez zmian
- 3 aktywne + 1 ulti slot (`SkillLoadout`).
- MutateSkill / AddSkillEffect / GrantSkill / AttachPersistent.
- `requiresTalents`, `requiresUltimateId`, `requiresTags`, `RequiresAnyTags` (OR), `requiresTagCounts`, `excludesTalents`.
- L3 mix independent + follow-up 1:1.
- L4 ulti bez filtra L2/L3.
- L5 zmienne N, min. 1, brak losowego fallbacku.
- `SkillActivationMode.Active/Passive`.
- `EffectTuning` (8 float + 3 int).
- AttackCycle ranged (Bow spawnuje pocisk po windup).
- `GetAttackSpeedMultiplier` (Furia/Kolos) — wzorzec pod override tempa.
- Overlap koła + LoS przeszkód (`SkillCircleOverlap`).
- Stun przez `ForcedMovementRequest.StunControl` + profil odporności.
- Strefy DoT (`SlowZone`) — tylko jako scorch L5, nie jako bomby.
- `StatusEffectType.Burn` **istnieje w enumie**.
- Brak friendly fire na pocisku (`ProjectileRules` filtruje `PlayerCharacter`).
- Input Q/E/R/F już mapuje 3 skille + ulti.

### Jest, ale za wąskie
- `Projectile`: liniowy, 1 cel, bez splash, bez łuku, bez wybuchu na przeszkodzie, hardcode nazwy Bow.
- `WeaponFamily`: Sword/Dagger/Axe/Bow — brak thrown explosive.
- `SkillShapeType`: Circle / Cone / Leap / ChargeLine / LaneWaves — brak place / detonate / launch-owned / aim-strip.
- `ForcedMovementStepper`: krawędź areny; enum `Obstacle` nieużywany; **brak kolizji wróg–wróg**.
- `StatusEffectReceiver`: tickuje tylko Poison; Burn = martwy enum (refresh jak default).
- `Health.TakeDamage`: brak flagi „to tick statusu” → Piroman na `Damaged` zrobiłby pętlę.
- `OfferRecipeKind`: brak oferty „wszystkie eligible w grupie” niezależnej od 3 skilli.
- `TalentTag`: brak Basic, Demolition, Chain, Trapper, Fire, Seeking, Physics, RapidFire, Orbital, Airstrike.
- `PlayerClassId`: Cwel, Pudzian, Cipak, Jamie — brak Bombermana.
- `CombatBootstrap.ConfigureSkillsForClass`: tylko Pudzian.
- `ProgressionApplier.AddPersistent`: 1 binding / kind (ważne przy projektowaniu kindów).

### Nie ma wcale
- Rejestr stawianych obiektów z capem i FIFO detonacją nadmiaru.
- Ręczna detonacja owned objects.
- Arm + proximity trigger.
- Child explosives po wybuchu.
- Homing z rozdziałem celów i porzuceniem martwego targetu.
- Per-target stacking na AA.
- Launch wroga jako pocisk + licznik kolizji + cap czasu/dystansu.
- Orbitujące pociski z **niezależnym** recharge per slot.
- Dziedziczenie modifierów normal / orbital / kicked.
- Absolute attack-interval override (timed i permanent).
- Nalot pasem od castera w kierunku aim (LaneWaves idzie geometrią **linii mapy**, nie aim strip).
- Runtime spawn wchodzący do capu zwykłych bomb.
- Zmienna wartość capu deployable.

Pudzian jest melee/control/tank. Bomberman ma testować inną oś frameworku: **obiekty ze swoim lifetime**.

---

# B. Mapowanie designu → framework

| Feature | Mechanizm | Nowa capability? |
|---|---|---|
| L2 3 karty (AA + bomba + kopniak) | `AllEligibleInGroup` / mutation | Tak — 1 enum recipe |
| L3 2+1 | `MixIndependentAndFollowup` | Nie |
| L4 3 ulti | `GrantSkill`, bez tag filtra | Nie (3 karty w katalogu) |
| L5 ulti+tagi+talenty | istniejące requirementy | tagi + content |
| AA petarda | `WeaponFamily.ThrownExplosive` + pocisk wybuchowy | Tak — uogólnić Projectile |
| Bomba L1 | skill `PlaceDeployable` | Tak — Deployable |
| Cap 3 / 11. detonuje najstarszą | `DeployableRegistry` FIFO | Tak |
| Detonator | shape `DetonateOwned` | Tak — mały shape |
| Kopniak | `LaunchNearestOwned` + motor bomby | Tak |
| Kasetowa | persistent ClusterOnExplode, generation 0 only | Tak |
| Większy Huk | persistent BasicSplashRadius | Tak |
| Wybuch ogłuszający | persistent KickExplodeStun | Tak |
| Saper | persistent ArmToMine | Tak |
| Piroman | persistent ApplyBurnOnPlayerDamage + Burn tick | Tak (Burn był martwy) |
| Mini-torpedy | HomingMotor na child | Tak |
| Ładunek | PerTargetHitStacks | Tak |
| Kula bilardowa | CollisionChain na wrogu przez ForcedMovement | Tak — stepper enemy hit |
| Szybkostrzelność | GrantSkill + TimedIntervalOverride | Tak |
| Orbitale | OrbitingSlotSet, poza capem | Tak |
| Nalot | `SkillShapeType.AimStripBurst` | Tak |
| Karabin | PermanentIntervalOverride + dmg mul | Tak |
| 10 BOMB | tuning cap na persistent | Tak (to samo registry) |
| Kasetowe satelity | ten sam ClusterOnExplode, kategoria Orbital | Nie (reuse cluster) |

Warstwa wykonawcza (zgodnie z `SKILLS.md`):

```
SkillDefinition / Attack profile
  → shape executor (place, detonate, launch-owned, strip, circle splash)
    → DeployableRegistry + Deployable (lifetime)
      → ExplosionResolver (overlap, dmg, burn proc, cluster spawn)
        → ForcedMovement (stun / billiard)
          → persistents (żywy snapshot modifierów)
```

---

# C. Brakujące capability (minimalne, generyczne)

Nie budować DSL-a ani `BombermanBomb`. Rozszerzyć enumy + 3–4 małe runtime typy.

### C1. `Deployable` + `DeployableRegistry` (serce kitu)
- Owner, category: `Normal | Child | Orbital | Strike`.
- Cap **tylko** dla `Normal` (domyślnie 3, persistent może ustawić 10).
- Place gdy count ≥ cap → detonuj najstarszy Normal, potem place.
- API: `Register`, `DetonateAllDetonatable(owner)`, `FindNearest(owner, pos, range, predicate)`, `SetCap(owner, n)`.
- Śmierć / respawn ownera: despawn wszystkich owned (D: śmierć kasuje bomby; respawn startuje od 0).
- Pauza level-upu: freeze lifetime/motor (jak skill CD).

### C2. `ExplosionResolver`
- Overlap koło, filtr wrogów (reuse `SkillTargetFilter` / `ProjectileRules`).
- Damage + optional stun (ForcedMovement) + burn proc jeśli caster ma ApplyBurn **i** flaga `CanApplyBurn`.
- `CanApplyBurn = false` dla: status tick, consume-AoE, Feniks generation>0 jeśli spec każe.
- Max targets z tuningu.

### C3. Uogólniony pocisk
`Projectile` dostaje profil (nie nowy singleton):
- `ImpactMode`: Direct | ExplodeOnFirstHitOrObstacle
- splash radius, arc height, speed, max range
- obstacle mask
- `WeaponFamily.ThrownExplosive` (feel: windup rzutu, spawn na końcu windup jak Bow)

Zero `if Bomberman` — Cipak/Cwel mogą later reuse.

### C4. Shape / effect
`SkillShapeType` += `PlaceDeployable`, `DetonateOwned`, `LaunchNearestOwned`, `AimStripBurst`.
`SkillEffectKind` += nic, jeśli place/detonate są kształtami. Cluster/mine to persistents, nie slot-effect Pudziana.

### C5. `DeployableMotor`
Stany: Idle, Armed, Kicked, Homing, Orbiting, Recharging (slot orbitalny).
- Kick: prędkość, max dystans; wróg → polityka `Explode | Billiard`; ściana/obstacle/koniec zasięgu → stop, **nie** wybuch.
- Homing: najbliższy żywy; retarget gdy `!IsAlive`; split gdy ≥N żywych; brak celu → fuse.
- Orbit: kąt + radius wokół ownera; kontakt wróg → explode; recharge per index.

### C6. Burn
W `StatusEffectReceiver`: tick Burn analogicznie do Poison, interwał z magnitude albo stały z Apply() duration/magnitude.
Hook Piroman: przy ofensywnym resolve gracza, nie w `Health.Damaged`.
Dodać `DamageHitFlags.StatusTick` (parametr opcjonalny `TakeDamage`) — tick nie woła on-hit procs.

Stacking v0.1: **refresh duration, brak stacku magnitude** (jak obecny `RefreshOrAdd`).

### C7. Per-target stacks
`PersistentEffectKind.PerTargetHitStacks`: last target id, stacks, last time. Reset przy zmianie celu albo timeout.

### C8. Collision chain (billiard)
Rozszerzyć `ForcedMovementStepper` / shove: overlap wrogów w kroku, event `EnemyCollision`.
`CollisionChainMotor` (owned by ForcedMovement, nie klasa): max collisions, max time, max distance, elite scale, boss = brak displace + bomba wybucha od razu.
Po limicie: callback explode (bomba jedzie z celem albo exploduje w pozycji celu).

### C9. Attack interval override
`PlayerAttackController`: jeśli persistents zwracają `TryGetAttackIntervalOverride(out float seconds)` → użyj tego (absolute), else istniejący interval × speed mul.
Timed (ulti 6 s) i permanent (Karabin) to **jeden** kind z tuningiem duration≤0 = permanent.

### C10. Aim strip burst
Executor: wektor aim, telegraph T, N eksplozji w odstępie D wzdłuż pasa, total duration S. Nie filtr `laneId`. Brak FF.

### C11. Recipe
```
OfferRecipeKind.AllEligibleInGroup
```
Zbiera eligible karty z `talent.Effect.OfferGroup == rule.IndependentGroup`.

### C12. Tagi (enum)
Dodać: `Basic, Demolition, Chain, Trapper, Fire, Seeking, Physics, RapidFire, Orbital, Airstrike`.
Istniejące: `Aoe, Control, Collision, Zone` — reuse.

### C13. PersistentEffectKind (zwarte)
Jeden kind gdy ten sam hook + inne tuningi. Mutex L2 mogą dzielić kind.

| Kind | Karty |
|---|---|
| `ClusterOnExplode` | Kasetowa; Satelity tylko włączają kategorię Orbital w tym samym kindzie (AddPersistent skip duplicate — **Satelity muszą być osobnym kindem `OrbitalAlsoClusters` albo flagą i0 na Cluster**) |
| `BasicSplashBonus` | Większy Huk |
| `KickExplodeStun` | Wybuch ogłuszający |
| `ArmToProximityMine` | Saper |
| `ApplyBurnOnPlayerDamage` | Piroman |
| `ChildHoming` | Mini-torpedy |
| `PerTargetHitStacks` | Ładunek kumulacyjny |
| `BilliardOnKick` | Kula (nadpisuje natychmiastowy wybuch kopnięcia) |
| `AttackIntervalOverride` | Szybkostrzelność (timed) **oraz** Karabin (permanent) — **konflikt AddPersistent** jeśli oba |

**Konflikt D14:** Szybkostrzelność (GrantSkill + Timed override) i Karabin (permanent) mogą współistnieć. **Dwa kindy:** `TimedAttackIntervalOverride`, `PermanentAttackIntervalOverride`. Permanent wygrywa gdy oba (Karabin jest transformacją).

Satelity: `ClusterOnExplode` już z Kasetowej. Satelity = `OrbitalInheritsCluster` (osobny kind, tylko flaga kategorii).

Pozostałe L5 kindy (po jednym): `ConsumeBurnOnHit`, `PhoenixOnBurnKill`, `PackHuntFocus`, `SpreadBurnOnSplash`, `MarkedDelayedBlast`, `MultiLaunchOwned`, `CarryBurningOnKick`, `BilliardBreakUpgrade`, `CollisionScalingExplosion`, `RapidSplashDuringOverride`, `OrbitalRechargeOnExplode`, `PlanetaryOrbitUpgrade`, `AirstrikeLeavesNormals`, `AirstrikeFinisher`, `DeployableCapBonus`.

`ConsumeBurnOnHit`: Reakcja termiczna i Termobaryczny są mutex L2 (K vs H) — **jeden kind**, różne tuningi (i0: 0=mini, 1=basic).

### C14. Czego nie robić
- Plugin/ECS deployable.
- Osobny tree Detonatora.
- Hardcode 27 buildów w OfferGenerator.
- Bazowy capstone „zawsze widać”.
- Fallback talent gdy 0 L5.
- Nowa kamera, nowe inputy poza istniejącym Q/E/R/F.
- Balance Agent.

---

# D. BASELINE VALUES v0.1

Wszystkie **DRAFT**. Parametr w danych. Benchmark: grunt fali 1 = 18 HP / 8 dmg / 1.4 s; Jamie AA 10 / 0.8 s ≈ 12.5 DPS; Pudzian combo ≈ 16.7 DPS; Ram stagger 35; Cipak HP 90 ranged 120%.

### Klasa

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| HP | 95 | Między Cipak 90 a Jamie 100; setup-char, nie tank |
| Move speed | 5.3 m/s | ~106% Jamie 5.0; musi kopać i stawiać |
| HP/lvl | +6 | Między Cwel +5 a Jamie +7 |
| Ranged dmg/lvl | +4% | Jak Cipak, AA to petarda |

### Basic — petarda

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Damage | 9 | 2 hity = 18; nie one-shot grunta |
| Interval | 0.70 s | PO; DPS 12.9 ≈ Jamie |
| Splash r | 1.10 m | 1 extra tylko w clumpie |
| Knockback | 0 | PO |
| Speed | 14 m/s | Czytelniejszy niż łuk 20 |
| Arc peak | 0.55 m | Lekki łuk |
| Max range | 8.0 m | Dystans linii, nie snajper |
| Windup/recovery frakcje | 0.28 / 0.72 (active 0, spawn po windup) | Jak Bow |
| Max celów splash | 5 | Mały granat |
| Boss stagger | 3 | AA nie przerwie Rama sam |

### Bomba (slot 0)

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Damage | 16 | Prawie grunt; payoff = 2+ bomby albo bomba+AA |
| Radius | 2.40 m | Czytelny clump, mniejszy niż Stomp 3.0 |
| Cap | 3 | PO |
| Fuse | brak | PO |
| Offset place | 0.60 m w aim | Nie w ciele gracza |
| CD | 0.40 s | 3 bomby w ~1.2 s, nie hold-spam |
| Windup / active / recovery | 0.12 / 0.08 / 0.15 | Snappy |
| Boss stagger | 8 | 3 bomby ≈ 24 / 35 |
| Max celów | 8 | |
| Detonatable | tak | |
| Category | Normal | liczy się do capu |

### Detonator (slot 1)

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| CD | 0.50 s | Chain po stawianiu, nie DPS button |
| Damage | 0 | Bomby liczą dmg |
| Windup / recovery | 0.08 / 0.10 | Core verb, natychmiast |
| Cel | wszystkie Detonatable owned | PO „aktywne bomby” |

### Kopniak (slot 2)

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| CD | 4.0 s | Skill, nie AA |
| Pick range | 3.5 m | Najbliższa własna |
| Speed | 14 m/s | Jak petarda |
| Max distance | 7.0 m | Środek linii |
| Windup / active / recovery | 0.15 / 0.20 / 0.25 | |
| On enemy | explode (bazowo) | PO |
| On wall/obstacle/pudło | stop, 0 explode | PO |
| Boss | bomba wybucha na kontakcie, 0 displace bossa | profil |

### L2

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Kasetowa N | 3 | PO |
| Mini fuse | 0.80 s | PO |
| Mini dmg / r | 7 / 1.40 m | < 16 / 2.4; 3×7=21 ≈ 1 grunt jeśli wszystkie trafią |
| Scatter | 1.2–1.8 m, 360° | Krótko na boki |
| Child generation | 1 (nie clusterują) | PO |
| Child detonatable | tak | PO |
| Child category | Child | **poza** capem 3 |
| Huk splash | 2.20 m | 2× baza; nadal nie Stomp |
| Huk extra dmg | 0 | PO |
| Stun kopnięcie | 1.0 / 0.5 / 0 s (zwykły/elita/boss) | Jak Stomp; boss 0 hard stun |
| Stun boss stagger extra | +8 | Razem z wybuchem |

### L3

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Saper arm | 1.00 s | PO |
| Saper trigger r | 1.60 m | < blast 2.4, trzeba wejść |
| Burn tick | 2 dmg / 0.50 s | Niski; 4 ticki / 2 s = 8 |
| Burn duration | 2.00 s | Refresh, 0 stack magnitude |
| Burn z tick/consume | nie | D8 |
| Homing speed | 8 m/s | Wolniej niż kick, czytelne |
| Homing contact r | 0.45 m | |
| Homing retarget | natychmiast gdy !alive | PO |
| Split gdy | ≥3 żywych w 8 m | PO; else najbliższy |
| Stacks max | 4 | 3 build-upy + cap |
| Stack mul | 1.00 / 1.20 / 1.40 / 1.60 | Pierwszy hit bazowy |
| Stack reset | 1.75 s lub zmiana celu | |
| Billiard speed | 11 m/s | Cięższy niż kick 14 |
| Billiard max collisions | 3 | |
| Billiard max time | 0.85 s | |
| Billiard max distance | 6.5 m | |
| Billiard elite | coll cap 1, speed ×0.5 | profil |
| Billiard boss | 0 launch, explode on contact | FROZEN brak przemieszczenia bossa |

### L4 ulti

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Rapid duration | 6.0 s | PO |
| Rapid interval | 0.22 s | Środek 0.20–0.25; DPS 9/0.22≈41 burst |
| Rapid extra dmg | 0 | PO |
| Rapid CD | 28 s | Środek 25–30; uptime 6/28≈21% |
| Orbital count | 4 | PO |
| Orbit r / deg/s | 2.2 m / 140 | Czytelny pierścień |
| Orbital dmg / blast r | 12 / 1.80 m | < placed 16/2.4 |
| Contact r | 0.50 m | |
| Recharge | 5.0 s niezależnie | PO |
| Orbital category | Orbital | poza capem 3; detonatable false |
| Kick orbital | tak, dziedziczy kick/bomb persistents | PO |
| Nalot count | 7 | Środek 6–8 |
| Telegraph | 1.00 s | PO |
| Sequence po telegraph | 1.70 s (całość 2.70) | Środek 2.5–3 |
| Spacing | 2.20 m | Pas ~13 m |
| Blast r / dmg | 2.00 m / 14 | Nie one-shot elite; grunt ginie w epicentrum |
| Strip half-width | 1.00 m | |
| Nalot CD | 30 s | PO |
| Nalot category | Strike | poza capem; detonatable false; 0 FF |

### L5

| Karta | Parametr | v0.1 | Uzasadnienie |
|---|---|---|---|
| Reakcja termiczna | consume | 50% remaining burn time | Część, nie cały DoT |
| | extra AoE | 8 dmg / 2.0 m | Mini+consume ≈ grunt |
| | ICD / cel | 0.40 s | Anti double-tick |
| | nakłada burn | nie | D8 |
| Feniks | spawn | 1 mini 6 dmg / 1.2 m / fuse 0.60 s | Słabszy child |
| | cap / event | 2 | |
| | max alive | 6 | |
| | generation | child nie feniksuje | Anti loop |
| Polowanie | focus | elite/boss w 8 m jeśli jest | Boss killer |
| | sequential mul | 1.00 / 1.20 / 1.44 (cap 3) | |
| Termobaryczny | consume | 1.0 s burn | |
| | explode | +8 dmg, splash ×1.50 | Huk 2.2→3.3 |
| Rozniecanie | spread | burn na splash Huk, 1 generacja | |
| Karabin | interval | 0.22 s permanent | Jak ulti, zawsze |
| | dmg mul | ×0.45 (9→4.05) | DPS ≈18.4 > 12.9, hit słabszy |
| Przełamanie | próg | pełne 4 stacki | |
| | delay / blast | 0.45 s / 22 dmg / 2.8 m | Elita/fala 4 grunt 30 — nie one-shot elity |
| Treser | pick r | 4.5 m, wszystkie owned Normal+Child+kicked-ready | |
| | max launched | = cap (3 lub 10) | |
| Płonący taran | carry | tylko burning, 0.40 s / 3.5 m, max 4 | |
| | end | explode | |
| BREAK! | collisions | 6 | 3→6 |
| | speed | 16 m/s | |
| | per collision dmg | 6 | |
| | final r mul | ×1.35 | |
| Łańcuch kolizji | per hit | +12% dmg, +0.15 m r, cap 5 | Max +60% / +0.75 m |
| Przeładowany | splash during ulti | baza 1.1→2.4; z Hukiem 2.2→3.2 | Jakość, nie +% |
| | scorch | 0.60 s, 2 dmg/0.5 s, r = splash | Strefa, nie flat dmg |
| Reakcja orbitalna | −1.0 s | tylko sloty w recharge, clamp 0 | Własny explode startuje pełne 5 s |
| Satelity | orbital explode → cluster jeśli ClusterOnExplode | Child dziedziczy Homing jeśli jest | |
| Planetarny | 6 bomb, r 3.0, 200 deg/s | Spektakl, nie dmg buff | |
| Nalot opóźniony | instant index 1,3,5,7; place 2,4,6 jako Normal | 3 bomby wchodzą w cap FIFO | |
| Ostatnia bomba | metric = **unikalne cele trafione** sekwencją (nie kille) | Prosty, czytelny | |
| | 0 unique | 18 / 2.6 m | 1 grunt |
| | +unique | +4 dmg / +0.20 m, cap 8 | Max 50 / 4.2 m |
| 10 BOMB | cap 10; 11. detonuje najstarszy Normal | PO | |

### Eligibility L5 (lock)

| Karta | Requirement |
|---|---|
| Reakcja termiczna | talents `kasetowa` + `piroman` |
| Feniks | talents `kasetowa` + `piroman` |
| Polowanie stadne | talents `mini_torpedy` |
| Termobaryczny | talents `wiekszy_huk` + `piroman` |
| Rozniecanie | talents `wiekszy_huk` + `piroman` |
| Karabin maszynowy | `Basic ≥ 2`, **bez** ulti id |
| Przełamanie | talents `ladunek_kumulacyjny` |
| Treser bomb | talents `wybuch_ogluszajacy` + `saper` |
| Płonący taran | talents `wybuch_ogluszajacy` + `piroman` |
| BREAK! | talents `kula_bilardowa` |
| Łańcuch kolizji | talents `kula_bilardowa` |
| Przeładowany magazynek | ulti `bomberman_szybkostrzelnosc` + tag `Basic` (tag z ulti → zawsze przy tym ulti) |
| Reakcja orbitalna | ulti `bomberman_orbitale` + tag `Orbital` (z ulti → zawsze) |
| Kasetowe satelity | ulti `bomberman_orbitale` + talent `kasetowa` |
| Układ Planetarny | ulti `bomberman_orbitale` + tag `Physics` (z ulti → zawsze) |
| Nalot z opóźnionym zapłonem | ulti `bomberman_nalot` + tag `Airstrike` (z ulti → zawsze) |
| Ostatnia bomba | ulti `bomberman_nalot` + tag `Zone` (z ulti → zawsze) |
| 10 BOMB | `requiresAnyTags: Demolition, Trapper` |

Ulti-ewolucje zawsze-on są **świadomym safety net** (jak AOE/ZONE na Aurze Pudziana). Nie są losowym fallbackiem.

---

# E. Walidacja grafu — 27 stanów

3 L2 × 3 L3 × 3 L4 = **27**. Tagi ulti liczą się.

Skróty L2: **K** kasetowa, **H** huk, **O** ogłuszający.  
L3: **S** saper, **P** piroman, **T** mini-torpedy, **L** ładunek, **B** kula.  
L4: **R** rapidly, **Orb** orbitale, **N** nalot.

| # | Build | L5 eligible | N |
|---|---|---|---|
| 1 | K S R | Przeładowany, 10 BOMB | 2 |
| 2 | K S Orb | Reakcja orbitalna, Satelity, Planetarny, 10 BOMB | 4 |
| 3 | K S N | Nalot opóźniony, Ostatnia, 10 BOMB | 3 |
| 4 | K P R | Reakcja termiczna, Feniks, Przeładowany, 10 BOMB | 4 |
| 5 | K P Orb | Termiczna, Feniks, Orbitalna, Satelity, Planetarny, 10 BOMB | 6 |
| 6 | K P N | Termiczna, Feniks, Nalot opóźniony, Ostatnia, 10 BOMB | 5 |
| 7 | K T R | Polowanie, Przeładowany, 10 BOMB | 3 |
| 8 | K T Orb | Polowanie, Orbitalna, Satelity, Planetarny, 10 BOMB | 5 |
| 9 | K T N | Polowanie, Nalot opóźniony, Ostatnia, 10 BOMB | 4 |
| 10 | H S R | Karabin (Basic 2), Przeładowany, 10 BOMB | 3 |
| 11 | H S Orb | Orbitalna, Planetarny, 10 BOMB | 3 |
| 12 | H S N | Nalot opóźniony, Ostatnia, 10 BOMB | 3 |
| 13 | H P R | Termobaryczny, Rozniecanie, Karabin, Przeładowany | 4 |
| 14 | H P Orb | Termobaryczny, Rozniecanie, Orbitalna, Planetarny | 4 |
| 15 | H P N | Termobaryczny, Rozniecanie, Nalot opóźniony, Ostatnia | 4 |
| 16 | H L R | Karabin, Przełamanie, Przeładowany, 10 BOMB | 4 |
| 17 | H L Orb | Karabin, Przełamanie, Orbitalna, Planetarny, 10 BOMB | 5 |
| 18 | H L N | Karabin, Przełamanie, Nalot opóźniony, Ostatnia, 10 BOMB | 5 |
| 19 | O S R | Treser, Przeładowany, 10 BOMB | 3 |
| 20 | O S Orb | Treser, Orbitalna, Planetarny, 10 BOMB | 4 |
| 21 | O S N | Treser, Nalot opóźniony, Ostatnia, 10 BOMB | 4 |
| 22 | O P R | Płonący taran, Przeładowany | 2 |
| 23 | O P Orb | Płonący taran, Orbitalna, Planetarny | 3 |
| 24 | O P N | Płonący taran, Nalot opóźniony, Ostatnia | 3 |
| 25 | O B R | BREAK!, Łańcuch, Przeładowany | 3 |
| 26 | O B Orb | BREAK!, Łańcuch, Orbitalna, Planetarny | 4 |
| 27 | O B N | BREAK!, Łańcuch, Nalot opóźniony, Ostatnia | 3 |

**Min N = 2. Żaden build nie ma 0. Singletonów brak.**  
Karabin **tylko** gdy Basic≥2: H+L (dowolne ulti), H+R (S lub P; przy L też). Nie na K-path (Basic tylko z R = 1), nie na O-path.

Follow-up 1:1: K→T, H→L, O→B. Inne follow-upy niewidoczne.

---

# F. Plan implementacji

Nie ruszać Pudziana poza reuse. Nie startować Balance Agenta. Verifier po każdym etapie. Max 1 automatyczna runda poprawki na FAIL.

| ID | Zakres | Docs | AC skrót |
|---|---|---|---|
| T1 | Tagi, `PlayerClassId.Bomberman`, `AllEligibleInGroup`, `EffectTuning` reuse, `DamageHitFlags`, Burn tick bez proc | PLAYER_PROGRESSION (receptura innej klasy OK) | EditMode: recipe L2 zbiera 3 mutation bez skill-match; Burn tick rani, nie re-Apply burn |
| T2 | `Deployable` + Registry cap FIFO; ExplosionResolver; Place/Detonate/Launch shapes | mini-spec bomba/detonator/kopniak | 4. bomba detonuje najstarszą; Detonator odpala idle; kick ściana=stop |
| T3 | ThrownExplosive AA splash+arc; kit L1 w factory; bootstrap override playtest; HUD 3 skilli | CLASSES additive, BASELINE | 2 petardy = grunt 18; 0 FF; Pudzian P1 default nietknięty gdy override None |
| T4 | L2: cluster, huk splash, kick stun | TALENTS | Kasetowa = 3 child generation 1; Huk r=2.2; kick stun 1.0 |
| T5 | L3: Saper arm+proximity+Detonator nadal; Piroman; Homing split/retarget; stacks reset; billiard limity | | Saper trigger; mine + Detonator; homing porzuca trupa; stacks timeout; billiard stop po cap |
| T6 | L4: rapid override 6s/0.22/28s; orbitale 4 niezależne recharge + kick dziedziczy; nalot strip 0 FF | mini-spec ulti | Rapid wraca 0.7; orbital slot A explode nie resetuje B jeśli B ready; nalot nie hit player |
| T7 | L5 wszystkie karty + graf 27; Karabin threshold; Satelity+torpedy inherit; 10 BOMB cap | TALENTS L5 | Enumeracja 27; N≥1; Karabin tylko Basic≥2; 11. detonuje oldest |
| T8 | Placeholdery VFX/telegraph, HUD cap bomb, orbital recharge pips, burn icon, override AA | VISUAL_FEEDBACK_CHECKLIST | Czytelne bez konsoli |
| T9 | Docs: TALENTS, skill mini-specy, CLASSES, BASELINE, MILESTONES M8.4, SESSION_HANDOFF, ten plan status | | Zgodne z kodem |
| T10 | Verifier Console 0 Error/Warning; regresja Pudzian L2–L5; playtest prep | | Osobny agent Verifier |

Handoff Implementer (`composer-2.5-fast`): zakres T1–T9, liczby wyłącznie z sekcji D, FROZEN bez zmian, class slot = A.

---

# G. Plan testów

### Automatyczne (EditMode) — minimum z canvasu PO

- L2→L3: K→tylko T jako follow-up; H→L; O→B. Saper i Piroman zawsze przy L3.
- L4 zawsze 3 ulti niezależnie od L2/L3.
- Graf 27: każde N∈[2,6]; 0 opcji = FAIL; lista N z tabeli E.
- Karabin: H+P+R tak; K+S+R nie; H+L+Orb tak; O+S+R nie.
- Saper: po 1.0 s armed; enemy w 1.6 m → explode; Detonator przed i po arm działa.
- Kasetowa: dokładnie 3 child; child explode nie spawnuje kolejnych 3.
- Mini-torpedy: 3 żywe cele → 3 różne targety gdy możliwe; target umiera → retarget albo fuse 0.8; brak lock na trupa.
- Piroman: explode nakłada burn; burn tick nie nakłada burn; consume AoE nie nakłada burn.
- Ładunek: ten sam cel buduje; zmiana celu reset; timeout 1.75 reset.
- Kula: stop po 3 coll / 0.85 s / 6.5 m; boss nie jest pociskiem.
- Orbitale: 4 sloty; A explode podczas recharge B skraca tylko B (Reakcja orbitalna); clamp ≥0; A nie kończy natychmiast własnego 5 s.
- Kick orbital: `BilliardOnKick` / stun działają.
- Nalot: overlap nie zawiera `PlayerCharacter`.
- Satelity: orbital explode spawnuje 3 child gdy Kasetowa; z Mini-torpedy child mają homing.
- 10 BOMB: cap 10; 11. register detonuje oldest Normal (Child/Orbital/Strike nie zjadają capu).
- Nalot opóźniony: 3 Normal wchodzą do registry i podlegają capowi FIFO.
- Regresja: Pudzian 36-graf nietknięty; 4 singleton Boss; join/respawn 20 s; loot off; level-up pause.

### Ręczny playtest (człowiek) — nie PASS z kodu

Petarda łuk i splash, stawianie 3 i 4., Detonator feel, kopniak ściana vs wróg, mina telegraph uzbrojenia, rapid 6 s, orbity czytelne vs karta, nalot pas aim, 10 bomb clutter, chain Feniks/kasetowa, billiard czytelność, brak self-dmg. Game feel = **NEEDS PLAYTEST**.

---

# H. Ryzyka

| Ryzyko | Mitygacja |
|---|---|
| Deployable god-class | Cienki `Deployable` + Registry + Motor + ExplosionResolver |
| Burn loop | flagi hit + D8 test |
| Feniks × kasetowa × 10 bomb | cap alive 6, generation 1 |
| FIFO 4. bomba zaskakuje | placeholder flash na detonowanej + krótki SFX |
| Nalot opóźniony detonuje setup przy cap 3 | udokumentowane; 10 BOMB to kontrbuild |
| Billiard vs EnemyLaneMotor | ForcedMovement honor collisions; po końcu AI wznawia; boss 0 displace |
| Karabin + Rapid oba override | Permanent wygrywa; timed ignorowany gdy permanent |
| AddPersistent skip duplicate kind | rozdzielone kindy w C13 |
| P1 override zepsuje Pudziana | default None; instrukcja playtestu |
| Performance 10 min + 6 orbitali + child | cap child/phoenix; EditMode nie łapie — playtest |
| FROZEN 4 klasy | lock A; nie kasować Jamie |

---

# Mini-specy do napisania (T9, nie blokują T1 jeśli YAML w kodzie z D)

`docs/content/skills/bomberman/`: `petarda.md` (AA), `bomba.md`, `detonator.md`, `kopniak.md`, `szybkostrzelnosc.md`, `orbitale.md`, `nalot.md`. Szablon `_TEMPLATE.md`.

---

# Raport pod przyszły Balance Agent (wypełnić po kodzie, nie teraz)

Szablon hipotez (Implementer/Verifier uzupełniają liczby z telemetrii, **nie** zmieniają D):

- DPS sources: AA vs bomb vs orbital vs nalot vs burn vs child
- Burn contribution % TTK
- Effective bomb uptime (armed mines vs waiting Detonator)
- Orbital uptime (4×5 s vs Reakcja −1 s w hordzie)
- CC duration (stun kick, billiard lock, saper)
- Boss: 0 displace billiard; stagger z bomb vs AA
- Chain-reaction: Feniks, kasetowa, termiczna, 10 bomb
- Infinite loop checks (burn, phoenix gen, orbital recharge)
- Rapid-fire 41 DPS / 6 s vs Karabin 18 DPS stałe
- 10-bomb CPU/czytelność
- Miejsca tylko playtest: kopniak feel, orbital clutter, nalot aiming, mine friendly-readability

---

# Definition of Done tego slice

AC T1–T10, brak 0 L5, build odpala, playtest bez edycji kodu (Inspector override), liczby z D, docs zaktualizowane, Verifier Console czysty, FROZEN pętli/skilli nienaruszone, game feel = NEEDS PLAYTEST PO. Nie DONE bez playtestu człowieka.

---

# Playtest PO

1. **BootScene** → w hierarchii znajdź `CombatBootstrap` → włącz `usePlaytestClassOverride` na **P1**, `playtestClassOverride` = Bomberman. Wyłączony = Pudzian (regresja).
2. **Lvl 1:** petarda splash (2 hity = grunt 18), Q bomba ×3, E detonator chain, R kopniak (ściana = stop, wróg = wybuch). HUD: `Bomby n/cap` bez konsoli.
3. **Lvl 2–5:** level-up pauza; sprawdź 3 mutacje L2, follow-up L3 (K→T, H→L, O→B), 3 ulti L4, L5 ≥2 opcje na build.
4. **Ulti:** F Szybkostrzelność → HUD `RAPID` / timer 6 s; Orbitale → `Orb 4/4`; Nalot → pas aim, 0 self-dmg.
5. **Regresja:** override OFF → P1 Pudzian, graf 36 nietknięty.
6. **Feel (NEEDS PLAYTEST):** FIFO 4. bomby, mina telegraph, billiard, 10 bomb clutter, burn tick na wrogu (status HUD).
