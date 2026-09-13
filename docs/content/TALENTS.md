# Talenty

## Status
**DRAFT — liczby v0.1 w `docs/balance/BASELINE_VALUES.md` i `docs/production/M81B_PUDZIAN_COMPLETE_PLAN.md`. Struktura kart FROZEN w `PLAYER_PROGRESSION.md` (lock PO 2026-09-09).**

Stary draft pod bronie/itemy / drzewko A/B **nie obowiązuje**. Nie implementować gołych `+%`.

Pula poniżej = wzorzec **Pudziana** (design PO 2026-09-09). Cwel / Cipak / Jamie — po playteście tego kitu.

# Pudzian

Kit lvl 1: buława 3-hit + **Stomp** + **Prowokacja** + **Byk**. Mini-specy: `docs/content/skills/pudzian/`.

Tagi niosą karty (nie kit L1). Licznik = suma wybranych kart, w tym ulti.

## Lvl 2 — jedna mutacja na aktywny skill

| Karta | Skill | Tagi | Efekt |
|---|---|---|---|
| Skok z Pierdolnięciem | Stomp | AOE, MOBILITY | Leap 0–6.5 m, i-frame w locie, stomp na lądowaniu (te same liczby). CD 5 s. |
| ŻRYJ MNIE | Prowokacja | TAUNT, HEAL | Pierwsze 3 trafienia od sprowokowanych leczą (heal = dmg po obronie). Overheal znika. Hity liczą się do Hartu/Furii. |
| Spychacz | Byk | CONTROL, AOE | Zamiast rozpychać na boki: zbiera zwykłych przed sobą, niesie, zrzuca grupę. Elita/boss = ciężka interakcja bazowa. |

## Lvl 3 — 2 rdzenie + 1 follow-up od lvl 2

Niezależne:

| Karta | Tagi | Efekt |
|---|---|---|
| Wkurw | BERSERKER, RISK | Pasek Furii z otrzymanych obrażeń; auto-proc ~6 s (+AA speed/dmg, mocniejszy hit 3, +incoming). W trakcie nie nabija. |
| Hart | TANK, DEFENCE | +1 stack/hit, max 5. Przy pełnym: następny hit = 0 dmg, consume, stun atakującego (2 / 1 / 0.5 s). |

Follow-up 1:1:

| Lvl 2 | Karta | Tagi | Efekt |
|---|---|---|---|
| Skok | Spalona Ziemia | AOE, ZONE, DAMAGE | Strefa DoT po lądowaniu, 4 s, tick 0.5 s, bez slowa. |
| ŻRYJ MNIE | Najeżony | TAUNT, REFLECT | 50% dmg zwrotnych od sprowokowanego hitu (także gdy ŻRYJ zamienił w heal). Bez statusów. |
| Spychacz | W ścianę | CONTROL, COLLISION, AOE | Wbicie w przeszkodę: bonus dmg + stun 2/1 s; boss stagger. Byk kończy się. |

## Lvl 4 — ulti (wszystkie 4, bez filtra L2/L3)

| Karta | Tagi | Typ | Efekt |
|---|---|---|---|
| JA JESTEM BOSS | KOLOS, TANK, CONTROL | aktywne ~8 s | Transform 1.7×, większy AA/Stomp, lekkie rozpychanie ruchem, mocniejsze zachwianie. Bez płaskiego % dmg. |
| Trzęsienie Świata | TRZĘSIENIE, AOE, CONTROL | aktywne | Windup ~0.9 s, trzy fale wzdłuż lane’ów. |
| Piekielna Aura | AURA, AOE, ZONE | **pasywne** | Aura always-on, ramp per wróg, grace 1.5 s. |
| Nie zabijecie mnie | OSTATNIA_SZANSA, TANK, HEAL, RISK | **pasywne** | Pierwszy śmiertelny hit fali → 6 s okno; wylecz 30% max HP albo śmierć. Raz na falę. |

## Lvl 5 — ewolucje wybranego ulti

Najpierw filtr `requiresUltimateId`, potem tagi. Ewolucja bez tagu **nie** jest pokazywana. Legalny build może mieć 1 opcję; nigdy 0.

