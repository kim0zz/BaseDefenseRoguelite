# Definition of Done

Feature jest DONE dopiero gdy:
1. acceptance criteria przechodzą,
2. brak znanych blockerów,
3. brak regresji w istniejących funkcjach krytycznych,
4. build uruchamia się,
5. feature można przetestować ręcznie bez edytowania kodu,
6. wartości konfiguracyjne nie są bez potrzeby zahardkodowane,
7. dokumentacja została zaktualizowana, jeśli zmieniło się zachowanie,
8. reviewer/verifier sprawdził zmianę,
9. nie naruszono decyzji FROZEN,
10. commit jest mały i opisuje jeden logiczny zakres.

## Dodatkowe wymagania dla funkcji widocznych w gameplayu
- funkcja ma podstawowy feedback wizualny lub placeholder,
- interakcja jest czytelna bez konsoli/logów,
- sprawdzono `VISUAL_FEEDBACK_CHECKLIST.md`,
- game feel wymaga playtestu człowieka.
