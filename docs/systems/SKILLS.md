# Umiejętności postaci

## Status
**FROZEN dla zasad, DRAFT dla liczb i contentu skilli**

PO zatwierdził pivot 2026-09-09. Wizja, pętla i zakres MVP są zgodne z tym dokumentem. Pionowy wycinek: `docs/production/M75_PLAN.md`.

Niezmienne: wspólny EXP, max. poziom 5, 4 decyzje w runie, pauza na awansie, local co-op, 3 linie, baza, win/lose.

## Cel systemu
Od poziomu 1 każda klasa ma własny atak podstawowy i około 3 unikalne umiejętności aktywne. Awans rozwija i modyfikuje te zdolności. Tożsamość ma być ostra od startu — jakościowe czasowniki, nie procentowe stat-sticki.

## Słownik (dokumentacja ↔ kod)

W dokumentacji designowej tylko polski. W kodzie angielski.

| Dokumentacja | Kod (proponowany) |
|---|---|
| przygotowanie | `Windup` |
| faza aktywna | `Active` |
| wykończenie | `Recovery` |
| czas odnowienia | `Cooldown` |
| celowanie | `Aim` |
| użycie umiejętności | `Cast` / `SkillUse` |
| trafienie | `Hit` |
| wstrząs kamery | `CameraShake` |
| wibracja pada | `GamepadRumble` |
| buforowanie wejścia | `InputBuffer` |
| odrzut | `Knockback` |
| przyciąganie | `Pull` |
| podrzucenie | `Launch` |
| szarża | `Charge` |
| przesuwanie | `Shove` |
| ogłuszenie | `Stun` |
| zachwianie (licznik) | `Stagger` |
| wymuszony ruch | `ForcedMovement` |
| obszar trafienia | `Overlap` |

## Warstwy wykonawcze

```
SkillDefinition (dane)
  → wykonawca kształtu (stożek, okrąg, linia, pocisk, strefa, wypad)
    → moduły efektów (obrażenia, status, sterowanie)
      → wymuszony ruch / kontrola tłumu (wspólne)
        → modyfikatory talentów
```

Zakaz: skill nie przesuwa wrogów przez `transform`, nie pisze własnego ogłuszenia i nie omija profilu odporności. Wszystko idzie przez wspólną warstwę fizyki umiejętności.

### Kształt i cele
Wykonawca kształtu zwraca listę trafień z **fizycznego obszaru** (overlap / ray / strefa) plus przeszkody i ściany. `laneId` **nie** jest globalnym filtrem. Skill może wymagać linii tylko wtedy, gdy jego mini-spec to jawnie mówi (np. „działa wyłącznie na przypisanej linii wieży”).

Dwa zbiegające się natarcia przy bazie: jeśli obszar nachodzi na wrogów z obu linii, wszyscy w obszarze są legalnymi celami.

### Wymuszony ruch (najważniejszy brak)

Dziś mamy tylko `KnockbackReceiver` (impuls XZ + tłumienie) oraz `StatusEffectReceiver` (ogłuszenie, licznik zachwiania). To za mało. Po kilku umiejętnościach każdy autor złoży własną fizykę.

Wspólny mechanizm musi obsłużyć:

| Tryb | Co robi | Kto jest ciałem |
|---|---|---|
| Odrzut | impuls od źródła | cel |
| Przyciąganie | impuls do źródła / punktu | cel |
| Podrzucenie | krótko wyłącza lokomocję, ciało w górę (wizualnie Y) i ląduje | cel |
| Szarża | przesuwa **używającego** po wektorze, z kolizją | caster |
| Przesuwanie | stała prędkość przez czas, nie impuls | cel |
| Ogłuszenie | blokada ruchu i ataku na czas | cel |

Kontrakt żądania (później jeden typ w kodzie, np. `ForcedMovementRequest`):

- tryb, źródło, kierunek albo punkt docelowy,
- siła, czas,
- czy honoruje kolizje/ściany,
- czy po zakończeniu wraca do korytarza linii (`przywróć_do_linii`, domyślnie **nie**).

Po wymuszonym ruchu silnik linii (`EnemyLaneMotor`) wznawia AI. Przywracanie do korytarza jest **opcją żądania**, nie globalną zasadą — obecne „po odrzucie zclampuj Grunta” nie może zostać ukrytym defaultem skilli.

### Odporność elit i bossów

Profil na definicji wroga (nie w skillu):

| Kategoria | Odrzut / przyciąganie / podrzucenie / przesuwanie | Ogłuszenie bezpośrednie |
|---|---|---|
| Zwykły | pełne | pełne |
| Elita | zmniejszone (skala DRAFT, np. 0,4–0,6) | skrócone |
| Boss | brak przemieszczenia | tylko przez istniejący próg zachwiania / przerwanie (M7) |
| Struktura | brak | brak |

Skill ustawia **chęć** („ogłusz 1,2 s”, „odrzuć siłą 2”). Warstwa fizyki mnoży przez profil celu. Dzięki temu Trzaśnięcie i przyszła szarża Cwela nie duplikują wyjątków na bossa.

Szarża castera też idzie przez tę warstwę (kolizja, przerwanie, śmierć w trakcie), nie przez osobny `DashController` w klasie.