Brak „bazowego” capstone’u samego `requiresUltimate`.

### JA JESTEM BOSS

| Karta | Wymaga | Tagi (karta) |
|---|---|---|
| Prawdziwy Boss | TAUNT | TAUNT, CONTROL |
| Pożeracz | HEAL | HEAL, KOLOS |
| Wkurwiony Kolos | BERSERKER | BERSERKER, RISK |
| Każdy krok to problem | AOE | AOE, KOLOS |

### Trzęsienie Świata

| Karta | Wymaga | Tagi (karta) |
|---|---|---|
| Wstrząsy Wtórne | AOE | TRZĘSIENIE, AOE |
| Rozpadlina | ZONE | ZONE, AOE, DAMAGE |

Krater Rozpadliny spawnuje się na pozycji trafionego wroga (DoT, bez kolizji), nie na końcu linii.
| Krwawa Sejsmika | HEAL | HEAL, AOE |
| Zatrzymajcie się wszyscy | CONTROL | CONTROL |

### Piekielna Aura

| Karta | Wymaga | Tagi (karta) |
|---|---|---|
| Wampiryczny Ogień | HEAL | AURA, HEAL |
| Przegrzanie | BERSERKER **lub** RISK | AURA, BERSERKER, RISK |
| Ślad Ognia | ZONE **lub** MOBILITY | ZONE, MOBILITY |
| Pożar Łańcuchowy | AOE | AOE |

### Nie zabijecie mnie

| Karta | Wymaga | Tagi (karta) |
|---|---|---|
| Nie chcę umierać | HEAL | HEAL, OSTATNIA_SZANSA |
| Agonia | AOE | AOE |
| Druga szansa | RISK | RISK |
| Ostatni Wkurw | BERSERKER | BERSERKER, RISK |

Wildcardy: brak. Meta-unlocki: brak w bazowym kicie (debug Pęknięcie zostaje poza ofertą).

### Buildy z dokładnie 1 kartą L5

Tagi ulti liczą się. Singleton tylko na ścieżce Kolosa bez TAUNT/HEAL/BERSERKER:

1. Skok + Hart + JA JESTEM BOSS → Każdy krok to problem
2. Skok + Spalona Ziemia + JA JESTEM BOSS → Każdy krok to problem
3. Spychacz + Hart + JA JESTEM BOSS → Każdy krok to problem
4. Spychacz + W ścianę + JA JESTEM BOSS → Każdy krok to problem

# Bomberman

Kit lvl 1: petarda + **Bomba** + **Wybuchowy odskok** + **Kopniak**. Mini-specy: `docs/content/skills/bomberman/`. Liczby v0.1 / kit rebuild DRAFT: `docs/production/M84_BOMBERMAN_PLAN.md` oraz `DeployableTuning`.

Receptura L2: `AllEligibleInGroup` (3 mutacje niezależne od skill-match — inna niż Pudzian). L3: `MixIndependentAndFollowup` (Saper + Piroman + 1 follow-up 1:1). L4: wszystkie 3 ulti. L5: capstone z eligibility poniżej.

Tagi niosą karty (w tym ulti). Licznik = suma **wszystkich** wybranych kart.

## Lvl 2 — mutacje (wszystkie eligible w grupie `mutation`)

| Karta | Skill | Tagi | Efekt |
|---|---|---|---|
| Kasetowa | Bomba | Basic, Demolition, Chain | Po wybuchu Normal (gen 0): **3** child, dmg **7** / r **1.40 m**, fuse **0.80 s**, scatter **1.2–1.8 m** 360°. Child gen **1**, detonatable, kategoria Child (poza cap 3). |
| Większy Huk | Petarda (AA) | Basic, Demolition, AOE | Splash petardy **2.20 m** (z 1.10 m). Extra dmg **0**. |
| Wybuch ogłuszający | Kopniak | Basic, Control | Kopnięcie + wybuch: stun **1.0 / 0.5 / 0 s** (grunt/elita/boss), boss stagger **+8**. |

