# M8 — Full MVP — Plan v1

## Status
**PLAN LEAD** — 2026-09-09. **M8.0 DONE** (PO playtest 2026-09-11). **M8.2 DONE** (kod+Verifier). **M8.3 DONE** (kod+Verifier 2026-09-11; feel = NEEDS PLAYTEST PO). M8.4+ otwarte. Nie zatwierdza całego M8 — to robi PO.

Stary szkic M8 mieszał win/lose, 4 kity, fale 6–10 i talenty w jeden worek. **Talenty + framework + kompletny Pudzian są DONE.** Ten dokument tnie resztę M8 na plastry.

## Ocena słuszności (Lead)

| Stary element | Werdykt |
|---|---|
| M8 jako kontener „pełny run MVP” | **Zostaje.** FROZEN: 10 fal, 2 bossów, 4 klasy, win/lose. |
| M8.0 win/lose jako następny plaster | **Zostaje, idzie pierwszy.** Run musi umieć przegrać zanim dokładamy fale 6–10. |
| Talenty / 4 decyzje / eligibility w M8 | **Wykreślić z TODO.** M8.1 + M8.1b DONE. Kolejne klasy **reuse** `ClassProgressionTable`, nie nowy silnik. |
| HUD 4 skilli w M8 | **Wykreślić jako feature.** Pudzian ma 4 sloty. Czytelność kart/HUD = **M9.0**, nie M8. |
| 4 kity w tym samym M8 co fale 6–10 | **Rozdzielić.** Każda klasa ≈ pion Pudziana (L1 + L2–L5). Nie blokować 10 fal na trzech kitach. |
| Fale 6–10 + Warden | **Zostają w M8**, osobny plaster **M8.3**, po rosterze wrogów. |
| Support / Flanker / Shielder | **M8.2** — FROZEN przebieg fal 6–8 ich wymaga. |
| Loot / unique z `content/BOSSES.md` | **PARKED.** Nagroda bossa: przerwa / kontynuacja / wygrana runu, nie broń. |
| Balance Agent w M8 | **Nie.** Pierwszy pass = M9.2, po 10 falach. |
| Co-op 2P level-up (M7 otwarte) | **Nie blokuje M8.0.** Pełny gate 2–4P = M9.3. |

### Rekomendowana kolejność (lock Lead, PO może przestawić)

```
M8.0 win/lose
  → M8.2 roster wrogów (Support / Flanker / Shielder)
  → M8.3 fale 6–10 + Warden + RunWon   [Pudzian-only pełna pętla MVP]
  → M9.0 czytelność kart/HUD            [przed następnymi klasami]
  → M8.4 Jamie → M8.5 Cipak → M8.6 Cwel
```

**Dlaczego 10 fal przed 3 klasami:** warunek wygranej FROZEN to Warden na fali 10. Pudzian jest wzorcem. Trzy kity bez pełnego runu odkładają jedyną grywalną pętlę MVP.

**Dlaczego M9.0 przed klasami:** PO (M8.1b): bez znajomości kitu nie wiadomo, co się rozwija. Powielanie tego na 3 klasach = 3× ten sam ból playtestu.

**Dlaczego nie M8.1:** numer zajęty (framework DONE).

---

## FROZEN — nie naruszać

`00`–`03`, `SKILLS.md` (zasady), `PLAYER_PROGRESSION` (pętla + struktura kart), `LOCAL_COOP`, 3 linie, baza, win/lose, max lvl 5, 4 decyzje, loot PARKED, `VISUAL_FEEDBACK_CHECKLIST`.

Przegrana: baza 0 HP **albo** wszyscy aktywni martwi naraz.  
Wygrana MVP: Boss 2 na fali 10.  
Respawn 20 s przy bazie, bez i-frame.

## Wejście (stan na 2026-09-09)

| Element | Status |
|---|---|
| Fale 1–5 + Ram | kod jest; M7 playtest Ram + 2P level-up **otwarty** (nie blokuje M8.0) |
| Fizyka skilli, loot off | DONE |
| Framework progresji | DONE |
| Pudzian L1–5 | DONE (PO PASS) |
| `RunComplete` | placeholder **po fali 5** (to nie wygrana MVP) |
| `RunFailed` | **brak** |
| Support / Flanker / Shielder | **brak** (M7 świadomie poza) |
| Fale 6–10, Warden, ekran wygranej | **brak** |
| Cwel / Cipak / Jamie kity | OPEN |

## Poza całym M8

