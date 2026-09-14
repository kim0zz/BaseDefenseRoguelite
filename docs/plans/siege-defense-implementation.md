# Plan implementacji Siege Defense

1. Utworzyć scenę helperem i sprawdzić, że istnieje tylko jedna arena, kamera Siege, director, reward controller, run controller i HUD.
2. Uruchomić Play: poczekać na PlayerJoinManager, przejść początkowy wybór, uruchomić falę Enterem i obserwować telegraphy oraz budżet.
3. Zniszczyć Bramę testowym damage: spawny muszą się zatrzymać, po 3 s arena przechodzi do wnętrza, a gracze trafiają na nowe punkty.
4. Zweryfikować nagrodę po każdej fali: złoty drop jest widoczny, proximity/Enter go odbiera, a wybór pojawia się tylko raz.
5. Zweryfikować fale 1–5, wybory małe/duże, zwycięstwo, śmierć rdzenia i wipe. Terminal zatrzymuje czas oraz respawny; Enter/Start uruchamia restart wyłącznie wtedy.

Ten pivot świadomie izoluje Siege od starego GameFlowManager/WaveManager. Stare assety wizualne i CombatBootstrap/BuildSystemBootstrap pozostają w kopii sceny, aby zachować prefabrykaty i klasy graczy.