### Talenty — struktura kart

Lock PO 2026-09-09: `docs/systems/PLAYER_PROGRESSION.md`. Nie drzewko A/B i nie losowe 2–3 z 12.

**FROZEN pętla:** wspólny EXP, poziom max. 5, **4 decyzje w runie**, pauza, pełne HP, własny wybór, automatyczne statystyki.

Karta może: zmutować istniejący skill (swap definicji albo dodać efekt), przyznać nowy skill (ulti), podpiąć efekt trwały na gracza. Zakaz `SkillModifierId` per karta. Dwa haki: efekt na resolve skilla, efekt trwały (tick / dmg / would-die).

W runie gracz **zawsze robi 4 wybory**. Liczba kart **na ekranie** jest zmienna (receptura klasy).

Talent bez zmiany czasownika, kształtu, warunku albo trybu umiejętności jest nielegalny w review (zakaz gołego `+% obrażeń`).

## Mini-spec — pola wymagane

1. Cel i rola
2. Wejście (tap / przytrzymaj / naładuj; roboczy przycisk)
3. Przygotowanie
4. Faza aktywna
5. Wykończenie
6. Zasięg
7. Obszar (kształt)
8. Obrażenia
9. Efekt kontroli (przez warstwę fizyki, nie ad hoc)
10. Czas odnowienia
11. Sposób celowania
12. Feedback wizualny / dźwiękowy
13. Animacja
14. Sytuacje brzegowe
15. Zachowanie w co-opie
16. Kierunki rozwoju talentami
17. Typ (`podstawowy` / `aktywny`)
18. Ruch podczas użycia
19. Przerywanie i anulowanie
20. Filtr celów
21. Limit celów / przebicie
22. Koszt albo ładunki
23. Blokada kierunku (zgodnie z Combat Playability, chyba że spec robi wyjątek)
24. Śmierć używającego i respawn
25. HUD
26. **Telemetria** (kontrakt pomiaru, nie UI)

### Telemetria
Każdy skill deklaruje, co zbieramy z playtestów / buildów:

- liczba użyć,
- procent użyć z ≥1 trafieniem,
- średnia liczba trafionych wrogów na użycie,
- suma obrażeń,
- suma czasu kontroli (po profilu odporności, nie „chciane” 1,2 s),
- anulowane użycia (przerwane w przygotowaniu, śmierć, pauza awansu).

Balance Agent dostaje te liczby; nie zgaduje z DPS na papierze.

### Pola zalecane
Zdanie fantazji, kiedy wciskam, ryzyko własne, wkład w zachwianie bossa, przeciwdziałanie, tagi, placeholder vs docelowy feedback.

## Feedback kamery i pada (co-op)

Jedna wspólna kamera (`LOCAL_COOP`, FROZEN). Wstrząs kamery **nie** zależy od tego, „czy to ty użyłeś umiejętności”. Zależy od:

- siły efektu,
- położenia zdarzenia względem kamery / centroidu drużyny (bliżej = mocniej, daleko na innej linii = ciszej albo zero).

Wibracja pada może być indywidualna (tylko pad używającego, ewentualnie pad trafionego sojusznika — osobna decyzja w specu).

## Dokumentacja contentu

```
docs/systems/SKILLS.md                 ← ten plik (zasady)
docs/content/skills/_TEMPLATE.md
docs/content/skills/<klasa>/<skill>.md
```

Plik skilla: YAML (kontrakt pod ScriptableObject) + proza. Klucze YAML po polsku; mapowanie na pola kodu jest w słowniku powyżej.

## Unity — kolejność (M7.5)

Plan: `docs/production/M75_PLAN.md`. Kolejność wycinka:

1. **Wymuszony ruch + profile odporności** — zanim powstanie drugi skill.
2. `SkillDefinition` + jeden kształt (okrąg wokół siebie).
3. Jeden skill wzorcowy (Trzaśnięcie) + placeholder feedback.
4. Jeden talent jako `SkillModifier` (zmiana kształtu albo strefa), nie `StatModifiers`.
5. Telemetria do logu / debug overlay (nie pełny backend).

Nie robić jednego SO z 80 polami. Nie wołać `KnockbackReceiver` bezpośrednio ze skilla po wprowadzeniu nowej warstwy — stary odrzut broni ma zostać podpięty pod ten sam mechanizm albo jawnie oznaczony jako dług techniczny.

Testy EditMode minimum: timing, overlap ze ścianą, profil elity/bossa, śmierć w przygotowaniu, brak filtra `laneId` na Trzaśnięciu.

## Proces refinementu dalszych skilli

0. M7.5: fizyka + Trzaśnięcie + wyłączenie lootu z pętli.
1. Kit canvas klasy: 1 atak + 3 aktywne, tylko rola i czasownik.
2. Macierz nachodzenia między klasami.
3. Mini-spec (szablon).
4. Overlay talentów wg `PLAYER_PROGRESSION` (receptury per klasa).
5. Wejście i HUD.
6. Freeze wzorca + playtest 1 skilla.

Nie projektować pełnych kitów czterech klas, dopóki wzorzec (Trzaśnięcie + fizyka) nie przejdzie playtestu.
