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
