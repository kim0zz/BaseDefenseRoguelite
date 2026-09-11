# Combat Readability Pass — Plan dla Implementera

## Status
**DONE** — PO playtest PASS (2026-09-08). Kod A–F + feel zatwierdzone. Nie jest to visual slice M4 ani polish M9.

To **nie jest M7**. To odłożony feeling z M3 + cienki placeholder visual, **przed** M7.
Pełne modele, klipy animacji, VFX Graph, SFX i polish UI = **poza zakresem** (M4 slice / M9).

## Cel
Walka ma być czytelna na placeholderach: widać **start ataku**, **okno hitu**, różnicę **lekka vs ciężka**, a **łuk ma pocisk**.
DPS i liczby z `BASELINE_VALUES.md` / `BuildContentFactory` **nie ruszamy**.

## Decyzje PO (zablokowane)

| ID | Decyzja | Wybór |
|----|---------|--------|
| D1 | Zakres | Readability pass: timing + placeholdery + gniazdo Animatora. Zero assetów od PO. |
| D2 | Feeling | 4 rodziny: miecz / sztylet / topór / łuk. |
| D3 | Warianty | młot = topór; pika = miecz; rare = rodzina bazy (bez unikalnych VFX). |
| D4 | Łuk | Prosty pocisk-placeholder (lot + trafienie). Bez przebicia, bounce, trapów. |
| D5 | Ruch | sztylet 100% / miecz spowolniony / topór mocno spowolniony w windupie. Bez twardego root. |
| D6 | Cancel | śmierć przerywa; swap czeka na Idle; brak cancel ataku atakiem. |

## FROZEN — nie naruszać

- `docs/technical/GAME_FEEL.md` — zasady (narzędzia feedbacku, tożsamość 4 broni). **Wolno dodać sekcję DRAFT z liczbami.**
- `docs/technical/ART_DIRECTION.md` — nadal placeholdery; brak visual slice M4.
- `docs/production/VISUAL_FEEDBACK_CHECKLIST.md` — Walka: start / hit / kto dostał / lekka vs ciężka.
- `docs/systems/COMBAT.md` — broń determinuje sposób ataku. **Wolno dodać DRAFT: fazy ataku.**
- `docs/balance/BASELINE_VALUES.md` — **zero zmian liczb balansowych.**
- `docs/03_MVP_SCOPE.md` — nie dodawać contentu.

## Mapowanie broni → rodzina

| `weaponId` | Rodzina `WeaponFamily` |
|------------|------------------------|
| `miecz`, `pika`, `knights_edge` | Sword |
| `sztylet`, `viper_fang` | Dagger |
| `topor`, `mlot` | Axe |
| `luk` | Bow |

`pika` zostaje Sword (dłuższy zasięg już jest w melee profile). Nie wymyślaj piątej rodziny.

## Parametry DRAFT (użyj dokładnie tych — nie zgaduj)

Fazy to **ułamki `AttackInterval`**. Suma windup+active+recovery = 1.
`AttackInterval` / dmg / range / arc / maxTargets zostają jak dziś.

| Rodzina | Windup | Active | Recovery | Move mul (windup+active) | Hit-stop (s) | Knockback | Shake |
|---------|--------|--------|----------|--------------------------|--------------|-----------|-------|
| Dagger | 0.08 | 0.12 | 0.80 | 1.00 | 0.00 | 0.15 | 0.00 |
| Sword | 0.18 | 0.12 | 0.70 | 0.55 | 0.04 | 0.60 | 0.04 |
| Axe | 0.38 | 0.10 | 0.52 | 0.25 | 0.10 | 1.80 | 0.12 |
| Bow | 0.28 | 0.00 | 0.72 | 0.40 | 0.03 | 0.20 | 0.05 |

Bow: pocisk spawnuje się **na końcu windupu**. `active = 0`. Speed pocisku: **20** jednostek/s. Max dystans = `Range` z profilu (nie zmieniaj Range). Jeden cel, niszczy się przy hicie albo po zasięgu.

Facing (Combat Readability, **nadpisane** w Combat Playability): dawniej lock od windupu do końca recovery.
Aktualnie: lock **tylko w Active**; windup śledzi aim. Szczegóły: [`COMBAT_PLAYABILITY_PLAN.md`](COMBAT_PLAYABILITY_PLAN.md).

## Kontrakt ataku (obowiązkowy)

```
Idle --(input, cooldown ready)--> Windup --> Active (melee overlap) | Bow: spawn pocisku na końcu Windup
     --> Recovery --> Idle
```

