# M8.1 — Framework progresji (kit + talenty)

## Status
**DONE** — PO playtest PASS 2026-09-09. Framework 4 decyzji + content Pudziana L2–L5 grywalny na placeholderach. Ulepszone skille/ulti/capstone’y = placeholdery jakościowe (liczby i feeling = później). Animacje/UI kart = M9.

## Cel
Generyczny, data-driven system 4 decyzji (lvl 2–5): eligibility, zmienna oferta, mutacje skilli, ulti jako `GrantSkill`. Wzorzec contentu: Pudzian. Nie hardkodować klasą.

## FROZEN
Pętla: wspólny EXP, max. 5, 4 decyzje, pauza, full HP, własny wybór, auto-staty.
Struktura kart: `docs/systems/PLAYER_PROGRESSION.md` (lock PO).
Zasady skilli: `docs/systems/SKILLS.md`.
Loot PARKED. Balance Agent / wildcardy / meta-konto: poza zakresem.

## Poza zakresem
- M8.0 win/lose, fale 6–10, Boss 2
- Cwel / Cipak / Jamie / Bomberman (framework ma ich obsłużyć później)
- Wildcardy, Balance Agent, unlocki konta

## Taski (skrót)

| ID | Zakres |
|---|---|
| T1 | TalentRequirement + Eligibility + snapshot + meta query |
| T2 | ClassProgressionTable + OfferGenerator |
| T3 | Apply; Skok/Pęknięcie bez `SkillModifierId` enum-switch |
| T4 | Loadout slot ulti; UI N kart; input 1..N |
| T5 | PersistentHost + IDeathGuard (paleta haków) |
| T6 | Content Pudzian L2–L5 (placeholdery jakościowe) |
| T7 | Odpiąć drzewko A/B od flow level-upu |

Verifier: 15 AC PASS (smoke MCP). **Playtest PO PASS 2026-09-09** — pętla i oferty OK; placeholdery skilli świadomie nie komentowane.
