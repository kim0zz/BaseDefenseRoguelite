# Agent Workflow

## Role

### Lead / Planner
- czyta wymagania,
- rozbija feature na taski,
- pilnuje architektury,
- wskazuje zależności,
- eskaluje niejasności.

### Implementer
- implementuje konkretny task,
- nie rozszerza zakresu bez potrzeby,
- aktualizuje testy.

### Verifier
- zakłada, że implementacja może być błędna,
- sprawdza acceptance criteria,
- szuka regresji,
- raportuje niezgodności.

### Balance Agent
- analizuje liczby,
- odpala symulacje,
- flaguje outliery,
- nie zmienia FROZEN designu.

## Pętla
requirements → plan → implementacja → weryfikacja → build/playtest → feedback → kolejna iteracja

## Poziom autonomii na start
Niski/średni:
- Lead planuje,
- Implementer koduje,
- Verifier sprawdza,
- PO zatwierdza milestone.

## Zwiększanie autonomii
Dopiero po ustabilizowaniu:
- kontekstu,
- architektury,
- testów,
- Definition of Done.
