# M7.6 — Pionowy slice Pudziana — Plan

## Status
**DONE** — PO playtest PASS 2026-09-09. Kit Pudziana (AA + 3 aktywne + talent Skok) grywalny na placeholderach. Animacje/widoki = M9.

To **nie** jest M8. To pierwsza kompletna postać nowego kitu (wzorzec dla Cwel / Cipak / Jamie).

To **nie** jest M8. To pierwsza kompletna postać nowego kitu (wzorzec dla Cwel / Cipak / Jamie).

## Cel milestone'u
Pudzian od poziomu 1 ma atak podstawowy + **3 aktywne**: Trzaśnięcie, NO CHODŹ TU, Byk. Talent może jakościowo zmienić Trzaśnięcie w **Skok z Pierdolnięciem** (stomp na lądowaniu). Wspólna fizyka M7.5 zostaje; brakujące tryby (leap, taunt, charge+shove, kolizje, multi-skill) wchodzą tutaj.

## Decyzje PO (zablokowane)

| ID | Decyzja | Wybór |
|----|---------|--------|
| E1 | Trzaśnięcie vs Skok | Lvl 1 = **Trzaśnięcie** (M7.5). Skok = talent: leap + ten sam stomp w punkcie lądowania. |
| E2 | Start vs playtest M7.5 | M7.5 **DONE** — PO playtest PASS (2026-09-09). M7.6 startuje. |
| E3 | Taunt vs korytarz Grunta | Grunt **nie** porzuca linii. Taunt zmienia cel w korytarzu. Hunter może zejść. Rusher/Siege schodzą ze struktury, ale idą ścieżką swojej linii. |
| D1 | Kolejność | Jak M7.5: fizyka P0 → multi-skill/HUD → content skilli 2–3 → karty debug. |
| D2 | Atak podstawowy | Zostaje `PlayerAttackController` (topór). Bez rewrite Combat Playability. |
| D3 | Input 2–3 | DRAFT: KBM E / R, pad North / Left Shoulder. Nie FROZEN. |
| D4 | Talenty w wycinku | 2–3 karty debug (Pęknięcie + Skok z Pierdolnięciem + 1 inna). Nie drzewko, nie 4 ekrany HotS. |

## FROZEN — nie naruszać

- `00`–`03`, `SKILLS.md` (zasady), `LOCAL_COOP`, Combat Playability, M6.5 linie/kamera.
- `PLAYER_PROGRESSION` — 4 decyzje, pauza; struktura kart OPEN.
- `WAVES_AND_ENEMIES` — Grunt zostaje na linii (E3 to honoruje, nie wyjątek).
- `VISUAL_FEEDBACK_CHECKLIST` — zapowiedź obszaru przed trafieniem, 3 CD czytelne.
- Loot PARKED.

## Kit lvl 1

| Slot | Umiejętność | Mini-spec |
|------|-------------|-----------|
| Atak | Ciężki zamach | `docs/content/skills/pudzian/ciezki_zamach.md` |
| 1 | Trzaśnięcie | `docs/content/skills/pudzian/trzasniecie.md` (bez zmian baseline) |
| 2 | NO CHODŹ TU | `docs/content/skills/pudzian/no_chodz_tu.md` |
| 3 | Byk | `docs/content/skills/pudzian/byk.md` |

Talent Skok: sekcja w `trzasniecie.md` (karta **Skok z Pierdolnięciem**).

## Architektura — braki P0 vs P1

M7.5 ma enumy trybów; runtime jest jednoskillowy, jeden kształt (koło), `ApplyShove` ignoruje duration, `HonorCollisions` nieczytane, Charge bez stopu, brak tauntu i odporności castera.