- Loot, inventory, rare, sklep ekwipunku, unique bossa
- Meta-konto / unlocki stałe
- Visual slice M4, Mixamo, polish VFX/SFX (M9.1+)
- Bomberman, wildcardy
- Encyklopedia skilli ponad ~4 klasy × (1 AA + 3 aktywne + 4 ulti)
- Balance Agent

---

# M8.0 — Win/lose (FAIL)

## Status
**DONE** — PO playtest PASS 2026-09-11. Run umie przegrać; overlay czytelny; świat stoi. Wygrana MVP nadal poza zakresem (brak fali 10).

## Cel
Run umie **przegrać**. Świat się zatrzymuje. Czytelne bez konsoli. Wygrana MVP **nie** wchodzi (brak fali 10).

## Poza M8.0

Cwel/Cipak/Jamie, fale 6–10, Warden, ekran wygranej, restart z menu, M9 karty, Balance Agent.

## Taski

### M80-T1 — Stan `RunFailed`

- **Zakres:** `GameFlowState.RunFailed` osobno od `RunComplete`. Wejście z bazy 0 HP albo wipe aktywnych graczy. Pauza świata (`timeScale` / combat disable — ten sam mechanizm co level-up pause, nie duplikat). Ram i moby przestają atakować. CD/fury/last chance nie tykają.
- **Docs:** `00_GAME_VISION`, `LOCAL_COOP`, ten plan.
- **AC:**
  - EditMode: baza 0 HP → `RunFailed`, nie `RunComplete`.
  - EditMode: wszyscy `IsAlive == false` (aktywni sloty) → `RunFailed`.
  - 1 martwy + baza stoi → gra trwa; respawn 20 s.
  - `RunComplete` po fali 5 nadal nie jest wygraną MVP (regresja M7 D3).
- **Handoff:** Implementer.

### M80-T2 — Wipe vs respawn

- **Zakres:** „aktywni” = dołączeni w tym runie, nie puste sloty 2–4. Solo: śmierć P1 przy stojącej bazie → **przegrana** (nie ma kogo respawnować jako żywego sojusznika; timer 20 s nie odracza wipe). 2P: jeden martwy, drugi żywy → run trwa.
- **AC:**
  - 1P śmierć → `RunFailed` (baza może mieć HP).
  - 2P: tylko P1 martwy → brak `RunFailed`; P1 respawn 20 s.
  - 2P: obaj martwi jednocześnie → `RunFailed` nawet gdy baza > 0.
- **Uwaga:** FROZEN „wszyscy aktywni martwi”. Solo = zbiór 1-elementowy.

### M80-T3 — UI placeholder końca runu (przegrana)

- **Zakres:** tekst/HUD: przyczyna (`Baza upadła` / `Drużyna wybita`). Świat pauza. Input skilli/ataku zablokowany. Stop w edytorze wystarczy jako „wyjście”. Bez menu restartu, bez ekranu wygranej.
- **AC:** czytelne bez konsoli; checklista `VISUAL_FEEDBACK_CHECKLIST` (śmierć/baza). Game feel = NEEDS PLAYTEST.

### M80-T4 — Ram i moby po FAIL

- **Zakres:** po `RunFailed` Ram nie szarżuje, nie klei się do bazy, AI wrogów stop. Brak dmg w struktury po FAIL.
- **AC:** EditMode lub smoke: FAIL w trakcie szarży Rama → brak dalszego dmg w wieżę/bazę.

### M80-T5 — Verifier + playtest PO

- Console 0 Error. AC T1–T4. Regresja: join, respawn 20 s (2P), fala 1–5, level-up pause, loot off, Pudzian kit.
- Playtest PO: 1P śmierć; baza zjedzona (Siege/Ram); 2P wipe jeśli pad dostępny.

## Kolejność M8.0

```
T1 (stan) → T2 (wipe) → T3 (UI) → T4 (Ram stop)
                              ↘ T5 Verifier → playtest PO
```

---

# M8.2 — Roster: Support / Flanker / Shielder

## Status
**DONE (kod + Verifier PASS 2026-09-11).** Playtest PO świadomie **po M8.3** (polecenie PO: bez playtestu między plasterami).

## Cel
Dokończyć 8 typów FROZEN (`WAVES_AND_ENEMIES`). Bez fal 6–10 — typy + AI + testy. Spawn w sekwencji = M8.3.

## Poza M8.2

Warden, fale 6–10, `RunWon`, nowe linie mapy, loot, NavMesh, Balance Agent, playtest PO.

