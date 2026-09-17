using UnityEngine;

[CreateAssetMenu(fileName = "FireRingSettingSO", menuName = "Circus/Fire Ring Settings")]
public class FireRingSettingSO : ScriptableObject
{
    [Header("불고리 생성 설정")]
    [SerializeField, Min(0.01f)] private float _spawnInterval = 3f;
    [SerializeField, Min(0f)] private float _spawnDistance = 10f;
    [SerializeField] private float _spawnHeight = 0f;

    public float SpawnInterval => _spawnInterval;
    public float SpawnDistance => _spawnDistance;
    public float SpawnHeight => _spawnHeight;
}