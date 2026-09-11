# M7.5 — Kit Foundation — Plan

## Status
**DONE** — PO playtest PASS (2026-09-09). Kod T1–T7 + Verifier T8 PASS. Kit foundation zamknięty.

To **nie** jest dokończenie M7 i **nie** jest M8. To fundament kitu umiejętności + wspólnej fizyki.

## Cel milestone'u
Pudzian na mid-runie (fale 1–5) ma atak podstawowy + **jedną** umiejętność (Trzaśnięcie), opartą o wspólną warstwę wymuszonego ruchu. Loot ekwipunku znika z pętli. Talent może jakościowo zmienić ten skill (jeden dowód `SkillModifier`).

Po M7.5 da się playtestować tożsamość Pudziana bez inventory. To **nie** są kity czterech klas.

## Decyzje (zablokowane)

| ID | Decyzja | Wybór |
|----|---------|--------|
| D1 | Kolejność | Najpierw wymuszony ruch + odporności, potem skill, potem HUD/talent/telemetria. |
| D2 | Zakres kitu | 1 klasa (Pudzian), 1 aktywny (Trzaśnięcie). Atak podstawowy = istniejący melee, bez rewrite'u Combat Playability. |
| D3 | Loot | Wyłączyć z pętli (brak dropu / pickup nie wpływa na moc). Nie kasować kodu M6 w pierwszym PR. |
| D4 | Unikaty Rama | PARKED. Śmierć Rama = WaveComplete, bez broni na ziemi. |
| D5 | Talenty | Jeden `SkillModifier` na Trzaśnięcie (np. Pęknięcie = strefa). Struktura A/B nadal OPEN — nie budować drzewka. |
| D6 | Wejście | Atak + umiejętność 1. Mapowanie DRAFT: KBM Q / pad East. |
| D7 | M7 playtest | Może iść równolegle (Ram, fale, co-op UI). M7.5 nie czeka na PASS M7, ale nie psuje mid-runu. |

## FROZEN — nie naruszać

- `00_GAME_VISION`, `01_DESIGN_PRINCIPLES`, `02_CORE_LOOP`, `03_MVP_SCOPE` — kit, nie loot.
- `SKILLS.md` — zasady fizyki, kamery, linii, mini-speca.
- `LOCAL_COOP` — jedna kamera; wstrząs od siły i pozycji, nie od castera.
- Combat Playability: celowanie, blokada kierunku tylko w fazie aktywnej.
- M6.5: linie, separacja, kamera centroid.
- `PLAYER_PROGRESSION` — 4 decyzje w runie, pauza; drzewko OPEN.
- `VISUAL_FEEDBACK_CHECKLIST` — zapowiedź obszaru, CD czytelny.

## Kontrakt Trzaśnięcia

Źródło liczb i edge case'ów: `docs/content/skills/pudzian/trzasniecie.md`.

```
Wejście tap → przygotowanie 0,35 s (korzeń, okrąg na ziemi)
  → faza aktywna 0,12 s (okrąg r=3 m, max 8 celów, 16 dmg, zachwianie 1,2 s)
  → wykończenie 0,45 s
Czas odnowienia 8 s od fazy aktywnej.
Obszar fizyczny + ściany. Brak filtra laneId.
Boss: brak przemieszczenia, wkład w zachwianie.
Elita: skala odporności z profilu, nie z skilla.
Wstrząs kamery: siła × odległość zdarzenia od centroidu.
Wibracja: tylko pad Pudziana, gdy ≥1 cel.
```

## Taski

### M75-T1 — Wymuszony ruch + odporności
- Wspólne API (`ForcedMovementRequest` lub równoważne): odrzut, przyciąganie, podrzucenie, szarża castera, przesuwanie, ogłuszenie.
- Profil na wrogu: zwykły / elita / boss / struktura.
- Podpiąć obecny odrzut ataku pod to API albo oznaczyć jako dług z ticketem.
- **AC:** skill i atak nie wołają `transform` wroga ani własnego ogłuszenia. Elita krócej/słabiej, boss bez przemieszczenia. Testy EditMode na tryby + profil.

