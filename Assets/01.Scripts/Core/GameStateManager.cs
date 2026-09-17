using UnityEngine;

internal sealed class GameStateManager
{
    private readonly GameManager _game;
    private readonly int[] _lives =
        new int[PlayerRoster.Count];

    private int _score;
    private int _phase;
    private bool _isGameOver;
    private bool _isChangingPhase;
    private PlayerNumber? _roundWinner;

    internal int Score => _score;
    internal int Phase => _phase;
    internal bool IsGameOver => _isGameOver;
    internal bool IsChangingPhase => _isChangingPhase;
    internal bool IsPlaying =>
        !_isGameOver && !_isChangingPhase;

    internal PlayerNumber? RoundWinner =>
        _roundWinner;

    internal GameStateManager(GameManager game)
    {
        _game = game;
        Reset();
    }

    internal int GetLives(
        PlayerNumber playerNumber)
    {
        return _lives[(int)playerNumber];
    }

    internal void RegisterEnemyDefeat()
    {
        if (!IsPlaying)
        {
            return;
        }

        _score += _game.EnemyScore;
    }

    internal bool RegisterPlayerDefeat(
        PlayerNumber defeatedPlayer)
    {
        if (!IsPlaying)
        {
            return false;
        }

        int defeatedIndex = (int)defeatedPlayer;

        _lives[defeatedIndex] = Mathf.Max(
            0,
            _lives[defeatedIndex] - 1);

        _roundWinner =
            GetOpponent(defeatedPlayer);

        if (_lives[defeatedIndex] <= 0)
        {
            _isGameOver = true;
            return true;
        }

        _isChangingPhase = true;
        return true;
    }

    internal void AdvancePhase()
    {
        _phase++;
    }

    internal void CompletePhaseChange()
    {
        _isChangingPhase = false;
        _roundWinner = null;
    }

    internal void Reset()
    {
        _score = 0;
        _phase = 1;
        _isGameOver = false;
        _isChangingPhase = false;
        _roundWinner = null;

        for (int index = 0;
             index < _lives.Length;
             index++)
        {
            _lives[index] =
                _game.StartingLives;
        }
    }

    private static PlayerNumber GetOpponent(
        PlayerNumber playerNumber)
    {
        return playerNumber == PlayerNumber.One
            ? PlayerNumber.Two
            : PlayerNumber.One;
    }
}