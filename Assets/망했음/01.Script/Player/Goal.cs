using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Goal : MonoBehaviour
{
    [SerializeField] private StageManager _stageManager;

    private BoxCollider2D _goalCollider;

    private void Awake()
    {
        _goalCollider = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckLanding(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckLanding(collision);
    }

    private void CheckLanding(Collision2D collision)
    {
        if (!_stageManager.IsPlaying)
        {
            return;
        }

        PlayerMove player = collision.collider.GetComponentInParent<PlayerMove>();

        if (player == null || player.transform != _stageManager.Player)
        {
            return;
        }

        float topPositionY = _goalCollider.bounds.max.y;

        if (collision.collider.bounds.center.y <= topPositionY)
        {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            if (Mathf.Abs(contact.normal.y) > 0.9f)
            {
                _stageManager.ClearStage();
                return;
            }
        }
    }
}