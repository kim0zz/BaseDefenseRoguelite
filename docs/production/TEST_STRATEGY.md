# Test Strategy

## Poziomy testów

### 1. Testy jednostkowe/logiczne
Dla:
- obrażeń,
- leczenia,
- EXP,
- ekonomii,
- talentów,
- warunków końca fali.

### 2. Testy integracyjne
Dla:
- gracz + atak podstawowy,
- gracz + umiejętność,
- gracz + talent,
- mob + targetowanie,
- fala + spawn,
- baza + wieże,
- wymuszony ruch + odporność elity/bossa.

### 3. Automatyczne testy scenariuszowe
Przykłady:
- 4 gracze dołączają,
- Player 2 ginie i wraca po 20 s,
- wszyscy giną → przegrana,
- baza ginie → przegrana,
- fala kończy się po czasie,
- fala kończy się po wybiciu mobów,
- lvl-up zatrzymuje świat i leczy wszystkich.

### 4. Playtest
Człowiek ocenia:
- czy walka jest czytelna,
- czy atak i umiejętności "czują się" dobrze,
- czy decyzje talentowe są ciekawe,
- czy boss jest uczciwy,
- czy buildy są różnorodne,
- czy pojawia się frustracja niezwiązana ze skillem.

## Balance Agent
Powinien:
- liczyć DPS/TTK/EHP,
- flagować synergie odstające od mediany,
- wyszukiwać dominujące buildy,
- wykrywać martwe talenty,
- raportować wyniki.

Nie powinien:
- samodzielnie zmieniać designu FROZEN.
