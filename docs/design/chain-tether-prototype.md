# Chain Tether Movement — prototyp v0.1

## Status

**DRAFT** — izolowany prototyp ruchu na sprężystej linie (nie wpływa na SiegeDefense / MVP loop).

## Cel

Sprawdzić feel współdzielonego ruchu 2–4 graczy połączonych linią: strefy Slack / Soft / Hard, sprężyna + tłumienie, zachowanie agencji gracza (nie da się w 100% „ciągnąć” partnera).

## Jak uruchomić

1. W Unity: menu **Game → Prototypes → Create ChainMovementTest Scene** (tworzy scenę i asset konfiguracji, jeśli brak).
2. Otwórz `Assets/Scenes/Prototypes/ChainMovementTest.unity` (dwuklik w Project) i **Play**.
   Menu Unity **Game → Create ChainMovementTest Scene** jest opcjonalne (nadpisze scenę).
   To **nie** jest folder `Assets/_Game` — pasek u góry okna Unity, obok Assets / Window.
3. **2 graczy (zalecane do testu liny):**
   - Scena ma `autoJoinConnectedGamepads=true` — podłącz **2 pady** przed Play → P1 i P2 spawnują się obok siebie.
   - Alternatywa: **P1 klawiatura (WASD)** + **drugi gracz: South (A)** na padzie (wyłącz auto-join w inspektorze lub odłącz jeden pad).
   - P1 z pada → klawiatura: **Tab**.
4. Długość: **1 / 2 / 3** = Baseline / Short / ExtraShort (default ExtraShort).
   Topologia: **7** = pierścień (2=para, 3=trójkąt, 4=kwadrat), **8** = hub/kupsko.
   Pad: D-pad lewo = pierścień, prawo = hub. Select/Start = Short/ExtraShort.
5. HUD debug (lewy górny róg) pokazuje dystans, strefę, siły i parametry presetu.

## Parametry presetów (DRAFT)

| Preset | rest | softStart | hardStart | maxStretch | softK | hardK | damp | maxAccel | agency |
|---|---|---|---|---|---|---|---|---|---|
| Baseline (1, default) | 4 | 5 | 8 | 12 | 16 | 40 | 9 | 42 | 0.40 |
| Short (2) | 3.2 | 4 | 6.5 | 10 | 18 | 44 | 10 | 44 | 0.40 |
| ExtraShort (3) | 2.6 | 3.2 | 5.2 | 8 | 20 | 48 | 11 | 46 | 0.38 |

Asset: `Assets/_Game/Config/Prototypes/ChainTetherConfig.asset`.

## Architektura (skrót)

- `PlayerCharacter` — nadal intent ruchu w `Update`; opcjonalnie `CharacterController.Move` gdy CC istnieje.
- `ChainTetherSolver` — `LateUpdate` (order 50): siły sprężyste między kolejnymi graczami P1→P2→…; pęd liny (`ChainTetherBody.Velocity`) **nie** jest zerowany co klatkę (bounce + damping).
- `ChainTetherMath` — czysta matematyka (EditMode tests).
- Bootstrap buduje arenę (~60 m), ściany (`BoxCollider`), dummy z `Health`, `CombatBootstrap` + `BuildSystemBootstrap`.

## Ograniczenia (znane)

- **Brak owijania** liny o przeszkody — `LineRenderer` może przechodzić przez geometrię.
- **Brak** tether damage, wrap, cut chain, respawn rules, sieci, final VFX.
- Tether **nie** nakłada stun/knockback na moby; dummy tylko blokują ciałem.
- `MapPlayArea` nadal clampuje pozycję transform (legacy) — może współistnieć z CC; do iteracji feel.
- Scena `ChainMovementTest.unity` i asset config powstają dopiero po menu edytora (nie są w repo dopóki nie odpalisz setupu).

## Testy

EditMode: `Assets/_Game/Tests/EditMode/ChainTetherMathTests.cs` — uruchom przez Unity Test Runner (Edit Mode).

## Pliki

- Skrypty: `Assets/_Game/Scripts/Prototypes/ChainTether/`
- Editor: `Assets/_Game/Editor/ChainMovementTestSceneSetup.cs`
- Scena: `Assets/Scenes/Prototypes/ChainMovementTest.unity` (po menu)
- Docs: ten plik
