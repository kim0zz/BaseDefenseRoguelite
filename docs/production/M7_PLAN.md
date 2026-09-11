# M7 — Mid-run game — Plan

## Status
**NEEDS PLAYTEST** — Verifier T10 runda 2 PASS na kod/Console (2026-09-08). Hotfix `TowerHealth` PropertyBlock. Milestone czeka na playtest PO (Ram + co-op UI). Nie zatwierdzone.

**Pivot 2026-09-09:** loot i unikaty broni są PARKED. Kod dropu może jeszcze leżeć w M7; playtest **nie** wymaga pickup broni. Kit umiejętności = M7.5 (`M75_PLAN.md`). M7.5 może iść równolegle i nie blokuje tego playtestu.

Opinia Leada (zakres vs playability): [`M7_LEAD_BRIEF.md`](M7_LEAD_BRIEF.md).

## Cel milestone'u
Dostarczyć **grywalny mid-run**: fale 1–5 kończą się Bossem 1 (The Ram), z czytelymi mechanikami,
warstwą statusów i pełnym UI level-up co-op. Drop unikatów broni — **PARKED** (pivot 2026-09-09).

Po M7 run da się rozegrać od fali 1 do pokonania Rama i przerwy po bossie.
To **nie** jest pełne MVP (fale 6–10, Boss 2, win/lose) ani polish M9.

## Decyzje Lead (zablokowane na start)

| ID | Decyzja | Wybór |
|----|---------|--------|
| D1 | Zakres obowiązkowy | DEF-01, DEF-02, DEF-05, DEF-09 z backlogu M6 |
| D2 | Timer fali bossa | Zostaje FROZEN (czas **albo** wybicie). Duration DRAFT **180 s**. Po complete — despawn żywych (boss nie zostaje w przerwie). |
| D3 | Po fali 5 | `RunComplete` mid-run (placeholder). **Nie** warunek wygranej MVP (to M8). |
| D4 | Meta-unlocki bossa | **Poza M7** (Epic 14 / M8). Unique drop w runie — PARKED (M7.5). |
| D5 | Statusy | Warstwa wspólna + **Stagger** (interrupt Rama) + **Poison** (historyczny proof Viper Fang). Burn/Bleed/Stun: API. |
| D6 | Wieże | HP + `IDamageable` + feedback uszkodzenia. **Bez** auto-ataku wież i bez naprawy wież. |
| D7 | DEF-13 (cały katalog SO) | **Poza M7**. |
| D8 | Unikaty Rama | **PARKED** — nie kryterium playtestu. |
| D9 | Typy wrogów | Rusher, Carrier, Siege + 3 modyfikatory elit. Support / Flanker / Shielder → M8. |
| D10 | DEF-03/04 (pełne rare) | **PARKED** z lootem. |

## FROZEN — nie naruszać

- `docs/02_CORE_LOOP.md` — fale 5 i 10 = boss solo; po bossie zwykła przerwa; fala kończy się po czasie albo wybiciu; każdy gracz wybiera własny talent.
- `docs/03_MVP_SCOPE.md` — 10 fal / 2 bossów to zakres **MVP**, nie M7. M7 = pierwsze 5 + Boss 1.
- `docs/systems/BOSSES.md` — mechaniki Rama (szarża na wieżę, interrupt, focus gracza, enrage 50%, fala po szarży).
- `docs/systems/WAVES_AND_ENEMIES.md` — przebieg fal 1–5; typy; elity (Szał / Opancerzony / Niestabilny).
- `docs/systems/PLAYER_PROGRESSION.md` + `docs/technical/UI_UX.md` — pause, full heal, **każdy** wybiera własny talent, resume po wszystkich aktywnych.
- `docs/technical/INPUT_AND_CONTROLLERS.md` — UI level-up per gracz; P1 KBM albo pad; P2–4 tylko pad; jedno urządzenie = jedna postać.
- `docs/systems/WEAPONS_AND_ITEMS.md` — **PARKED**; unikatowe dropy nie są kryterium M7 po pivocie.
- `docs/production/VISUAL_FEEDBACK_CHECKLIST.md` — sekcja Bossowie (zapowiedzi, strefy, fazy).
- Combat Readability / Playability / M6.5: nie cofać aim, lock Active, lane motor, separacji, kamery.

## Stan kodu (punkt startu)

