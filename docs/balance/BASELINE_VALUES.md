# Baseline Values v0.1

## Status
**DRAFT**

## Jamie
- HP: 100
- ruch: 100%
- melee: 100%
- ranged: 100%
- szybkość ataku: 100%

## Pudzian
- HP: 135
- ruch: 88%
- ciężka/melee: 120%
- ranged: 85%
- szybkość ataku: 90%

Pełny kit Pudziana (combo 3-hit, skille, talenty, ulti) — **BASELINE v0.1 DRAFT**: `docs/production/M81B_PUDZIAN_COMPLETE_PLAN.md` sekcja D. Implementer nie wymyśla liczb.

## Cwel
- HP: 85
- ruch: 115%
- melee: 95%
- ranged: 95%
- szybkość ataku: 115%
- krytyk: +5 pp

## Cipak
- HP: 90
- ruch: 100%
- melee: 85%
- ranged: 120%
- szybkość ataku: 105%

## Automatyczny wzrost na level
Jamie:
- +7 HP,
- +4% obrażeń wręcz i dystansowych.

Pudzian:
- +10 HP,
- +5% obrażeń wręcz,
- +2% siły zachwiania.

Cwel:
- +5 HP,
- +4% szybkości ataku,
- +2 pp krytyka.

Cipak:
- +5 HP,
- +5% obrażeń dystansowych,
- +2% prędkości pocisków.

## Benchmark moba fala 1
- HP: 18
- dmg: 8
- interwał ataku: 1,4 s
- prędkość ruchu: 82% Jamiego

## Benchmark miecza
- 10 dmg,
- 0,8 s interwał,
- 3 cele.

## Krzywa HP podstawowego moba — DRAFT
- fala 1: 18
- fala 2: 22
- fala 3: 26
- fala 4: 30
- fala 6: 38
- fala 7: 42
- fala 8: 46
- fala 9: 50

## Skalowanie per liczba graczy — DRAFT (raport 2026-09-08, czeka na PO)

Playtest PO: solo za trudne vs fale strojone pod co-op. Balance Agent: problem **nie** leży w HP grunta (18/8/1.4 — FROZEN 2 trafienia trzyma).

**1P = 100% baseline, skaler w GÓRĘ dla 2–4P.** Nie mnożyć HP mobów (łamie „2 trafienia"). W3/W4 mają `count: 1` — mnożnik poniżej 1 wycina całą flankę.

Propozycja mnożników (`1 + k·(n−1)`):

| Parametr | 1P | 2P | 3P | 4P |
|---|---|---|---|---|
| count spawnów | 1.00 | 1.40 | 1.80 | 2.20 |
| linie naraz | 1 | 2 | 3 | 3 |
| dmg w struktury | 1.00 | 1.25 | 1.50 | 1.75 |
| HP mobów | 1.00 | 1.00 | 1.00 | 1.00 |
| HP bossa | 1.00 | 1.60 | 2.20 | 2.80 |
| EXP/gold per mob | 1.00 | 0.71 | 0.56 | 0.45 |

Liczby DRAFT (baseline 1P): Ram HP **300** / focus 5 / shockwave 6 / impact struktury 27 (PO 2026-09-11: ~3× słabszy, żeby dało się przejść playtest 6–10). Siege structure 28 → **20**. Grunt 8 dmg / 18 HP — bez zmian.

**P0 (kod, zero liczb) — przed skalerem:**
- respawn nadal 20 s przy bazie (FROZEN), ale **offset od rdzenia** — dziś spawnuje w punkcie zbiegu 3 linii (`BaseCore + up`);
- `FocusPlayer` Rama bez wyjścia → czysty DPS race (narusza założenie „boss nie jest testem DPS") — **playtest T11 / poprawka:** timed focus + wznowienie szarży po cooldownie (DRAFT `focusDuration` 3.5 s / 2.2 s enraged).

Nie ruszamy FROZEN pętli (wspólny EXP, 3 linie, win/lose, 2 trafienia, enrage 50%). Nie zmieniamy baseline klas bez osobnego passu. Implementacja po zatwierdzeniu PO.

## M8.2 — Support / Flanker / Shielder — DRAFT v0.1 (lock Lead 2026-09-11)

Implementer nie wymyśla liczb. Pełne zasady: `docs/production/M8_PLAN.md` § M8.2.

| Typ | HP | dmg | interval | speed | struct dmg | player mul | exp | gold |
|---|---|---|---|---|---|---|---|---|
| Support | 24 | 6 | 1.5 | 3.4 | 0 (używa dmg) | 1.0 | 10 | 6 |
| Flanker | 18 | 5 | 1.15 | 5.8 | 12 | 0.65 | 10 | 6 |
| Shielder | 40 | 6 | 1.5 | 3.2 | 0 | 1.0 | 12 | 7 |

- Support aura: radius 5 m, move ×1.20, dmg ×1.20, refresh 0.35 s, nie self, nie stack.
- Flanker bypass: lateral 3.5 m, corridor half-width 2.2 m, `ChokeZ` bez zmian.
- Shielder protect radius 3.5 m, ta sama linia, sojusznik za (większe Z).

## M8.3 — Fale 6–10 + Warden — DRAFT v0.1 (lock Lead 2026-09-11)

Implementer nie wymyśla liczb. Pełne zasady: `docs/production/M8_PLAN.md` § M8.3.

### Grunty
| Asset | HP | speed | dmg | interval |
|---|---|---|---|---|
| M8_Grunt_F6 | 38 | 4.1 | 8 | 1.4 |
| M8_Grunt_F7 | 42 | 4.1 | 8 | 1.4 |
| M8_Grunt_F8 | 46 | 3.0 | 8 | 1.4 |
| M8_Grunt_F9 | 50 | 4.1 | 8 | 1.4 |

Duration: W6 150 s, W7 150 s, W8 180 s, W9 180 s, W10 240 s.

### Warden
HP 700; slam 16 / r 4 / telegraph 1.1 s / interval 3.4 s (szał 2.0 s). Totemy @70% HP 45, DR ×0.25 dmg taken. Wieża off @40% + 2 Siege. Strefy @20%: telegraph 0.6 s, r 3.5, 3.5 s, 8 dmg/0.5 s, co 5 s.
