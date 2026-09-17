using UnityEngine;

[CreateAssetMenu(fileName = "StuntSettingSO", menuName = "Circus/Stunt Settings")]
public class StuntSettingSO : ScriptableObject
{
    [Header("발끝과 피해 영역 상단 사이의 거리")]
    [SerializeField, Min(0f)] private float _extremeDistance = 0.1f;
    [SerializeField, Min(0f)] private float _dangerDistance = 0.3f;
    [SerializeField, Min(0f)] private float _goodDistance = 0.6f;

    public float ExtremeDistance => _extremeDistance;
    public float DangerDistance => _dangerDistance;
    public float GoodDistance => _goodDistance;

    private void OnValidate()
    {
        _extremeDistance = Mathf.Max(0f, _extremeDistance);
        _dangerDistance = Mathf.Max(_extremeDistance, _dangerDistance);
        _goodDistance = Mathf.Max(_dangerDistance, _goodDistance);
    }
}