| Jest | Brakuje |
|------|---------|
| 2 fale (Grunt + Hunter), `M5_WaveSequence` | Fale 3–5, sekwencja 5 fal |
| `EnemyKind`: Grunt, Hunter | Rusher, Carrier, Siege, Boss, elity |
| Wieże = `TowerMarker` (brak HP) | `TowerHealth`, atak wrogów w struktury |
| `EnemyController` bije tylko graczy | Target struktur (Rusher/Siege/Ram) |
| Loot z mobów/elit | Gwarantowany drop unikatów z bossa |
| Level-up: P1 proxy (klawiatura 1/2) | Wybór per pad / per gracz, status, resume all |
| Brak statusów | Warstwa `StatusEffectReceiver` |
| Brak bossa | The Ram + telegraph + enrage |

## Powiązane docs

- FROZEN: `02_CORE_LOOP.md`, `03_MVP_SCOPE.md`, `BOSSES.md` (założenia), `WAVES_AND_ENEMIES.md` (struktura), `PLAYER_PROGRESSION.md`, `WEAPONS_AND_ITEMS.md`, `UI_UX.md`, `INPUT_AND_CONTROLLERS.md`, `VISUAL_FEEDBACK_CHECKLIST.md`
- DRAFT/content: `docs/content/BOSSES.md`, `docs/content/ENEMIES.md`, `docs/content/WEAPONS.md`, `docs/balance/BASELINE_VALUES.md`
- Backlog: Epic 10 (część), Epic 12, Epic 7 (UI), Epic 8 (boss loot)
- Architektura: `docs/technical/ARCHITECTURE.md` (Boss System)
- DoD: `docs/production/DEFINITION_OF_DONE.md`

## Kontrakt mechanik Rama

```
Idle → TelegraphCharge (linia/strefa czytelna)
     → Charge (do żywego gracza; wieża/baza tylko gdy nikt nie żyje)
     → Impact (gracz = focus dmg; struktura = structureImpact) + Shockwave
     → Recover
FocusPlayer: okresowo goni jednego żywego gracza (nie w Charge).
Interrupt: w Telegraph/Charge, gdy stagger ≥ próg LUB burst dmg w oknie ≥ próg
           → Stun krótki, szarża przerwana.
HP ≤ 50%: Enrage — krótszy telegraph, wyższa częstotliwość szarż.
Fala 5: spawn 1× Ram, zero zwykłych mobów.
Śmierć Rama: WaveComplete (wybicie) → Intermission. Unique drop — PARKED.
```

Placeholdery: większy mesh, inny kolor, HP bar bossa, linia szarży, dysk shockwave.
Zero FBX / Mixamo / SFX (M9).

## Przebieg fal 1–5 (content)

| Fala | Skład | Uwagi |
|------|--------|--------|
| 1 | Grunts, 3 linie | Istniejący baseline; HP wg `BASELINE_VALUES` fala 1 |
| 2 | Więcej Gruntów + Hunter | Istniejący Hunter; więcej densu |
| 3 | Rusher + pierwszy Carrier | Rusher → struktury; Carrier = dużo gold (Grunt-like movement) |
| 4 | Siege + pierwsza elita | Siege wolny, duży dmg w struktury; 1 elit z modyfikatorem |
| 5 | Boss 1 solo | The Ram |

Liczby spawnów/HP — DRAFT w SO, spójne z krzywą HP w `BASELINE_VALUES.md` (f1=18, f2=22, f3=26, f4=30). Nie zmieniać benchmarku miecza / klas.

## Taski (kolejność)

### M7-T1 — Warstwa statusów (DEF-05)

- `StatusEffectType`: `Stagger`, `Poison`, `Burn`, `Bleed`, `Stun` (+ opcjonalnie `MoveSpeedBuff` dla Horn).
- `StatusEffectReceiver` na `Health` (gracze, wrogowie, boss).
- Apply / stack policy (Poison odświeża duration; Stagger kumuluje do progu i resetuje).
- Tick DoT na `Poison`; `Stun` blokuje movement+atak na czas.
- `Health`: event `Damaged(amount, source)` — potrzebny do stagger/burst interrupt.
- HUD: ważne statusy (FROZEN `UI_UX.md`) — placeholder tekst/kolor, bez ikon M9.
- Proof: `Viper Fang` aplikuje Poison (25% / 2/s / 3 s z `WEAPONS.md`) zamiast samego stat-sticka.
- **AC:** Apply+tick+expire w EditMode; Poison z Viper Fang działa w walce; Stun zatrzymuje cel; HUD pokazuje aktywny status bez logów.
- **Moduły:** Combat (nowe), Health, Build (Viper Fang), CombatHudPlaceholder
- **Poza:** pełne rare (DEF-03/04), Burn/Bleed content