## Lvl 3 — 2 rdzenie + 1 follow-up od lvl 2

Niezależne (zawsze w ofercie L3):

| Karta | Tagi | Efekt |
|---|---|---|
| Saper | Trapper, Control | Po place głównej bomby: arm **1.00 s**, trigger r **1.60 m** (proximity). Działa równolegle z lontem — wróg może odpalić minę wcześniej. Zwykłe podejście bez talentu **nie** detonuje. |
| Piroman | Fire, Demolition | Ofensywne wybuchy gracza nakładają burn: **2 dmg / 0.50 s**, duration **2.00 s** (refresh, 0 stack magnitude). Tick/consume **nie** nakłada burnu (D8). |

Follow-up 1:1:

| Lvl 2 | Karta | Tagi | Efekt |
|---|---|---|---|
| Kasetowa | Mini-torpedy | Seeking, Chain | Child homing **8 m/s**, contact r **0.45 m**. Retarget gdy cel martwy. Split gdy **≥3** żywych w **8 m**. |
| Większy Huk | Ładunek kumulacyjny | Basic, Demolition | Per-target stacks: max **4**, mul **1.00 / 1.20 / 1.40 / 1.60**, reset **1.75 s** lub zmiana celu. |
| Wybuch ogłuszający | Kula bilardowa | Physics, Collision | Kopnięcie → billiard: **11 m/s**, max **3** coll / **0.85 s** / **6.5 m**. Elita: cap 1, speed ×0.5. Boss: 0 launch, explode on contact. |

## Lvl 4 — ulti (wszystkie 3, bez filtra L2/L3)

| Karta | ID skill | Tagi | Typ | Efekt |
|---|---|---|---|---|
| Szybkostrzelność | `bomberman_szybkostrzelnosc` | Basic, RapidFire | aktywne | **6.0 s** override AA **0.22 s** (9 dmg, extra 0). CD **28 s**. |
| Orbitale | `bomberman_orbitale` | Orbital, Physics | aktywne | **4** sloty, orbit **2.2 m** / **140°/s**, dmg **12** / r **1.80 m**, recharge **5.0 s**/slot. Kick orbital dziedziczy persistents. |
| Nalot | `bomberman_nalot` | Airstrike, Zone | aktywne | Telegraph **1.00 s**, **7** wybuchów / **2.20 m**, dmg **14** / r **2.00 m**, sekwencja **1.70 s**, CD **30 s**, 0 FF. |

## Lvl 5 — capstone (eligibility lock)

| Karta | Wymaga | Tagi (karta) |
|---|---|---|
| Reakcja termiczna | `kasetowa` + `piroman` | Fire, Demolition |
| Feniks | `kasetowa` + `piroman` | Fire, Chain |
| Polowanie stadne | `mini_torpedy` | Seeking, Demolition |
| Termobaryczny | `wiekszy_huk` + `piroman` | Fire, AOE |
| Rozniecanie | `wiekszy_huk` + `piroman` | Fire, AOE |
| Karabin maszynowy | `requiresTagCounts: Basic ≥ 2` (**bez** `requiresUltimateId`) | Basic, RapidFire |
| Przełamanie | `ladunek_kumulacyjny` | Demolition, Control |
| Treser bomb | `wybuch_ogluszajacy` + `saper` | Trapper, Physics |
| Płonący taran | `wybuch_ogluszajacy` + `piroman` | Fire, Physics |
| BREAK! | `kula_bilardowa` | Physics, Collision |
| Łańcuch kolizji | `kula_bilardowa` | Physics, Collision |
| Przeładowany magazynek | ulti `bomberman_szybkostrzelnosc` + tag `Basic` | RapidFire, AOE |
| Reakcja orbitalna | ulti `bomberman_orbitale` + tag `Orbital` | Orbital, RapidFire |
| Kasetowe satelity | ulti `bomberman_orbitale` + `kasetowa` | Orbital, Chain |
| Układ Planetarny | ulti `bomberman_orbitale` + tag `Physics` | Orbital, Physics |
| Nalot z opóźnionym zapłonem | ulti `bomberman_nalot` + tag `Airstrike` | Airstrike, Demolition |
| Ostatnia bomba | ulti `bomberman_nalot` + tag `Zone` | Zone, AOE |
| 10 BOMB | `requiresAnyTags: Demolition, Trapper` | Demolition, Trapper |

