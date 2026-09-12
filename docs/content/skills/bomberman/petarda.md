---
id: bomberman_petarda
display_name: Petarda
klasa: Bomberman
typ: podstawowy
slot: atak
status: DRAFT
wejscie: tap
celowanie: kierunek
ksztalt: pocisk
zasieg_m: 8.0
obszar: { promien_splash_m: 1.10, arc_peak_m: 0.55 }
przygotowanie_s: 0.28
faza_aktywna_s: 0
wykonczenie_s: 0.72
czas_odnowienia_s: 0.70
obrazenia: 9
max_celow: 5
kontrola: { tryb: brak }
odrzut: 0
boss_stagger: 3
predkosc_pocisku_m_s: 14
---

# Petarda

## Cel i rola
Podstawowy rzut granatem — splash na clumpie, bez knockbacku; setup pod bomby i detonator.

## Wejście i celowanie
Tap w kierunku aim (mysz / prawy stick). Spawn pocisku po windup (jak łuk Bow).

## Timing i ruch
Windup 0.28 / recovery 0.72 (active 0). Interwał bazowy **0.70 s** (DPS ≈ 12.9). Brak friendly fire.

## Trafienie
Thrown explosive: arc peak 0.55 m, prędkość 14 m/s, max range 8 m. Splash r **1.10 m**, max **5** celów. Boss stagger **3**.

## Kontrola i fizyka
Brak knockbacku (0). Wybuch na pierwszym trafieniu lub przeszkodzie.

## Czas odnowienia i koszt
Interwał ataku = cooldown AA. Mutacje L2/L4/L5 mogą zmienić splash lub tempo (np. Większy Huk, Szybkostrzelność, Karabin).

## Animacja
Placeholder M8.4 — windup rzutu; pełna animacja M9.

## Feedback
Mały flash splashu; wstrząs skalowany do siły (wspólna kamera).

## Sytuacje brzegowe
Pudło na przeszkodę = wybuch w punkcie kontaktu. Pauza level-upu zatrzymuje windup. Śmierć castera anuluje windup.

## Co-op
Brak FF. Splash czytelny przy 4 graczach.

## Kierunki talentów
Większy Huk (splash 2.20 m), Szybkostrzelność (0.22 s / 6 s), Karabin (0.22 s permanent, dmg ×0.45), Przeładowany magazynek (splash podczas ulti).

## Telemetria
- użycia AA
- trafienia splash ≥2 cele
- suma obrażeń AA vs bomby