- Obrażenia melee **tylko w Active**, nigdy w klatce inputu.
- Ponowny atak dopiero po pełnym cyklu (= `AttackInterval` po starcie, czyli po Recovery).
- `PlayerMeleeAttack.PerformAttack()` w obecnej postaci (hit w tej samej klatce co input) **znika**.

## Hit-stop vs pauza (pułapka)

`GameFlowManager` ustawia `Time.timeScale = 0` na level-up. Hit-stop **nie może** tego zepsuć.

Wymagania:
- `CombatFeelService` (lub równoważny) — jeden owner hit-stop + shake.
- Hit-stop obniża `timeScale` **tylko gdy aktualny scale == 1** (gra nie jest spauzowana).
- Restore: jeśli w międzyczasie weszła pauza level-up → **zostaw 0**, nie wracaj do 1.
- Stackowane hity: jeden timer, bierz max pozostałego czasu, nie zagnieżdżaj ślepo.

Nie wolno sypać `Time.timeScale` po `PlayerMeleeAttack` / pocisku bezpośrednio.

## Placeholdery wizualne (zero FBX / klipów)

- Broń = child primitive przy kapsule (bok/przód). Inna **sylwetka i skala** per rodzina. Kolor czytelny, nie zasłania walki.
- Melee: w Windup słaby łuk/strefa; w Active jaśniejszy flash. **Żółta kula `AtakPlaceholder` do usunięcia.**
- Bow: kula/capsule lecąca do przodu.
- Trafienie: istniejący `HitFlashFeedback` zostaje; dodaj knockback + hit-stop + shake wg tabeli.
- Animator: **opcjonalny**. Jeśli brak `Animator` na graczu — proceduralny zamach childa broni (rotacja w windup→active). Jeśli jest — trigger `Attack`, int `WeaponFamily`, float `Speed`, bool `Dead`. **Nie importuj Mixamo, nie twórz AnimationClipów.**

## Pliki — wolno ruszać

**Nowe (propozycja nazw — trzymaj się tej siatki, nie rób 20 klas):**
- `Assets/_Game/Scripts/Combat/WeaponFamily.cs` — enum
- `Assets/_Game/Scripts/Combat/WeaponFeelProfile.cs` — SO albo serializowany blok na `WeaponDefinition`
- `Assets/_Game/Scripts/Combat/PlayerAttackController.cs` — state machine (zastępuje logikę timingową `PlayerMeleeAttack`)
- `Assets/_Game/Scripts/Combat/CombatFeelService.cs` — hit-stop + shake request
- `Assets/_Game/Scripts/Combat/WeaponPlaceholderView.cs` — mesh broni + procedural swing + łuk ataku
- `Assets/_Game/Scripts/Combat/Projectile.cs` — lot, hit, destroy
- `Assets/_Game/Scripts/Combat/KnockbackReceiver.cs` — na wrogach
- `Assets/_Game/Tests/EditMode/AttackTimingTests.cs`
- `Assets/_Game/Tests/EditMode/CombatFeelServiceTests.cs` (timeScale vs pauza — jeśli da się bez PlayMode; w przeciwnym razie logika czystych funkcji wyciągnięta ze service)

**Istniejące (minimalny diff):**
- `WeaponDefinition.cs` — rodzina + feel (lub referencja do profilu)
- `BuildContentFactory.cs` — przypisz rodziny; **nie zmieniaj** dmg/interval/range/arc/targets
- `PlayerBuildState.cs` — podłącz nowy attack controller zamiast gołego melee
- `PlayerMeleeAttack.cs` — albo cienki wrapper overlap w Active, albo wchłoń do `PlayerAttackController` i usuń stary instant-hit
- `PlayerCharacter.cs` — mnożnik ruchu z ataku; lock facing (małe API, nie przepisuj inputu)
- `PlayerWeaponController.cs` — queue swap gdy atak w toku
- `EnemyController.cs` — honoruj knockback
- `CombatBootstrap.cs` — dodaj view/feel komponenty przy spawnie
- `SharedCamera.cs` — offset shake w `LateUpdate` (nie psuj follow)
- `PlayerRespawn.cs` — na śmierci cancel ataku / schowaj swing

**Nie ruszaj bez zgody Leada:** `GameFlowManager`, talenty, loot, fale, UI buildów, `BASELINE_VALUES.md`, import assetów, nowe pakiety Unity.

---

## Taski — **jeden na raz**, mały commit per task

