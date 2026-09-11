# M6 — Build System — Plan

## Status
**ZIMPLEMENTOWANY** (historyczny). Po pivocie 2026-09-09 loot/ekwipunek są PARKED; tożsamość = M7.5.

## Cel milestone'u
Dostarczyć **grywalny build system** w trybie vertical slice: klasy, sloty broni/itemów,
loot, drzewko talentów i podstawowy UI — z architekturą data-driven gotową na rozszerzenie
contentu w M7+.

## Decyzje PO (zablokowane)

| ID | Decyzja | Wybór PO |
|----|---------|----------|
| D1 | Zakres contentu | **Vertical slice** — mała pula contentu + rozszerzalna architektura |
| D2 | Klasy w M6 | **Tak** — wszystkie 4 klasy MVP |
| D3 | Efekty rare | **Tylko stat boosty** — unikalne zachowania (trucizna, fale, warunki) później |
| D4 | Dropy bossów | **M7** — M6: loot z mobów i elit |
| D5 | UI level-up co-op | **Placeholder P1 proxy** — pełny UI per gracz w M7 |
| D6 | Dokumentacja | Ten plan + rozszerzony wpis w `MILESTONES.md` |

### ⚠️ Świadome odstępstwo od FROZEN (D5)
`docs/technical/UI_UX.md` (FROZEN) wymaga: każdy gracz wybiera własny talent,
gra wznawia się po zatwierdzeniu przez wszystkich aktywnych graczy.

**M6:** P1 wybiera talent za wszystkich (proxy). **M7:** pełny UI co-op zgodny z FROZEN.
To jest jawny tech debt — nie traktować jako docelowe zachowanie.

## Granica M5 → M6

| Obszar | M5 (gotowe) | M6 (do zrobienia) |
|--------|-------------|-------------------|
| Wspólny EXP / poziom drużyny | `SharedRunState`, `ProgressionConfig` | — |
| Pauza przy awansie + full heal | `GameFlowManager.LevelUpPause` | — |
| Wybór talentu | Stub (Space/Enter) | Talent System + UI (P1 proxy) |
| Broń | 1 slot, 3 definicje melee, ten sam loadout | 2 sloty, klasa → startowe bronie, swap |
| Klasy | Brak | 4 klasy MVP |
| Loot / inventory / itemy | Brak | Drop, pickup, 6 slotów itemów |
| UI buildów | Placeholder HUD | HUD broni/itemów, panel przerwy, level-up |

## Zakres contentu (vertical slice)

| Typ | M6 (slice) | Pełny MVP (później) |
|-----|------------|---------------------|
| Klasy | 4 (pełne tożsamości) | 4 |
| Talenty | 1 klasa pełne drzewko (12), 3 klasy stub (SO + 1 efekt każda) | 48 |
| Bronie | 4–6 common + 2 rare (stat boosty) | 16 (`docs/content/WEAPONS.md`) |
| Itemy | 6 (proste efekty stat/modifier) | 20 (`docs/content/ITEMS.md`) |
| Loot | Moby + elity | + bossy (M7) |

## Powiązane docs (source of truth)

- FROZEN: `docs/03_MVP_SCOPE.md`, `docs/02_CORE_LOOP.md`, `docs/systems/PLAYER_PROGRESSION.md`,
  `docs/systems/WEAPONS_AND_ITEMS.md`, `docs/systems/COMBAT.md`, `docs/content/CLASSES.md`
- DRAFT/content: `docs/content/WEAPONS.md`, `docs/content/ITEMS.md`, `docs/content/TALENTS.md`
- Backlog: Epic 7 (Shared Progression), Epic 8 (Loot and Inventory), Epic 11 (Classes and Talents)
- Architektura: `docs/technical/ARCHITECTURE.md`, `docs/technical/UI_UX.md`
- DoD: `docs/production/DEFINITION_OF_DONE.md`

## Taski (kolejność implementacji)

