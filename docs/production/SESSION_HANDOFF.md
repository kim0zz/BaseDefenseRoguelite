# Stan sesji — 2026-09-11

## Gdzie jesteśmy

**M8.0 DONE** (PO playtest). **M8.2 / M8.3** kod + Verifier (feel 1–10 / Warden = NEEDS PLAYTEST).  
**M9.0 DONE** (kod + Verifier PASS). Karty/HUD = **NEEDS PLAYTEST PO**.  
**M9.1a + M9.1b DONE** (kod Implementer). Sloty feedbacku puste — placeholdery. Feel = **NEEDS PLAYTEST PO** (Q/E/R + Hart GOTOWY stun).

Playtest 2026-09-11: karty widać; Hart ma własny pasek; Ram szarżuje gracza, nie bazę, dopóki ktoś żyje.

PO projektuje pozostałe kity (Bomberman / reszta) — kod klas **nie** startuje bez canvasu od PO.

## Playtest (gdy wrócisz)

1. Stop + Play BootScene — HUD większy (`HudSkin.uiScale` 1.20 w `Assets/_Game/Config/HudSkin.asset`).
2. Q/E/R: windup = kółko/linia + krótki ton; hit = flash pierścienia + thud (placeholder sine).
3. Po wzięciu Harta: `Hart 0/5` → hity → `Hart GOTOWY` → 6. hit: wróg dostaje **żółty pierścień stuna** + ton stun.
4. Fala 5 Ram: szarża w gracza, nie w rdzeń.
5. Level-up: karta ma nazwę, tagi i 1–2 zdania *co się zmienia*. Pasywne ulti = **PASYWNE**.
6. Kolos / Piekielna aura (ulti): pierścień na graczu gdy aktywne (sloty puste = placeholder).
7. Trzęsienie + Rozpadlina: fale = jasne kółka wzdłuż linii; krater (pomarańczowa tarcza) **na wrogu trafionym falą**, nie przy rdzeniu. Konsola: `[Rozpadlina] krater @ trafienie`.

## Następne (nie startuje samo)

- Playtest M8.3 (Warden), M9.0 i **M9.1** feel (PO).
- **M9.1c** polish po playteście — dopiero na polecenie.
- Canvas kolejnej klasy gdy będzie gotowy.

**Unity MCP:** nie wołać `Unity_RunCommand` / Test Runner przez MCP.
