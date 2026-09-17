using UnityEngine;

public sealed class BalloonHud : MonoBehaviour
{
    [SerializeField] private Vector2 _referenceSize = new(1280f, 720f);
    [SerializeField] private int _hudFontSize = 22;
    [SerializeField] private int _smallFontSize = 15;
    [SerializeField] private int _titleFontSize = 40;
    [SerializeField] private int _subtitleFontSize = 20;
    [SerializeField] private Color _textColor = Color.white;
    [SerializeField] private Color _smallColor = new(0.82f, 0.9f, 1f);
    [SerializeField] private Color _titleColor = new Color32(255, 229, 102, 255);
    [SerializeField] private Rect _scoreRect = new(20f, 15f, 280f, 35f);
    [SerializeField] private Rect _phaseRect = new(570f, 15f, 150f, 35f);
    [SerializeField] private Rect _playerOneLivesRect = new(1030f, 15f, 230f, 35f);
    [SerializeField] private Rect _playerTwoLivesRect = new(1030f, 48f, 230f, 35f);
    [SerializeField] private Rect _enemiesRect = new(20f, 48f, 230f, 28f);
    [SerializeField] private Rect _playerOneControlsRect = new(20f, 636f, 1240f, 30f);
    [SerializeField] private Rect _playerTwoControlsRect = new(20f, 668f, 1240f, 30f);
    [SerializeField] private Rect _panelRect = new(425f, 290f, 430f, 140f);
    [SerializeField] private Rect _titleRect = new(425f, 305f, 430f, 60f);
    [SerializeField] private Rect _subtitleRect = new(425f, 370f, 430f, 35f);
    private GUIStyle _hudStyle;
    private GUIStyle _smallStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _subtitleStyle;

    internal void Draw(
        int score,
        int phase,
        int playerOneLives,
        int playerTwoLives,
        int enemyCount,
        bool isChangingPhase,
        bool isGameOver,
        bool isAllClear,
        PlayerInput input)
    {
        EnsureStyles();
        Matrix4x4 previous = GUI.matrix;
        Vector2 reference = _referenceSize;
        Vector3 scale = new(
            Screen.width / Mathf.Max(1f, reference.x),
            Screen.height / Mathf.Max(1f, reference.y),
            1f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
        try
        {
            GUI.Label(_scoreRect, $"SCORE {score:000000}", _hudStyle);
            GUI.Label(_phaseRect, $"PHASE {phase}", _hudStyle);
            GUI.Label(_playerOneLivesRect, $"P1 LIVES {playerOneLives}", _hudStyle);
            GUI.Label(_playerTwoLivesRect, $"P2 LIVES {playerTwoLives}", _hudStyle);
            GUI.Label(_enemiesRect, $"ENEMIES {enemyCount}", _smallStyle);
            string playerOneControls = string.Format(
                "P1  {0}", GetControlsLabel(PlayerNumber.One, input));
            string playerTwoControls = string.Format(
                "P2  {0}", GetControlsLabel(PlayerNumber.Two, input));
            GUI.Label(_playerOneControlsRect, playerOneControls, _smallStyle);
            GUI.Label(_playerTwoControlsRect, playerTwoControls, _smallStyle);
            if (isGameOver || isAllClear)
            {
                DrawCenter(isGameOver ? "GAME OVER" : "ALL CLEAR", $"Press {input.Restart} to restart");
            }
            else if (isChangingPhase)
            {
                DrawCenter("PHASE CLEAR", "Next phase incoming");
            }
        }
        finally
        {
            GUI.matrix = previous;
        }
    }

    private void EnsureStyles()
    {
        if (_hudStyle != null)
        {
            return;
        }

        _hudStyle = CreateStyle(_hudFontSize, _textColor, true, false);
        _smallStyle = CreateStyle(_smallFontSize, _smallColor, false, false);
        _titleStyle = CreateStyle(_titleFontSize, _titleColor, true, true);
        _subtitleStyle = CreateStyle(_subtitleFontSize, _textColor, false, true);
    }

    private static GUIStyle CreateStyle(int size, Color color, bool bold, bool centered)
    {
        GUIStyle style = new(GUI.skin.label);
        style.fontSize = size;
        style.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        style.alignment = centered ? TextAnchor.MiddleCenter : TextAnchor.UpperLeft;
        style.normal.textColor = color;
        return style;
    }

    private static string GetControlsLabel(PlayerNumber playerNumber, PlayerInput input)
    {
        string left = string.Join("/", input.GetLeftKeys(playerNumber));
        string right = string.Join("/", input.GetRightKeys(playerNumber));
        string flap = string.Join("/", input.GetFlapKeys(playerNumber));
        return $"{left} / {right} : move    {flap} : flap";
    }

    private void DrawCenter(string title, string subtitle)
    {
        GUI.Box(_panelRect, GUIContent.none);
        GUI.Label(_titleRect, title, _titleStyle);
        GUI.Label(_subtitleRect, subtitle, _subtitleStyle);
    }
}