## Lock Lead (DRAFT v0.1) — Implementer NIE wymyśla liczb

Źródło liczb też: `docs/balance/BASELINE_VALUES.md` § M8.2.

### Support
- AI: jak Carrier/Grunt (`StaysOnLane`), nie goni poza korytarz, nie preferuje struktur.
- Nie buffuje siebie. Nie stackuje z drugim Supportem (max multiplier).
- Promień buffu: **5 m** (XZ). Działa na pobliskich (wszystkie linie w zasięgu).
- `moveSpeed × 1.20`, obrażenia w gracza i struktury `× 1.20`.
- Odświeżenie **0.35 s**; po śmierci Supporta buff gaśnie z tym timeoutem.
- Stats SO `M8_Support`: HP 24, dmg 6, interval 1.5 s, speed 3.4, range 1.6, detect 30, exp 10, gold 6, playerDmgMul 1, structureDamage 0.
- Visual: cyjan `(0.15, 0.75, 0.85)`, scale `0.75`; pierścień aury (cylinder Y≈0.08, bez collidera).

### Flanker
- `AttackLineId` z spawnu bez zmian. Cel struktury = wieża **tej** linii → baza (jak Rusher).
- **Nie teleport.** Polyline: spawn → punkt ominięcia (ten sam `ChokeZ`, offset X **3.5 m** na zewnątrz; Center: +X) → wieża linii → baza.
- `FlankerBypassLateralOffset = 3.5`, `BypassHalfWidth = 2.2` w `MapGreyboxLayout`.
- `PrefersStructureOverPlayer` = true. Taunt: jak Rusher (bez wymogu korytarza Grunta; ignoruje strukturę w tauncie).
- Stats SO `M8_Flanker`: HP 18, dmg 5, interval 1.15 s, speed 5.8, range 1.5, detect 35, exp 10, gold 6, structureDamage 12, playerDmgMul 0.65.
- Visual: limonka `(0.35, 0.85, 0.22)`, scale `0.7`.

### Shielder
- AI: jak Grunt (`StaysOnLane`), atakuje gracza w korytarzu, nie struktury.
- Chroni sojuszników **tej samej linii**, w promieniu **3.5 m**, którzy są **za** Shielderem (bliżej spawnu niż Shielder; `ally.z > shielder.z` na greyboxie, bo baza ma mniejsze Z).
- 100% obrażeń sojusznika idzie w HP Shieldera (żywy, w zasięgu, warunek „za”). Własne obrażenia Shieldera bez redirectu (zero rekurencji).
- Overflow: jeśli hit zabija Shieldera, reszta **nie** wraca na sojusznika w tym samym hicie (MVP: cały hit w Shieldera).
- Stats SO `M8_Shielder`: HP 40, dmg 6, interval 1.5 s, speed 3.2, range 1.6, detect 30, exp 12, gold 7, playerDmgMul 1.
- Visual: stal `(0.25, 0.42, 0.78)`, scale `1.2`.

### Wspólne
- `EnemyKind`: Support=5, Flanker=6, Shielder=7 (dopisać na końcu enumu).
- Elity: istniejący `EliteModifier` na `WaveSpawnEntry` / `Spawn(..., elite)` — bez osobnego AI. Frenzy/Armored/Unstable działają.
- Taunt: `CanBeTaunted` = true dla trzech. Support+Shielder = `GruntRequiresTaunterInCorridor`. Flanker = `IgnoresStructureWhileTaunted`.
- Resistance: Normal, chyba że elite.
- Parametry roli na `EnemyDefinition` (data-driven, default 0 / 1 = wyłączone): `supportBuffRadius`, `supportMoveSpeedMultiplier`, `supportDamageMultiplier`, `shieldProtectRadius`.
- Brak zmian fal 1–5 / `M7_WaveSequence`.

## Taski

### M82-T1 — Enum, SO, visual, spawner
- `EnemyKind` + 3 assety YAML (`guid` skryptu `04ca658c4c6c46c194b274b28950b656`).
- `EnemySpawner.ApplyKindVisual` + pierścień Supporta.
- **AC:** trzy kindy spawnują się kapsułą o innym kolorze/skali; elita nadal lerp+1.15.

### M82-T2 — Support aura
- Czysta matematyka `EnemySupportBuffMath` (zasięg XZ, brak self, max mul).
- Tick Supporta po `EnemyRegistry`; `EnemyController.ApplyAllyBuff` / runtime mul na ruch i dmg.
- **AC:** EditMode: Grunt w 5 m dostaje ×1.2 speed i dmg; poza zasięgiem nie; Support nie buffuje siebie; po „śmierci” Supporta buff gaśnie po timeout.

