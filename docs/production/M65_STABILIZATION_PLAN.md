# M6.5 — Core Lane / Crowd / Camera — Plan

## Status
**DONE** — PO playtest PASS (2026-09-08). Kod + Verifier runda 2 + playtest. M7 odblokowany (nie startuje sam).

## Cel
Domknąć niedokończony core z M2/M4 zanim wejdzie Boss 1:
1. separacja przeciwników (koniec bloba),
2. prawdziwy movement po trzech liniach (nie bee-line do gracza),
3. wspólna kamera z dynamicznym centrum i zoomem dla 1–4 graczy,
4. skala greyboxu pod 1–4 graczy i 5 stref obrony.

To **nie** jest Combat Readability, visual slice M4, polish M9 ani M7.

## Decyzje PO (zablokowane)

| ID | Decyzja | Wybór PO |
|----|---------|----------|
| D1 | Milestone | **M6.5** przed M7 |
| D2 | Grunt vs linia | **Grunt zostaje na przypisanej linii** (presja na linię). Atakuje gracza, gdy jest w zasięgu na tej linii / w korytarzu. |
| D3 | Hunter vs linia | **Hunter może zejść z linii** i gonić przypisanego gracza. |
| D4 | Pathfinding | **Szyny logiczne (waypoints + clamp boczny).** Bez NavMesh, bez Crowd package. |
| D5 | FROZEN | Bez zmian dokumentów FROZEN. Wymiary mapy zostają DRAFT (greybox). |

## FROZEN — nie naruszać

- `docs/02_CORE_LOOP.md` — wrogowie nadchodzą trzema liniami.
- `docs/03_MVP_SCOPE.md` — 1 mapa, 3 linie, 5 wież, local co-op 1–4, brak split-screen.
- `docs/systems/BASE_DEFENSE.md` — 3 linie, 5 wież, 5 stref obrony (intencja). Liczby greyboxu **nie** są FROZEN.
- `docs/systems/LOCAL_COOP.md` — wspólna kamera.
- `docs/systems/WAVES_AND_ENEMIES.md` — struktura typów. Grunt: priorytet gracz (w walce). Hunter: ściga konkretnego gracza.
- Combat Readability: nie ruszać faz ataku, shake math, placeholderów broni, `CameraShakeMath`.
- `docs/balance/BASELINE_VALUES.md` — zero zmian HP/DPS/interwałów.

## Diagnoza (skrót)

| Problem | Przyczyna w kodzie |
|--------|-------------------|
| Blob | `EnemyController` rusza `transform.position`; brak separacji; spawn spread 1.2. |
| Lane’y wizualne | `LaneMarker` bez ścieżki; `AttackLineId` ginie po spawnie; `detectRange=30` na mapie ~24 j. → agro od klatki 1; wektor do najbliższego gracza. |
| Kamera „P1” | `SharedCamera` ma centroid, ale typowy play to 1 gracz; zoom radialny bez AABB/aspect; `minOrthoSize=6`; brak clampu do mapy; brak testów. |
| Mapa za mała | Play area ~24×24; 5 stref upchnięte w ~18 j. Z. |

## Zakres tasków

```
T0 (docs) → T1 (mapa + LanePath) → T2 (lane motor) → T3 (separacja)
                                              ↘ T4 (kamera, po bounds T1)
T1–T4 → T5 (EditMode) → T6 (Verifier)
```

### M6.5-T1 — Skala greybox + LanePath
- Powiększyć play area ~1.6–2× w Z i szerzej rozstawić linie (serialized fields, nie magiczne liczby w AI).
- 5 czytelnych stref: daleko / choke / wieża linii / wieże bazy / rdzeń.
- Placeholder przewężeń (klocki) na każdej linii.
- API: `GetLanePath(AttackLineId)` → waypoints spawn→choke→wieża linii→baza + szerokość korytarza.
- `MapPlayArea` bounds = nowy greybox.

**Kierunek liczb (start, wolno stroić w Inspektorze):**
- play X ≈ −20…20, play Z ≈ −16…28, ground ≈ 50×50
- spawn: środek z≈26, lewa/prawa x≈±16
- choke z≈14, wieże linii z≈8 (x ≈ −14 / 0 / 14)
- wieże bazy z≈−6, rdzeń z≈−10, spawn graczy przy bazie

**AC:** 4 gracze mieszczą się na 3 liniach; 5 stref widać; ścieżka odczytywalna w teście; 3 linie i 5 wież bez zmian liczby.

