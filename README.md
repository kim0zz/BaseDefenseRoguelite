# Base Defense Roguelite — Project Documentation v0.1

Status projektu: **MVP refinement complete / implementation not started**

Ten pakiet jest źródłem prawdy dla projektu. Dokumentacja jest celowo podzielona na małe pliki Markdown, aby mogła być używana przez ludzi i agentów AI.

## Statusy decyzji

- **FROZEN** — decyzja świadomie zatwierdzona; agent nie może jej sam zmienić.
- **DRAFT** — robocza wartość lub mechanika; może zostać zmieniona przez balans, testy lub decyzję PO.
- **OPEN** — temat celowo pozostawiony do późniejszej decyzji.

## Zasada nadrzędna dla agentów

Jeżeli implementacja wymaga zmiany elementu oznaczonego jako FROZEN, agent ma zatrzymać się i eskalować decyzję do PO zamiast zmieniać wymaganie.

## Główna struktura

- `docs/00_GAME_VISION.md`
- `docs/01_DESIGN_PRINCIPLES.md`
- `docs/02_CORE_LOOP.md`
- `docs/03_MVP_SCOPE.md`
- `docs/systems/*`
- `docs/content/*`
- `docs/balance/*`
- `docs/technical/*`
- `docs/production/*`

## Metodyka

Dokumentacja jest oparta na:
1. modularnym GDD,
2. wymaganiach testowalnych i acceptance criteria,
3. rozdzieleniu intencji projektowej od danych balansowych,
4. iteracyjnym development loop: plan → implementacja → weryfikacja → playtest → feedback.

To repo ma być jednocześnie dokumentacją produktu i kontekstem dla zespołu agentów.