| Priorytet | Brak | Blokuje |
|-----------|------|---------|
| P0.1 | Stepper kolizji (`HonorCollisions` + `MapPlayArea`) | Byk, Skok-talent |
| P0.2 | Shove ze stałą prędkością przez czas | Byk |
| P0.3 | Charge castera + `StopReason` (boss / struktura / krawędź) | Byk |
| P0.5 | Threat / taunt (osobna warstwa, nie ForcedMovement) | NO CHODŹ TU |
| P0.6 | Immunity castera KB+stagger + receiver na graczu | NO CHODŹ TU |
| P0.7 | Multi-skill: 3 SO, 3 CD, 1 cast naraz, aim/shape | Skill 2–3 |
| P1 | Leap do punktu | karta Skok |
| P1 | Telegraph punktu / linii; HUD 3; telemetria per-skill | content |
| P2 | Launch Y, collidery ścian, migracja `KnockbackReceiver`, AA → SkillDefinition | nie ten slice |

## Taski

### M76-T1 — Fizyka P0 (Byk + immunity)

- **Zakres:** P0.1–P0.3, P0.6. Stepper, shove duration, charge outcome, immunity KB+stagger na casterze, `ForcedMovementReceiver` na graczu. **Bez** leap executora (P1). **Bez** tauntu (T2). **Bez** podpinania skilli 2–3.
- **Docs:** `SKILLS.md` warstwa wymuszonego ruchu; ten plan.
- **AC:**
  - EditMode: shove trzyma stałą prędkość przez `duration`, potem 0 (nie damping w oknie shove).
  - EditMode: elite shove × `EliteDisplacementScale` na **speed**.
  - EditMode: charge stop na obiekcie z profilu Boss i Structure; boss 0 displacement.
  - EditMode: `HonorCollisions=true` nie wychodzi poza `MapPlayArea`.
  - EditMode: immunity na graczu blokuje knockback; stun nadal przechodzi.
  - Regresja: istniejące testy ForcedMovement (elite/boss stun/knockback) PASS.
  - Żaden skill nie rusza `transform` wroga poza stepperem.
- **Handoff:** Implementer `composer-2.5-fast`.

### M76-T2 — Threat / taunt

- **Zakres:** P0.5. Override aggro. Boss ignoruje. Last-write-wins. **E3:** Grunt zostaje w korytarzu; Hunter może zejść; Rusher/Siege porzucają strukturę, path = linia.
- **AC:** EditMode: Grunt w radiusie i w korytarzu zmienia cel na tauntera; Grunt poza korytarzem nie schodzi z linii; Rusher przestaje bić wieżę na duration; Hunter lock nadpisany; BossRam charge target bez zmiany; po wygaśnięciu Rusher wraca do struktury.
- **Zależności:** T1 immunity równolegle OK.
- **Handoff:** Implementer.

### M76-T3 — Multi-skill + input DRAFT + HUD

- **Zakres:** 3 sloty, input Q/E/R i East/North/LB, HUD 3 radial. `INPUT_AND_CONTROLLERS.md` zostaje DRAFT (nie oznaczać FROZEN).
- **AC:** 3 definicje, 3 CD, 1 cast naraz, bufor z AA recovery per slot, pauza awansu pauzuje wszystkie CD, śmierć w windup nie startuje CD tego slota. HUD czytelny bez konsoli.
- **Handoff:** Implementer.

### M76-T4 — Atak podstawowy (lock)

- **Zakres:** Mini-spec już w docs. Zero zmiany dmg/interval/arc. Opcjonalnie: AA knockback wprost do resolvera (dług, nie brama).
- **AC:** Topór 22 / 1,6 / 140° / 6 celów. Aim/lock bez regresji.

### M76-T5 — Trzaśnięcie zostaje na slocie 1

- **Zakres:** Brak swapu na Skok. Q / East nadal stomp. Pęknięcie nadal na tym skillu.
- **AC:** Regresja M7.5: koło r=3, 16 dmg, zachwianie 1,2 s, 0 KB, CD 8 s.

### M76-T6 — NO CHODŹ TU

- **Zakres:** Skill 2 + znaczniki tauntu + outline betonu. Pudło bez immunity. 0 dmg.
- **AC:** r=6, 3,5 s, Rusher schodzi z wieży (ścieżką linii), Ram nie, HUD CD #2, E3 honorowane.
- **Zależności:** T2, T3.

