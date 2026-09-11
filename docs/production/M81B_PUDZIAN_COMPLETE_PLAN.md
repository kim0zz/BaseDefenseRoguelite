# M8.1b — Pudzian kompletny (pierwszy pełny test frameworku progresji)

## Status
**DONE** — PO playtest PASS 2026-09-09. Pudzian L1–5 na nowym designie, framework generyczny.

Uwaga PO (nie blocker): karty level-upu nie tłumaczą, *co* się ładuje — bez znajomości kitu nie wiadomo, co się rozwija. Świadomie **M9** (polish UI kart / HUD), nie ten slice.

Nie startujemy M8.0 / Bombermana / Balance Agenta z tego planu.

Cel: Pudzian lvl 1–5 grywalny ręcznie na nowym designie, przez **generyczny** framework M8.1 (nie logika w `PudzianController`).

## FROZEN — bez zmian
- Pętla: `PLAYER_PROGRESSION.md` (4 decyzje, pauza, full HP, wspólny EXP, max 5).
- Receptury Pudziana: L2=3 mutacje, L3=2 rdzenie+1 follow-up, L4=4 ulti bez filtra L2/L3, L5=eligible capstone’y, N zmienne, min. 1.
- Zasady skilli: `SKILLS.md`. E3 taunt Grunta w korytarzu. Loot PARKED.
- Content talentów w `TALENTS.md` był DRAFT — **ten dokument + nowy `TALENTS.md` zastępują placeholdery**.

## Decyzje Leada (lock na implementację)

| ID | Decyzja |
|---|---|
| D1 | Skill ID zostają (`pudzian_trzasniecie`, `pudzian_no_chodz_tu`, `pudzian_byk`). Display: **Stomp**, **Prowokacja**, **Byk**. |
| D2 | Licznik tagów = suma **wszystkich** wybranych kart, w tym ulti (FROZEN). Skutek: część bramek L5 jest zawsze spełniona tagiem ulti — to akceptowane. |
| D3 | `requiresUltimateId` = `GrantedSkill.SkillId` (jak dziś). Ulti talent id = skill id. |
| D4 | Nowe pole `RequiresAnyTags` (OR) obok AND `RequiresTags`. Potrzebne dla Przegrzania i Śladu Ognia; generyczne. |
| D5 | Pasywne ulti: `GrantSkill` + `SkillActivationMode.Passive` (brak F/RB). Persistent i tak się podpina. |
| D6 | Wszystkie liczby z tabeli BASELINE v0.1 poniżej — **Implementer nie wymyśla wartości**. Tuning w danych (`EffectTuning` / pola `SkillDefinition` / `AttackComboDefinition`), nie w ifach. |
| D7 | Combo AA = `AttackComboDefinition` na broni/klasie, nie `PudzianController`. Inne klasy bez combo profilu zachowują 1-hit. |
| D8 | Hart/Furia liczą **dyskretne ataki wroga** (nie DoT/aura tick). |
| D9 | Ostatnia Szansa: raz na **falę** (reset `WaveStarted`); próg = suma leczenia w oknie ≥ 30% max HP. |
| D10 | Placeholdery VFX OK; telegraph + HUD bez konsoli obowiązkowe. |

Brak eskalacji FROZEN. Brak blokera.

---

# A. Diagnoza repo

Framework M8.1 **działa**: `TalentEligibility` → `OfferGenerator` → `ProgressionApplier` → sloty + `SkillEffectKind` + `PlayerPersistentEffects`. Content w `BuildContentFactory` / `SkillContentFactory` (runtime SO, brak `.asset`).

**Jest (placeholder jakościowy, stary design):**
- Kit L1: 1-hit topór 22/1.6s; Stomp CD 8; taunt 12s + immunity KB; Byk 7 m forward shove.
- L2: Skok (bez i-frame dmg), ŻRYJ = heal 4 HP/cel na cast tauntu, Spychacz = szersza kapsuła.
- L3: Wkurw = % dmg od brakującego HP; Hart = DR poniżej 50%; strefa DoT; reflect 40%; pulse na ścianie.
- L4: ulti-placeholdery (duży taunt, duże koło, puls+aura, revive 35% raz na run).
- L5: bazowe capstone’y zawsze widoczne + kilka tagowych.

