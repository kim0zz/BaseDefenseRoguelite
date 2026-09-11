# Waves and Enemies

## Status
**FROZEN dla struktury, DRAFT dla parametrów liczbowych**

## Fale
- 10 fal.
- Zwykła fala: 2–3 minuty.
- Fala kończy się:
  - po upływie czasu, albo
  - po wybiciu wszystkich wrogów.
- Fale 5 i 10 = boss solo.

## Proponowany przebieg
1. Grunts
2. więcej Grunts + Hunter
3. Rusher + pierwszy Carrier
4. Siege + pierwsza elita
5. Boss 1
6. mocniejszy miks + Support
7. Flanker
8. Shielder + więcej elit
9. pełna presja na 3 liniach
10. Boss 2

## Typy wrogów
- Basic Grunt — prosty melee, priorytet gracz.
- Rusher — szybki, niski HP, idzie na struktury.
- Hunter — aktywnie ściga konkretnego gracza.
- Siege — wolny, wysoki HP, duże obrażenia w struktury.
- Support — wzmacnia pobliskich wrogów.
- Flanker — próbuje omijać główne przewężenia.
- Shielder — zapewnia przednią ochronę innym.
- Carrier — rzadki; daje dużo wspólnej kasy.

## Poruszanie po liniach — DRAFT (M6.5)

Struktura FROZEN się nie zmienia. To doprecyzowanie implementacji greyboxu:

- Każdy wróg ma **przypisaną linię** od spawnu (`AttackLineId`).
- Podejście jest wzdłuż ścieżki linii (spawn → przewężenie → wieża linii → baza), nie wektorem przez mapę.
- **Grunt** zostaje na swojej linii. Priorytet „gracz” oznacza: atakuje gracza, gdy jest w zasięgu **na tej linii / w korytarzu**, nie porzuca linii żeby gonić P1 na drugiej stronie mapy. Brak gracza na linii → schodzi w stronę bazy (presja na linię).
- **Hunter** może zejść z linii i gonić przypisanego gracza.
- Rusher/Siege/Flanker: zachowania strukturalne i ominięcia choke — **nie M6.5** (Epic 10 / M7+).

## Elity — MVP
Modyfikatory:
- Szał — więcej ruchu i szybkości ataku,
- Opancerzony — dużo pancerza,
- Niestabilny — eksploduje po śmierci.
