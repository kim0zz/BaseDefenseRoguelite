# Szablon mini-speca umiejętności

Skopiuj do `docs/content/skills/<klasa>/<id>.md`. Zasady: `docs/systems/SKILLS.md`.
Liczby DRAFT, dopóki Balance Agent / playtest nie powiedzą inaczej.

```yaml
---
id: klasa_nazwa
klasa: 
typ: podstawowy | aktywny
slot: atak | umiejetnosc_1 | umiejetnosc_2 | umiejetnosc_3
status: DRAFT
wejscie: tap | przytrzymaj | naladuj
celowanie: kierunek | wokol_siebie | punkt_na_ziemi | wektor_ruchu
ksztalt: stozek | okrag | linia | punkt | pocisk | strefa | wypad
zasieg_m: 0
obszar: { }
przygotowanie_s: 0
faza_aktywna_s: 0
wykonczenie_s: 0
czas_odnowienia_s: 0
ladunki: 1
koszt: brak
obrazenia: 0
max_celow: 0
kontrola:
  tryb: brak | odrzut | przyciaganie | podrzucenie | przesuwanie | ogluszenie | zachwianie
  czas_s: 0
  sila: 0
ruch_w_uzyciu: pelny | spowolnienie | korzen | szarza
blokada_kierunku: tylko_faza_aktywna | brak | caly_cast
filtr_celow: [wrog, elita, boss]
bije_struktury: nie
wymaga_linii: nie
przywroc_do_linii_po_ruchu: nie
smierc_castera: anuluj | dobij_efekt | zostaw_strefe
---
```

`wymaga_linii: nie` jest domyślne. Ustaw `tak` tylko gdy skill naprawdę ma ignorować cele spoza wskazanej linii.

# Tytuł po polsku

## Cel i rola
Jedno zdanie: po co to wciskam na linii, nie „robi obrażenia”.

## Wejście i celowanie

## Timing i ruch
Przygotowanie / faza aktywna / wykończenie. Czy wolno buforować z wykończenia ataku. Co przerywa użycie.

## Trafienie
Obszar, limit celów, przeszkody. Fizyczny overlap — bez ukrytego filtra linii.

## Kontrola i fizyka
Żądanie do warstwy wymuszonego ruchu. Nie opisywać tu wyjątków bossa — to profil celu.

## Czas odnowienia i koszt

## Animacja

## Feedback
Wstrząs kamery od siły i pozycji zdarzenia (wspólna kamera). Wibracja pada indywidualna.

## Sytuacje brzegowe
Pudło, śmierć w przygotowaniu, pauza awansu, respawn, 0 celów.

## Co-op
Sojusznicy, stackowanie kontroli, czytelność przy 4 graczach.

## Kierunki talentów
2–4 jakościowe modyfikacje. Nie mapować na A/B, dopóki struktura talentów jest OPEN.

## Telemetria
- użycia
- procent użyć z ≥1 trafieniem
- średnia liczba trafionych
- suma obrażeń
- suma czasu kontroli (po odporności)
- anulowane użycia