Kolejność sztywna. Nie łącz T3+T4 „bo i tak ruszam attack”.

### CR-T1 — Dane: `WeaponFamily` + feel profile
- Enum + dane DRAFT z tabeli (SO albo pola na `WeaponDefinition`).
- `BuildContentFactory` mapuje 8 broni wg tabeli mapowania.
- Docs: dopisz **DRAFT parametry** na końcu `GAME_FEEL.md` (nie edytuj zasad FROZEN). Krótka notka DRAFT w `COMBAT.md`: atak = windup/active/recovery.
- **AC:** `catalog.GetWeaponById("topor").Family == Axe`; `luk` == Bow; `pika` == Sword. Interval topora nadal 1.6.
- **Test:** EditMode — mapowanie id→family; suma frakcji faz = 1 dla każdej rodziny.
- **Poza:** żaden gameplay timing jeszcze nie działa inaczej.

### CR-T2 — State machine ataku melee
- `PlayerAttackController`: Idle/Windup/Active/Recovery.
- Melee overlap (obecna logika sfery+łuku) **tylko w Active**.
- Cooldown = `AttackInterval` od startu cyklu.
- Śmierć (`Health.Died` / `SetCombatEnabled(false)`) → cancel do Idle, brak dmg.
- Facing lock na czas cyklu.
- `PlayerCharacter`: `AttackMoveMultiplier` (1 poza atakiem).
- **AC:** miecz nie zadaje dmg w klatce inputu; dmg w Active; sztylet vs topór — inny delay do hitu (widać w logice czasu). Swap w toku nie zmienia rodziny w środku zamachu (albo jest zablokowany — pełny queue w T5, tu minimum: ignoruj swap w trakcie cyklu).
- **Test:** EditMode na czystej funkcji faz (czas windup = interval * frac). Jeśli overlap wymaga sceny — wyciągnij `MeleeHitResolver` bez MonoBehaviour Update i przetestuj kąt/zasięg jak dziś.
- **Regresja:** wrogowie nadal giną, HP/respawn/loot nietknięte.

### CR-T3 — Combat feel: hit-stop, knockback, shake
- `CombatFeelService` + `KnockbackReceiver`.
- Po udanym hicie melee: flash (istniejący) + tabela feel aktywnej rodziny.
- Shake: krótki offset na `SharedCamera`, łuk mały, topór większy, sztylet zero.
- **AC:** topór odpycha i hit-stopuje wyraźniej niż sztylet; pauza level-up nadal działa (timeScale 0 zostaje 0).
- **Test:** restore timeScale nie odpauzowuje `GameFlowManager` (symuluj: scale 0 → hit-stop end → nadal 0).
- **Poza:** rumble pada, SFX, cząsteczki (oprócz prostych primitów z T4).

### CR-T4 — Placeholder broni + łuk ataku (wywal żółtą kulę)
- `WeaponPlaceholderView` na spawnie gracza; rebuild przy `BuildChanged` / zmianie aktywnej broni.
- Sylwetki: Dagger mały, Sword dłuższy, Axe szerszy/cięższy, Bow inny kształt (nie ten sam box co miecz).
- Windup: słaba strefa; Active: mocniejsza; po Active znika.
- Usuń `AtakPlaceholder` sphere z `PlayerMeleeAttack`.
- Procedural swing childa **albo** Animator trigger jeśli komponent jest — nie wymagaj clipów.
- **AC:** 4 rodziny rozróżnialne bez HUD i bez logów; checklista Walka: start ataku widać przed dmg.
- **Poza:** modele low-poly, tekstury, ikony HUD.

### CR-T5 — Swap w kolejce + ruch wg D5
- `PlayerWeaponController`: jeśli cykl trwa → zapamiętaj żądanie, wykonaj na Idle.
- Move mul z tabeli tylko w Windup+Active; Recovery wraca do 1 (chyba że recovery axe ma jeszcze 0.25 — **nie**, tylko windup+active).
- **AC:** E w trakcie zamachu topora nie zmienia broni w połowie; po Recovery zmienia. Sztylet nie tnie speedu.
- **Test:** EditMode kolejki swap (stan ataku mock/stub).

### CR-T6 — Łuk: pocisk
- `luk` nie używa overlap melee.
- Spawn na końcu Windup, kierunek = locked facing, speed 20, max dist = Range, 1 cel, `HitFlash` + feel Bow.
- Warstwa/kolizja: te same wrogowie co melee (`enemyLayers`). Nie trafia sojuszników / bazy / loot cubów.
- **AC:** Cipak/Jamie z łukiem: widać lot; dmg w momencie trafienia (lub zniknięcia na zasięgu bez dmg). Miecz nadal melee.
- **Test:** pocisk niszczy się po Range; dmg tylko `IDamageable` wroga.
- **Poza:** przebicie, pocisk grawitacja, charge bow, kusza jako osobny typ (kuszy nie ma w slice).

