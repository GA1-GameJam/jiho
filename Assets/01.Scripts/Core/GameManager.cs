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
    
    [Header("플레이어 스폰 위치")]
    [SerializeField] private Transform _playerOneSpawn;
    [SerializeField] private Transform _playerTwoSpawn;

    [Header("적 스폰 위치")]
    [SerializeField] private Transform[] _enemySpawnPoints;
    
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
    internal int EnemySpawnCount => _enemySpawnPoints.Length;

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
        if (_spawner == null)
        {
            return;
        }

        UpdateHud();

        if (!_gameStateManager.IsPlaying && _input.RestartPressed)
        {
            Restart();
        }
    }
    private void UpdateHud()
    {
        _hud.Refresh(
            _gameStateManager.Score,
            _gameStateManager.Phase,
            _gameStateManager.GetLives(PlayerNumber.One),
            _gameStateManager.GetLives(PlayerNumber.Two),
            _spawner.ActiveEnemyCount,
            _gameStateManager.IsChangingPhase,
            _gameStateManager.IsGameOver,
            _gameStateManager.IsAllClear,
            _input);
    }

    private void OnDestroy()
    {
        _popPool?.Clear();
        _spawner?.Clear();
    }

    internal void BalloonPopped(Vector3 position) => _popPool?.Play(position);
    internal PlayerController GetNearestPlayer(Vector3 position) => _spawner.GetNearestPlayer(position);
    
    internal Vector2 GetPlayerSpawn(PlayerNumber playerNumber)
    {
        return playerNumber == PlayerNumber.One
            ? _playerOneSpawn.position
            : _playerTwoSpawn.position;
    }

    internal Vector2 GetEnemySpawn(int index)
    {
        return _enemySpawnPoints[index].position;
    }

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
        bool hasComponents =
            _input != null
            && _mapBoundary != null
            && _hud != null
            && _popEffect != null
            && _retroFactory != null;

        if (!hasComponents)
        {
            Debug.LogError(
                "GameManager의 기능 컴포넌트 참조를 모두 연결해주세요.",
                this);
        }

        bool hasSpawnPoints = HasRequiredSpawnPoints();
        bool isHudConfigured = _hud != null && _hud.IsConfigured;

        if (_hud != null && !isHudConfigured)
        {
            Debug.LogError(
                "BalloonHud의 UI 참조를 모두 연결해주세요.",
                _hud);
        }

        return hasComponents && hasSpawnPoints && isHudConfigured;
    }

    private bool HasRequiredSpawnPoints()
    {
        if (_playerOneSpawn == null || _playerTwoSpawn == null)
        {
            Debug.LogError(
                "GameManager에 P1Spawn과 P2Spawn을 연결해주세요.",
                this);

            return false;
        }

        if (_enemySpawnPoints == null || _enemySpawnPoints.Length == 0)
        {
            Debug.LogError(
                "GameManager에 적 스폰 위치를 한 개 이상 연결해주세요.",
                this);

            return false;
        }

        for (int index = 0; index < _enemySpawnPoints.Length; index++)
        {
            if (_enemySpawnPoints[index] != null)
            {
                continue;
            }

            Debug.LogError(
                $"Enemy Spawn Points의 {index}번 항목이 비어 있습니다.",
                this);

            return false;
        }

        return true;
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