# Game Feel

## Status
**FROZEN dla zasad, DRAFT dla parametrów**

Gra ma być responsywna i czytelna nawet na placeholderach. Feel należy do **klasy i umiejętności**, nie do lootowanej broni.

Wstrząs kamery: siła efektu i położenie zdarzenia na wspólnej kamerze — nie „czy to ty użyłeś”. Wibracja pada może być indywidualna. Szczegóły: `docs/systems/SKILLS.md`.

DRAFT — archetypy ataku podstawowego (historyczne rodziny broni, do podpięcia pod klasy w M7.5+):

- lekki (sztylet): bardzo szybki rytm, prawie brak zatrzymania trafienia, niski odrzut,
- średni (miecz): szybki start, średni odrzut, niewielkie zatrzymanie trafienia,
- ciężki (topór): wyraźne przygotowanie, mocny moment trafienia, zauważalne zatrzymanie, duży odrzut,
- dystans (łuk): czytelny lot pocisku i trafienie, mały wstrząs kamery.

Dozwolone narzędzia feedbacku:
- zatrzymanie trafienia,
- odrzut,
- błysk celu,
- efekt cząsteczkowy,
- dźwięk trafienia,
- animacja reakcji,
- wibracja pada,
- wstrząs kamery.

Nie wszystkie efekty muszą występować jednocześnie.

Game feel wymaga ludzkiego playtestu; agent nie może sam uznać go za dobry na podstawie kodu.

## Parametry DRAFT (Combat Readability Pass)

Fazy to ułamki `AttackInterval` (suma = 1). Liczby dmg/interval/range nie są tutaj — zostają w contentcie.

| Rodzina | Bronie | Windup | Active | Recovery | Move mul (windup+active) | Hit-stop (s) | Knockback | Shake |
|---------|--------|--------|--------|----------|--------------------------|--------------|-----------|-------|
| Dagger | sztylet, Viper Fang | 0.08 | 0.12 | 0.80 | 1.00 | 0.00 | 0.15 | 0.00 |
| Sword | miecz, pika, Knight's Edge | 0.18 | 0.12 | 0.70 | 0.55 | 0.04 | 0.60 | 0.04 |
| Axe | wielki topór, młot | 0.38 | 0.10 | 0.52 | 0.25 | 0.10 | 1.80 | 0.12 |
| Bow | łuk | 0.28 | 0.00 | 0.72 | 0.40 | 0.03 | 0.20 | 0.05 |

Łuk: pocisk na końcu windupu, prędkość 20 j/s, max dystans = `Range`. Hit-stop nie ustawia `timeScale`, gdy gra jest już spauzowana (`timeScale == 0`).

Kamera: shake z tabeli jest skalowany (`CameraShakeMath.VisualScale`) i gaszony po narysowaniu klatki — surowe 0.04–0.12 nie są przesunięciem świata 1:1.
Odrzut: siła z tabeli × `KnockbackMath.ImpulseScale`, damping 3 — topór ma być czytelnie mocniejszy od sztyletu.

## DRAFT — Combat Playability (celowanie)

- P1 klawiatura+mysz: kursor na płaszczyźnie XZ ustawia kierunek ataku i facing.
- Pad: prawy stick = look (względem kamery); lewy stick = tylko ruch. Martwa strefa prawego sticka: `AimMath.RightStickDeadzone` (0.25).
- Brak aim inputu: facing z wektora ruchu; bez ruchu zostaje ostatni kierunek.
- Lock facing **tylko w fazie Active**. Windup śledzi aim. Recovery odblokowany.
- Łuk (active = 0) nie lockuje; pocisk leci w aim z końca windupu.