### M82-T3 — Flanker bypass
- `LanePath` / `MapGreyboxLayout.GetBypassLanePath`.
- `EnemyLaneMotor` wybiera bypass gdy `Kind == Flanker`.
- `StructureTargeting.PrefersStructureOverPlayer` += Flanker.
- **AC:** waypoint[1] Flankera ≠ choke linii (inny X); `AssignedLane` bez zmian; AdvanceAlongPath ze spawnu nie przechodzi przez `GetChokePosition`; Flanker preferuje strukturę.

### M82-T4 — Shielder redirect
- `EnemyShielderGuardMath.IsProtected` + serwis wywoływany z `Health.TakeDamage` **tylko** gdy cel ma `EnemyController`.
- **AC:** sojusznik za Shielderem, ta sama linia, ≤3.5 m → 0 dmg na sojuszniku, dmg na Shielderze. Sojusznik przed / inna linia / martwy Shielder → normalny dmg. Shielder nie redirectuje w siebie.

### M82-T5 — Threat + elity + regresja
- `EnemyThreatMath` jak w locku. `StaysOnLane` += Support, Shielder.
- **AC:** `CanBeTaunted` trzech kindów; test elit Frenzy/Armored na Support (math); Grunt korytarz E3 bez zmian.

### M82-T6 — Testy
Nowe: `EnemySupportBuffTests`, `EnemyFlankerPathTests`, `EnemyShielderGuardTests`; rozszerzyć `EnemyThreatTests.CanBeTaunted`, `StructureTargetingTests`.
Nie ruszać `BootSceneWiringTests` (nadal 5 fal).

## Kolejność

```
T1 → T2 + T3 + T4 (równolegle w jednym PR) → T5 → T6
```

---

# M8.3 — Fale 6–10 + Warden + `RunWon`

## Status
**DONE (kod + Verifier PASS 2026-09-11).** Game feel Wardena / pełny run 1–10 = **NEEDS PLAYTEST PO**. Nie zatwierdza całego M8.

## Cel
Pełna pętla MVP **na Pudzianie**: 10 fal, Boss 2, wygrana. Przegrana M8.0 działa na falach 6–10. Loot nadal PARKED.

## Poza M8.3

Cwel/Cipak/Jamie, M9 karty, Balance Agent, visual slice, restart z menu, unique drop.

## FROZEN (nie naruszać)

- 10 fal; 5 i 10 = boss solo; koniec fali: czas **albo** wybicie.
- Przebieg: 6 Support+miks, 7 Flanker, 8 Shielder+elity, 9 presja 3 linie, 10 Boss 2.
- Warden: ~70% totemy 3 linie + DR; ~40% wyłącza wieżę + siege na bazę; ~20% szał/strefy. Nie worek HP.
- Wygrana = Boss 2 na fali 10. Nagroda: wygrana runu, **nie** broń.
- `RunFailed` (baza 0 / wipe) zostaje.

## Lock Lead — flow

- Dodać `GameFlowState.RunWon` (na końcu enumu).
- Po fali 5: **Intermission → Enter/N = fala 6**. Usunąć happy-path `EnterRunCompleteMidRun`. `RunComplete` zostaje tylko jako fallback (brak fal).
- `RunWon` gdy **Warden ginie** (`Health.Died`), nie gdy timer fali 10. Overlay od razu (jak FAIL): `WYGRANA — Warden pokonany`. `timeScale = 0`. Combat off.
- Timeout fali 10 przy żywym Wardenie: fala się kończy (FROZEN czas), **brak** `RunWon`. Intermission; Enter/N na ostatniej fali nic nie startuje i nie udaje wygranej.
- `EnterRunFailed` nie nadpisuje `RunWon`; `EnterRunWon` nie nadpisuje `RunFailed`.
- `RunFailRules.ShouldEnemyCombatTick` nadal tylko `WaveActive` (RunWon nie tickuje).
- HUD: `Fala X/10` (nie `/5`). Przerwa po Ramie: ten sam hint co inne intermission (naprawa + następna fala).
- Loot: Warden **nie** woła `DropBossUnique`.

## Lock Lead — liczby DRAFT v0.1

Szczegóły tabel: `docs/balance/BASELINE_VALUES.md` § M8.3.

