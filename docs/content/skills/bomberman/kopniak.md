---
id: bomberman_kopniak
display_name: Kopniak
klasa: Bomberman
typ: aktywny
slot: umiejetnosc_3
status: DRAFT
wejscie: tap
celowanie: kierunek
ksztalt: linia
zasieg_pick_m: 3.5
predkosc_bomby_m_s: 14
max_dystans_m: 7.0
przygotowanie_s: 0.15
faza_aktywna_s: 0.20
wykonczenie_s: 0.25
czas_odnowienia_s: 4.0
obrazenia: 0
on_enemy: explode
on_wall: stop_bez_wybuchu
---

# Kopniak

## Cel i rola
Kopie **najbliższą** własną bombę w promieniu **3.5 m** w kierunku aim. Skill, nie AA.

## Wejście i celowanie
Tap + aim. Pick: najbliższa owned deployable w zasięgu.

## Timing i ruch
Windup 0.15 / active 0.20 / recovery 0.25. CD **4.0 s**.

## Trafienie
Bomba leci **14 m/s**, max **7.0 m**. Kontakt z wrógiem → wybuch (bazowo). Ściana / obstacle / koniec zasięgu → **stop, 0 explode**. Boss: wybuch na kontakcie, **0** displace bossa.

## Kontrola i fizyka
`DeployableMotor` — nie rusza transformu wroga. Wybuch ogłuszający (L2): stun 1.0 / 0.5 / 0 s + boss stagger +8. Kula bilardowa nadpisuje natychmiastowy wybuch.

## Czas odnowienia i koszt
CD 4.0 s. Działa na Normal, Child i orbitale (kick orbital dziedziczy persistents).

## Animacja
Placeholder kopnięcia + trail bomby.

## Feedback
Czytelny kierunek lotu; stop na ścianie bez wybuchu (PO feel).

## Sytuacje brzegowe
Brak bomby w pick range = pudło. Pauza level-upu zatrzymuje motor.

## Co-op
Tylko własne bomby.

## Kierunki talentów
Wybuch ogłuszający, Kula bilardowa, Płonący taran, Treser bomb, BREAK! / Łańcuch kolizji.

## Telemetria
- pick success rate
- wall stop vs enemy explode ratio
