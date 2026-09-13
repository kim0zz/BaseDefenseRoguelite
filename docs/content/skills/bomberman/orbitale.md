---
id: bomberman_orbitale
display_name: Orbitale
klasa: Bomberman
typ: aktywny
slot: ulti
status: DRAFT
wejscie: tap
celowanie: wokol_siebie
ksztalt: strefa
czas_odnowienia_s: 0
liczba_slotow: 4
orbit_r_m: 2.2
orbit_deg_s: 140
obrazenia: 12
blast_r_m: 1.80
contact_r_m: 0.50
recharge_s: 5.0
kategoria: Orbital
detonatable: false
tagi: Orbital, Physics
---

# Orbitale

## Cel i rola
Ulti L4 — **4** niezależne sloty orbitujących bomb wokół gracza; poza capem Normal (3).

## Wejście i celowanie
Aktywacja ulti — pierścień wokół ownera. Kick orbital dziedziczy kick/bomb persistents.

## Timing i ruch
Orbit r **2.2 m**, **140°/s**. Recharge **5.0 s** per slot (niezależnie). Kontakt wróg → explode.

## Trafienie
Dmg **12**, blast r **1.80 m**, contact r **0.50 m**. `detonatable: false` — ręczny Detonator usunięty z kitu; orbitale nadal nie detonują się zdalnie.

## Kontrola i fizyka
Kategoria Orbital. Reakcja orbitalna (L5): −1.0 s na slotach w recharge (clamp ≥0).

## Czas odnowienia i koszt
Brak CD po aktywacji — zarządza recharge per slot.

## Animacja
Placeholder pierścień + pips HUD.

## Feedback
HUD: `Orb 4/4` (ready/total). Kick orbital jak zwykła bomba.

## Sytuacje brzegowe
Śmierć despawn orbit. Pauza zamraża orbit i recharge.

## Co-op
Własne sloty per gracz.

## Kierunki talentów
Reakcja orbitalna, Kasetowe satelity, Układ Planetarny (6 bomb, r 3.0, 200°/s).

## Telemetria
- uptime slotów
- contact explode count
