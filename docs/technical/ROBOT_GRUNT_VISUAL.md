# Robot podstawowego przeciwnika — integracja

RobotDefender jest modelem wizualnym typu Grunt, nie nową klasą ani nowym typem wroga.
Przypisano go do ośmiu istniejących definicji Grunta. Po Play w BootScene używają go standardowe spawny fal. Inne typy wrogów zachowują kapsuły.

EnemyDefinition przechowuje opcjonalny visualPrefab i visualOffset. EnemySpawner dodaje model jako dziecko, ukrywa renderer kapsuły i zachowuje collider. Brak prefabu zachowuje dotychczasowy wygląd. EnemyModelView wiąże prędkość z Animator.Speed, zdarzenie EnemyController.Attacked z Attack i Health.Died z Death. Błysk trafienia działa na materiałach modelu przez MaterialPropertyBlock. Statystyki, nagrody, logika celowania i czas usuwania wroga pozostają takie jak wcześniej.

Otwórz BootScene i naciśnij Play. Roboty pojawiają się jako Grunty od pierwszej fali. Model nie jest graczem; kolorystyka stanowi prototyp do oceny. Istniejące obiekty testowe Capsule bez przypisanej definicji assetowej pozostają kapsułami.

Weryfikacja w edytorze i Play: kompilacja bez błędów, Grunt fali 1 ma Animator, ukryty renderer kapsuły, jeden collider i szybkość około 4.1; HP po testowym trafieniu spada z 18 do 17. Zweryfikowano powiązanie śmierci z animacją. Nie wykonano pełnego buildu ani długiej regresji wszystkich fal. Ocena czytelności ataku i wyglądu pozostaje do playtestu użytkownika.
