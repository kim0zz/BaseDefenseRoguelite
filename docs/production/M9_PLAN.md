# M9 — Playtest-ready — Plan v1

## Status
**PLAN LEAD** — 2026-09-09 (refaktor po M8.1b PASS i uwadze PO o kartach). **M9.0 DONE** (kod+Verifier 2026-09-11; feel = NEEDS PLAYTEST). Nie zatwierdza całego M9.

Stary M9 („stabilność, balans, UI, audio”) był jednym workiem **po** całym M8. Po zmianie talentów: czytelność decyzji musi wejść **wcześnie**, a balans **po** 10 falach.

## Ocena słuszności (Lead)

| Stary element | Werdykt |
|---|---|
| M9 jako „playtest-ready”, nie nowy content klas/fal | **Zostaje.** Content = M8. |
| UI / feedback / audio skilli (nie rodzin broni) | **Zostaje**, ale pocięte na plastry. |
| Karty level-upu schowane na koniec M9 | **Źle.** PO już boli na Pudzianie. → **M9.0 pierwsze**, przed kitami Cwel/Cipak/Jamie. |
| Balans wstępny w M9 | **Zostaje jako M9.2**, ale **po M8.3** (10 fal). Nie po samym M8.0. |
| Animacje Mixamo / visual slice M4 w M9 | **Nie.** Slice M4 nadal PARKED. M9 = czytelność placeholderów + feel skilli, nie nowy art direction. |
| Game feel PASS z kodu | **Nigdy.** DoD: człowiek. |
| Balance Agent | **Pierwszy raz w M9.2.** Nie wcześniej (lock: nie w M0–M5; M8 świadomie bez). |
| Co-op 2–4P | **M9.3** jako gate, nie „przy okazji”. M7 2P level-up nadal otwarte. |

M9 **nie startuje w całości** przed M8.0. **M9.0 może** iść zaraz po M8.0 (nawet przed M8.2/M8.3) — nie zależy od Wardena.

---

## FROZEN

`UI_UX.md` zakres informacji HUD (HP, sojusznicy, baza, timer fali, EXP, kasa, respawn, skille/CD, statusy). Layout DRAFT.  
`VISUAL_FEEDBACK_CHECKLIST.md`.  
`PLAYER_PROGRESSION`: UI dostaje N kart i rysuje `Length` — **nie** logika oferty w HUD.  
Loot PARKED: panel przerwy **bez** slotów broni/itemów.

---

# M9.0 — Czytelność decyzji (karty + HUD kitu)

## Status
**DONE (kod + Verifier PASS 2026-09-11).** Game feel kart/HUD = **NEEDS PLAYTEST PO**. Nie zatwierdza całego M9.

## Cel
Gracz **nieznający** Pudziana rozumie, *co wybiera* i *co ma na pasku*. HUD trochę przyjemniejszy niż czarny `OnGUI`, ze **slotami** pod późniejszy pack.

## Poza M9.0

Ikony finalne / pack UI, animacje kart, SFX, Mixamo, M9.1 telegraph, nowe klasy, Bomberman, balans liczb, zmiana oferty L2–L5.

## Lock Lead — architektura (podmiana packa tania)

- Combat HUD + karty level-upu = **uGUI Canvas** (Screen Space Overlay). `OnGUI` walki i level-upu **wyłączone**, gdy Canvas żyje (uniknąć podwójnego HUD).
- Wygląd w slotach, nie w ifach skillId:
  - `SkillDefinition`: `Sprite icon` (nullable)
  - `TalentDefinition`: `Sprite icon` (nullable)
  - `HudSkin` SO: kolory + opcjonalne sprite ramki/paska (null = Unity default UI sprite)
- Brak ikony → kolorowy placeholder slotu, nie crash.
- UI **nie** liczy eligibility. Dostaje listę N kart z `BuildFlowController` / `GetAvailableTalents` i rysuje `Count`.
- Intermission loot panel zostaje za `LootEnabledInRun` (off).

## Lock Lead — copy kart (1–2 zdania czasownika)

Zmienić stringi w `BuildContentFactory.BuildPudzianTalents`. Implementer **nie** skraca ani nie wymyśla.