### CR-T7 — Docs + handoff Verifier
- `GAME_FEEL.md` / `COMBAT.md` — DRAFT zsynchronizowane z kodem.
- Krótka notka w tym pliku: status **ZIMPLEMENTOWANY**, jak playtestować.
- Handoff Verifiera na dole.

**AC całego passa** — patrz niżej. CR-T7 nie dodaje featurów.

---

## Acceptance criteria całego passa

1. Atak melee ma windup → hit → recovery; dmg nie pada w klatce inputu.
2. Sztylet / miecz / topór różnią się delayem hitu, ruchem w zamachu i siłą feel (stop/knockback/shake).
3. Łuk to pocisk z czytelnym lotem; dmg przy trafieniu.
4. Żółta kula ataku usunięta; widać start ataku (strefa windup).
5. Broń-placeholder zmienia sylwetkę przy swapie (po zakończeniu cyklu).
6. Swap w trakcie ataku nie psuje cyklu (kolejka).
7. Śmierć canceluje atak.
8. Level-up pause (`timeScale = 0`) nie jest odpauzowywany przez hit-stop.
9. Brak zmian dmg/interval/range w `BuildContentFactory` vs przed passam.
10. EditMode testy nowych reguł zielone; Unity Console bez Error przy ręcznym odpaleniu.
11. Checklist Walka: do playtestu PO (Verifier: **NEEDS PLAYTEST**, nie PASS z kodu).

## Poza zakresem (explicit — nie implementuj)

- Modele, Mixamo, AnimationClipy, VFX Graph, Audio
- Visual slice M4 (1 klasa low-poly, 3 docelowe bronie)
- Unique rare (trucizna, fale) — DEF-03, M7
- Status system — DEF-05
- Rumble pada, SFX
- Kusza, charge, pierce
- Zmiana balansu / DPS
- Polish HUD (DEF-14)
- Przepisanie `GameFlowManager`

## Zależności

```
CR-T1 → CR-T2 → CR-T3
              → CR-T4
       CR-T2 → CR-T5
       CR-T2 → CR-T6
CR-T3 + T4 + T5 + T6 → CR-T7 → Verifier
```

T3 i T4 można po T2; **nie** startuj T6 zanim T2 nie ma faz (łuk potrzebuje Windup).

## Handoff — Implementer

**Wejście:** ten dokument. Rób **CR-T1**, potem stop i kolejny task. Nie ciągnij całego passa w jednym PR/commitcie jeśli da się podzielić.

**Wyjście per task:** kod + testy + 5–10 zdań: co ruszone, jak kliknąć w Play, co świadomie pominięte.

**Eskalacja:** po **jednej** nieudanej rundzie poprawek zgłoś Lead/PO. Nie dokładaj „przy okazji” Animator Controllerów ze stockowych paczek.

## Handoff — Verifier (po T7)

Sprawdź AC 1–10 w kodzie + Console. AC 11 i game feel = **NEEDS PLAYTEST** (PO).

**PO playtest (2026-09-08): PASS.** Checklist Walka (ten pass) zatwierdzona przez PO.

Playtest PO (minimum):
1. Jamie + miecz: widać zamach zanim mob flashnie.
2. Cwel + sztylet: szybciej, słabszy feel.
3. Pudzian + topór: długi windup, mocniejszy hit, odrzut.
4. Cipak + łuk: widać pocisk.
5. Level-up w trakcie walki: pauza nie psuje się po hicie.
6. Swap E w połowie topora: broń zmienia się dopiero po zamachu.

## Implementacja (CR-T1…T7)

Kod: `AttackCycle`, `PlayerAttackController`, `WeaponPlaceholderView`, `Projectile`, `CombatFeelService`.
Testy EditMode: `AttackTimingTests`, `CombatFeelServiceTests`.

**Jak playtestować:** Play BootScene → dołącz graczy → atakuj Space/LMB. Jamie miecz (windup), Cwel sztylet (szybki), Pudzian topór (długi zamach + odrzut), Cipak łuk (pocisk). E w trakcie topora nie zmienia broni w połowie. Level-up pauza nie może wrócić do 1 przez hit-stop.