**Brak / nie to:** 3-hit combo, hold, i-frame skoku, side-shove/carry, fury meter, Hart stacki, convert-hit→heal, timed last stand per wave, transform 8s, fale 3 lane, aura ramp, pasywne ulti, OR-tagi.

HP Pudziana: kod 130 vs docs 135 → wyrównać do **135**.

---

# B. Mapowanie designu → framework

| Feature | Mechanizm | Nowa capability? |
|---|---|---|
| L2 mutacja skilli | `TalentEffectOp.MutateSkill` | Nie |
| L3 follow-up 1:1 | `requiresTalents` + `MixIndependentAndFollowup` | Nie |
| L4 4 ulti | `GrantSkill`, bez tag filtra | Nie |
| L5 filtr ulti+tagi | `requiresUltimateId` + `requiresTags` | `RequiresAnyTags` (OR) |
| Stomp / Prowokacja / Byk L1 | istniejące skille, nowe liczby/policy | Charge contact policy |
| Skok i-frame | leap locomotion | `DamageImmunity` na czas lotu |
| ŻRYJ 3 hity → heal | persistent + incoming pipeline | Convert incoming (taunt source, count) |
| Spychacz carry | charge policy | `ChargeContactMode.Carry` |
| Spalona Ziemia | `SpawnDamageZone` (bez slowa) | Parametry strefy, cap 2 |
| Najeżony 50% | `DamageReflect` | Tuning 0.5; działa też przy convert |
| W ścianę | `PulseRingOnWallStop` | Stun per kategoria + bonus dmg |
| Wkurw Furia | persistent | **Fury meter** (nowy kind + tuning) |
| Hart stacki | persistent | **Hart stacks** (nowy kind) |
| JA JESTEM BOSS | GrantSkill aktywne | **Timed form** (scale, AA, shove ruchu) |
| Trzęsienie | GrantSkill | **`SkillShapeType.LaneWaves`** |
| Piekielna Aura | GrantSkill pasywne + persistent | Aura **per-target ramp + grace** |
| Nie zabijecie mnie | GrantSkill pasywne | **Timed last chance**, raz/falę |
| L5 ewolucje | AddEffect / AttachPersistent | Kilka kindów (on-kill, trail, pulse, aftershock) |
| Combo 3-hit | `PlayerAttackController` + `AttackCycle` | `AttackComboDefinition` |
| Tap + hold | input ataku | hold kontynuuje combo po recovery |

---

# C. Brakujące capability (minimalne, generyczne)

Nie budować osobnego systemu per talent. Rozszerzyć istniejące enumy + tuning.

1. **`AttackComboDefinition`** — N hitów (timing, dmg, zasięg, łuk, stagger, knockback). Reset `comboResetSeconds`. Hold = auto next. Brak profilu = obecny 1-hit.
2. **`DamageImmunity`** — generyczny, leap włącza na travel. Blokuje HP dmg, nie statusy inne niż to co spec każe.
3. **`ChargeContactMode`** na `SkillDefinition`: `ForwardShove` \| `SideShove` \| `Carry`. Elite brake + mały shove. Boss = stop (już jest).
4. **Incoming pipeline** w `PlayerPersistentEffects` (kolejność D10):
   - zlicz hit (Hart/Furia) z ilością *po obronie*;
   - jeśli Hart pełny → 0 dmg, consume, stun źródło, stop;
   - else jeśli convert (ŻRYJ, max 3, źródło zatauntowane) → heal = amount, 0 HP loss;
   - else normalny incoming (Furia +10% gdy aktywna).