### Grunty fal 6–9
| Asset | HP | speed | dmg | interval | exp | gold |
|---|---|---|---|---|---|---|
| M8_Grunt_F6 | 38 | 4.1 | 8 | 1.4 | 8 | 5 |
| M8_Grunt_F7 | 42 | 4.1 | 8 | 1.4 | 8 | 5 |
| M8_Grunt_F8 | 46 | **3.0** | 8 | 1.4 | 8 | 5 |
| M8_Grunt_F9 | 50 | 4.1 | 8 | 1.4 | 8 | 5 |

F8 speed 3.0 < Shielder 3.2, żeby Shielder został z przodu (niższe Z) i chronił paczkę.

Reuse: `M8_Support`, `M8_Flanker`, `M8_Shielder`, `M7_Rusher`, `M7_Siege`, `M4_Hunter`.

### Skład fal (duration / treść)

**Fala 6** (150 s) Support + miks  
- 1 Support / linię (delay 0 / 0.4 / 0.8)  
- 2× Grunt F6 / linię (start 2 s, between 0.9)  
- 1 Hunter center @ 8 s  

**Fala 7** (150 s) Flanker  
- 2× Flanker / linię (between 1.0; start L0 / C0.5 / R1.0)  
- 1× Grunt F7 / linię @ 6 s  
- 1 Rusher Left @ 3 s, 1 Rusher Right @ 5 s  

**Fala 8** (180 s) Shielder + elity  
- 1 Shielder / linię (delay 0 / 0.3 / 0.6)  
- 2× Grunt F8 / linię (start 1.5 s, between 0.8)  
- 1 Grunt F8 elite Frenzy center @ 8 s  
- 1 Grunt F8 elite Armored left @ 10 s  
- 1 Grunt F8 elite Unstable right @ 12 s  

**Fala 9** (180 s) presja 3 linie  
- 2× Grunt F9 / linię (between 0.8)  
- 1 Support center @ 2 s  
- 1 Flanker Left @ 3 s, 1 Flanker Right @ 3.5 s  
- 1 Shielder center @ 4 s  
- 1 Siege / linię (start 8 / 10 / 12)  
- 1 Hunter center @ 14 s  
- 1 Grunt F9 elite Frenzy center @ 16 s  

**Fala 10** (240 s) Warden solo — `isBossWave: 1`, puste spawnEntries, `wardenDefinition` ustawione, `bossDefinition` puste.

### Warden (mechaniki FROZEN, liczby DRAFT)

Nowy `WardenDefinition` SO (nie pchać pól Rama do `BossDefinition`).

| Param | Wartość |
|---|---|
| maxHealth | 700 |
| bodyScale | 2.4 |
| bodyColor | (0.15, 0.35, 0.32) ciemny teal |
| slamDamage | 16 |
| slamRadius | 4.0 |
| slamTelegraph | 1.1 s |
| slamInterval | 3.4 s (faza 0–1) / 2.0 s (faza 3 szał) |
| totemHpRatio trigger | 0.70 |
| totemMaxHealth | 45 |
| damageTakenWhileTotemsAlive | ×0.25 (DR 75%) |
| towerDisableHpRatio | 0.40 |
| siegeSummonCount | 2 (`M7_Siege`, linia wyłączonej wieży) |
| frenzyHpRatio | 0.20 |
| zoneTelegraph | 0.6 s |
| zoneRadius | 3.5 |
| zoneDuration | 3.5 s |
| zoneTick | 8 dmg / 0.5 s |
| zoneInterval | 5.0 s |

Fazy (HP ratio, próg w dół, bez cofania):
- **0** (>70%): slam AoE w stożku/okręgu przed sobą (placeholder cylinder telegraph). Cel: najbliższy żywy gracz, inaczej baza.
- **1** (≤70%): raz spawnuje 3 totemy na `GetChokePosition` każdej linii. Dopóki ≥1 totem żyje, Warden bierze ×0.25 dmg. Totemy: kapsuła/cylinder, bez AI, atakowalne. Śmierć Wardena niszczy totemy.
- **2** (≤40%): raz wyłącza **jedną żywą wieżę linii** (`TowerHealth` — `IsOperational==false` bez zabijania HP). Spawn 2 Siege na tej linii przez `EnemySpawner`; `WaveManager.RegisterMidWaveEnemy` żeby liczyły się do clear.
- **3** (≤20%): szybszy slam + okresowe strefy (decal na XZ, tick graczy w promieniu).