### M6.5-T2 — EnemyLaneMotor
- `EnemySpawner.Spawn(..., AttackLineId lane)` — wróg trzyma `assignedLane`.
- Grunt: polyline + clamp boczny; nie przecina innych linii żeby gonić P1.
- Hunter: może zejść (peel) i gonić `_huntTarget`.
- Brak gracza na linii → Grunt idzie w stronę bazy wzdłuż ścieżki (presja), nie bee-line.
- `CombatTestSpawner` (jeśli nadal spawnuje): przekazać lane albo nie używać w BootScene.

**AC:** prawa linia żyje, gdy P1 stoi na lewej; Grunt atakuje gracza na swojej linii; Hunter schodzi.

### M6.5-T3 — Separacja
- Math 2D (XZ) między żywymi wrogami; radius z `BodyScale`; siła w Inspektorze.
- Cache listy (np. z `WaveManager` / rejestr), **nie** `FindObjectsByType` N² co klatkę na każdego.
- Nie wypychać na sąsiednią linię (separacja wzdłuż pasa / po clampie lane).
- Nie nadpisywać aktywnego knockbacku.

**AC:** 6+ Gruntów widać jako osobne ciała; EditMode: dwa agenty w jednym punkcie rozchodzą się ≥ radius.

### M6.5-T4 — SharedCamera
- Centroid **wszystkich dołączonych** `PlayerCharacter` (żywi + martwi w slocie), nie lock P1.
- Zoom: AABB XZ + aspect + padding, clamp min/max.
- Soft clamp kadru do `MapPlayArea`.
- `maxOrthoSize` dopasować do nowej mapy (T1).
- Wyciągnąć math do `SharedCameraMath` (testy).
- `AddShake` / `CameraShakeMath` bez zmian kontraktu.

**AC:** 2–4 graczy → środek = drużyna; rozrzut powiększa zoom; nikt żywy poza kadrem w granicach max; shake działa.

### M6.5-T5 — Testy EditMode
- Lane polyline / clamp / Grunt nie przecina linii (czysta logika).
- Separacja.
- Camera centroid + ortho size AABB.
- Map bounds pokrywają spawn i rdzeń.

### M6.5-T6 — Verifier
Patrz sekcja Verifier. Feel = NEEDS PLAYTEST, nie PASS z kodu.

## Poza zakresem (explicit)

- Flanker, Rusher/Siege, atak struktur, elity
- NavMesh / Unity Crowd
- CharacterController rewrite graczy
- Split-screen, druga mapa
- Boss 1, dropy bossów, co-op level-up UI (M7)
- Zmiana liczb HP/DPS z `BASELINE_VALUES`
- Fazy ataku / placeholdery broni

## Acceptance criteria całego M6.5

1. Wrogowie idą swoją linią (polyline + korytarz), nie najkrótszą drogą przez mapę.
2. Grunt nie opuszcza linii, żeby gonić gracza na innej linii.
3. Hunter może zejść z linii.
4. Tłum nie zlewa się w jedną kapsułę; choke nie rozrzuca na sąsiednie linie.
5. Kamera: centroid drużyny + zoom do rozrzutu 1–4; clamp do mapy.
6. Greybox ma 5 stref i miejsce na 4 graczy.
7. Brak regresji M4–M6 i Combat Readability.
8. Unity Console bez Error.
9. Docs zaktualizowane (ten plik + `MILESTONES.md`; DRAFT w `WAVES_AND_ENEMIES` / `ARCHITECTURE`).

## Weryfikacja / playtest PO

Po PASS kodu Verifiera (feel = NEEDS PLAYTEST):
1. 1P na lewej — prawa linia schodzi na bazę.
2. 8+ mobów na środku — osobne ciała, nie jednolita masa.
3. 2–4 graczy na skrajnych liniach — nikt poza kadrem.
4. 4 gracze przy bazie — da się rozstać na linie.

## Handoff

- **Implementer:** T1 … T5 (małe commity logiczne; jeden task na raz jeśli się da).
- **Verifier:** T6, model `composer-2.5`.
- **Lead:** przy FAIL Verifiera — recenzja i druga pętla bez PO (jedna runda poprawek).
- **Balance Agent:** nie.
- **M7:** nie, dopóki M6.5 nie przejdzie Verifiera + playtestu PO na linie/kamerę.
