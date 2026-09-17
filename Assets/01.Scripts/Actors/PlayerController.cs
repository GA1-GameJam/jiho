using System.Collections;
using UnityEngine;

public sealed class PlayerController : Fighter
{
    [Header("플레이어 이동")]
    [SerializeField] private int _balloonLimit = 2;
    [SerializeField] private float _gravityScale = 0.78f;
    [SerializeField] private float _normalFlapForce = 4.7f;
    [SerializeField] private float _damagedFlapForce = 3.7f;
    [SerializeField] private float _groundAcceleration = 24f;
    [SerializeField] private float _airAcceleration = 9f;
    [SerializeField] private float _groundDamping = 2.2f;
    [SerializeField] private float _airDamping = 0.12f;
    [SerializeField] private Vector3 _velocityLimits = new(5.8f, 6.4f, 6.2f);

    [Header("피격과 연출")]
    [SerializeField] private float _hitProtection = 0.65f;
    [SerializeField] private float _deathDelay = 0.9f;
    [SerializeField] private float _groundNormalThreshold = 0.45f;
    [SerializeField] private float _balloonLossImpulse = 0.9f;
    [SerializeField] private float _deathGravity = 2.1f;
    [SerializeField] private float _deathSpin = 220f;
    [SerializeField] private float _visualTiltMultiplier = 2.5f;
    [SerializeField] private float _maximumVisualTilt = 14f;

    private bool _isGrounded;
    private Transform _visual;
    private PlayerNumber _playerNumber;

    internal int BalloonLimit => _balloonLimit;
    internal PlayerNumber PlayerNumber => _playerNumber;
    internal bool IsAvailable => gameObject.activeSelf && !IsDead;

    internal void InitializePlayer(GameManager game, PlayerNumber playerNumber)
    {
        _playerNumber = playerNumber;
        Initialize(game, _balloonLimit);
        _visual = transform.Find("Visual");
        _isGrounded = false;
        Body.gravityScale = _gravityScale;
        Body.linearDamping = _airDamping;
    }

    private void Update()
    {
        if (IsDead || Game == null || !Game.IsPlaying || !Game.Input.IsFlapPressed(_playerNumber)) return;
        float flapForce = BalloonCount >= _balloonLimit ? _normalFlapForce : _damagedFlapForce;
        Body.AddForce(Vector2.up * flapForce, ForceMode2D.Impulse);
        _isGrounded = false;
    }

    private void FixedUpdate()
    {
        if (IsDead || Game == null || !Game.IsPlaying) return;
        float horizontalInput = Game.Input.GetHorizontal(_playerNumber);
        float acceleration = _isGrounded ? _groundAcceleration : _airAcceleration;
        Body.AddForce(Vector2.right * horizontalInput * acceleration);
        Body.linearDamping = _isGrounded ? _groundDamping : _airDamping;
        ClampVelocity(_velocityLimits);
        Game.ClampVertical(transform, Body);
        Game.Wrap(transform);
        UpdateVisualTilt();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        for (int index = 0; index < collision.contactCount; index++)
        {
            if (collision.GetContact(index).normal.y > _groundNormalThreshold)
            {
                _isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision) => _isGrounded = false;

    protected override void OnBalloonLost()
    {
        Body.AddForce(Vector2.down * _balloonLossImpulse, ForceMode2D.Impulse);
        if (BalloonCount > 0) return;
        MarkDead();
        BodyCollider.enabled = false;
        Body.gravityScale = _deathGravity;
        Body.freezeRotation = false;
        Body.angularVelocity = _deathSpin;
        Game.PlayerDefeated(this);
        StartCoroutine(DisableAfterDelay());
    }

    protected override float GetHitProtection() => _hitProtection;

    private IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(_deathDelay);
        gameObject.SetActive(false);
    }

    private void UpdateVisualTilt()
    {
        if (_visual == null) return;
        float tilt = Mathf.Clamp(-Body.linearVelocity.x * _visualTiltMultiplier, -_maximumVisualTilt, _maximumVisualTilt);
        _visual.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }
}
