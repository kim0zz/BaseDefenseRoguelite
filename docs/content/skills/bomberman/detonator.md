---
id: bomberman_detonator
display_name: Detonator
klasa: Bomberman
typ: aktywny
slot: umiejetnosc_2
status: DRAFT
wejscie: tap
celowanie: wokol_siebie
ksztalt: wypad
przygotowanie_s: 0.08
faza_aktywna_s: 0
wykonczenie_s: 0.10
czas_odnowienia_s: 0.50
obrazenia: 0
cel: wszystkie_detonatable_owned
---

# Detonator

## Cel i rola
Jednym przyciskiem wysadza **wszystkie** własne obiekty oznaczone `Detonatable` (Normal, Child, kicked-ready). Orbitale i nalot domyślnie **nie**.

## Wejście i celowanie
Tap — bez aim; zasięg globalny w obrębie owned deployables gracza.

## Timing i ruch
Windup 0.08 / recovery 0.10. CD **0.50 s** — chain po stawianiu, nie przycisk DPS.

## Trafienie
Skill nie zadaje dmg sam — liczą wybuchy bomb. Działa na idle i armed (Saper) miny.

## Kontrola i fizyka
Brak. Mutacje burn/stun idą z persistents właściciela w momencie wybuchu.

## Czas odnowienia i koszt
CD 0.50 s.

## Animacja
Placeholder — krótki pulse na wszystkich detonowanych.

## Feedback
SFX chain; flash na każdej detonowanej bombie.

## Sytuacje brzegowe
0 bomb = skill się odpala, brak wybuchów. Pauza level-upu: CD zamrożony.

## Co-op
Tylko własne bomby gracza.

## Kierunki talentów
Piroman (burn on explode), Termobaryczny / Reakcja termiczna (consume burn), combo z Saperem.

## Telemetria
- użycia z ≥1 detonacją
- średnia liczba bomb na detonację
