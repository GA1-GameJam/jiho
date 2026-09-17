using System.Collections;
using UnityEngine;

public sealed class EnemyController : Fighter, IPoolable
{
    [Header("적 이동")]
    [SerializeField] private int _balloonLimit = 1;
    [SerializeField] private float _gravityScale = 0.63f;
    [SerializeField] private float _flapForce = 3.45f;
    [SerializeField] private float _acceleration = 5.2f;
    [SerializeField] private Vector3 _velocityLimits = new(4.2f, 5.1f, 5.8f);
    [SerializeField] private Vector2 _flapInterval = new(0.55f, 0.95f);
    [SerializeField] private Vector2 _decisionInterval = new(0.65f, 1.35f);
    [SerializeField, Range(0f, 1f)] private float _trackingChance = 0.72f;
    [SerializeField, Range(0f, 1f)] private float _randomFlapChance = 0.22f;
    [SerializeField] private float _trackingDeadZone = 0.4f;
    [SerializeField] private float _flapHeightThreshold = -0.35f;

    [Header("적 사망")]
    [SerializeField] private float _hitProtection = 0.2f;
    [SerializeField] private float _deathDelay = 1.4f;
    [SerializeField] private float _deathGravity = 1.8f;
    [SerializeField] private float _deathSpin = 220f;
    [SerializeField] private float _deathImpulse = 1.2f;

    private BalloonEnemyPool _pool;
    private float _moveDirection;
    private float _nextDecisionTime;
    private float _nextFlapTime;

    internal int BalloonLimit => _balloonLimit;
    internal void ConfigurePool(BalloonEnemyPool pool) => _pool = pool;

    internal void Initialize(GameManager game)
    {
        base.Initialize(game, _balloonLimit);
        Body.gravityScale = _gravityScale;
        _moveDirection = 1f;
        _nextFlapTime = 0f;
        DecideDirection();
    }

    public void OnSpawned() => gameObject.SetActive(true);
    public void OnDespawned()
    {
        StopAllCoroutines();
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (IsDead || Game == null || !Game.IsPlaying) return;
        if (Time.time >= _nextDecisionTime) DecideDirection();
        TryFlap();
        Body.AddForce(Vector2.right * _moveDirection * _acceleration);
        ClampVelocity(_velocityLimits);
        Game.ClampVertical(transform, Body);
        Game.Wrap(transform);
    }

    protected override void OnBalloonLost()
    {
        if (BalloonCount > 0) return;
        MarkDead();
        BodyCollider.enabled = false;
        Body.gravityScale = _deathGravity;
        Body.freezeRotation = false;
        Body.angularVelocity = Random.Range(-_deathSpin, _deathSpin);
        Body.AddForce(Vector2.up * _deathImpulse, ForceMode2D.Impulse);
        Game.EnemyDefeated(this);
        StartCoroutine(ReturnAfterDelay());
    }

    protected override float GetHitProtection() => _hitProtection;

    private void TryFlap()
    {
        PlayerController player = Game.GetNearestPlayer(transform.position);
        if (player == null || Time.time < _nextFlapTime) return;
        float playerDeltaY = player.transform.position.y - transform.position.y;
        if (playerDeltaY > _flapHeightThreshold || Random.value < _randomFlapChance)
        {
            Body.AddForce(Vector2.up * _flapForce, ForceMode2D.Impulse);
        }

        _nextFlapTime = Time.time + Random.Range(_flapInterval.x, _flapInterval.y);
    }

    private void DecideDirection()
    {
        PlayerController player = Game == null ? null : Game.GetNearestPlayer(transform.position);
        if (player != null && Random.value < _trackingChance)
        {
            float delta = player.transform.position.x - transform.position.x;
            _moveDirection = Mathf.Abs(delta) < _trackingDeadZone ? _moveDirection : Mathf.Sign(delta);
        }
        else _moveDirection = Random.value > 0.5f ? 1f : -1f;
        _nextDecisionTime = Time.time + Random.Range(_decisionInterval.x, _decisionInterval.y);
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(_deathDelay);
        _pool?.Release(this);
    }
}
