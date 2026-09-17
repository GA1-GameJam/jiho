using System.Collections;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [Header("게임 진행")]
    [SerializeField] private int _targetFrameRate = 60;
    [SerializeField] private int _startingLives = 3;
    [SerializeField] private int _maximumPhase = 3;
    [SerializeField] private int _enemyScore = 500;
    [SerializeField] private float _phaseDelay = 1.2f;
    [SerializeField] private float _respawnDelay = 1.1f;
    [SerializeField] private float _respawnInvincibility = 1.4f;

    [Header("카메라와 스폰")]
    [SerializeField] private Camera _gameCamera;
    [SerializeField] private int _phaseEnemyOffset = 2;
    [SerializeField] private int _enemyPoolCapacity = 5;
    [SerializeField] private Vector2[] _playerSpawns = { new(-1f, -3.35f), new(1f, -3.35f) };
    [SerializeField] private Vector2[] _enemySpawns =
    {
        new(-4.7f, 0.2f), new(4.7f, 0.45f), new(-1.8f, 2.7f), new(1.9f, 3f), new(0f, 0.65f)
    };

    [Header("선택 프리팹")]
    [SerializeField] private GameObject[] _playerPrefabs;
    [SerializeField] private GameObject[] _enemyPrefabs;

    [Header("기능 컴포넌트")]
    [SerializeField] private PlayerInput _input;
    [SerializeField] private MapBoundary _mapBoundary;
    [SerializeField] private BalloonHud _hud;
    [SerializeField] private BalloonPopEffect _popEffect;
    [SerializeField] private RetroFactory _retroFactory;

    private GameStateManager _gameStateManager;
    private BalloonPopPool _popPool;
    private FighterSpawner _spawner;

    internal PlayerInput Input => _input;
    internal bool IsPlaying => _gameStateManager.IsPlaying;
    internal int StartingLives => _startingLives;
    internal int MaximumPhase => _maximumPhase;
    internal int EnemyScore => _enemyScore;
    internal int PhaseEnemyOffset => _phaseEnemyOffset;
    internal int EnemyPoolCapacity => _enemyPoolCapacity;
    internal Vector2[] EnemySpawns => _enemySpawns;

    private void Awake()
    {
        if (!HasRequiredComponents())
        {
            enabled = false;
            return;
        }

        RetroFactory.Configure(_retroFactory);
        _gameStateManager = new GameStateManager(this);
    }

    private void Start()
    {
        Application.targetFrameRate = _targetFrameRate;
        if (_gameCamera == null)
        {
            _gameCamera = Camera.main;
        }

        if (_gameCamera == null)
        {
            Debug.LogError("GameManager에 Main Camera를 연결해주세요.", this);
            enabled = false;
            return;
        }

        _mapBoundary.SetCamera(_gameCamera);
        _spawner = new FighterSpawner(transform, this, _playerPrefabs, _enemyPrefabs);
        _popPool = new BalloonPopPool(transform, _popEffect, RetroFactory.GetSquare());
        _spawner.SpawnAllPlayers();
        _spawner.SpawnPhase(_gameStateManager.Phase);
    }

    private void Update()
    {
        if (!_gameStateManager.IsPlaying && _input.RestartPressed)
        {
            Restart();
        }
    }

    private void OnGUI()
    {
        if (_spawner == null)
        {
            return;
        }

        _hud?.Draw(_gameStateManager.Score, _gameStateManager.Phase,
            _gameStateManager.GetLives(PlayerNumber.One), _gameStateManager.GetLives(PlayerNumber.Two),
            _spawner.ActiveEnemyCount, _gameStateManager.IsChangingPhase,
            _gameStateManager.IsGameOver, _gameStateManager.IsAllClear, _input);
    }

    private void OnDestroy()
    {
        _popPool?.Clear();
        _spawner?.Clear();
    }

    internal void BalloonPopped(Vector3 position) => _popPool?.Play(position);
    internal PlayerController GetNearestPlayer(Vector3 position) => _spawner.GetNearestPlayer(position);
    internal Vector2 GetPlayerSpawn(PlayerNumber playerNumber) => _playerSpawns[(int)playerNumber];

    internal void EnemyDefeated(EnemyController enemy)
    {
        if (_spawner.RemoveEnemy(enemy) && _gameStateManager.RegisterEnemyDefeat(_spawner.ActiveEnemyCount))
        {
            StartCoroutine(NextPhase());
        }
    }

    internal void PlayerDefeated(PlayerController player)
    {
        if (player != null && _gameStateManager.IsPlaying && _gameStateManager.RegisterPlayerDefeat(player.PlayerNumber))
        {
            StartCoroutine(RespawnPlayer(player.PlayerNumber));
        }
    }

    internal void Wrap(Transform target) => _mapBoundary.Wrap(target);
    internal void ClampVertical(Transform target, Rigidbody2D body) => _mapBoundary.ClampVertical(target, body);

    private bool HasRequiredComponents()
    {
        bool isReady = _input != null && _mapBoundary != null && _hud != null && _popEffect != null && _retroFactory != null;
        if (!isReady)
        {
            Debug.LogError("GameManager의 기능 컴포넌트 참조를 모두 연결해주세요.", this);
        }

        return isReady;
    }

    private IEnumerator NextPhase()
    {
        yield return new WaitForSeconds(_phaseDelay);
        _gameStateManager.AdvancePhase();
        _spawner.SpawnPhase(_gameStateManager.Phase);
        _gameStateManager.CompletePhaseChange();
    }

    private IEnumerator RespawnPlayer(PlayerNumber playerNumber)
    {
        yield return new WaitForSeconds(_respawnDelay);
        if (_gameStateManager.CanRespawn(playerNumber))
        {
            _spawner.SpawnPlayer(playerNumber).SetInvincible(_respawnInvincibility);
        }
    }

    private void Restart()
    {
        StopAllCoroutines();
        _popPool.Reset();
        _spawner.Reset();
        _gameStateManager.Reset();
        _spawner.SpawnAllPlayers();
        _spawner.SpawnPhase(_gameStateManager.Phase);
    }
}