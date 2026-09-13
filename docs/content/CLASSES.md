# Classes

## Status
**FROZEN dla tożsamości, DRAFT dla liczb i kitów umiejętności**

Każda klasa od poziomu 1: 1 atak podstawowy + około 3 unikalne umiejętności aktywne. Mini-specy: `docs/content/skills/`. Wzorzec kitu: Pudzian (M7.6 + M8.1b). Cwel / Cipak / Jamie — plastry M8.4–M8.6 po canvasie PO; karty czytelne od M9.0.

## Cwel
Rogue/skirmisher.
- szybki,
- mobilny,
- crit,
- bleed/poison,
- naturalny build: mobilność/statusy,
- off-meta: boss hunter.

Kit: OPEN (M8.6 Cwel).

## Pudzian
Ciężki wojownik.
- dużo HP,
- kontrola tłumu,
- naturalny build: tank / przytrzymanie linii,
- off-meta: OPEN (nie „dwa miecze”).

Kit lvl 1: buława 3-hit + Stomp + Prowokacja + Byk. Mini-specy: `docs/content/skills/pudzian/`. Progresja L2–L5: `docs/content/TALENTS.md`. Plan kompletnego kitu: `docs/production/M81B_PUDZIAN_COMPLETE_PLAN.md`.

## Cipak
Łucznik/specjalista dystansowy.
- zasięg,
- przebicie,
- krytyki,
- statusy,
- pułapki.

Kit: OPEN (M8.5 Cipak).

## Jamie
Uniwersalny miecznik.
- najłatwiejszy dla początkującego,
- melee,
- support,
- obrona.

Kit: OPEN (M8.4 Jamie). Tożsamość „weapon-swap” jest wycofana.

## Bomberman
**DRAFT** — piąta klasa playable (opcja A, canvas PO M8.4). Drugi kompletny kit testujący framework progresji (po Pudzianie). MVP nadal ma **4** tożsamości (Cwel / Pudzian / Cipak / Jamie); Bomberman jest slice poza kolejnością M8.4→M8.6.

Specjalista od stawianych ładunków, kontroli tłumu i odskoku z ładunkiem.
- setup bomb z automatycznym lontem + kopniak w tłum,
- burn / homing / orbitale / nalot jako oś progresji,
- naturalny build: Demolition + Physics,
- off-meta: Trapper (miny) / RapidFire (petarda).

Kit lvl 1: petarda + Bomba + Wybuchowy odskok + Kopniak. Mini-specy: `docs/content/skills/bomberman/`. Progresja L2–L5: `docs/content/TALENTS.md` § Bomberman. Plan: `docs/production/M84_BOMBERMAN_PLAN.md`.

Playtest: `CombatBootstrap.usePlaytestClassOverride` na P1, klasa `playtestClassOverride` = Bomberman; domyślnie Pudzian gdy override wyłączony.

## Główna zasada klas
Każda klasa:
- ma czytelny kit od poziomu 1,
- ma 1–2 naturalne ścieżki talentów,
- ma co najmniej jedną grywalną off-metę,
- nie dzieli tożsamości z lootowanym ekwipunkiem.
