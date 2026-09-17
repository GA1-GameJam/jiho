using UnityEngine;

public class FireRingSpawner : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private StageManager _stageManager;
    [SerializeField] private FireRingPool _pool;

    [Header("불고리 설정")]
    [SerializeField] private FireRingSettingSO _fireRingSettings;

    private float _spawnTimer;

    private void Awake()
    {
        if (_stageManager == null || _pool == null || _fireRingSettings == null)
        {
            Debug.LogError("FireRingSpawner의 Stage Manager, Pool, Fire Ring Settings를 연결하세요.", this);
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (!_stageManager.IsPlaying || _stageManager.HasReachedEnd)
        {
            return;
        }

        _spawnTimer += Time.deltaTime;

        if (_spawnTimer < _fireRingSettings.SpawnInterval)
        {
            return;
        }

        _spawnTimer = 0f;
        SpawnRing();
    }

    private void SpawnRing()
    {
        float positionX = _stageManager.Player.position.x + _fireRingSettings.SpawnDistance;
        Vector3 position = new Vector3(positionX, _fireRingSettings.SpawnHeight, 0f);

        _pool.GetRing(position);
    }
}