using TMPro;
using UnityEngine;

public sealed class BalloonHud : MonoBehaviour
{
    [Header("게임 진행 표시")]
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _phaseText;
    [SerializeField] private TMP_Text _playerOneLivesText;
    [SerializeField] private TMP_Text _playerTwoLivesText;
    [SerializeField] private TMP_Text _enemyCountText;

    [Header("조작법 표시")]
    [SerializeField] private TMP_Text _playerOneControlsText;
    [SerializeField] private TMP_Text _playerTwoControlsText;

    [Header("중앙 메시지")]
    [SerializeField] private GameObject _messagePanel;
    [SerializeField] private TMP_Text _messageTitleText;
    [SerializeField] private TMP_Text _messageSubtitleText;

    internal bool IsConfigured =>
        _scoreText != null
        && _phaseText != null
        && _playerOneLivesText != null
        && _playerTwoLivesText != null
        && _enemyCountText != null
        && _playerOneControlsText != null
        && _playerTwoControlsText != null
        && _messagePanel != null
        && _messageTitleText != null
        && _messageSubtitleText != null;

    internal void Refresh(
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
        _scoreText.text = $"SCORE {score:000000}";
        _phaseText.text = $"PHASE {phase}";
        _playerOneLivesText.text = $"P1 LIVES {playerOneLives}";
        _playerTwoLivesText.text = $"P2 LIVES {playerTwoLives}";
        _enemyCountText.text = $"ENEMIES {enemyCount}";

        _playerOneControlsText.text =
            $"P1  {GetControlsLabel(PlayerNumber.One, input)}";

        _playerTwoControlsText.text =
            $"P2  {GetControlsLabel(PlayerNumber.Two, input)}";

        RefreshMessage(
            isChangingPhase,
            isGameOver,
            isAllClear,
            input);
    }

    private void RefreshMessage(
        bool isChangingPhase,
        bool isGameOver,
        bool isAllClear,
        PlayerInput input)
    {
        if (isGameOver)
        {
            ShowMessage(
                "GAME OVER",
                $"Press {input.Restart} to restart");

            return;
        }

        if (isAllClear)
        {
            ShowMessage(
                "ALL CLEAR",
                $"Press {input.Restart} to restart");

            return;
        }

        if (isChangingPhase)
        {
            ShowMessage(
                "PHASE CLEAR",
                "Next phase incoming");

            return;
        }

        _messagePanel.SetActive(false);
    }

    private void ShowMessage(string title, string subtitle)
    {
        _messageTitleText.text = title;
        _messageSubtitleText.text = subtitle;
        _messagePanel.SetActive(true);
    }

    private static string GetControlsLabel(
        PlayerNumber playerNumber,
        PlayerInput input)
    {
        string left = string.Join(
            "/",
            input.GetLeftKeys(playerNumber));

        string right = string.Join(
            "/",
            input.GetRightKeys(playerNumber));

        string flap = string.Join(
            "/",
            input.GetFlapKeys(playerNumber));

        return $"{left} / {right} : move    {flap} : flap";
    }
}