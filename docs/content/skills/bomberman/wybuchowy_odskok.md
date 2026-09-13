---
id: bomberman_wybuchowy_odskok
display_name: Wybuchowy odskok
klasa: Bomberman
typ: aktywny
slot: umiejetnosc_2
status: DRAFT
wejscie: tap
celowanie: kierunek_ruchu
ksztalt: szarza
dystans_m: 4.0
predkosc_m_s: 20
przygotowanie_s: 0.06
faza_aktywna_s: 0.20
wykonczenie_s: 0.12
czas_odnowienia_s: 5.5
ladunek: { lont_s: 0.70, obrazenia: 10, promien_m: 1.80, stagger: 4 }
nietykalnosc: nie
---

# Wybuchowy odskok

Zastępuje ręczny Detonator w slocie 2.

## Cel i rola
Zostawia słabszy ładunek dokładnie w miejscu startu i wykonuje krótki odskok w **kierunku ruchu**. Może prowadzić od walki albo w jej stronę. Nie wymaga wskazywania lądowania ani potwierdzenia.

## Wejście i celowanie
Tap. Kierunek: lewa gałka / WASD. Brak wejścia ruchu → ostatni kierunek ruchu → zwrócenie postaci (`AimMath.ResolveDashDirection`). Celowanie (prawy stick / mysz) **nie** steruje odskokiem.

## Timing i ruch
Windup 0.06 / active **0.20 s** / recovery 0.12. Prędkość **20 m/s** ≈ **4.0 m**. CD **5.5 s** (DRAFT).
Ruch = `ForcedMovementRequest.CasterCharge` z `honorCollisions`. Stop na krawędzi `MapPlayArea` i na `Structure` (baza/wieże). Nie przenosi przez geometrię.

## Trafienie
Ładunek kategorii `DashCharge` (poza capem Normal): lont **0.70 s**, dmg **10**, r **1.80 m**, stagger **4**. Można go kopnąć; te same zasady kontaktu co główna bomba.
Kasetowa **nie** spawnuje child z tego ładunku.

## Kontrola i fizyka
Brak nietykalności — `DamageImmunity` jest tylko przy Skoku Pudziana, nie przy Charge/odskoku.
Brak capsule-hitów Byka.

## Czas odnowienia i koszt
CD 5.5 s. Bez dodatkowego kosztu.

## Animacja
Placeholder: różowy ładunek + krótka szarża.

## Feedback
Natychmiastowy leave-behind + przemieszczenie ciała.

## Sytuacje brzegowe
0 wejścia ruchu = last move / facing. Pauza level-upu zamraża CD i lont.

## Co-op
Tylko własny ładunek.

## Kierunki talentów
Ten sam kick/contact co bomba. Ulepszenia Normal (Kasetowa, cap, Saper-on-place) **nie** obejmują DashCharge.

## Telemetria
- kierunek: current / last / facing
- early stop: play area / structure
