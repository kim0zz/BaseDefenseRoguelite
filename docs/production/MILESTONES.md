# Milestones

## M0 — Project boots
Unity odpala pustą scenę i build.

## M1 — 4 players moving
4 pady, 4 postacie, wspólna kamera.

## M2 — Base defense greybox
3 linie, baza, wieże, ruch po mapie.

## M3 — Combat prototype
gracz atakuje, moby giną, gracze mogą umrzeć i wrócić.

## M4 — First playable wave
fala 1 działa od początku do końca.

## M5 — Core loop
fale + przerwy + kasa + naprawa bazy + EXP.

## M6 — Build system (historyczny)
Klasy, 2 sloty broni, 6 itemów, loot, drzewko talentów — vertical slice **zrobiony**.
Po pivocie 2026-09-09 loot/ekwipunek są PARKED. Tożsamość przechodzi na kit w M7.5.

**Plan:** [`M6_PLAN.md`](M6_PLAN.md)

## Combat Readability Pass (między M6 a M7)
Timing ataku (przygotowanie / faza aktywna / wykończenie), placeholdery, pocisk łuku, hit-stop/odrzu/wstrząs.
**DONE** — PO playtest PASS (2026-09-08). **Plan:** [`COMBAT_READABILITY_PLAN.md`](COMBAT_READABILITY_PLAN.md)

## M6.5 — Core lane / crowd / camera (przed M7)
Separacja wrogów, ruch po 3 liniach, kamera 1–4.
**DONE** — PO playtest PASS (2026-09-08). **Plan:** [`M65_STABILIZATION_PLAN.md`](M65_STABILIZATION_PLAN.md)

## Combat Playability Pass (przed M7)
Celowanie (mysz / prawy stick), lock kierunku tylko w fazie aktywnej, HUD dmg/tempo.
**DONE** — PO playtest PASS (2026-09-08). **Plan:** [`COMBAT_PLAYABILITY_PLAN.md`](COMBAT_PLAYABILITY_PLAN.md)

## M7 — Mid-run game
fale 1–5 + Boss 1.
**Plan:** [`M7_PLAN.md`](M7_PLAN.md)
**Status:** kod + Verifier Console PASS; **NEEDS PLAYTEST** PO (Ram + co-op level-up). Unique drop broni — PARKED (kryterium playtestu bez pickup). Nie DONE.

## M7.5 — Kit Foundation
Wspólna fizyka skilli, Pudzian + Trzaśnięcie, loot poza pętlą, HUD CD, jeden modyfikator, telemetria.
**Plan:** [`M75_PLAN.md`](M75_PLAN.md)
**DONE** — PO playtest PASS (2026-09-09).

## M7.6 — Pionowy slice Pudziana
Pełny kit 1 klasy: AA + Trzaśnięcie + NO CHODŹ TU + Byk. Skok z Pierdolnięciem = talent (stomp na lądowaniu).
**Plan:** [`M76_PLAN.md`](M76_PLAN.md)
**DONE** — PO playtest PASS (2026-09-09). Animacje = M9.

## M8 — Full MVP
10 fal + 2 bossów + win/lose + 4 kity. **Nie** pula broni. Talenty/framework = już M8.1/M8.1b.
**Plan:** [`M8_PLAN.md`](M8_PLAN.md) (v1, 2026-09-09).

Plastry:
- **M8.0** win/lose (FAIL) — **DONE** — PO playtest PASS (2026-09-11). Plan: [`M8_PLAN.md`](M8_PLAN.md) § M8.0.
- **M8.2** Support / Flanker / Shielder — **DONE** (kod + Verifier PASS 2026-09-11; playtest PO razem z M8.3).
- **M8.3** fale 6–10 + Warden + wygrana — kod + Verifier PASS (2026-09-11); **NEEDS PLAYTEST** PO (feel bossa). Plan: [`M8_PLAN.md`](M8_PLAN.md)
- **M8.4–M8.6** Jamie → Cipak → Cwel (wzorzec Pudziana)

## M8.1 — Framework progresji
**DONE** — PO PASS (2026-09-09). [`M81_PROGRESSION_PLAN.md`](M81_PROGRESSION_PLAN.md).

## M8.1b — Pudzian kompletny
**DONE** — PO PASS (2026-09-09). [`M81B_PUDZIAN_COMPLETE_PLAN.md`](M81B_PUDZIAN_COMPLETE_PLAN.md). Karty „co się ładuje” → M9.0.

## M9 — Playtest-ready
Czytelność, feedback skilli, balans po 10 falach, co-op 2–4P. **Nie** nowy content fal/klas (to M8).
**Plan:** [`M9_PLAN.md`](M9_PLAN.md).

- **M9.0** karty/HUD — **DONE** (kod + Verifier PASS 2026-09-11; feel = NEEDS PLAYTEST PO). Plan: [`M9_PLAN.md`](M9_PLAN.md) § M9.0.
- **M9.1** telegraph/VFX/SFX — **IN PROGRESS** a+b (PO 2026-09-11), sloty pod podmianę.
- **M9.2** Balance Agent v0.2 (po M8.3)
- **M9.3** co-op 2–4P + stabilność

Visual slice M4 nadal PARKED — nie jest M9.

## Visual / UX integration
- M0–M2: funkcjonalne placeholdery UI i assetów.
- M3: pierwszy game-feel pass (placeholder flash; pełny timing → Combat Readability Pass).
- M4: visual slice w docelowym low-poly (nadal odłożony; readability ≠ slice).
- M5–M6: pierwszy HUD / level-up / panel bazy (inventory historyczne).
- M7.5: HUD umiejętności (1 skill).
- M7.6: HUD 3 umiejętności Pudziana.
- M7–M8: zapowiedzi bossów i czytelność.
- M9: polish UI, animacji, VFX, SFX i feedbacku — wg [`M9_PLAN.md`](M9_PLAN.md) (M9.0 karty wcześniej).
