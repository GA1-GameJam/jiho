using UnityEngine;

internal sealed class GameStateManager
{
    private readonly GameManager _game;
    private readonly int[] _lives = new int[PlayerRoster.Count];
    private int _score;
    private int _phase;
    private bool _isGameOver;
    private bool _isAllClear;
    private bool _isChangingPhase;

    internal int Score => _score;
    internal int Phase => _phase;
    internal bool IsGameOver => _isGameOver;
    internal bool IsAllClear => _isAllClear;
    internal bool IsChangingPhase => _isChangingPhase;
    internal bool IsPlaying => !_isGameOver && !_isAllClear;

    internal GameStateManager(GameManager game)
    {
        _game = game;
        Reset();
    }

    internal int GetLives(PlayerNumber playerNumber) => _lives[(int)playerNumber];

    internal bool RegisterEnemyDefeat(int activeEnemyCount)
    {
        _score += _game.EnemyScore;
        if (activeEnemyCount > 0 || _isChangingPhase || !IsPlaying) return false;
        if (_phase >= _game.MaximumPhase)
        {
            _isAllClear = true;
            return false;
        }

        _isChangingPhase = true;
        return true;
    }

    internal bool RegisterPlayerDefeat(PlayerNumber playerNumber)
    {
        int index = (int)playerNumber;
        _lives[index] = Mathf.Max(0, _lives[index] - 1);
        if (_lives[index] > 0) return true;
        if (AllPlayersEliminated()) _isGameOver = true;
        return false;
    }

    internal bool CanRespawn(PlayerNumber playerNumber) => IsPlaying && _lives[(int)playerNumber] > 0;
    internal void AdvancePhase() => _phase++;
    internal void CompletePhaseChange() => _isChangingPhase = false;

    internal void Reset()
    {
        _score = 0;
        _phase = 1;
        _isGameOver = false;
        _isAllClear = false;
        _isChangingPhase = false;
        for (int index = 0; index < _lives.Length; index++) _lives[index] = _game.StartingLives;
    }

    private bool AllPlayersEliminated()
    {
        foreach (int life in _lives)
        {
            if (life > 0) return false;
        }

        return true;
    }
}
