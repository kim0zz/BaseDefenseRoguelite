# Combat Playability Pass — Plan

## Status
**DONE** — PO playtest PASS (2026-09-08). Kod + Verifier PASS + playtest. M7 odblokowany (nie startuje sam).

To **nie jest M7**, visual slice M4 ani polish M9.
To korekta feelu po Combat Readability: da się celować i wiedzieć, ile bijesz.

## Decyzje PO (zablokowane)

| ID | Decyzja | Wybór |
|----|---------|--------|
| D1 | Kolejność | Combat Playability **teraz**. M7 dopiero po playteście PASS. |
| D2 | Celowanie | P1: mysz → punkt na płaszczyźnie XZ. Pad: prawy stick = look. Atak w look. Brak aim inputu → facing z ruchu. |
| D3 | Lock facing | **Tylko Active** (okno hitu). Windup śledzi aim. Recovery: pełny obrót. |

HUD: aktywna broń pokazuje **efektywne dmg i tempo** (po talentach/itemach). Bez character sheetu.
Łuk ataku: większy kontrast windup vs active. Zero FBX.

## FROZEN — nie naruszać

- `GAME_FEEL.md` — zasady (responsywność na placeholderach). **Wolno dopisać DRAFT** (aim / lock).
- `INPUT_AND_CONTROLLERS.md` — FROZEN: kto czym steruje. Mapowanie przycisków jest OPEN; mysz/prawy stick = look.
- `UI_UX.md` — lista informacji FROZEN; dmg/tempo jako treść „aktualne bronie” (DRAFT layout).
- `BASELINE_VALUES.md` / `BuildContentFactory` — **zero zmian** dmg / interval / range.
- `03_MVP_SCOPE.md` — bez nowego contentu.
- Combat Readability: fazy windup/active/recovery zostają. Zmienia się tylko lock i źródło kierunku.

## Kontrakt

```
Aim (mysz / prawy stick / fallback ruchu) aktualizuje facing, gdy faza != Active.
Windup: łuk i hit-facing śledzą AimDirection.
Active: facing zamrożony; melee overlap z tego kierunku.
Recovery / Idle: lock zdjęty.
Łuk: pocisk na końcu windupu w bieżącym AimDirection (active = 0 → nigdy nie lockuje).
```

## Poza zakresem

- M7 (boss, fale 1–5, statusy, co-op level-up)
- Visual slice M4, Mixamo, SFX, rumble
- Auto-aim na najbliższego wroga
- Zmiana DPS / interwałów / recovery fraction
- Pełny polish HUD (DEF-14)

## Acceptance criteria

1. P1 KBM: postać patrzy tam, gdzie kursor na ziemi; atak melee/łuk idzie w ten kierunek.
2. Pad: prawy stick obraca postać niezależnie od lewego (ruch). Martwa strefa sticka → facing z chodu.
3. W windupie można skorygować cel; w Active kierunek hitu się nie zmienia.
4. W recovery postać od razu się obraca (nie czeka na koniec cyklu).
5. HUD: widać efektywną broń jako `Nazwa X dmg / Y.YYs` (po buildzie).
6. Łuk zamachu: windup ≠ active kolorem; w Idle krótki wskaźnik aim.
7. Brak zmian liczb w `BuildContentFactory` vs przed passam.
8. EditMode testy aim/lock/HUD zielone; Console bez Error przy Play.

Feel = **PASS** — PO zatwierdził (2026-09-08).

## Playtest PO (minimum)

1. Jamie + mysz: celuj w moba obok, idź w inną stronę — hit idzie w kursor.
2. Wciśnij atak, w windupie zjedź myszą na innego moba — hit (lub pocisk) idzie w nowy kierunek.
3. Pad: lewy stick chód, prawy stick obrót, atak w look.
4. Topór: po hicie (recovery) od razu można się obrócić.
5. HUD pokazuje dmg/tempo; talent +dmg zmienia liczbę.

## Implementacja

Kod: `AimMath`, `AttackFacingPolicy`, `PlayerCharacter` (mysz/prawy stick), `PlayerAttackController` (lock tylko Active), `WeaponPlaceholderView` (cyjan windup / żółty active / tick aim), `CombatHudCopy`.
Testy: `AimMathTests`, `CombatHudCopyTests`, rozszerzony `AttackTimingTests`.

**Jak playtestować:** Play BootScene → dołącz P1. Mysz nad ziemią obraca postać; WASD chodzi niezależnie. Atak Space/LMB w kierunku kursora. W windupie zjedź myszą — hit idzie w nowy cel. Po hicie (recovery) od razu można się obrócić. Pad: lewy stick chód, prawy stick look. HUD: `Miecz 10 dmg / 0.80s`.
