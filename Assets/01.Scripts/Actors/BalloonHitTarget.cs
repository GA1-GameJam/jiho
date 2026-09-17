using UnityEngine;

public sealed class BalloonHitTarget : MonoBehaviour
{
    private Fighter _owner;

    internal Fighter Owner => _owner;

    internal void SetOwner(Fighter owner)
    {
        _owner = owner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        FighterBody attackingBody = other.GetComponent<FighterBody>();
        if (_owner == null || attackingBody == null || attackingBody.Owner == null
            || attackingBody.Owner == _owner)
        {
            return;
        }

        _owner.PopBalloon(this);
    }
}