5. **`EffectTuning`** na `TalentEffect` — floaty/inty per kind, zero magicznych stałych w logice.
6. **`SkillActivationMode`**: Active / Passive.
7. **`SkillShapeType.LaneWaves`** — 3 fale wzdłuż istniejących lane pathów (nie filtr `laneId` na overlap — fale *jadą* geometrią mapy).
8. **`PersistentEffectKind` dodać** (nie zastępować starych nazw jeśli reused): FuryMeter, HartStacks, TimedLastChance, KolosForm, HellAuraRamping, MoveShockwave, OnKillFormExtend, PeriodicTauntInForm, AftershockWaves, GroundCracks, VampireAura, OverheatAura, FireTrail, ChainIgnite, HealAmpInLastChance, AgonyPulses, SecondWind, AutoFuryOnLastChance. *Gdzie dwa talenty to ten sam hook z innym tuningiem — jeden kind.*
9. **`RequiresAnyTags`** w `TalentRequirement` + `TalentEligibility`.
10. **Wave reset** last chance: subskrypcja `WaveManager.WaveStarted` (generyczna, nie klasa).

**Nie robić:** IDeathGuard interface (Health już woła persistents); plugin DSL; per-Pudzian controller; bazowych capstone’ów L5 (`prawdziwy_kolos`, `wieczny_ogien`, `drugie_zycie`, `rzeznia`).

---

# D. BASELINE VALUES v0.1

Wszystkie **DRAFT**. Parametr w danych. Uzasadnienie krótko vs benchmark: grunt fali 1 = 18 HP / 8 dmg / 1.4 s; Jamie AA 10/0.8 s; Ram stagger próg 35; Pudzian HP **135**, melee ~120%.

### Atak podstawowy (buława, 3-hit)

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Combo total | 2.16 s | Środek 2.0–2.2 |
| Hit1 dmg / zasięg / łuk / stagger / KB | 10 / 2.2 m / 140° / 4 / 0.8 | 2 hity zabijają grunta (20>18); AA dobija |
| Hit2 | 10 / 2.2 m / 140° / 4 / 0.8 | Lustrzane |
| Hit3 | 16 / 2.8 m / 160° / 10 / 1.2 | Finisher; 1 grunt już martwy, clump/elita |
| Timing h1 | 0.28 / 0.10 / 0.28 | Cięższy niż miecz, lżejszy niż stary topór 1.6 s |
| Timing h2 | 0.28 / 0.10 / 0.28 | |
| Timing h3 | 0.35 / 0.14 / 0.35 | Wolniejszy impact |
| Combo reset | 1.25 s | PO |
| Hold | kontynuuje po recovery | PO |
| Max celów | 6 / 6 / 8 | Jak klin topora; finisher szerszy |

DPS pełnego combo: 36 / 2.16 ≈ 16.7 (wyżej niż stary 13.75 — kara = reset i windup).

### Stomp (L1)

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| CD | 5 s | PO |
| Dmg | 12 | Nie zabija grunta; CD szybszy niż 8 s więc dmg w dół z 16 |
| Promień | 3.0 m | Bez zmian czytelności |
| Zachwianie (stun zwykły) | 1.0 s | Elita ×0.5; boss 0 stun |
| Boss stagger | 10 | Było 12 przy wolniejszym CD |
| Windup/active/recovery | 0.30 / 0.12 / 0.35 | Korzeń w przygotowaniu+uderzeniu |
| Odrzut | 0 | PO |
| Max celów | 8 | |

### Prowokacja (L1)

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| CD | 8 s | PO |
| Promień | 5.5 m | Środek 5–6 |
| Czas | 4.0 s | PO |
| Dmg | 0 | Czyste aggro |
| Immunity castera | **brak** | PO: bez pancerza/healu/DR |
| Boss | nie tauntuje | E3/M7 |
| Max celów | 16 | |

### Byk (L1)

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| CD | 6 s | PO |
| Dystans | 5.5 m | Środek 5–6 |
| Prędkość | 12.2 m/s | 5.5 / 0.45 s |
| Active (szarża) | 0.45 s | |
| Windup / recovery | 0.25 / 0.35 | Brak skrętu w active |
| Dmg kontakt | 8 raz/cel | Małe; nie czyści fali |
| Side shove zwykły | 1.2 m, 0.20 s, prostopadle | Rozpychanie na boki |
| Elita shove | 0.35 m w przód | „Trochę” |
| Elita brake | prędkość castera ×0.40 gdy overlap | Mocno hamuje |
| Boss | stop ≤0.15 s, stagger 14 | Duży wkład / 35 |
| Skręcanie | 0 w active | PO |

