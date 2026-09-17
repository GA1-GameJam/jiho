using UnityEngine;

public enum FireRingTriggerType
{
    Damage,
    ToeCurl
}

[RequireComponent(typeof(PolygonCollider2D))]
public class FireRingTrigger : MonoBehaviour
{
    [Header("영역 역할")]
    [SerializeField] private FireRingTriggerType _triggerType;

    private FireRing _fireRing;
    private PolygonCollider2D _triggerCollider;

    private void Awake()
    {
        _fireRing = GetComponentInParent<FireRing>();
        _triggerCollider = GetComponent<PolygonCollider2D>();
        _triggerCollider.isTrigger = true;

        if (_fireRing == null)
        {
            Debug.LogError("FireRingTrigger는 FireRing의 자식에 배치하세요.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        CheckPlayer(other);
    }

    private void CheckPlayer(Collider2D other)
    {
        if (_fireRing == null)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        switch (_triggerType)
        {
            case FireRingTriggerType.Damage:
                _fireRing.TryDamage(playerHealth);
                break;

            case FireRingTriggerType.ToeCurl:
                PlayerToeCurl playerToeCurl = playerHealth.GetComponent<PlayerToeCurl>();

                if (playerToeCurl == null || playerToeCurl.FootPoint == null)
                {
                    return;
                }

                if (_triggerCollider.OverlapPoint(playerToeCurl.FootPoint.position))
                {
                    _fireRing.TryToeCurl(playerToeCurl);
                }

                break;
        }
    }
}