| id | Tekst |
|---|---|
| pudzian_skok | Stomp staje się skokiem w cel. Lądujesz z tym samym uderzeniem; w locie jesteś nietrafialny. |
| pudzian_zryj_mnie | Prowokacja: pierwsze 3 hity od sprowokowanych leczą zamiast ranić. Nadmiar HP przepada. |
| pudzian_spychacz | Byk zbiera zwykłych wrogów przed sobą i zrzuca ich na końcu, zamiast rozpychać na boki. |
| pudzian_wkurw | Obrywanie nabija Furię. Po pełnym pasku atak przyspiesza, ale bierzesz więcej obrażeń. |
| pudzian_hart | Każdy hit daje stack Harta. Przy 5 następny hit Cię nie rani i ogłusza atakującego. |
| pudzian_spalona_ziemia | Po lądowaniu Skoku zostaje paląca strefa na ziemi. |
| pudzian_najezony | Sprowokowany wróg, który Cię uderzy, dostaje część obrażeń z powrotem. |
| pudzian_w_sciane | Wbicie niesionej grupy w ścianę ogłusza ich i kończy Byka. |
| pudzian_ja_jestem_boss | Aktywne ulti: urośniesz, szerszy zamach i Stomp. Zwykłych wrogów rozpychasz samym ruchem. |
| pudzian_trzesienie | Aktywne ulti: po zapowiedzi trzy fale lecą wzdłuż wszystkich linii. |
| pudzian_piekielna_aura | Pasywne ulti: cały czas palisz wrogów wokół. Im dłużej stoją, tym mocniej. |
| pudzian_nie_zabijecie_mnie | Pasywne ulti: raz na falę śmiertelny hit nie zabija — wylecz się w oknie albo giniesz. |
| pudzian_prawdziwy_boss | W formie Kolosa automatycznie ciągniesz aggro wokół siebie. |
| pudzian_pozeracz | Zabójstwa w formie leczą i trochę przedłużają Kolosa. |
| pudzian_wkurwiony_kolos | Kolos macha szybciej i mocniej, ale bierzesz więcej obrażeń. |
| pudzian_kazdy_krok | Każdy krok w formie wypuszcza małą falę pod stopami. |
| pudzian_wstrzasy_wtorne | Po głównym Trzęsieniu idą mniejsze fale wtórne. |
| pudzian_rozpadlina | Na trafionych wrogach zostają pęknięcia ziemi, które dalej ranią. |
| pudzian_krwawa_sejsmika | Trafieni Trzęsieniem leczą Cię (z limitem). |
| pudzian_zatrzymajcie_sie | Trzęsienie mocniej zachwia wrogów. Boss nie jest stun-lockowany. |
| pudzian_wampiryczny_ogien | Część obrażeń aury wraca jako leczenie. |
| pudzian_przegrzanie | Przy niskim HP aura jest większa i szybciej wchodzi na pełny żar. |
| pudzian_slad_ognia | Ruch zostawia krótki palący ślad. |
| pudzian_pozar_lancuchowy | Wróg długo w aurze zapala pobliskich nawet po wyjściu. |
| pudzian_nie_chce_umierac | W Ostatniej Szansie leczenie jest mocniejsze — łatwiej przeżyć okno. |
| pudzian_agonia | W oknie Ostatniej Szansy wychodzą fale obrażeń wokół Ciebie. |
| pudzian_druga_szansa | Jeśli przeżyjesz okno: reset CD aktywnych i krótki boost obrażeń. |
| pudzian_ostatni_wkurw | Ostatnia Szansa sama odpala Furię. |

Pęknięcie (debug meta) — bez zmiany copy.

Karta na ekranie: **nazwa**, linia tagów (`AOE · TAUNT`), body 1–2 zdań, ulti: znacznik `Aktywne` albo `Pasywne`. `TalentCardCopy` testowalny (jak `CombatHudCopy`).

## Lock Lead — HUD kitu

