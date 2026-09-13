# Stan sesji — 2026-09-11

## Gdzie jesteśmy

**M8.0 DONE** (PO playtest). **M8.2 / M8.3** kod + Verifier (feel 1–10 / Warden = NEEDS PLAYTEST).  
**M9.0 DONE** (kod + Verifier PASS). Karty/HUD = **NEEDS PLAYTEST PO**.  
**M9.1a + M9.1b DONE** (kod Implementer). Sloty feedbacku puste — placeholdery. Feel = **NEEDS PLAYTEST PO** (Q/E/R + Hart GOTOWY stun).

**M8.4 Bomberman — NEEDS PLAYTEST PO** (kod + Verifier 2026-09-11: AC 1–12 PASS, Console 0 Error; feel = człowiek). Plan: [`M84_BOMBERMAN_PLAN.md`](M84_BOMBERMAN_PLAN.md). HUD: `Bomby n/cap`, `RAPID`, `Orb n/n`. L5 stuby (5 kart) nie blokują L1.

Playtest 2026-09-11: karty widać; Hart ma własny pasek; Ram szarżuje gracza, nie bazę, dopóki ktoś żyje.

## Playtest Bomberman (gotowe — Twój ruch)

1. Stop + Play **BootScene**.
2. Inspector → `CombatBootstrap` → **`usePlaytestClassOverride` = true** na P1, klasa `playtestClassOverride` = Bomberman. **Wyłączone** = Pudzian (domyślna regresja).
3. Sprawdź HUD bez konsoli: `Bomby 0/3` → po place rośnie; ulti Szybkostrzelność → `RAPID` lub `RAPID Xs`; Orbitale → `Orb 4/4`.
4. Q/E/R/F = Bomba / Wybuchowy odskok / Kopniak / ulti (jak Pudzian Q/E/R/F).
5. Level-up L2–L5: 27 buildów, min. 2 karty L5 — szczegóły w planie § Playtest PO.
6. Po teście: override OFF → Pudzian L2–L5 regresja.

## Playtest ogólny (gdy wrócisz)

1. Stop + Play BootScene — HUD większy (`HudSkin.uiScale` 1.20 w `Assets/_Game/Config/HudSkin.asset`).
2. Q/E/R: windup = kółko/linia + krótki ton; hit = flash pierścienia + thud (placeholder sine).
3. Po wzięciu Harta: `Hart 0/5` → hity → `Hart GOTOWY` → 6. hit: wróg dostaje **żółty pierścień stuna** + ton stun.
4. Fala 5 Ram: szarża w gracza, nie w rdzeń.
5. Level-up: karta ma nazwę, tagi i 1–2 zdania *co się zmienia*. Pasywne ulti = **PASYWNE**.
6. Kolos / Piekielna aura (ulti): pierścień na graczu gdy aktywne (sloty puste = placeholder).
7. Trzęsienie + Rozpadlina: fale = jasne kółka wzdłuż linii; krater (pomarańczowa tarcza) **na wrogu trafionym falą**, nie przy rdzeniu. Konsola: `[Rozpadlina] krater @ trafienie`.

## Następne (nie startuje samo)

- **Playtest M8.4 Bomberman** (PO) — checklista poniżej + plan § Playtest PO. Nie DONE bez człowieka.
- Playtest M8.3 (Warden), M9.0 i **M9.1** feel (PO).
- **M9.1c** polish po playteście — dopiero na polecenie.
- Jamie / Cipak / Cwel — po canvasie PO (pierwotna kolejność M8.4–M8.6).

**Unity MCP:** nie wołać `Unity_RunCommand` / Test Runner przez MCP.
