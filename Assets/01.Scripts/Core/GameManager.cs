using System.Collections;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [Header("게임 진행")]
    [SerializeField] private int _targetFrameRate = 60;
    [SerializeField, Min(1)] private int _startingLives = 3;
    [SerializeField] private int _enemyScore = 500;
    [SerializeField, Min(0f)] private float _winSoundDelay = 1.3f;
    [SerializeField, Min(0f)] private float _nextStageDelay = 1.5f;

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

    [Header("오디오 소스")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("현재 사용하는 효과음")]
    [SerializeField] private AudioClip _winSound;
    [SerializeField] private AudioClip _jumpSound;
    [SerializeField] private AudioClip _waterDeathSound;
    [SerializeField] private AudioClip _fallingSound;
    [SerializeField] private AudioClip _flapSound;
    [SerializeField] private AudioClip _killSound;
    [SerializeField] private AudioClip _balloonPopSound;
    [SerializeField] private AudioClip _respawnSound;

    [Header("추후 기능 효과음")]
    [SerializeField] private AudioClip _fishAttackSound;
    [SerializeField] private AudioClip _balloonRefillSound;

    private GameStateManager _gameStateManager;
    private BalloonPopPool _popPool;
    private FighterSpawner _spawner;

    internal PlayerInput Input => _input;
    internal bool IsPlaying => _gameStateManager.IsPlaying;
    internal int StartingLives => _startingLives;
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
            Debug.LogError(
                "GameManager에 Main Camera를 연결해주세요.",
                this);

            enabled = false;
            return;
        }

        _mapBoundary.SetCamera(_gameCamera);

        _spawner = new FighterSpawner(
            transform,
            this,
            _playerPrefabs,
            _enemyPrefabs);

        _popPool = new BalloonPopPool(
            transform,
            _popEffect,
            RetroFactory.GetSquare());

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

        if (_gameStateManager.IsGameOver
            && _input.RestartPressed)
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
            _gameStateManager.RoundWinner,
            _input);
    }

    private void OnDestroy()
    {
        _popPool?.Clear();
        _spawner?.Clear();
    }

    internal void BalloonPopped(Vector3 position)
    {
        _popPool?.Play(position);
        PlaySound(_balloonPopSound);
    }

    internal void PlayJumpSound()
    {
        PlaySound(_jumpSound);
    }

    internal void PlayWaterDeathSound()
    {
        PlaySound(_waterDeathSound);
    }

    internal void PlayFallingSound()
    {
        PlaySound(_fallingSound);
    }

    internal void PlayFlapSound()
    {
        PlaySound(_flapSound);
    }

    internal void PlayKillSound()
    {
        PlaySound(_killSound);
    }

    internal void PlayFishAttackSound()
    {
        PlaySound(_fishAttackSound);
    }

    internal void PlayBalloonRefillSound()
    {
        PlaySound(_balloonRefillSound);
    }

    internal PlayerController GetNearestPlayer(
        Vector3 position)
    {
        return _spawner.GetNearestPlayer(position);
    }

    internal Vector2 GetPlayerSpawn(
        PlayerNumber playerNumber)
    {
        return playerNumber == PlayerNumber.One
            ? _playerOneSpawn.position
            : _playerTwoSpawn.position;
    }

    internal Vector2 GetEnemySpawn(int index)
    {
        return _enemySpawnPoints[index].position;
    }

    internal void EnemyDefeated(
        EnemyController enemy)
    {
        if (_spawner.RemoveEnemy(enemy))
        {
            _gameStateManager.RegisterEnemyDefeat();
        }
    }

    internal void PlayerDefeated(
        PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        bool wasRegistered =
            _gameStateManager.RegisterPlayerDefeat(
                player.PlayerNumber);

        if (!wasRegistered)
        {
            return;
        }

        StartCoroutine(FinishRound());
    }

    internal void Wrap(Transform target)
    {
        _mapBoundary.Wrap(target);
    }

    internal void ClampVertical(
        Transform target,
        Rigidbody2D body)
    {
        _mapBoundary.ClampVertical(target, body);
    }

    private IEnumerator FinishRound()
    {
        yield return new WaitForSeconds(
            _winSoundDelay);

        StopBackgroundMusic();
        PlaySound(_winSound);

        if (_gameStateManager.IsGameOver)
        {
            yield break;
        }

        float winSoundLength = _winSound != null
            ? _winSound.length
            : 0f;

        float stageWaitTime = Mathf.Max(
            _nextStageDelay,
            winSoundLength);

        yield return new WaitForSeconds(
            stageWaitTime);

        _popPool.Reset();
        _spawner.Reset();

        _gameStateManager.AdvancePhase();

        _spawner.SpawnAllPlayers();
        _spawner.SpawnPhase(
            _gameStateManager.Phase);

        _gameStateManager.CompletePhaseChange();

        PlayBackgroundMusic();
        PlaySound(_respawnSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (_sfxSource == null || clip == null)
        {
            return;
        }

        _sfxSource.PlayOneShot(clip);
    }

    private void StopBackgroundMusic()
    {
        if (_bgmSource == null)
        {
            return;
        }

        _bgmSource.Stop();
    }

    private void PlayBackgroundMusic()
    {
        if (_bgmSource == null)
        {
            return;
        }

        _bgmSource.Stop();
        _bgmSource.Play();
    }

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

        bool hasSpawnPoints =
            HasRequiredSpawnPoints();

        bool isHudConfigured =
            _hud != null
            && _hud.IsConfigured;

        if (_hud != null && !isHudConfigured)
        {
            Debug.LogError(
                "BalloonHud의 UI 참조를 모두 연결해주세요.",
                _hud);
        }

        return hasComponents
            && hasSpawnPoints
            && isHudConfigured;
    }

    private bool HasRequiredSpawnPoints()
    {
        if (_playerOneSpawn == null
            || _playerTwoSpawn == null)
        {
            Debug.LogError(
                "GameManager에 P1Spawn과 P2Spawn을 연결해주세요.",
                this);

            return false;
        }

        if (_enemySpawnPoints == null
            || _enemySpawnPoints.Length == 0)
        {
            Debug.LogError(
                "GameManager에 적 스폰 위치를 한 개 이상 연결해주세요.",
                this);

            return false;
        }

        for (int index = 0;
             index < _enemySpawnPoints.Length;
             index++)
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

    private void Restart()
    {
        StopAllCoroutines();

        _popPool.Reset();
        _spawner.Reset();
        _gameStateManager.Reset();

        _spawner.SpawnAllPlayers();
        _spawner.SpawnPhase(
            _gameStateManager.Phase);

        PlayBackgroundMusic();
    }
}