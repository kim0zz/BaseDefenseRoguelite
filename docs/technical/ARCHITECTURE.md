# Technical Architecture — Initial Guidance

## Status
**DRAFT**

## Założenia
Silnik: Unity 6+.

## Proponowane moduły
- GameFlowManager
- WaveManager
- PlayerManager
- Input/Join System
- PlayerCharacter
- Health/Damage System
- Weapon System (PARKED — M6; pętla runu wyłącza w M7.5)
- Item System (PARKED)
- Talent System (eligibility + oferta per poziom; mutacje skilli / GrantSkill / persistents)
- Skill System — kit klasy; `docs/systems/SKILLS.md`; wycinek `docs/production/M75_PLAN.md`
- ForcedMovement / CrowdControl — wspólna fizyka skilli
- Enemy System
- EnemyTargeting
- EnemyLaneMotor (M6.5 — waypoints + korytarz linii; bez NavMesh)
- EnemySeparation (M6.5 — lokalne rozpychanie XZ)
- SharedCamera / SharedCameraMath (centroid drużyny + zoom AABB)
- Base System
- Tower System
- Loot System (PARKED — drop w runie wyłączany w M7.5)
- SharedXP System
- SharedCurrency System
- Boss System
- UI System
- Save/Unlock System

## Boss System (M7)

- `BossDefinition` (SO) — liczby DRAFT: HP, szarża, telegraph, interrupt, enrage.
- `BossRamController` — maszyna stanów: Idle → Telegraph → Charge → Impact → Recover; FocusPlayer **ograniczony czasem** (DRAFT `focusDuration`), **przerywa się gdy charge gotowy** → Idle → szarża w **żywego gracza** (PO 2026-09-11); chase-through (bez glue standoff); Stunned po interrupt.
- Interrupt wyłącznie przez `StatusEffectReceiver` (stagger próg lub burst dmg w oknie).
- Cel szarży: żywy gracz → żywa wieża linii → żywa `BaseHealth` (baza tylko gdy nikt nie żyje). Szarża zatrzymuje się przed ciałem celu (nie wjeżdża w rdzeń).
- Fala 5: `WaveDefinition.isBossWave` + spawn przez `BossSpawner`.
- Śmierć → WaveComplete (wybicie) → Intermission. Unique drop broni **PARKED** (M7.5 wyłącza loot z pętli).

## Zasada architektoniczna
Dane contentowe (talenty, umiejętności, moby, fale) powinny być możliwie data-driven, np. przez ScriptableObjecty, a nie zahardkodowane w klasach. Fizyka skilli (wymuszony ruch, kontrola, odporność) jest wspólną warstwą, nie kopią w każdym skillu.

## Zakaz
Agent nie może robić dużych refaktorów przekrojowych bez uzasadnienia i zgody Lead/Planner.