- Slot 4 pasywny: podpis **PASYWNE**, **nie** hint F/RB jak do wciśnięcia.
- Widoczne i podpisane: Furia (aktualny/próg albo AKTYWNA), Hart (stacki / READY), Ostatnia szansa (heal/próg gdy aktywna), combo AA 1/2/3.
- Nadal: HP swoje i sojuszników, baza, timer fali `X/10`, EXP, kasa, respawn, CD skilli, win/lose overlay.
- Layout DRAFT: ciemny panel, złoty nagłówek, HP jako fill bar — „trochę ładniej”, nie AAA.
- `HudSkin.uiScale` — skala całego Canvas (skille, karty).
- `HudSkin.topBarScale` (default 0.78) + `chromeColor` (alpha ~0.38) — osobno mniejszy, bardziej przezroczysty panel góra-lewo.

## Taski

### M90-T1 — Copy + format kart
`BuildContentFactory` teksty z tabeli. `TalentCardCopy.FormatTags` / `FormatCard`. Testy: każda karta L2–L4 ma czasownik i wskazuje skill/tryb; pasywne ulti zawiera „Pasywne”.

### M90-T2 — Sloty sprite
Pola `icon` na skill/talent; `HudSkin` kolory. Brak assetu ≠ błąd.

### M90-T3 — Canvas HUD + level-up
Runtime bootstrap Canvas (jak inne fallbacki BootScene). Level-up: N kart, input 1..N bez zmian. `BuildFlowController.OnGUI` DrawLevelUpPanel off.

### M90-T4 — Kit meters + pasywne ulti
Jak lock HUD.

### M90-T5 — Regresja
Oferta L2–L5 / eligibility nietknięta. `TalentEligibilityTests` / `PudzianM81BTests` bez zmiany grafu. Loot off. Win/lose overlay zostaje czytelny.

## AC

- Na zimno widać na karcie, *jaki skill się zmienia i jak*.
- Pasywne ulti ≠ przycisk F.
- Checklist `UI_UX.md` nadal spełniona.
- Graf oferty bez zmian.
- Game feel = NEEDS PLAYTEST.

---

# M9.1 — Feedback skilli (czytelność walki)

## Status
**IN PROGRESS** — PO lock 2026-09-11: **M9.1a + M9.1b razem**, potem playtest PO. Placeholdery ze **slotami** pod oryginalne VFX/SFX (null = fallback, zero ifów `skillId`).

## Cel
Każdy skill Pudziana ma zapowiedź obszaru, moment hitu i odróżnialny feeling — placeholdery, nie final art. Podmiana assetu = wrzucenie w slot, bez przerabiania logiki.

## Lock Lead — podmiana tania (jak HUD `icon`)

Null w slocie **nigdy** nie jest błędem. Brak assetu → wbudowany placeholder (kółko `LineRenderer`, ton sine, flash koloru).

`SkillDefinition` (nullable, per skill):
- `telegraphPrefab` — zapowiedź obszaru (windup)
- `impactPrefab` — moment hitu
- `windupClip` / `impactClip` — SFX

`CombatFeedbackSkin` SO (`Assets/_Game/Config/CombatFeedbackSkin.asset`), jak `HudSkin`:
- `stunPrefab`, `auraPrefab`, `kolosPrefab`
- `defaultWindupClip`, `defaultImpactClip`, `stunClip`
- kolory placeholderów (stun żółty)

`SkillFeedbackResolver.Resolve(authored, fallback)` testowalny. **Zakaz** `if (skillId == "pudzian_byk")` przy VFX.

Istniejący `SkillTelegraphView` / `HitFlashFeedback` / rumble / shake **zostają** jako fallback. Nowe prefaby spawnują się obok, nie zamiast hitboxów.

## M9.1a — czytelność hitu

- Trafiony wróg: `HitFlashFeedback` (już jest) + opcjonalny `impactPrefab` na pozycji hitu.
- **Stun** (Hart, Stomp, skill control): `StunFeedbackView` na `StatusEffectReceiver` — żółty pierścień albo `stunPrefab`, znika z końcem stuna.
- Trzęsienie: fala już ma pierścień; jeśli `impactPrefab` na skillu — spawn na pulsie, inaczej placeholder.
- Aura (Hell) / Kolos: widoczny pierścień na casterze gdy aktywne; slot `auraPrefab` / `kolosPrefab`.
- Windup skilli L1 bez zmian kształtu (kółko / linia Byka / lądowanie Skoku).

## M9.1b — SFX + pad

