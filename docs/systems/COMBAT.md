# Combat

## Status
**FROZEN dla zasad, DRAFT dla liczb**

Źródło kitu: `docs/systems/SKILLS.md`.

## Główne zasady
- Klasa determinuje sposób walki: atak podstawowy + umiejętności aktywne od poziomu 1.
- Gracz nie zbiera broni ani przedmiotów w bieżącym MVP.
- Atak i umiejętności mają fazy: przygotowanie → faza aktywna → wykończenie.
- Kontrola tłumu i przemieszczenie idą przez wspólną warstwę wymuszonego ruchu, nie przez logikę pojedynczego skilla.
- Obszar trafienia jest fizyczny (overlap + przeszkody). Filtr linii tylko gdy mini-spec skilla tego wymaga.
- Klasy nie pożyczają sobie kitów; tożsamość jest w umiejętnościach, nie w lootowanym ekwipunku.

## Kategorie wartości bojowej
Każdą umiejętność (i atak podstawowy) oceniamy w wielu wymiarach:
- obrażenia ciągłe,
- obrażenia chwilowe,
- zasięg,
- obszar ataku,
- bezpieczeństwo użycia,
- kontrola tłumu,
- odrzut/zachwianie,
- użyteczność.

Nie balansujemy wyłącznie DPS-em.

## DRAFT — fazy (Combat Readability)

Timing, nie tylko liczby:

- **Przygotowanie** (`Windup`) — zapowiedź widoczna, bez obrażeń.
- **Faza aktywna** (`Active`) — melee: okno trafienia (overlap). Pocisk: często spawnuje się na końcu przygotowania.
- **Wykończenie** (`Recovery`) — nie można zacząć kolejnego ataku podstawowego do Idle. Umiejętność może być zbuforowana z wykończenia, jeśli mini-spec na to pozwala.

Pełny cykl ataku podstawowego = `AttackInterval`. Parametry frakcji: `docs/technical/GAME_FEEL.md` (DRAFT).

Kierunek trafienia (Combat Playability):
- źródło: celowanie (mysz / prawy stick) albo fallback z ruchu,
- przygotowanie śledzi celowanie,
- melee overlap używa kierunku z wejścia w fazie aktywnej,
- blokada obrotu postaci tylko w fazie aktywnej, chyba że mini-spec robi wyjątek.
