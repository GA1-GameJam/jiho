using System.Collections.Generic;
using UnityEngine;

internal sealed class FighterSpawner
{
    private readonly GameManager _game;
    private readonly Transform _parent;
    private readonly GameObject[] _playerPrefabs;
    private readonly List<EnemyController> _enemies = new();
    private readonly PlayerController[] _players = new PlayerController[PlayerRoster.Count];
    private readonly BalloonEnemyPool _enemyPool;

    internal int ActiveEnemyCount => _enemies.Count;

    internal FighterSpawner(Transform parent, GameManager game, GameObject[] playerPrefabs, GameObject[] enemyPrefabs)
    {
        _parent = parent;
        _game = game;
        _playerPrefabs = playerPrefabs;
        _enemyPool = new BalloonEnemyPool(parent, enemyPrefabs, game.EnemyPoolCapacity);
    }

    internal void SpawnAllPlayers()
    {
        SpawnPlayer(PlayerNumber.One);
        SpawnPlayer(PlayerNumber.Two);
    }

    internal PlayerController SpawnPlayer(PlayerNumber playerNumber)
    {
        int index = (int)playerNumber;
        if (_players[index] == null) _players[index] = FighterFactory.CreatePlayer(_parent, _playerPrefabs, playerNumber);
        PlayerController player = _players[index];
        player.gameObject.SetActive(true);
        player.transform.position = _game.GetPlayerSpawn(playerNumber);
        player.InitializePlayer(_game, playerNumber);
        return player;
    }

    internal void SpawnPhase(int phase)
    {
        int enemyCount = Mathf.Min(phase + _game.PhaseEnemyOffset, _game.EnemySpawns.Length);
        for (int index = 0; index < enemyCount; index++)
        {
            EnemyController enemy = _enemyPool.Get(_game.EnemySpawns[index]);
            enemy.Initialize(_game);
            _enemies.Add(enemy);
        }
    }

    internal bool RemoveEnemy(EnemyController enemy) => enemy != null && _enemies.Remove(enemy);

    internal PlayerController GetNearestPlayer(Vector3 position)
    {
        PlayerController nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (PlayerController player in _players)
        {
            if (player == null || !player.IsAvailable) continue;
            float distance = (player.transform.position - position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = player;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    internal void Reset()
    {
        foreach (PlayerController player in _players) player?.gameObject.SetActive(false);
        _enemyPool.ReleaseAll(_enemies);
        _enemies.Clear();
    }

    internal void Clear() => _enemyPool.Clear();
}