**Karabin maszynowy:** jedna karta, `Basic ≥ 2`, **bez** `requiresUltimateId`. Permanent interval **0.22 s**, dmg mul **×0.45**. Tylko ścieżki H (Basic z ulti Rapid lub z Ładunku).

**10 BOMB:** `requiresAnyTags: Demolition | Trapper` — cap **10**, 11. detonuje najstarszy Normal. Brak fallback UI.

### Graf 27 buildów (L2 × L3 × L4)

Skróty: L2 **K** kasetowa, **H** huk, **O** ogłuszający. L3 **S** saper, **P** piroman, **T** mini-torpedy, **L** ładunek, **B** kula. L4 **R** szybkostrzelność, **Orb** orbitale, **N** nalot.

| # | Build | L5 eligible | N |
|---|---|---|---|
| 1 | K S R | Przeładowany, 10 BOMB | 2 |
| 2 | K S Orb | Reakcja orbitalna, Satelity, Planetarny, 10 BOMB | 4 |
| 3 | K S N | Nalot opóźniony, Ostatnia, 10 BOMB | 3 |
| 4 | K P R | Reakcja termiczna, Feniks, Przeładowany, 10 BOMB | 4 |
| 5 | K P Orb | Termiczna, Feniks, Orbitalna, Satelity, Planetarny, 10 BOMB | 6 |
| 6 | K P N | Termiczna, Feniks, Nalot opóźniony, Ostatnia, 10 BOMB | 5 |
| 7 | K T R | Polowanie, Przeładowany, 10 BOMB | 3 |
| 8 | K T Orb | Polowanie, Orbitalna, Satelity, Planetarny, 10 BOMB | 5 |
| 9 | K T N | Polowanie, Nalot opóźniony, Ostatnia, 10 BOMB | 4 |
| 10 | H S R | Karabin, Przeładowany, 10 BOMB | 3 |
| 11 | H S Orb | Orbitalna, Planetarny, 10 BOMB | 3 |
| 12 | H S N | Nalot opóźniony, Ostatnia, 10 BOMB | 3 |
| 13 | H P R | Termobaryczny, Rozniecanie, Karabin, Przeładowany | 4 |
| 14 | H P Orb | Termobaryczny, Rozniecanie, Orbitalna, Planetarny | 4 |
| 15 | H P N | Termobaryczny, Rozniecanie, Nalot opóźniony, Ostatnia | 4 |
| 16 | H L R | Karabin, Przełamanie, Przeładowany, 10 BOMB | 4 |
| 17 | H L Orb | Karabin, Przełamanie, Orbitalna, Planetarny, 10 BOMB | 5 |
| 18 | H L N | Karabin, Przełamanie, Nalot opóźniony, Ostatnia, 10 BOMB | 5 |
| 19 | O S R | Treser, Przeładowany, 10 BOMB | 3 |
| 20 | O S Orb | Treser, Orbitalna, Planetarny, 10 BOMB | 4 |
| 21 | O S N | Treser, Nalot opóźniony, Ostatnia, 10 BOMB | 4 |
| 22 | O P R | Płonący taran, Przeładowany | 2 |
| 23 | O P Orb | Płonący taran, Orbitalna, Planetarny | 3 |
| 24 | O P N | Płonący taran, Nalot opóźniony, Ostatnia | 3 |
| 25 | O B R | BREAK!, Łańcuch, Przeładowany | 3 |
| 26 | O B Orb | BREAK!, Łańcuch, Orbitalna, Planetarny | 4 |
| 27 | O B N | BREAK!, Łańcuch, Nalot opóźniony, Ostatnia | 3 |

Min **N = 2**. Żaden build bez L5. Karabin tylko gdy Basic≥2 (ścieżki H, nie K/O).
