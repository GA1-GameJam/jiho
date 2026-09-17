using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private StageManager _stageManager;

    [Header("생명")]
    [SerializeField, Min(1)] private int _maxLife = 3;

    [Header("피격")]
    [SerializeField, Min(0f)] private float _invincibleDuration = 1.5f;

    [Header("피격 깜빡임")]
    [SerializeField] private SpriteRenderer[] _spriteRenderers;
    [SerializeField, Min(0.01f)] private float _blinkInterval = 0.1f;

    private PlayerMove _playerMove;
    private Rigidbody2D _rigid;
    private bool[] _originalRendererStates;
    private int _life;
    private float _invincibleEndTime;
    private float _nextBlinkTime;
    private bool _isBlinking;
    private bool _isVisible;

    public int Life => _life;

    public event Action Damaged;

    private void Awake()
    {
        _playerMove = GetComponent<PlayerMove>();
        _rigid = GetComponent<Rigidbody2D>();
        _life = _maxLife;

        if (_spriteRenderers == null)
        {
            _spriteRenderers = new SpriteRenderer[0];
        }

        _originalRendererStates = new bool[_spriteRenderers.Length];
    }

    private void Update()
    {
        if (!_isBlinking)
        {
            return;
        }

        if (Time.time >= _invincibleEndTime || !_stageManager.IsPlaying)
        {
            StopBlink();
            return;
        }

        if (Time.time < _nextBlinkTime)
        {
            return;
        }

        _isVisible = !_isVisible;
        SetVisibility(_isVisible);
        _nextBlinkTime = Time.time + _blinkInterval;
    }

    private void OnDisable()
    {
        StopBlink();
    }

    public void TakeDamage()
    {
        if (_life <= 0 || !_stageManager.IsPlaying)
        {
            return;
        }

        if (Time.time < _invincibleEndTime)
        {
            return;
        }

        StopBlink();

        _life--;
        _invincibleEndTime = Time.time + _invincibleDuration;
        _stageManager.ResetSpeed();

        Debug.Log($"피격! 남은 LIFE: {_life}");
        Damaged?.Invoke();

        if (_life <= 0)
        {
            _playerMove.enabled = false;
            _rigid.linearVelocity = Vector2.zero;
            _rigid.simulated = false;
            _stageManager.FailStage();
            return;
        }

        if (_invincibleDuration > 0f)
        {
            StartBlink();
        }
    }

    private void StartBlink()
    {
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = _spriteRenderers[i];

            if (spriteRenderer != null)
            {
                _originalRendererStates[i] = spriteRenderer.enabled;
            }
        }

        _isBlinking = true;
        _isVisible = false;
        _nextBlinkTime = Time.time + _blinkInterval;
        SetVisibility(_isVisible);
    }

    private void StopBlink()
    {
        if (!_isBlinking)
        {
            return;
        }

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = _spriteRenderers[i];

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = _originalRendererStates[i];
            }
        }

        _isBlinking = false;
    }

    private void SetVisibility(bool isVisible)
    {
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = _spriteRenderers[i];

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isVisible && _originalRendererStates[i];
            }
        }
    }
}