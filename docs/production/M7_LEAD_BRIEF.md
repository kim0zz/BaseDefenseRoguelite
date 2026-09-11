# M7 — opinia Leada (podsumowanie dla PO)

Data: 2026-09-08.  
Pełny plan tasków: [`M7_PLAN.md`](M7_PLAN.md).

## Werdykt (aktualny)

M7 **jest spójne z kierunkiem MVP** (mid-run: fale 1–5 + Boss 1).  
Start **przed** Combat Playability byłby błędem — playability jest **DONE** (PO PASS), więc M7 jest właściwym następnym milestone’em.

Kod T1–T9 jest w repo. Zamknięcie milestone’u = **M7-T10 Verifier + Twój playtest** (szarża Rama, interrupt, co-op level-up). Lead **nie** zatwierdza milestone’u.

## Co jest M7, a co nie

Obowiązkowe z backlogu M6: **DEF-01** (level-up co-op), **DEF-02** (dropy unikatów bossa), **DEF-05** (warstwa statusów), **DEF-09** (fale 1–5 + The Ram).

Świadomie poza M7: Boss 2 / fale 6–10 / win-lose (M8), pełne rare i drzewka (M7–M8), visual slice M4, polish M9, migracja całego katalogu na `.asset` (DEF-13 → M8).

## Nota PO o „nie da się grać”

Trzy osobne dziury, **żadna nie była zakresem M7**:

1. **Celowanie** — facing lockowany na cały cykl + brak look (mysz/prawy stick). Naprawione w Combat Playability.
2. **Placeholdery** — nie niosą feelu. M7 dokłada bossa na placeholderach (telegraph/HP bar), nie modele.
3. **HUD bez siły** — nie było widać dmg/tempa. Playability dodało `Nazwa X dmg / Y.YYs`.

Zasada FROZEN nr 9: najpierw pętla i feeling, content po playteście. Dlatego M7 czekało na Twój PASS playability.

## Ryzyka, które zostają na playteście M7

- Ram musi dać się **nauczyć** (FROZEN: nie worek HP) — szarża, telegraph, interrupt staggeriem/burstem.
- Co-op level-up per pad — FROZEN `UI_UX` / `PLAYER_PROGRESSION`.
- Dłuższa sesja (5 fal) na greyboxie — czytelność, nie polish.

## Następny krok

Playtest PO wg `M7_PLAN.md` (Verifier T10). PASS → można planować **M8**. FAIL feel Rama / UI → poprawka, nie skok do M8.
