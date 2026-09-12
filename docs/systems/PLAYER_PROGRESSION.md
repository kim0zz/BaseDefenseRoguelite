# Player Progression

## Status
**FROZEN dla pętli awansu, FROZEN dla struktury kart (lock PO 2026-09-09)**

Pivot skilli (`docs/systems/SKILLS.md`) nie zmienia pętli poniżej.

## EXP
- EXP jest wspólny dla całej drużyny.
- Cała drużyna awansuje jednocześnie.
- Poziomy są stosunkowo rzadkie.

## Poziom
- maksymalny poziom klasy w MVP: 5,
- **4 decyzje talentowe w runie** (nie 12; 12 w starym drafcie to była pula kart, nie liczba wyborów),
- po awansie świat zatrzymuje się całkowicie,
- wszyscy gracze odzyskują pełne HP,
- każdy gracz wybiera własny talent,
- podstawowe statystyki rosną automatycznie zgodnie z klasą.

## Struktura kart (lock PO)

Jedna karta = `TalentDefinition`. Ulti to skill przyznawany talentem (`GrantSkill`), nie osobny typ definicji.

Każda klasa ma `ClassProgressionTable` (receptury oferty). Logika poziomu **nie** siedzi w UI. UI dostaje N kart i rysuje `Length`. N **nie** jest globalnie stałe.

Tagi niosą karty (nie kit lvl 1). Licznik tagów = suma z wybranych talentów, zawsze wyliczany od nowa.

Wymagania karty (AND, puste pole = brak ograniczenia):
`requiresTalents`, `requiresUltimateId`, `requiresTags`, `requiresTagCounts`, `excludesTalents`, `metaUnlockId`, `exactLevel` / `minLevel`.

`metaUnlockId` pusty = część bazowego kitu. W MVP query meta zawsze odblokowuje bazowy kit. Rozbudowane konto = poza MVP.

### Receptury — Pudzian (wzorzec)

Inna klasa może mieć inną tabelę bez zmiany silnika (np. Bomberman L2: `AllEligibleInGroup` zamiast `OneMutationPerActive` — bez zmiany pętli FROZEN).

| Poziom | Receptura | N |
|---|---|---|
| 2 | jedna mutacja każdego z 3 aktywnych skilli | 3 |
| 3 | 2 karty niezależne (rdzeń) + 1 follow-up 1:1 od wyboru lvl 2 | 3 |
| 4 | wszystkie eligible ulti (`GrantSkill`) | zmienne (Pudzian: 4), **bez** filtra od L2/L3 |
| 5 | wszystkie eligible capstone’y (ulti + tagi / talenty) | zmienne, min. 1 na każdy legalny build |

Zakaz gołego `+%` bez zmiany czasownika, kształtu, warunku albo trybu (`SKILLS.md`).

Szczegóły kart Pudziana: `docs/content/TALENTS.md`. Mini-specy skilli: `docs/content/skills/`.

## Target pacing — DRAFT
- lvl 2: okolice fali 2–3,
- lvl 3: okolice fali 4,
- lvl 4: okolice fali 6–7,
- lvl 5: okolice fali 8–9.