### M6-T1 — PlayerBuild model + data layer
- ScriptableObjecty: `WeaponDefinition`, `ItemDefinition`, `TalentDefinition`, `ClassDefinition`
- `PlayerBuildState` per gracz: klasa, 2 sloty broni, 6 itemów, ścieżka talentów
- Integracja z `PlayerCharacter`, `CombatBootstrap`
- **AC:** loadout z SO; zmiana broni w runtime zmienia parametry ataku
- **Moduły:** PlayerCharacter, CombatBootstrap, nowe SO

### M6-T2 — System klas (4 klasy MVP)
- Przypisanie klasy per gracz przy joinie
- Startowe bronie wg `CLASSES.md`
- Auto-scale statów przy level-up (FROZEN `PLAYER_PROGRESSION.md`)
- **AC:** 4 gracze mogą mieć różne klasy i startowe bronie

### M6-T3 — Weapon System (2 sloty)
- 2 sloty broni, przełączanie aktywnej w walce
- Content slice: 4–6 common + 2 rare (tylko staty)
- **AC:** 2 bronie na graczu, swap aktywnej, różne parametry widoczne w gameplayu
- **Verifier:** regresja M3–M5 combat

### M6-T4 — Wymiana broni między graczami
- Tylko w `Intermission` (FROZEN `02_CORE_LOOP.md`)
- **AC:** przekazanie broni w przerwie; blokada w trakcie fali

### M6-T5 — Loot System
- Drop na ziemię po zabiciu (common z mobów, rare z elit)
- Pickup per gracz
- **AC:** zabicie → drop → pickup → inventory
- **Poza zakresem M6:** dropy bossów (M7)

### M6-T6 — Inventory (6 slotów itemów)
- Equip/unequip; 6 itemów ze slice contentu
- Efekty: stat/modifier (bez złożonych warunków rare z docs)
- **AC:** efekt itemu widoczny w walce

### M6-T7 — Talent System + drzewko
- Śledzenie ścieżki A/B → A1/A2 → C/D → C1/C2 (FROZEN)
- 1 klasa: pełne 12 talentów (SO); 3 klasy: stub
- **AC:** lvl 2–5 pokazuje właściwe opcje; efekt talentu działa w walce

### M6-T8 — Level-up UI (placeholder P1 proxy)
- Pauza już istnieje (`Time.timeScale = 0`)
- P1 wybiera talent dla każdego gracza sekwencyjnie lub z listy
- **AC:** awans kończy się wyborem talentów; gra wznawia się po P1 confirm
- **Tech debt:** pełny co-op UI → M7 (FROZEN compliance)

### M6-T9 — HUD + panel między falami
- HUD: bronie, skrót itemów, team EXP/level
- Intermission: inventory, wymiana broni, rarity czytelne
- **AC:** zgodność z `UI_UX.md` (poza level-up co-op — odłożone)
- **Checklist:** `docs/production/VISUAL_FEEDBACK_CHECKLIST.md`

### M6-T10 — Integracja GameFlowManager
- Zamiana stubu `HandleLevelUpInput` na flow z M6-T8
- Resume po level-up w trakcie fali vs w przerwie
- **AC:** pełna pętla bez regresji M5

### M6-T11 — Weryfikacja milestone'u
- EditMode: talent path, inventory limits, weapon swap rules
- Playtest: run z buildami, level-up, wymiana broni
- Balance Agent (opcjonalnie PO): review slice vs `docs/balance/BALANCE_MODEL.md`
- **AC:** Definition of Done; Unity Console bez Error

## Acceptance criteria całego M6

1. Każdy gracz ma klasę, 2 sloty broni, 6 slotów itemów (data-driven).
2. Loot z mobów/elit → pickup → equip działa end-to-end.
3. Wymiana broni między graczami tylko między falami.
4. Awans drużyny (lvl 2–5) → pauza → wybór talentu (P1 proxy) → resume.
5. Drzewko talentów respektuje strukturę FROZEN (A/B branching).
6. UI buildów czytelne bez logów (HUD + intermission).
7. Brak regresji M4–M5 (fale, kasa, naprawa bazy, EXP).
8. Architektura SO gotowa na rozszerzenie contentu i efektów rare (M7+).

