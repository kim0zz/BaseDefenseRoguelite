---
id: bomberman_kopniak
display_name: Kopniak
klasa: Bomberman
typ: aktywny
slot: umiejetnosc_3
status: DRAFT
wejscie: tap
celowanie: kierunek_aim
ksztalt: kapsula_przed
szerokosc_m: 3.40
dlugosc_m: 3.80
predkosc_bomby_m_s: 16
max_dystans_m: 6.5
shove_predkosc_m_s: 11
shove_czas_s: 0.30
przygotowanie_s: 0.08
faza_aktywna_s: 0.10
wykonczenie_s: 0.18
czas_odnowienia_s: 3.0
obrazenia: 0
on_enemy_bomba: explode
on_wall: stop_bez_wybuchu
---

# Kopniak

## Cel i rola
Natychmiastowe, szerokie kopnięcie przed postacią w **kierunku celowania**. Odpycha wrogów w obszarze i wystrzeliwuje wszystkie własne ładunki w tym samym polu. Działa bez bomby jako narzędzie kontroli tłumu.

## Wejście i celowanie
Tap + aim (mysz / prawy stick; puszczony prawy stick = istniejąca konwencja `AimMath.ResolveFacing`). Nie wymaga ustawienia się za konkretną bombą.

## Timing i ruch
Windup 0.08 / active 0.10 / recovery 0.18. CD **3.0 s** (DRAFT).

## Trafienie
Kapsuła **3.40 × 3.80 m**. Bomby lecą **16 m/s**, max **6.5 m**. Kontakt kopniętej bomby z wrogiem → wybuch. Ściana / obstacle / koniec zasięgu → **stop, 0 explode**.
Wróg wepchnięty w nieruchomą bombę odpala ją wcześniej. Zwykłe podejście **nie** detonuje (miejsce na Saper).

## Kontrola i fizyka
Shove przez `ForcedMovementRequest.ShoveAlong` + profil odporności: grunt pełny, elita ×0.5, boss **0** displace.
`DeployableMotor` rusza bombę. Wybuch ogłuszający (L2): stun 1.0 / 0.5 / 0 s. Kula bilardowa nadpisuje natychmiastowy wybuch kopniętej bomby.
Jedna bomba: `IsDetonated` — brak podwójnego wybuchu.

## Czas odnowienia i koszt
CD 3.0 s. Trafia Normal, Child, DashCharge i orbitale. Treser: dodatkowo wszystkie pobliskie wokół castera.

## Animacja
Placeholder kopnięcia + trail bomby.

## Feedback
Czytelny obszar przed postacią; stop na ścianie bez wybuchu.

## Sytuacje brzegowe
0 bomb = nadal shove wrogów. Pauza level-upu zatrzymuje motor. Lont kopniętej bomby **nie** restartuje.

## Co-op
Tylko własne bomby.

## Kierunki talentów
Wybuch ogłuszający, Kula bilardowa, Płonący taran, Treser bomb, BREAK! / Łańcuch kolizji.

## Telemetria
- liczba wrogów / bomb w obszarze
- wall stop vs enemy explode ratio
