# Siege Defense — pivot trybu

Siege Defense jest osobną, pięciofalową sceną obrony twierdzy. Drużyna zaczyna od wyboru dużej nagrody, następnie ma przygotowanie i uruchamia falę. Po falach 1 i 3 wybierane jest ulepszenie umiejętności, po 2 i 4 talent. Fala 5 kończy się zwycięstwem.

Arena ma dwa fronty. Najpierw wrogowie nacierają na Bramę; jej zniszczenie uruchamia trzysekundowy dobrowolny odwrót do wnętrza, po czym cel staje się Sercem Twierdzy. Zniszczenie Serca albo śmierć wszystkich graczy kończy bieg. Respawn zawsze wraca do aktywnego frontu.

Każda fala pokazuje telegraph grupy, licznik żywych wrogów i budżet. Po oczyszczeniu pojawia się złoty, widoczny drop gwarantowanej nagrody (proximity lub Enter); jeśli gracz go nie podniesie, zostaje odebrany automatycznie przy dalszym przebiegu nagrody. CombatHudView zachowuje dolny panel HP/skills, a SiegeHudView prowadzi górny chrome.

## Konfiguracja i uruchomienie

W Unity wybierz `Game/Siege/Create SiegeDefense Scene`. Helper kopiuje BootScene do `Assets/Scenes/SiegeDefense.unity`, usuwa legacy map/waves/camerę/test spawnerów i dodaje `SiegeArena`, `SiegeWaveDirector`, nagrody, kontroler, HUD oraz kamerę. BootScene nie jest zapisywana. Konfiguracja areny jest zapisana jako `Assets/_Game/Config/SiegeArenaConfig.asset`; harmonogram jest ładowany z `Assets/_Game/Resources/Siege/SiegeWaves.json`.

## Sterowanie

P1 wybiera nagrody cyfrą i Enter/Space, pady używają d-pada i South. Enter lub Start uruchamia przygotowanie/falę. W terminalnym ekranie Enter restartuje scenę.
