using UnityEngine;

public class FireRingSpawner : MonoBehaviour
{
    [SerializeField] private StageManager _stageManager;
    [SerializeField] private FireRingPool _pool;

    [Header("등장 설정")]
    [SerializeField] private float _spawnInterval = 3f;
    [SerializeField] private float _spawnDistance = 10f;
    [SerializeField] private float _spawnHeight = 0f;

    private float _spawnTimer;

    private void Update()
    {
        if (!_stageManager.IsPlaying || _stageManager.HasReachedEnd)
        {
            return;
        }

        _spawnTimer += Time.deltaTime;

        if (_spawnTimer < _spawnInterval)
        {
            return;
        }

        _spawnTimer = 0f;
        SpawnRing();
    }

    private void SpawnRing()
    {
        float positionX = _stageManager.Player.position.x + _spawnDistance;
        Vector3 position = new Vector3(positionX, _spawnHeight, 0f);

        _pool.GetRing(position);
    }
}