### L2

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Skok zasięg | 6.5 m | Środek 6–7 |
| Skok i-frame | cały lot (0.30 s travel) | PO nietrafialny |
| Skok land | te same liczby Stompa | PO |
| ŻRYJ hitów | 3 | PO |
| ŻRYJ heal | 100% dmg po obronie | PO; overheal clamp |
| Spychacz carry dmg | 0 w trakcie | PO |
| Spychacz zrzut dmg | 6 | Drobny bonus, nie main DPS |
| Spychacz dump radius | 1.8 m | Zwarta grupa |

### L3

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Spalona: czas / tick / dmg | 4 s / 0.5 s / 3 | 24 jeśli stoi; stomp 12 + 2 ticki ≈ grunt |
| Spalona slow | 0 | PO |
| Spalona max stref | 2 | PO |
| Spalona r | 3.0 m | Jak stomp |
| Najeżony reflect | 50% | PO; ten sam amount co heal ŻRYJ |
| W ścianę bonus dmg | 14 | Dobija ranne |
| W ścianę stun | 2 / 1 / boss stagger 16 (0 hard stun) | PO |
| Furia próg dmg | 45 | ~33% HP; 2 gruntów ~4 s do procu |
| Furia czas | 6 s | PO |
| Furia AA speed | +25% | PO |
| Furia AA dmg | +20% | PO |
| Furia hit3 dmg extra | +15% (na wierzchu 20%) | Mocniejszy impact |
| Furia hit3 zasięg extra | +0.4 m | |
| Furia hit3 stagger mul | ×1.5 | |
| Furia incoming | +10% | PO |
| Furia podczas furii | nie nabija | PO |
| Hart max stack | 5 | PO |
| Hart stun | 2 / 1 / 0.5 s | PO; też ranged |
| Hart consume | cały pasek, 0 dmg tego hitu | PO |

### L4 ulti

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Kolos czas / CD | 8 s / 24 s | ~33% uptime |
| Kolos scale | 1.7× | Środek 1.6–1.8 |
| Kolos AA zasięg mul | 1.35 | |
| Kolos Stomp/Skok r mul | 1.35 | |
| Kolos % dmg | 0 | PO |
| Kolos shove ruchu | zwykły 0.6 m / elita 0.15 / boss 0 | Co ~0.35 s gdy moving |
| Trzęsienie windup | 0.90 s | Środek 0.8–1.0 |
| Trzęsienie dmg | 28 | Fala 6–7 grunt ~38–42; nie one-shot |
| Trzęsienie zachwianie | 1.4 s (elita ×0.5) | |
| Trzęsienie boss stagger | 22 | Duży kawałek z 35 |
| Trzęsienie CD | 28 s | Środek 25–30 |
| Trzęsienie fala prędkość / szer. | 14 m/s / 3.2 m | Czytelny przejazd linii |
| Aura r | 3.75 m | Środek 3.5–4 |
| Aura tick | 0.5 s | |
| Aura dmg bazowy / +ramp / max extra | 3 / +1 / +5 | 6→16 DPS na celu; max po 2.5 s |
| Aura grace | 1.5 s | PO |
| Last chance czas | 6 s | PO |
| Last chance próg heal | 30% max HP | PO = 40.5 przy 135 |
| Last chance | raz/falę; HP może spaść do 0; `IsAlive` true w oknie | |

### L5

