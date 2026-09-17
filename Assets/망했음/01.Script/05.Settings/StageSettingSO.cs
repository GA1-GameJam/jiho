using UnityEngine;

[CreateAssetMenu(fileName = "StageSettingSO", menuName = "Circus/Stage Setting")]
public class StageSettingSO : ScriptableObject
{
    [Header("공연 설정")]
    [SerializeField, Min(0.01f)] private float _stageDistance = 100f;
    [SerializeField, Min(0f)] private float _baseObstacleSpeed = 2f;
    [SerializeField, Min(0f)] private float _restartDelay = 1f;

    [Header("다음 스테이지")]
    [SerializeField] private string _nextSceneName;

    public float StageDistance => _stageDistance;
    public float BaseObstacleSpeed => _baseObstacleSpeed;
    public float RestartDelay => _restartDelay;
    public string NextSceneName => _nextSceneName;
}