## Zależności i handoff

```
M6-T1 → M6-T2 → M6-T3 ─┬→ M6-T4
                        ├→ M6-T5 → M6-T6
                        └→ M6-T7 → M6-T8
M6-T3..T8 → M6-T9 → M6-T10 → M6-T11 (Verifier)
```

- **Implementer:** M6-T1 … M6-T10 (małe commity, jeden task na raz)
- **Verifier:** M6-T11
- **Balance Agent:** opcjonalnie po T11, jeśli PO zleci

## Poza zakresem M6 (explicit)

- Pełne zachowania rare broni/itemów (trucizna, fale, warunki)
- Dropy i unikaty bossów
- Pełny UI level-up co-op per pad
- Pełny content 48 talentów / 16 broni / 20 itemów
- Boss 1, fale 5+, win/lose (M7–M8)
- Polish UI/FX (M9)

## Backlog odłożony — do realizacji później

Centralna lista rzeczy **świadomie pominiętych w M6**. Przy planowaniu M7+ Lead
sprawdza ten backlog i nie traktuje pozycji jako „zrobione”.

| ID | Co pomijamy w M6 | Docelowy milestone | Powiązany doc / task |
|----|------------------|--------------------|----------------------|
| DEF-01 | Pełny UI level-up co-op (każdy gracz na swoim padzie, resume po wszystkich) | **M7** | `UI_UX.md` (FROZEN), M6-T8 tech debt |
| DEF-02 | Dropy i unikaty bossów | **M7** | `WEAPONS_AND_ITEMS.md`, Boss 1 |
| DEF-03 | Pełne zachowania rare broni (trucizna, fale, warunki, przebicia specjalne) | **M7–M8** | `WEAPONS.md`, `COMBAT.md` |
| DEF-04 | Pełne zachowania rare itemów (warunki, stacki, efekty drużynowe) | **M7–M8** | `ITEMS.md` |
| DEF-05 | System efektów/statusów (poison, bleed, burn, stun…) jako warstwa wspólna | **M7** | wymagane przed DEF-03/04 |
| DEF-06 | Pełne drzewka talentów dla 3 klas (stub → 12 talentów każda) | **M7–M8** | `TALENTS.md`, Epic 11 |
| DEF-07 | Rozszerzenie puli broni (16 łącznie) | **M7–M8** | `WEAPONS.md`, `03_MVP_SCOPE.md` |
| DEF-08 | Rozszerzenie puli itemów (20 łącznie) | **M7–M8** | `ITEMS.md`, `03_MVP_SCOPE.md` |
| DEF-09 | Boss 1 (The Ram), fale 1–5 | **M7** | `MILESTONES.md`, Epic 12 |
| DEF-10 | Boss 2, fale 6–10, win/lose | **M8** | `MILESTONES.md`, Epic 13–14 |
| DEF-11 | Polish UI buildów (animacje, ikony, przejścia) | **M9** | `UI_UX.md`, Epic 15 |
| DEF-12 | Pass balansu buildów (Balance Agent) | **M6+** (opcjonalnie po M6-T11) | `BALANCE_MODEL.md` |
| DEF-13 | Persistencja contentu jako `.asset` SO (obecnie `BuildContentFactory` runtime) | **M7** | AC8, workspace rule data-driven |
| DEF-14 | Pełny polish HUD (uGUI/TMP, ikony, layout, animacje, health bary) | **M9** | `UI_UX.md`, MILESTONES visual integration |
| DEF-15 | Combat readability (fazy ataku, placeholder broni, pocisk łuku) | **DONE** (PO playtest 2026-09-08) | [`COMBAT_READABILITY_PLAN.md`](COMBAT_READABILITY_PLAN.md) |

**Uwaga:** DEF-01, DEF-02, DEF-05, DEF-09 przeniesione do [`M7_PLAN.md`](M7_PLAN.md) jako obowiązkowe (2026-09-08).

## Następny krok

PO zatwierdza ten plan → Lead deleguje **M6-T1** do Implementera.