| Parametr | v0.1 | Uzasadnienie |
|---|---|---|
| Prawdziwy Boss taunt r / refresh | 6 m / 2.0 s | W oknie formy |
| Pożeracz heal/kill | 8 | |
| Pożeracz extend/kill | 0.4 s | |
| Pożeracz max extra | 4.0 s | Cap 8→12 s, nie infinite |
| Wkurwiony Kolos loop | +20% speed w formie | Na wierzchu furii jeśli obie |
| Wkurwiony Kolos hit3 | +30% dmg finisher | |
| Wkurwiony Kolos incoming | +15% w formie | RISK |
| Każdy krok | co 1.2 m albo 0.45 s (co pierwsze), 6 dmg, r 1.6, stagger 0.3 s | Małe fale |
| Wstrząsy wtórne | 2 fale, delay 0.7 s, 50% dmg, 70% stagger | |
| Rozpadlina | 4 s, 3 dmg / 0.5 s, szer. fali; spawn na pozycji trafionego (nie na końcu linii) | PO 2026-09-11 |
| Krwawa Sejsmika | 4 HP / trafiony, cap 32 / cast | Max 8 celów |
| Zatrzymajcie | stagger ×1.6; boss contrib ×1.5 (22→33) | Nie perma stun-lock |
| Wampiryczny | 20% dmg aury → heal, cap 12 HP/s | Anti 4-target infinite |
| Przegrzanie HP próg | 35% | |
| Przegrzanie r mul / ramp mul | 1.4 / 2.0 | |
| Ślad | 2.0 s, 3/0.5s, spawn 0.8 m, max 6 segmentów | |
| Pożar łańcuchowy | po 3.0 s w aurze; zapłon r 2.5 m, 4 s, 2/0.5s; **1 generacja** (nie chain-chain) | |
| Nie chcę umierać | heal ×1.75 w oknie; próg 20% max | Łatwiejszy survive |
| Agonia | co 1.0 s, r 3.5, 10 dmg | 6 pulsów / 6 s |
| Druga szansa (win) | reset CD aktywnych; +25% outgoing 4 s | |
| Druga szansa (downside) | +15% incoming 4 s | RISK po cheat death |
| Ostatni Wkurw | auto Furia na starcie okna (nawet 0 meter) | Wymaga BERSERKER |

---

# E. Walidacja grafu (36 legalnych buildów)

L2×L3×L4 = 3×3×4 = **36**. Tagi ulti **liczą się**.

L5 pule:
- Boss: TAUNT / HEAL / BERSERKER / AOE → Prawdziwy Boss, Pożeracz, Wkurwiony Kolos, Każdy krok
- Trzęsienie: AOE / ZONE / HEAL / CONTROL → Wstrząsy, Rozpadlina, Krwawa, Zatrzymajcie (**AOE+CONTROL z karty ulti → zawsze ≥2**)
- Aura: HEAL / (BERSERKER\|RISK) / (ZONE\|MOBILITY) / AOE → Wampiryczny, Przegrzanie, Ślad, Pożar (**AOE+ZONE z ulti → zawsze ≥2**)
- NZM: HEAL / AOE / RISK / BERSERKER → Nie chcę, Agonia, Druga szansa, Ostatni Wkurw (**HEAL+RISK z ulti → zawsze ≥2**)

**Min 1: PASS.** Żaden legalny build nie ma 0.

### Dokładnie 1 opcja L5 (4 buildy)

Wszystkie: ulti **JA JESTEM BOSS**, brak TAUNT/HEAL/BERSERKER, jest AOE z L2.

1. Skok + Hart + JA JESTEM BOSS → **Każdy krok to problem**
2. Skok + Spalona Ziemia + JA JESTEM BOSS → **Każdy krok to problem**
3. Spychacz + Hart + JA JESTEM BOSS → **Każdy krok to problem**
4. Spychacz + W ścianę + JA JESTEM BOSS → **Każdy krok to problem**

Żryj+Boss zawsze ≥2 (TAUNT+HEAL). Wkurw+Boss zawsze ≥2 (BERSERKER + zwykle AOE).

Hart **nie otwiera** żadnej bramki L5 (TANK/DEFENCE). To zamierzone.

---

# F. Plan implementacji (kolejność)

Nie łączyć z M8.0. Jeden zakres: kompletny Pudzian.

| ID | Zakres | AC skrót |
|---|---|---|
| T1 | `EffectTuning`, `RequiresAnyTags`, `SkillActivationMode`, combo definition + AttackCycle | EditMode combo 3 hity, reset 1.25, hold; eligibility OR |
| T2 | Kit L1 liczby + Prowokacja bez immunity + Byk SideShove/elite brake | Testy Byk/NoChodzTu z nowymi liczbami; grunt na boki |
| T3 | Skok i-frame; ŻRYJ convert 3; Spychacz Carry+dump | Leap: 0 dmg w locie; 4. hit taunta boli; carry dump |
| T4 | Wkurw Furia, Hart stacki, Spalona, Najeżony 50%, W ścianę CC | Furia auto po 45 dmg; Hart 6. hit = 0+stun |
| T5 | 4 ulti wg spec (form, lane waves, aura ramp, last chance/wave) | Passive slot bez F; form 8s; 3 fale; survive/fail last chance |
| T6 | 16 capstone’ów L5 (usunąć stare bazowe) | Graf 36 buildów; 4 singleton L5 |
| T7 | HUD placeholdery: fury, hart, last chance, combo, pasywne ulti | Czytelne bez konsoli |
| T8 | Docs: TALENTS, skill mini-specy, BASELINE, CLASSES, SESSION_HANDOFF | Zgodne z kodem |