### M7-T2 — Wieże jako struktury

- `TowerHealth` (`IDamageable` + `Health`) na 5 wieżach greybox.
- HP z `ProgressionConfig` / SO (DRAFT, nie hardcode w logice).
- Feedback: zmiana koloru przy niskim HP; przy 0 HP — „zniszczona” (ciemny placeholder, bez funkcji), nie despawn markera.
- **AC:** wróg/boss potrafi zadać dmg wieży; 0 HP = zniszczona i czytelna; baza nadal `BaseHealth`.
- **Moduły:** Map (TowerMarker/TowerHealth), MapGreyboxBuilder, Health
- **Poza:** auto-atak wież, upgrade/naprawa wież, poziomy 2–3

### M7-T3 — Rusher, Carrier, Siege + elity

- `EnemyKind`: + `Rusher`, `Carrier`, `Siege`.
- Targeting:
  - **Rusher** — wzdłuż linii do wieży linii, potem baza; atakuje `IDamageable` struktury; niski HP, wysoki moveSpeed.
  - **Siege** — to samo, wolny, wysoki HP, duży dmg w struktury, mały dmg w gracza jeśli gracz blokuje.
  - **Carrier** — ruch jak Grunt (zostaje na linii); goldReward wyraźnie wyższy.
- Elity: `EliteModifier` { None, Frenzy, Armored, Unstable } na `WaveSpawnEntry` albo wrapper SO.
  - Szał: +moveSpeed, +attackSpeed.
  - Opancerzony: redukcja dmg (albo +maxHP jako DRAFT proxy pancerza).
  - Niestabilny: AOE dmg placeholder po śmierci (nie niszczy bazy instant).
- Wizual: inny kolor/skala per kind; elita = większa + obramowanie/kolor.
- Loot: Hunter **albo** elita → rare chance (nie psuć M6). Carrier nie jest elitą.
- **AC:** 3 nowe typy czytelne w gameplayu; Rusher/Siege biją wieżę; Carrier sypie kasą; 3 modyfikatory działają; EditMode targeting/modifier.
- **Moduły:** EnemyDefinition, EnemyController, EnemySpawner, LootDropService, EnemyLaneMotor
- **Poza:** Support, Flanker, Shielder

### M7-T4 — Sekwencja fal 1–4 (bez bossa)

- Nowe `WaveDefinition` SO: `M7_Wave1`…`M7_Wave4` + `M7_WaveSequence` (5 slotów, wave 5 w T5).
- `GameFlowManager` czyta sekwencję M7 (nie zostawiać 2 fal M5 jako domyślnej).
- Skład zgodny z tabelą powyżej; duration zwykłej fali DRAFT 120–180 s (FROZEN 2–3 min).
- `WaveDefinition.ShouldComplete` bez zmian kontraktu FROZEN.
- **AC:** Play BootScene → fale 1–4 po kolei, przerwy, kasa/EXP; skład typów zgodny z docs; brak regresji M5 (koniec po czasie/wybiciu, naprawa bazy).
- **Moduły:** Waves, GameFlowManager, Config assets
- **Zależność:** T3

### M7-T5 — Boss 1 The Ram (DEF-09)

- `BossDefinition` SO (HP, charge speed, telegraph, interrupt thresholds, shockwave, enrage) — liczby DRAFT w data, nie w if-ach.
- `BossRamController` (osobny od `EnemyController`; może reuse motor/health/knockback).
- State machine wg kontraktu.
- Telegraph: linia/strefa na XZ (placeholder primitive), czytelna **przed** ruchem.
- Charge target: żywa wieża linii (los / najbliższa / rotacja — DRAFT, jedna strategia, udokumentowana); brak żywych wież → `BaseHealth`.
- Interrupt przez T1 (stagger próg **lub** burst dmg w oknie telegraph/charge).
- Enrage ≤50% HP.
- Shockwave po impakcie: overlap dmg + knockback w promieniu.
- Focus gracza poza charge.
- Wave 5: 1 boss, `IsBossWave` (lub równoważne) — bez grup gruntów.
- Po complete fali: despawn żywych (T9 może domknąć, T5 musi nie zostawiać Rama w przerwie jeśli fala skończyła się timerem).
- **AC:** szarża czytelna i przerywalna; enrage wyczuwalny; shockwave trafia; focus działa; fala 5 = solo boss; checklista Bossowie (placeholder PASS, feel = playtest).
- **Moduły:** nowe Boss/, WaveManager (spawn bossa), Health
- **Zależność:** T1, T2