### M75-T2 — SkillDefinition + kształt okręgu
- SO + wykonawca overlap XZ + przeszkody.
- Timing przygotowanie / faza aktywna / wykończenie, bufor z wykończenia ataku, anulowanie w przygotowaniu.
- **AC:** da się odpalić skill z danych, nie z hardcode'u w klasie Pudziana. Śmierć w przygotowaniu nie startuje CD.

### M75-T3 — Trzaśnięcie na Pudzianie
- Slot umiejętność 1, wejście wg `INPUT_AND_CONTROLLERS.md`.
- Placeholder: okrąg zapowiedzi, błysk, wstrząs, thud.
- **AC:** playtest: clump w kole pada i staje; pudło pali CD; przy zbiegu linii przy bazie trafia cele z obu linii.

### M75-T4 — Loot poza pętlą
- Brak dropu z mobów/elit/bossa w runie (albo drop niepodnoszalny / nieaplikowany).
- HUD i przerwa: nie pokazują slotów broni/itemów jako mocy.
- Swap broni (E / bumpers) nie działa.
- **AC:** po zabiciu Rama nie leży unique. Zmiana loadoutu nie jest częścią pętli. Kod M6 może zostać w assembly.

### M75-T5 — HUD kitu
- Ikona + radial CD Trzaśnięcia. Atak podstawowy: jak dziś dmg/tempo, bez nazwy lootu.
- **AC:** czytelne bez konsoli (checklista).

### M75-T6 — Jeden SkillModifier
- Jedna karta (np. Pęknięcie: strefa spowolnienia po stompie). Podpięta pod wybór talentu lvl 2 **tylko dla Pudziana w tym wycinku** albo debug/force — nie projektować 12 kart.
- **AC:** po wzięciu karty skill zostawia strefę; bez karty nie zostawia. To nie jest `+% dmg`.

### M75-T7 — Telemetria debug
- Liczniki z mini-speca do logu / overlay (użycia, % trafień, średnia liczba celów, dmg, czas kontroli, anulowane, trafienia wieloliniowe).
- **AC:** po sesji Play widać liczby. Nie backend.

### M75-T8 — Verifier
- Console 0 Errors w Play BootScene: Pudzian, stomp, fala 1, Ram bez dropu.
- AC T1–T7. Regresja: aim, lock fazy aktywnej, linie, respawn, co-op join.

## Kolejność

```
T1 (fizyka) → T2 (definicja+kształt) → T3 (Trzaśnięcie)
                                    ↘ T4 (loot off) → T5 (HUD)
T3 → T6 (modyfikator) → T7 (telemetria) → T8 (Verifier)
```

T4 może iść równolegle z T2 po starcie.

## Poza zakresem M7.5

- Kity Cwela, Cipaka, Jamiego
- Umiejętności 2–3 Pudziana
- Pełne drzewko / 4 ekrany HotS
- Kasowanie `WeaponDefinition` / `ItemDefinition` z repo
- Fale 6–10, Boss 2, win/lose
- Mixamo / SFX docelowe / visual slice M4
- Zmiana liczb baseline ataku (Balance Agent)

## Ryzyka

| Ryzyko | Mitygacja |
|--------|-----------|
| Każdy skill z własnym knockbackiem | T1 przed T3; Verifier fail jeśli T3 omija API |
| Rewrite ataku podstawowego | Zostaje `PlayerAttackController`; skill osobny |
| M7 unique w playteście Rama | T4; zaktualizowany playtest M7 (bez pickup broni) |
| Scope 4 klasy | D2; Lead nie zleca canvasu w tym milestone |

## Playtest PO (minimum)

1. Pudzian, fala 1–2: Q / East — widać koło, clump pada, CD 8 s.
2. Pudło w puste: CD idzie, brak wstrząsu.
3. Przy bazie, moby z dwóch linii w kole — oba trafione.
4. Ram: stomp nie teleportuje bossa; interrupt nadal przez zachwianie/burst.
5. Zabicie Rama: przerwa, **brak** broni na ziemi.
6. (Jeśli T6) talent zostawia strefę — widać bez konsoli.

## Role

- **Lead:** ten plan; nie koduje szeroko.
- **Implementer:** T1–T7, małe commity, testy przy T.
- **Verifier:** T8 (`composer-2.5`).
- **Balance Agent:** nie w M7.5, chyba że PO każe patrzeć na telemetrię po playteście.
- **PO:** zatwierdza po playteście stompu, nie na podstawie kodu.