Warden `ForcedMovementResistanceCategory.Boss`. Taunt false (jak Ram). Combat tick tylko `WaveActive`.

Wieża wyłączona: nowy flag `_disabledByBoss` na `TowerHealth`; visual ciemniejszy. Nie przywracać w trakcie fali 10.

## Taski

### M83-T1 — `RunWon` + kontynuacja po fali 5
- Enum, `EnterRunWon`, overlay, HUD X/10.
- Intermission po 5 → fala 6.
- Testy: `RunWon` ≠ `RunComplete` ≠ `RunFailed`; fail nie nadpisuje won; won nie tickuje combat; BootScene sequence count **10**.

### M83-T2 — Assety fal 6–9 + grunty
- YAML jak M7_Wave1 (script guid `f1a2b3c4d5e6478990a1b2c3d4e5f603`).
- Dopisać 5 wpisów do `M7_WaveSequence.asset` (nazwa pliku zostaje).

### M83-T3 — Warden
- `WardenDefinition`, `BossWardenLogic` (czysta), `BossWardenController`, spawn w `BossSpawner` / `WaveManager`.
- `_spawnedBosses` nie może być tylko `BossRamController` — śledź `Health` albo wspólny kontrakt.
- Testy EditMode na progi faz, DR totemów, brak RunWon na timeout.

### M83-T4 — Fala 10 + wygrana + fail regresja
- `M8_Wave10.asset` `isBossWave`, warden SO.
- Zabicie Wardena → `RunWon`, bez dropu broni.
- Baza 0 / wipe na fali 6–10 → `RunFailed`.

### M83-T5 — HUD / placeholdery
- Fazy Warden czytelne tekstem (Totemy / Wieża wyłączona / Szał).
- Totemy i strefy widoczne bez konsoli (kolor). Game feel = NEEDS PLAYTEST.

## Kolejność

```
T1 (flow) → T2 (fale 6–9) → T3 (Warden) → T4 (fala 10) → T5 HUD
```

---

# M8.4 / M8.5 / M8.6 — Jamie / Cipak / Cwel

Wzorzec **Pudziana** (nie nowy framework):

1. Kit canvas (rola + czasowniki) — PO zanim kod.
2. Mini-specy YAML (`docs/content/skills/<klasa>/`).
3. L1: 1 AA + 3 aktywne, input jak Pudzian (Q/E/R, F ulti).
4. `ClassProgressionTable` własna receptura (może być 3/3/4/zmienne jak Pudzian **albo** inna tabela — silnik to umie).
5. L2–L5: mutacje / rdzeń+follow-up / 4 ulti / capstone’y tagowe. Zakaz gołego `+%`.
6. Graf: każdy legalny build L5 min. 1 karta.
7. Baseline v0.1 liczb **Lead przed kodem** (Implementer nie wymyśla).
8. Playtest PO per klasa.

**Kolejność Lead:** Jamie (onboarding, melee ≠ bruiser) → Cipak (dystans, inny czasownik) → Cwel (mobility/status; ryzyko nachodzenia na Cipaka).

**Nie** w jednym PR. **Nie** ruszać Pudziana poza regresją. **Nie** Balance Agent per klasa w tym plasterze (flagi outlierów = M9.2).

Canvas **przed** implementacją — bez canvasu klasa nie startuje.

---

## Zależności (diagram)

```
M8.0 FAIL
  → M8.2 wrogowie
      → M8.3 run 1–10 + Warden + WIN     ← tu jest „pełna pętla” na Pudzianie
M9.0 karty/HUD  ──(po M8.0, idealnie przed M8.4)
M8.4 Jamie → M8.5 Cipak → M8.6 Cwel
M7 Ram/2P playtest — równolegle, nie brama M8.0
```

M8 **DONE** (PO) dopiero gdy: M8.0–M8.3 PASS **oraz** 4 klasy grywalne (M8.4–M8.6) **albo** PO świadomie uzna Pudzian-only 10 fal za bramę „pętli” a klasy jako M8b. Lead **rekomenduje** 4 klasy w M8 (FROZEN `03_MVP_SCOPE`: 4 klasy).

---

## Role

- **Lead:** ten plan; canvas klas; baseline liczb przed każdym kitem/Wardem.
- **Implementer:** `composer-2.5-fast`, tylko plaster.
- **Verifier:** `composer-2.5-fast`, Console, AC.
- **Balance Agent:** nie w M8.
- **PO:** playtest per plaster; kolejność klas vs 10 fal jeśli chce inaczej.