### M7-T6 — Dropy unikatów bossa (DEF-02) — **PARKED po pivocie**

Zadanie historyczne. Nie rozwijać. M7.5 wyłącza loot z pętli. Kod dropu może zostać do T4 w `M75_PLAN.md`.

### M7-T7 — Level-up co-op UI (DEF-01)

- Usunąć P1-proxy jako jedyną ścieżkę.
- Każdy **aktywny** gracz wybiera na swoim urządzeniu:
  - P1 KBM: 1/2 + Enter/Space (jak dziś, ale tylko dla **siebie**).
  - Pad (P1 na padzie i P2–4): D-pad / face West-East = opcja, South = zatwierdź. Mapowanie OPEN, izolacja FROZEN.
- Panel: lista graczy ze statusem `wybiera…` / `gotowy` (FROZEN `UI_UX.md`).
- `Time.timeScale = 0` zostaje; input przez `wasPressedThisFrame` (unscaled) — nie psuć.
- Resume **dopiero** gdy wszyscy aktywni zatwierdzą (martwy-w-trakcie-fali, który dostał full heal, też wybiera).
- Gracz bez opcji talentu (nie powinien w MVP lvl 2–5, ale guard) = auto-ready.
- **AC:** 2+ graczy: każdy wybiera swój talent; P1 nie wybiera za P2; gra nie wznawia się wcześniej; 1 gracz = bez regresji (sam zatwierdza).
- **Moduły:** BuildFlowController, GameFlowManager, PlayerCharacter (input UI), PlayerJoinManager
- **FROZEN:** `PLAYER_PROGRESSION.md`, `UI_UX.md`, `INPUT_AND_CONTROLLERS.md`

### M7-T8 — HUD / feedback mid-run

- HUD fali: numer 1–5, timer, HP bazy, statusy (T1), dmg/tempo (Playability — nie ruszać).
- Boss: pasek HP Rama + etykieta fazy (`Szarża` / `Enrage`).
- Wieże: czytelny niski HP (T2).
- Koniec runu po fali 5: placeholder „Boss 1 pokonany — mid-run complete” (nie ekran wygranej M8).
- **AC:** da się rozegrać fale 1–5 bez czytania konsoli; checklista Bossowie/Baza (loot PARKED).
- **Moduły:** CombatHudPlaceholder, BuildFlow OnGUI, ewentualnie BossHud
- **Checklist:** `VISUAL_FEEDBACK_CHECKLIST.md`

### M7-T9 — Integracja GameFlow

- `CompleteWave`: despawn pozostałych wrogów/bossa (timer failsafe).
- Po fali 5: Intermission (FROZEN: po bossie przerwa), potem `RunComplete` mid-run (brak fali 6).
- Level-up w trakcie fali bossa: pause+heal+wybór co-op, resume walki (regresja M5/M6).
- Docs: ten plan → Status **ZIMPLEMENTOWANY**; `SESSION_HANDOFF.md`; krótka notatka w `ARCHITECTURE.md` (Boss System).
- `M5_WaveSequence` zostawić w repo (nie usuwać), nie używać jako default BootScene.
- **AC:** pełna pętla 1–5 + Ram + przerwa + mid-run complete; brak Error w Console; regresje listy poniżej zielone.

### M7-T10 — Weryfikacja milestone'u

- EditMode: statusy, elite mods, wave complete, talent all-ready, boss interrupt math, unique roll.
- Unity Console: Error = FAIL.
- Playtest PO: Ram (czytelność szarży, interrupt, enrage), co-op level-up, fale 1–5.
- **AC:** DoD; Verifier PASS lub lista FAIL z reprodukcją. Game feel Rama = najwyżej NEEDS PLAYTEST z kodu.

