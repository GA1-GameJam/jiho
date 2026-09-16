using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private Transform _player;

    [Header("공연 설정")]
    [SerializeField] private float _stageDistance = 40f;
    [SerializeField] private float _baseObstacleSpeed = 2f;
    [SerializeField] private float _restartDelay = 1f;

    private float _startPositionX;
    private float _remainingDistance;
    private float _obstacleSpeed;
    private bool _isPlaying;
    private bool _hasReachedEnd;

    public Transform Player => _player;
    public float RemainingDistance => _remainingDistance;
    public float ObstacleSpeed => _obstacleSpeed;
    public bool IsPlaying => _isPlaying;
    public bool HasReachedEnd => _hasReachedEnd;

    private void Awake()
    {
        _startPositionX = _player.position.x;
        _remainingDistance = _stageDistance;
        _obstacleSpeed = _baseObstacleSpeed;
        _isPlaying = true;
    }

    private void Update()
    {
        if (!_isPlaying || _hasReachedEnd)
        {
            return;
        }

        float travelDistance = _player.position.x - _startPositionX;
        _remainingDistance = Mathf.Clamp(_stageDistance - travelDistance, 0f, _stageDistance);

        if (_remainingDistance <= 0f)
        {
            _hasReachedEnd = true;
            Debug.Log("목표 거리 도달! 불고리 생성을 종료합니다.");
        }
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _obstacleSpeed = _baseObstacleSpeed * Mathf.Max(1f, multiplier);
    }

    public void ResetSpeed()
    {
        _obstacleSpeed = _baseObstacleSpeed;
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
        yield return new WaitForSecondsRealtime(_restartDelay);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}