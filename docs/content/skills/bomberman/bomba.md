---
id: bomberman_bomba
display_name: Bomba
klasa: Bomberman
typ: aktywny
slot: umiejetnosc_1
status: DRAFT
wejscie: tap
celowanie: kierunek
ksztalt: punkt
zasieg_m: 0.60
obszar: { promien_m: 2.40 }
przygotowanie_s: 0.12
faza_aktywna_s: 0.08
wykonczenie_s: 0.15
czas_odnowienia_s: 0.40
obrazenia: 16
max_celow: 8
kontrola: { tryb: brak }
boss_stagger: 8
cap_normalnych: 3
detonatable: tak
kategoria: Normal
---

# Bomba

## Cel i rola
Stawia własną bombę w aim (offset **0.60 m**). Payoff = łańcuch z Detonatorem lub kopniakiem.

## Wejście i celowanie
Tap — punkt place w kierunku aim, nie w ciele gracza.

## Timing i ruch
Windup 0.12 / active 0.08 / recovery 0.15. CD **0.40 s** (~3 bomby w 1.2 s).

## Trafienie
Blast r **2.40 m**, dmg **16**, max **8** celów. Boss stagger **8**. Brak fuse — wybuch tylko na detonacji / kick / chain.

## Kontrola i fizyka
Cap **3** Normal; 4. place detonuje najstarszą (FIFO). Child / Orbital / Strike **nie** liczą się do capu.

## Czas odnowienia i koszt
CD 0.40 s. Talent 10 BOMB: cap **10** (Demolition **lub** Trapper).

## Animacja
Placeholder place + krótki flash na detonacji FIFO.

## Feedback
HUD: `Bomby n/cap`. Telegraph place (Implementer 1).

## Sytuacje brzegowe
Śmierć kasuje wszystkie owned. Respawn od 0. Pauza level-upu zamraża lifetime.

## Co-op
Każdy gracz ma własny rejestr owned.

## Kierunki talentów
Kasetowa (3 child), Saper (mina), Nalot opóźniony (place jako Normal), Treser (multi-launch).

## Telemetria
- place count / detonacje
- średni czas bomby na polu
- FIFO detonacje (4.+)
