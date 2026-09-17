using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class WaterDeathZone : MonoBehaviour
{
    private BoxCollider2D _trigger;

    private void Awake()
    {
        _trigger = GetComponent<BoxCollider2D>();
        _trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Fighter fighter = other.GetComponentInParent<Fighter>();

        if (fighter == null)
        {
            return;
        }

        fighter.FallIntoWater();
    }
}