using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerController : Fighter
{
    private static readonly int _isGroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int _speedHash =
        Animator.StringToHash("Speed");

    private static readonly int _flapHash =
        Animator.StringToHash("Flap");

    private static readonly int _fallHash =
        Animator.StringToHash("Fall");

    private static readonly int _dieHash =
        Animator.StringToHash("Die");

    private static readonly int _refillHash =
        Animator.StringToHash("Refill");

    [Header("플레이어 이동")]
    [SerializeField] private int _balloonLimit = 2;
    [SerializeField] private float _gravityScale = 0.78f;
    [SerializeField] private float _normalFlapForce = 4.7f;
    [SerializeField] private float _damagedFlapForce = 3.7f;
    [SerializeField] private float _groundAcceleration = 24f;
    [SerializeField] private float _airAcceleration = 9f;
    [SerializeField] private float _groundDamping = 2.2f;
    [SerializeField] private float _airDamping = 0.12f;

    [SerializeField] private Vector3 _velocityLimits =
        new(5.8f, 6.4f, 6.2f);

    [Header("피격과 연출")]
    [SerializeField] private float _hitProtection = 0.65f;
    [SerializeField] private float _deathDelay = 0.9f;
    [SerializeField] private float _groundNormalThreshold = 0.45f;
    [SerializeField] private float _balloonLossImpulse = 0.9f;
    [SerializeField] private float _deathGravity = 2.1f;
    [SerializeField] private float _deathSpin = 220f;
    [SerializeField] private float _visualTiltMultiplier = 2.5f;
    [SerializeField] private float _maximumVisualTilt = 14f;

    private readonly HashSet<Collider2D> _groundColliders =
        new();

    private bool _isGrounded;
    private Transform _visual;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private BalloonVisual _balloonVisual;
    private PlayerNumber _playerNumber;

    internal int BalloonLimit => _balloonLimit;
    internal PlayerNumber PlayerNumber => _playerNumber;

    internal bool IsAvailable =>
        gameObject.activeSelf && !IsDead;

    internal void InitializePlayer(
        GameManager game,
        PlayerNumber playerNumber)
    {
        _playerNumber = playerNumber;

        Initialize(game, _balloonLimit);

        _visual = transform.Find("Visual");

        if (_visual == null)
        {
            Debug.LogError(
                $"{name} 아래에 Visual 오브젝트가 없습니다.",
                this);
        }
        else
        {
            _animator =
                _visual.GetComponent<Animator>();

            _spriteRenderer =
                _visual.GetComponent<SpriteRenderer>();
        }
        _balloonVisual =
            GetComponentInChildren<BalloonVisual>(true);

        _groundColliders.Clear();
        _isGrounded = false;

        Body.gravityScale = _gravityScale;
        Body.linearDamping = _airDamping;

        ResetAnimator();
    }

    private void Update()
    {
        if (IsDead
            || Game == null
            || !Game.IsPlaying
            || !Game.Input.IsFlapPressed(
                _playerNumber))
        {
            return;
        }

        if (_isGrounded)
        {
            Game.PlayJumpSound();
        }
        else
        {
            Game.PlayFlapSound();
        }

        if (_animator != null)
        {
            _animator.SetTrigger(_flapHash);
        }

        float flapForce =
            BalloonCount >= _balloonLimit
                ? _normalFlapForce
                : _damagedFlapForce;

        Body.AddForce(
            Vector2.up * flapForce,
            ForceMode2D.Impulse);

        _isGrounded = false;
    }

    private void FixedUpdate()
    {
        if (IsDead
            || Game == null
            || !Game.IsPlaying)
        {
            return;
        }

        float horizontalInput =
            Game.Input.GetHorizontal(
                _playerNumber);

        float acceleration =
            _isGrounded
                ? _groundAcceleration
                : _airAcceleration;

        Body.AddForce(
            Vector2.right
            * horizontalInput
            * acceleration);

        Body.linearDamping =
            _isGrounded
                ? _groundDamping
                : _airDamping;

        ClampVelocity(_velocityLimits);

        Game.ClampVertical(transform, Body);
        Game.Wrap(transform);

        UpdateFacing(horizontalInput);
        UpdateAnimation(horizontalInput);
        UpdateVisualTilt();
    }

    private void OnCollisionStay2D(
        Collision2D collision)
    {
        bool hasGroundContact = false;

        for (int index = 0;
             index < collision.contactCount;
             index++)
        {
            if (collision
                .GetContact(index)
                .normal.y
                > _groundNormalThreshold)
            {
                hasGroundContact = true;
                break;
            }
        }

        if (hasGroundContact)
        {
            _groundColliders.Add(
                collision.collider);
        }
        else
        {
            _groundColliders.Remove(
                collision.collider);
        }

        _isGrounded =
            _groundColliders.Count > 0;
    }

    private void OnCollisionExit2D(
        Collision2D collision)
    {
        _groundColliders.Remove(
            collision.collider);

        _isGrounded =
            _groundColliders.Count > 0;
    }
    protected override void OnBalloonLost()
    {
        if (_balloonVisual != null)
        {
            _balloonVisual.SetBalloonCount(
                BalloonCount);
        }

        Body.AddForce(
            Vector2.down * _balloonLossImpulse,
            ForceMode2D.Impulse);

        if (BalloonCount > 0)
        {
            return;
        }

        Die(true);
    }

    protected override void OnFellIntoWater()
    {
        Game.PlayWaterDeathSound();
        Die(false);
    }

    protected override float GetHitProtection() =>
        _hitProtection;

    private void Die(bool playKillSound)
    {
        if (IsDead)
        {
            return;
        }

        MarkDead();

        if (_animator != null)
        {
            _animator.ResetTrigger(_flapHash);

            _animator.SetTrigger(
                playKillSound
                    ? _fallHash
                    : _dieHash);
        }

        BodyCollider.enabled = false;
        Body.gravityScale = _deathGravity;
        Body.freezeRotation = false;
        Body.angularVelocity = _deathSpin;

        Game.PlayerDefeated(this);

        StartCoroutine(
            DisableAfterDelay(playKillSound));
    }

    private IEnumerator DisableAfterDelay(
        bool playKillSound)
    {
        yield return new WaitForSeconds(
            _deathDelay);

        if (playKillSound)
        {
            Game.PlayKillSound();
        }

        gameObject.SetActive(false);
    }

    private void ResetAnimator()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.Rebind();
        _animator.Update(0f);

        _animator.SetBool(
            _isGroundedHash,
            false);

        _animator.SetFloat(
            _speedHash,
            0f);

        _animator.SetTrigger(
            _refillHash);
    }

    private void UpdateAnimation(
        float horizontalInput)
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetBool(
            _isGroundedHash,
            _isGrounded);

        _animator.SetFloat(
            _speedHash,
            Mathf.Abs(horizontalInput));
    }

    private void UpdateFacing(
        float horizontalInput)
    {
        if (_spriteRenderer == null
            || Mathf.Approximately(
                horizontalInput,
                0f))
        {
            return;
        }

        bool flipX =
            horizontalInput < 0f;

        _spriteRenderer.flipX =
            flipX;

        if (_balloonVisual != null)
        {
            _balloonVisual.SetFacing(
                flipX);
        }
    }

    private void UpdateVisualTilt()
    {
        if (_visual == null)
        {
            return;
        }

        float tilt = Mathf.Clamp(
            -Body.linearVelocity.x
            * _visualTiltMultiplier,
            -_maximumVisualTilt,
            _maximumVisualTilt);

        _visual.localRotation =
            Quaternion.Euler(0f, 0f, tilt);
    }
}