### M76-T7 — Byk

- **Zakres:** Composite charge+shove, dmg 12 raz na cel na cast, telegraph linii, stop reasons.
- **AC:** Zwykli jadą przed Pudzianem; elita wolniej; boss stop + 0 move; wieża stop + 0 dmg; early stop → recovery + CD.
- **Zależności:** T1, T3.

### M76-T8 — Telemetria per-skill

- **Zakres:** Liczniki per `skillId`; overlay 3 skilli.
- **AC:** Quit log pokazuje 3 bloki.

### M76-T9 — Karty debug (2–3)

- **Zakres:** Pęknięcie (zostaje) + **Skok z Pierdolnięciem** (leap 0–6 m, stomp na landzie, 0 KB) + opcjonalnie 1 z {Echo, Tłok, Szeroki klin}. Nie drzewko.
- **Zależności:** T5 + leap P1 (może wejść tu, nie w T1). T6/T7 jeśli karta ich dotyczy.

### M76-T10 — Verifier (`composer-2.5-fast`)

- BootScene: Pudzian, 3 skille, fala 1–2, Ram. Console 0 Errors.
- Checklista: telegraph przed hitem; 3 CD bez konsoli.
- Regresja: aim lock AA, linie, respawn, loot off, join, Trzaśnięcie na Q.

### M76-T11 — Playtest PO

Checklista na końcu tego pliku. Game feel ≠ PASS z kodu.

**Wynik T11 (2026-09-09):** pierwszy run FAIL (mapa, leap, charge NRE, Ram glue). Playtest-fix + Ram nie szarżuje martwej bazy. **Drugi run: PO PASS** („fajnie działa”).

## Kolejność

```
T1 (fizyka) → T2 (taunt) → T3 (multi-skill + HUD)
T4 (AA docs/lock) równolegle z T1
T5 (regresja stompu) po T3
T6 po T2+T3
T7 po T1+T3
T8 po T5–T7
T9 (Skok talent + leap) po T5; leap P1 w tym tasku
T10 Verifier → T11 PO playtest
```

**Nie** T6 i T7 w jednym PR.

## Poza zakresem M7.6

- Kity Cwel / Cipak / Jamie
- Drzewko 12 kart / decyzja HotS vs A/B
- Fale 6–10, Boss 2, win/lose
- Balance Agent, zmiana baseline topora
- Launch Y, Mixamo, rewrite AA → SkillDefinition
- I-frame Skoku, stun-immune baseline na tauncie
- Przywrócenie lootu

## Playtest PO — minimum

1. AA: 140° czytelne; grunt fali 1 ginie od ~1 zamachu (22 vs 18).
2. Trzaśnięcie: jak M7.5 (koło, clump staje, pudło pali 8 s).
3. NO CHODŹ TU: znaczniki; Rusher odchodzi od wieży ~3,5 s ścieżką linii; po wygaśnięciu wraca.
4. Taunt pudło: CD 12 s, brak betonu.
5. Grunt na innej linii w kole **nie** zbiega przez mapę (E3).
6. Ram: taunt nie zgina szarży; Byk/stomp nie przesuwają bossa.
7. Byk: 3+ gruntów jedzie przed Pudzianem; elita zostaje w tyle; Ram stopuje szarżę.
8. Byk w wieżę: stop, wieża bez dmg.
9. Kit tank: Pudzian nie czyści fali samymi skillami; AA dobija.
10. CD HUD: 3 ikony, Q/E/R albo East/North/LB.
11. Loot: brak broni na ziemi po Ramie.
12. (Jeśli T9) Skok: kursor → koło na landzie → stomp tam; cel w stopy = stary stomp.

## Role

- **Lead:** ten plan; nie koduje szeroko.
- **Implementer:** T1–T9, małe commity, testy przy T.
- **Verifier:** T10 (`composer-2.5-fast`).
- **Balance Agent:** nie w M7.6.
- **PO:** zatwierdza po playteście kitu, nie na podstawie kodu.
