using UnityEngine;

public sealed class BalloonVisual : MonoBehaviour
{
    private static readonly int _balloonCountHash =
        Animator.StringToHash("BalloonCount");

    private static readonly int _twoBalloonsIdleHash =
        Animator.StringToHash("TwoBalloonsIdle");

    private static readonly int _oneBalloonIdleHash =
        Animator.StringToHash("OneBalloonIdle");

    [Header("풍선 컴포넌트")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private CapsuleCollider2D _hitCollider;

    [Header("풍선 2개 판정")]
    [SerializeField] private Vector2 _twoBalloonColliderSize =
        new(0.9f, 0.45f);

    [SerializeField] private Vector2 _twoBalloonColliderOffset =
        Vector2.zero;

    [Header("풍선 1개 판정")]
    [SerializeField] private Vector2 _oneBalloonColliderSize =
        new(0.42f, 0.45f);

    [SerializeField] private Vector2 _oneBalloonColliderOffset =
        new(0.2f, 0f);

    private int _balloonCount;
    private bool _flipX;

    internal void ResetState(int balloonCount)
    {
        _balloonCount = Mathf.Clamp(balloonCount, 0, 2);

        _spriteRenderer.enabled =
            _balloonCount > 0;

        _animator.Rebind();
        _animator.Update(0f);

        _animator.SetInteger(
            _balloonCountHash,
            _balloonCount);

        if (_balloonCount >= 2)
        {
            _animator.Play(
                _twoBalloonsIdleHash,
                0,
                0f);
        }
        else if (_balloonCount == 1)
        {
            _animator.Play(
                _oneBalloonIdleHash,
                0,
                0f);
        }

        UpdateCollider();
    }

    internal void SetBalloonCount(int balloonCount)
    {
        _balloonCount = Mathf.Clamp(
            balloonCount,
            0,
            2);

        if (_balloonCount > 0)
        {
            _spriteRenderer.enabled = true;
        }

        _animator.SetInteger(
            _balloonCountHash,
            _balloonCount);

        UpdateCollider();
    }

    internal void SetFacing(bool flipX)
    {
        _flipX = flipX;
        _spriteRenderer.flipX = flipX;

        UpdateCollider();
    }

    // OneToZero 애니메이션 마지막 프레임에서 호출한다.
    public void FinishPop()
    {
        if (_balloonCount <= 0)
        {
            _spriteRenderer.enabled = false;
        }
    }

    private void UpdateCollider()
    {
        if (_hitCollider == null)
        {
            return;
        }

        _hitCollider.enabled =
            _balloonCount > 0;

        Vector2 size;
        Vector2 offset;

        if (_balloonCount >= 2)
        {
            size = _twoBalloonColliderSize;
            offset = _twoBalloonColliderOffset;
        }
        else
        {
            size = _oneBalloonColliderSize;
            offset = _oneBalloonColliderOffset;
        }

        if (_flipX)
        {
            offset.x *= -1f;
        }

        _hitCollider.size = size;
        _hitCollider.offset = offset;
    }

    private void Reset()
    {
        _animator =
            GetComponent<Animator>();

        _spriteRenderer =
            GetComponent<SpriteRenderer>();

        _hitCollider =
            GetComponentInChildren<CapsuleCollider2D>(true);
    }
}