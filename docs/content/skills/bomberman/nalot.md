---
id: bomberman_nalot
display_name: Nalot
klasa: Bomberman
typ: aktywny
slot: ulti
status: DRAFT
wejscie: tap
celowanie: kierunek
ksztalt: strefa
czas_odnowienia_s: 30
liczba_wybuchow: 7
telegraph_s: 1.00
sekwencja_s: 1.70
odstep_m: 2.20
blast_r_m: 2.00
obrazenia: 14
strip_half_width_m: 1.00
kategoria: Strike
detonatable: false
friendly_fire: nie
tagi: Airstrike, Zone
---

# Nalot

## Cel i rola
Ulti L4 — pas eksplozji wzdłuż aim od castera (nie geometria linii mapy).

## Wejście i celowanie
Aim strip: wektor od gracza w kierunku celowania.

## Timing i ruch
Telegraph **1.00 s**, sekwencja **1.70 s** (całość **2.70 s**). **7** wybuchów co **2.20 m** (~13 m pasa). CD **30 s**.

## Trafienie
Blast r **2.00 m**, dmg **14**, half-width pasa **1.00 m**. **0 FF** — overlap nie zawiera `PlayerCharacter`.

## Kontrola i fizyka
Kategoria Strike, `detonatable: false`.

## Czas odnowienia i koszt
CD **30 s**.

## Animacja
Telegraph pasa (Implementer 1) + sekwencyjne flash.

## Feedback
Jasny pas przed wybuchami; kolejność czytelna w co-op.

## Sytuacje brzegowe
Cele poza pasem nietknięte. Nalot opóźniony (L5): część indeksów place jako Normal (cap FIFO).

## Co-op
Brak FF — bezpieczny przy 4 graczach na linii.

## Kierunki talentów
Nalot z opóźnionym zapłonem, Ostatnia bomba (finisher od unikalnych celów).

## Telemetria
- trafienia per wybuch
- unikalne cele (pod Ostatnią bombę)
