using UnityEngine;
using UnityEngine.Events;

public class PlayerToeCurl : MonoBehaviour
{
    [Header("발 판정 위치")]
    [SerializeField] private Transform _footPoint;

    [Header("발가락 접기 이벤트")]
    [SerializeField] private UnityEvent _onToeCurl = new UnityEvent();

    public Transform FootPoint => _footPoint;

    private void Awake()
    {
        if (_footPoint == null)
        {
            Debug.LogError("PlayerToeCurl의 Foot Point를 연결하세요.", this);
        }
    }

    public void TriggerToeCurl()
    {
        Debug.Log("발가락 접기 이벤트 발생!", this);
        _onToeCurl.Invoke();
    }
}