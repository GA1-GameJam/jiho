using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private Transform _player;

    [Header("스테이지 설정")]
    [SerializeField] private StageSettingSO _stageSettings;

    [Header("클리어")]
    [SerializeField] private GameObject _clearPanel;

    private float _startPositionX;
    private float _remainingDistance;
    private float _obstacleSpeed;
    private bool _isPlaying;
    private bool _hasReachedEnd;
    private bool _isCleared;

    public Transform Player => _player;
    public float RemainingDistance => _remainingDistance;
    public float ObstacleSpeed => _obstacleSpeed;
    public bool IsPlaying => _isPlaying;
    public bool HasReachedEnd => _hasReachedEnd;

    private void Awake()
    {
        if (_stageSettings == null || _player == null)
        {
            Debug.LogError("StageManager의 Stage Settings와 Player를 연결하세요.", this);
            enabled = false;
            return;
        }

        _startPositionX = _player.position.x;
        _remainingDistance = _stageSettings.StageDistance;
        _obstacleSpeed = _stageSettings.BaseObstacleSpeed;
        _isPlaying = true;

        if (_clearPanel != null)
        {
            _clearPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (_isCleared)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                LoadNextStage();
            }

            return;
        }

        if (!_isPlaying || _hasReachedEnd)
        {
            return;
        }

        float travelDistance = _player.position.x - _startPositionX;
        _remainingDistance = Mathf.Clamp(_stageSettings.StageDistance - travelDistance, 0f, _stageSettings.StageDistance);

        if (_remainingDistance <= 0f)
        {
            _hasReachedEnd = true;
            Debug.Log("목표 거리 도달! 장애물 생성을 종료합니다.");
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        if (_stageSettings == null)
        {
            return;
        }

        _obstacleSpeed = _stageSettings.BaseObstacleSpeed * Mathf.Max(1f, multiplier);
    }

    public void ResetSpeed()
    {
        if (_stageSettings == null)
        {
            return;
        }

        _obstacleSpeed = _stageSettings.BaseObstacleSpeed;
    }

    public void ClearStage()
    {
        if (!_isPlaying)
        {
            return;
        }

        float travelDistance = _player.position.x - _startPositionX;

        if (travelDistance < _stageSettings.StageDistance)
        {
            return;
        }

        _isPlaying = false;
        _isCleared = true;
        _hasReachedEnd = true;
        _remainingDistance = 0f;

        PlayerMove playerMove = _player.GetComponent<PlayerMove>();
        Rigidbody2D rigid = _player.GetComponent<Rigidbody2D>();

        playerMove.enabled = false;
        rigid.linearVelocity = Vector2.zero;
        rigid.simulated = false;

        if (_clearPanel != null)
        {
            _clearPanel.SetActive(true);
        }

        Debug.Log("스테이지 클리어!");
    }

    public void LoadNextStage()
    {
        if (!_isCleared)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_stageSettings.NextSceneName))
        {
            Debug.Log("다음 스테이지가 아직 설정되지 않았습니다.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(_stageSettings.NextSceneName))
        {
            Debug.LogWarning("다음 스테이지 이름과 Build Profiles의 Scene List를 확인하세요.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(_stageSettings.NextSceneName);
    }

    public void FailStage()
    {
        if (!_isPlaying)
        {
            return;
        }

        _isPlaying = false;
        Debug.Log("공연 실패! 잠시 후 다시 시작합니다.");
        StartCoroutine(RestartStage());
    }

    private IEnumerator RestartStage()
    {
        yield return new WaitForSecondsRealtime(_stageSettings.RestartDelay);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}