using UnityEngine;

/// <summary>Czytelny chrome Siege; dolny CombatHudView pozostaje właścicielem HP/skill UI.</summary>
[DisallowMultipleComponent]
public sealed class SiegeHudView : MonoBehaviour
{
    private SiegeRunController _run;
    private SiegeWaveDirector _director;
    private SiegeArena _arena;
    private GUIStyle _label, _title, _small;
    private void Awake() { _run = FindAnyObjectByType<SiegeRunController>(); _director = FindAnyObjectByType<SiegeWaveDirector>(); _arena = FindAnyObjectByType<SiegeArena>(); }
    private void OnGUI()
    {
        if (_run == null || _arena == null) return;
        var scale = Mathf.Max(.7f, Mathf.Min(Screen.width / 1600f, Screen.height / 900f)); var old = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
        var width = 1600f; _label ??= Style(22, FontStyle.Bold, Color.white); _title ??= Style(30, FontStyle.Bold, new Color(1f, .78f, .3f)); _small ??= Style(17, FontStyle.Normal, new Color(.75f, .84f, .9f));
        Fill(new Rect(22, 18, width - 44, 112), new Color(.03f, .06f, .1f, .94f));
        var wave = Mathf.Clamp(_run.CurrentWave + 1, 1, 5); GUI.Label(new Rect(42, 30, 470, 40), $"OBLĘŻENIE  •  FALA {wave}/5", _title);
        GUI.Label(new Rect(42, 72, 500, 30), _director != null && _director.CurrentWave != null ? _director.CurrentWave.Title : "Przygotuj obronę", _small);
        Bar(new Rect(600, 38, 310, 18), _arena.Gate, "BRAMA"); Bar(new Rect(600, 78, 310, 18), _arena.Core, "SERCE");
        if (_director != null) GUI.Label(new Rect(940, 38, 330, 28), $"Wrogowie: {_director.AliveEnemyCount}", _label);
        if (_director != null && _director.CurrentWave != null) GUI.Label(new Rect(940, 75, 330, 25), $"Pozostałe posiłki: {Mathf.Max(0, _director.TotalBudget - _director.TotalSpawned)}", _small);
        if (_run.PreparationRemaining > 0f) GUI.Label(new Rect(width - 330, 34, 290, 32), $"START ZA {_run.PreparationRemaining:0.0}s", _label);
        if (_run.RetreatRemaining > 0f) GUI.Label(new Rect(42, 180, 560, 30), $"ODWRÓT DO RDZENIA: {_run.RetreatRemaining:0.0}s", _label);
        else if (!string.IsNullOrEmpty(_run.LastWarning)) GUI.Label(new Rect(42, 180, 700, 30), $"⚠ {_run.LastWarning}", _label);
        if (_run.PendingDrop != null && !_run.PendingDrop.Collected) GUI.Label(new Rect(42, 145, 540, 28), "◆ GWARANTOWANA NAGRODA — podejdź lub Enter", _label);
        if (_run.IsTerminal)
        {
            Fill(new Rect(0, 0, width, 900), new Color(.01f, .015f, .03f, .78f));
            GUI.Label(new Rect(0, 300, width, 70), _run.IsVictory ? "TWIERDZA OBRONIONA" : "OBRONA ZAKOŃCZONA", _title);
            GUI.Label(new Rect(0, 380, width, 35), "Enter / przycisk Start — zagraj ponownie", new GUIStyle(_small) { alignment = TextAnchor.MiddleCenter });
            if (GUI.Button(new Rect(width * .5f - 150, 445, 300, 58), "ZAGRAJ PONOWNIE")) _run.Restart();
            var restartPad = false;
            foreach (var pad in UnityEngine.InputSystem.Gamepad.all) restartPad |= pad.startButton.wasPressedThisFrame;
            if (UnityEngine.InputSystem.Keyboard.current?.enterKey.wasPressedThisFrame == true || restartPad) _run.Restart();
        }
        GUI.matrix = old;
    }
    private static GUIStyle Style(int size, FontStyle style, Color color) { var s = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = style }; s.normal.textColor = color; return s; }
    private static void Bar(Rect rect, Health health, string text)
    {
        Fill(rect, new Color(.12f, .16f, .2f)); var ratio = health != null ? Mathf.Clamp01(health.CurrentHealth / Mathf.Max(1f, health.MaxHealth)) : 0f; Fill(new Rect(rect.x, rect.y, rect.width * ratio, rect.height), text == "BRAMA" ? new Color(.95f, .42f, .18f) : new Color(.2f, .75f, .9f)); GUI.Label(new Rect(rect.x + 8, rect.y - 3, rect.width, 24), $"{text}  {(health != null ? health.CurrentHealth : 0):0}/{(health != null ? health.MaxHealth : 0):0}", Style(14, FontStyle.Bold, Color.white));
    }
    private static void Fill(Rect rect, Color color) { GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white; }
}
