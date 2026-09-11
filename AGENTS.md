# AGENTS.md — Base Defense Roguelite

Konfiguracja pracy multi-agentowej dla projektu (Unity 6+, local co-op roguelite).
Ten plik jest indeksem; szczegóły ról w `.cursor/agents/`, zasady w `.cursor/rules/`.

## Dokumentacja = source of truth
Cała wiedza projektowa: `docs/`. Zawsze weryfikuj decyzje z docs.
- Wizja/zasady/pętla/zakres (FROZEN): `docs/00_GAME_VISION.md`, `docs/01_DESIGN_PRINCIPLES.md`,
  `docs/02_CORE_LOOP.md`, `docs/03_MVP_SCOPE.md`
- Kit umiejętności (zasady FROZEN): `docs/systems/SKILLS.md`
- Produkcja: `docs/production/`  • Technika: `docs/technical/`
- Systemy: `docs/systems/`  • Content: `docs/content/`  • Balans: `docs/balance/`

## Decyzje FROZEN
Dokument z `## Status = **FROZEN**` jest nienaruszalny. Konflikt z FROZEN → **eskalacja do PO**
(oznacz „⚠️ ESKALACJA FROZEN", wskaż dokument i opcje). Nie obchodź i nie modyfikuj FROZEN.

## Role
| Rola | Robi | Nie robi | Plik |
|---|---|---|---|
| Lead / Planner | planuje, deleguje, pilnuje architektury, eskaluje | nie koduje szeroko, nie zatwierdza milestone | `.cursor/agents/lead.md` |
| Implementer | implementuje powierzony zakres, testy | nie rozszerza zakresu, nie zmienia balansu/FROZEN | `.cursor/agents/implementer.md` |
| Verifier | acceptance criteria, regresja, Unity Console (`composer-2.5-fast`) | nie naprawia kodu, nie zmienia designu | `.cursor/agents/verifier.md` |
| Balance Agent | liczby, synergie, flagi odchyleń | nie zmienia FROZEN designu | `.cursor/agents/balance-agent.md` |

## Workflow
plan → implementacja → weryfikacja → playtest → feedback → iteracja.
- Autonomia niska/średnia; PO zatwierdza milestone.
- Każdy handoff: zakres, powiązane docs, acceptance criteria, status FROZEN.
- Definition of Done: `docs/production/DEFINITION_OF_DONE.md`.

## Unity przez MCP
- Verifier sprawdza `Unity_GetConsoleLogs` (Error/Warning) — błąd = FAIL.
- Zmiany w scenie/projekcie tylko w ramach zleconego zakresu i zgodnie z FROZEN.