Testy automatyczne obowiązkowe przy T1–T6 (lista w G).

---

# G. Plan testów

### Automatyczne (EditMode)

- Graf: każdy z 36 buildów ma L5 ∈ [1,4]; enumeracja singletonów = 4 powyższe.
- L4 zawsze 4 ulti niezależnie od L2/L3.
- L3 follow-up 1:1 (Skok→Spalona, ŻRYJ→Najeżony, Spychacz→W ścianę).
- `RequiresAnyTags`: Przegrzanie widoczne przy RISK bez BERSERKER (sztuczny snapshot); ukryte gdy brak obu.
- Combo: 3 fazy, dmg 10/10/16, reset po 1.25 idle, hold startuje next.
- Stomp 12/5s/0 KB; Prowokacja 5.5/4s/0 immunity.
- Byk: normal side vector; elite brake; boss stop+stagger 14.
- Leap: TakeDamage w locie ignorowane; po lądowaniu nie.
- ŻRYJ: 3 convert, 4. hit normalny; overheal clamp; Hart/Furia zliczają te hity.
- Furia: 45 → proc; podczas 6s meter nie rośnie; po końcu 0.
- Hart: 5 stacków, 6. hit 0 dmg + stun 2/1/0.5; ranged OK.
- Last chance: pierwszy lethal/falę → okno; heal 30% → żyje; brak → śmierć; drugi lethal w tej samej fali zabija; nowa fala resetuje.
- Kolos: scale 1.7, czas 8, CD 24, 0% dmg card.
- LaneWaves: 3 instancje wzdłuż lane; boss stagger 22.
- Aura ramp: ten sam wróg 3→8 dmg/tick; grace 1.5 s trzyma ramp; po grace reset.
- Pożeracz: extend capped 4 s.
- Pożar: 1 generacja, brak pętli.
- Regresja: join, respawn 20 s, fala timer/clear, level-up pause+heal, E3 grunt korytarz, loot off, Ram taunt ignore.

### Ręczny playtest (człowiek) — nie PASS z kodu

Combo feel, hold vs tap, i-frame skoku, Byk boki vs carry, Furia/Hart czytelność, 4 ulti WOW, last chance stres, Trzęsienie na 3 liniach, aura ramp, singleton L5 (Skok+Hart+Boss), co-op 2P jeśli dostępne. Game feel = NEEDS PLAYTEST.

---

# H. Ryzyka

| Ryzyko | Mitygacja |
|---|---|
| Combo psuje Combat Playability (aim lock) | Lock tylko w Active **danego hitu**; windup śledzi aim |
| Carry vs EnemyLaneMotor | ForcedMovement honor collisions; po dump AI wznawia; nie `transform` wroga poza stepperem |
| Lane waves zależą od geometrii mapy | Użyć istniejących lane path; test z 3 waypoint liniami |
| Last chance vs HealToFull na level-up | Level-up pełne HP kończy potrzebę okna; nie resetować „used this wave” na level-up |
| Tag ulti rozmiękcza L5 | Zaakceptowane (D2); singleton tylko na Boss path |
| PlayerPersistentEffects god-class | Moduły zwykłe C# owned by persistents, bez nowej architektury plugin |
| Stare testy liczb CD/dmg | Zaktualizować do v0.1, nie łatać asercji pod stary design |

Handoff: Implementer `composer-2.5-fast` → Verifier `composer-2.5-fast`. Max 1 runda poprawek na FAIL. Potem raport balance-hipotez **bez** uruchamiania Balance Agenta.
