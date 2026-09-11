# Input and Controllers

## Status
**FROZEN dla zachowania, DRAFT dla implementacji**

## Wymagania FROZEN (sterowanie)
- **Player 1** może używać klawiatury + myszy **albo** pada.
- **Player 2–4** używają wyłącznie padów.
- Klawiatura jest domyślnie przypisana do Player 1.
- Jeśli Player 1 gra na padzie, klawiatura **nie może** równolegle sterować tą samą postacią bez jawnego przełączenia.
- Jedno urządzenie może sterować tylko jedną postacią.
- Do 4 graczy jednocześnie (1× klawiatura/mysz lub pad dla P1 + do 3 padów dla P2–4).
- Osobny profil wejścia na gracza.
- Join flow.
- Odłączenie pada jednego gracza **nie może** wpływać na sterowanie pozostałych.
- Brak przejmowania postaci po odłączeniu innego pada.
- Wspólna kamera.
- Nawigacja UI level-upu per gracz.

## OPEN (implementacja)
- dokładny UX dołączania,
- mapowanie przycisków klawiatury i pada,
- UX jawnego przełączenia Player 1 między klawiaturą a padem.

## DRAFT — mapowanie walki (kit)

Zmiana broni wycofana. M7.6 zamyka 3 umiejętności aktywne Pudziana. Mapowanie skill 2–3 jest **DRAFT**, nie FROZEN.

- P1 klawiatura+mysz: WASD ruch, kursor = kierunek ataku/celowania, LMB/Space = atak podstawowy, Q = umiejętność 1, E = umiejętność 2, R = umiejętność 3, F = ulti (slot 4, puste do lvl 4).
- Pad: lewy stick = ruch, prawy stick = look, West = atak podstawowy, East = umiejętność 1, North = umiejętność 2, Left Shoulder = umiejętność 3, Right Shoulder = ulti.
- Level-up: KBM 1–9 = karta; pad D-pad / West/East = cykl opcji (N zmienne).
- South zostaje na confirm UI. Bumpers **nie** wracają do swapa broni.
- Jedno urządzenie nadal steruje tylko jedną postacią (FROZEN).