- Windup / impact: `windupClip` / `impactClip`, fallback `CombatFeedbackSkin` albo wygenerowany sine (whoosh niski / thud niższy / stun wysoki).
- Rumble pada: już `RequestGamepadRumble` na hicie — zostawić, nie duplikować.
- Shake: istniejący `CombatFeelService` — nie ruszać liczb balansu.

## Poza M9.1a+b

Mixamo, visual slice M4, soundtrack, inne klasy, nowe mechaniki, zmiana dmg/CD, M9.1c polish po playteście.

## Taski

### M91-T1 — Sloty + resolver
Pola na `SkillDefinition` + `CombatFeedbackSkin`. Testy: null → fallback; authored ≠ null → authored.

### M91-T2 — Stun + impact presenter
`StunFeedbackView` auto na enemy/boss/player (jak `HitFlashFeedback`). Spawn impact prefab / placeholder przy `ResolveActiveHits` i Hart stun (przez status, nie osobny if Hart).

### M91-T3 — Aura / Kolos / Trzęsienie
Pierścień gdy forma/aura; lane pulse honoruje `impactPrefab`.

### M91-T4 — Placeholder SFX
`PlaceholderSfx` sine + play na windup/hit/stun. Test: długość clipu > 0.

### M91-T5 — Verifier
Console 0 Error. Checklist Walka: zapowiedź, hit, stun, aura/Kolos. Game feel = NEEDS PLAYTEST PO.

## AC

- Puste sloty = placeholder, gra działa.
- Wrzucenie prefabu/clipu w Inspector zmienia tylko look/sound.
- Stun Harta widać na wrogu bez konsoli.
- Q/E/R + Byk: windup słychać/widać, hit osobno.
- `VISUAL_FEEDBACK_CHECKLIST` Walka: kodowo spełnione albo residual jawny; feel = PO.
- Graf oferty / dmg / CD nietknięte.

---

# M9.2 — Balance pass v0.2

## Cel
Liczby Pudziana (i klas, jeśli już są) po **pełnym runie 1–10**. Outliery, nie redesign FROZEN.

## Wejście

M8.3 PASS (10 fal + Warden). Hipotezy z raportu M8.1b (leczenie stacked, Pożeracz cap, Furia+Kolos, stagger Wardena, itd.).

## W zakresie

- Balance Agent: DPS/TTK/EHP, uptime Furii/Hartu, synergie, martwe karty L5.
- Lead zatwierdza, które DRAFT liczby ruszamy.
- Implementer tylko lista parametrów — bez nowej mechaniki.

## Poza

Zmiana struktury kart, nowe talenty, loot, „nerf tożsamości” Pudziana bez zgody PO.

## AC

- Raport Agenta z flagami.
- Patch liczb w danych (`EffectTuning` / `SkillDefinition`), nie w ifach.
- Playtest PO po patchu.

---

# M9.3 — Co-op 2–4P + stabilność

## Cel
Bramka, której M7 nie zamknął: level-up 2+ graczy, wipe, kamera, Ram/Warden w co-opie.

## AC

- 2P: join, śmierć jednego ≠ FAIL, wipe = FAIL, level-up wszyscy muszą wybrać.
- 4P jeśli sprzęt: taunt markery, shake nie „tylko caster”.
- 0 Error w Console na pełnym runie 1–10 (Pudzian).
- Respawn 20 s, offset od rdzenia (jeśli P0 z baseline nadal otwarte — domknąć tu albo wcześniej w M8.3).

---

## Świadomie nie w M9

| Temat | Gdzie |
|---|---|
| Win/lose, fale 6–10, Warden, 3 kity | M8 |
| Visual slice M4 | PARKED |
| Loot / unique boss | PARKED |
| Meta-konto | poza MVP |
| MCP hang edytora | operacyjne (wyłączyć Unity MCP przy starcie), nie milestone content |

## M9 DONE (PO)

M9.0 + M9.1 + M9.2 + M9.3 PASS playtestem. Można uznać M9.0 wcześniej osobno (PO).

## Role

- Lead: ten plan; lista liczb do M9.2.
- Implementer / Verifier: jak M8.
- Balance Agent: **tylko M9.2**, na polecenie PO/Leada.
- PO: playtest M9.0 (karty) i feel M9.1/M9.3.