## Acceptance criteria całego M7

1. Sekwencja fal 1–5 działa end-to-end (spawn, timer/clear, przerwy, kasa, EXP).
2. Fala 5 = solo The Ram; mechaniki z `BOSSES.md` (szarża, interrupt, focus, enrage 50%, shockwave).
3. Po zabiciu Rama: przerwa, potem mid-run complete (nie win M8). Unique drop PARKED.
4. Rusher / Carrier / Siege + ≥1 elita na fali 4 są w grze i czytelne.
5. Wieże mają HP i mogą zostać uszkodzone/zniszczone; baza bez regresji naprawy.
6. Warstwa statusów działa (Stagger + Poison); HUD pokazuje ważne statusy.
7. Level-up: każdy aktywny gracz wybiera na swoim inputcie; resume po wszystkich.
8. Brak regresji krytycznych: join 1–4, respawn 20 s, koniec fali czas/wybicie, level-up pause+heal, aim/lock Active, lane Grunt/Hunter, kamera 1–4.
9. Console bez Error; wartości w SO/config, nie w magicznych if-ach walki.
10. Docs zaktualizowane przy zmianie zachowania.

## Zależności i handoff

```
M7-T1 (statusy) ─┬→ M7-T5 (Ram) → M7-T6 (dropy)
M7-T2 (wieże)   ─┘         ↘
M7-T3 (nowe moby) → M7-T4 (fale 1–4) ─┬→ M7-T8 (HUD) → M7-T9 → M7-T10
M7-T7 (co-op UI) ─────────────────────┘
```

T1, T2, T3, T7 są równoległe po starcie. T5 wymaga T1+T2. T4 wymaga T3. T6 wymaga T5. T8 po T5+T7. T9 spina wszystko.

- **Implementer:** T1 … T9 (kolejność powyżej; małe, jednozakresowe zmiany; testy przy każdym T).
- **Verifier:** T10 (`composer-2.5`, nie Sonnet, nie fast).
- **Balance Agent:** nie w M7 (M0–M5 zasada; M7 bez jawnego polecenia PO).
- **PO:** zatwierdza milestone po playteście (szarża Rama + co-op level-up).

## Poza zakresem M7 (explicit)

- Fale 6–10, Boss 2, win/lose, meta-unlocki (M8)
- Support, Flanker, Shielder
- Pełne rare behaviors poza Poison proof (DEF-03/04)
- Thick Hide, Siege Breaker; pula broni/itemów (PARKED); pełny content talentów (M8, po decyzji struktury)
- Auto-atak / upgrade / naprawa wież
- Migracja całego `BuildContentFactory` na `.asset` (DEF-13)
- Visual slice M4, Mixamo, SFX, polish HUD (M9)
- Zmiana liczb klas/broni baseline (Balance Agent / PO)

## Ryzyka

| Ryzyko | Mitygacja |
|--------|-----------|
| Ram bez HP wież nie ma celu szarży | T2 przed T5 |
| Interrupt bez statusów = ad-hoc if w bossie | T1 najpierw; boss tylko woła API |
| Co-op UI przy `timeScale=0` gubi input pada | czytać `wasPressedThisFrame`; test 2 padów + KBM |
| Timer 180 s kończy bossa w przerwie | despawn na CompleteWave; duration DRAFT długie |
| Scope creep rare/meta | D4/D8/D10; Verifier fail jeśli Implementer doda M8 |

## Playtest PO (minimum)

1. Solo: fale 1–4, widać Rushera bijącego wieżę, Carriera, Siege, elitę.
2. Fala 5: telegraph szarży **zanim** Ram ruszy; interrupt młotem/staggerem; po 50% szybsze szarże.
3. Zabicie Rama → przerwa → mid-run complete. Brak wymogu unique na ziemi (PIVOT).
4. 2 graczy: level-up — P2 wybiera na padzie, P1 nie może zatwierdzić za niego; gra czeka.
5. Regresja: Hunter schodzi z linii, Grunt nie; aim myszą; respawn 20 s.

## Następny krok

Lead deleguje **M7-T1 … M7-T9** do Implementera, potem **M7-T10** do Verifiera.
PO nie zatwierdza milestone'u na podstawie samego kodu — wymagany playtest Rama i co-op UI.
