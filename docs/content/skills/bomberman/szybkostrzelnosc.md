---
id: bomberman_szybkostrzelnosc
display_name: Szybkostrzelność
klasa: Bomberman
typ: aktywny
slot: ulti
status: DRAFT
wejscie: tap
celowanie: kierunek
ksztalt: pocisk
czas_odnowienia_s: 28
trwanie_s: 6.0
interval_override_s: 0.22
obrazenia_aa: 9
extra_dmg: 0
tagi: Basic, RapidFire
---

# Szybkostrzelność

## Cel i rola
Ulti L4 — burst petardy: absolutny override interwału AA na **6.0 s**.

## Wejście i celowanie
Jak petarda (aim + tap). Slot **F** / RB.

## Timing i ruch
Po aktywacji: interwał **0.22 s** (DPS ≈ 41 burst). Po 6 s wraca bazowy **0.70 s**. CD ulti **28 s**.

## Trafienie
Te same parametry petardy (9 dmg, splash 1.10 m) — **extra dmg 0**.

## Kontrola i fizyka
Brak. Konflikt z Karabinem: permanent wygrywa nad timed.

## Czas odnowienia i koszt
CD **28 s**. Uptime ≈ 21%.

## Animacja
Placeholder — szybszy windup feel.

## Feedback
HUD: `RAPID` lub `RAPID Xs` podczas override.

## Sytuacje brzegowe
Śmierć nie resetuje CD ulti. Pauza level-upu zatrzymuje timer.

## Co-op
Indywidualne ulti per gracz.

## Kierunki talentów
L5 Przeładowany magazynek (splash + scorch podczas ulti).

## Telemetria
- aktywacje / uptime
- dmg podczas rapid vs poza
