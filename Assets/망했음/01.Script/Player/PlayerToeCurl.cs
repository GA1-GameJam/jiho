using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerMove), typeof(Rigidbody2D), typeof(PlayerHealth))]
public class PlayerToeCurl : MonoBehaviour
{
    [Header("발 판정 위치")]
    [SerializeField] private Transform _footPoint;
    [Header("연타 설정")]
    [SerializeField, Min(0.1f)] private float _duration = 1.5f;
    [SerializeField, Min(1)] private int _requiredPresses = 6;
    [SerializeField, Range(0.01f, 1f)] private float _slowScale = 0.2f;
    [SerializeField] private KeyCode _key = KeyCode.Space;
    [Header("확대 카메라")]
    [SerializeField] private Camera _camera;
    [SerializeField, Range(0.1f, 1f)] private float _zoomRatio = 0.4f;
    [SerializeField, Min(0.01f)] private float _cameraDuration = 0.18f;
    [SerializeField] private Vector2 _focusOffset;
    [Header("엑스레이 UI")]
    [SerializeField] private GameObject _recoveryPanel;
    [SerializeField] private Image _footImage;
    [SerializeField] private Sprite[] _curlFrames;
    [SerializeField] private Slider _progress;
    [SerializeField] private Slider _timeRemaining;
    [SerializeField] private TMP_Text _prompt;
    [Header("이벤트")]
    [SerializeField] private UnityEvent _onToeCurl = new UnityEvent();
    [SerializeField] private UnityEvent _onPress = new UnityEvent();
    [SerializeField] private UnityEvent _onSuccess = new UnityEvent();
    [SerializeField] private UnityEvent _onFailure = new UnityEvent();

    private PlayerMove _move;
    private Rigidbody2D _rigid;
    private PlayerHealth _health;
    private Animator _animator;
    private StageManager _stage;
    private FireRing _ring;
    private CameraFollow _follow;

    private Vector2 _velocity;
    private Vector3 _cameraPosition;
    private float _cameraSize;
    private float _oldScale;
    private float _elapsed;
    private float _finishElapsed;
    private float _returnZoomAmount;
    private int _presses;
    private int _startFrame;

    private bool _moveEnabled;
    private bool _inputEnabled;
    private bool _simulated;
    private bool _animatorEnabled;
    private bool _followEnabled;
    private bool _success;
    private bool _finishing;
    private bool _isActive;

    public Transform FootPoint => _footPoint;
    public bool IsActive => _isActive;

    private void Awake()
    {
        _move = GetComponent<PlayerMove>();
        _rigid = GetComponent<Rigidbody2D>();
        _health = GetComponent<PlayerHealth>();
        _animator = GetComponent<Animator>();

        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera != null)
        {
            _follow = _camera.GetComponent<CameraFollow>();
        }

        ShowPanel(false);
    }

    public bool TryBegin(FireRing ring, StageManager stage)
    {
        if (!isActiveAndEnabled || _isActive || _footPoint == null ||
            ring == null || stage == null || !stage.IsPlaying ||
            stage.IsRecovering || _health.Life <= 0 || Time.timeScale <= 0f)
        {
            return false;
        }

        _ring = ring;
        _stage = stage;
        _elapsed = 0f;
        _finishElapsed = 0f;
        _presses = 0;
        _startFrame = Time.frameCount;
        _finishing = false;
        _success = false;

        _oldScale = Time.timeScale;
        _velocity = _rigid.linearVelocity;
        _simulated = _rigid.simulated;
        _moveEnabled = _move.enabled;
        _inputEnabled = _move.IsInputEnabled;
        _animatorEnabled = _animator != null && _animator.enabled;
        _followEnabled = _follow != null && _follow.enabled;

        if (_camera != null)
        {
            _cameraPosition = _camera.transform.position;
            _cameraSize = _camera.orthographicSize;
        }

        _isActive = true;
        _stage.SetRecovering(true);
        _move.SetInputEnabled(false);
        _move.enabled = false;
        _rigid.simulated = false;

        if (_animator != null)
        {
            _animator.enabled = false;
        }

        if (_follow != null)
        {
            _follow.enabled = false;
        }

        Time.timeScale = _oldScale * Mathf.Clamp(_slowScale, 0.01f, 1f);

        ShowPanel(true);
        RefreshUI();
        _onToeCurl.Invoke();

        return _isActive;
    }

    public void TriggerToeCurl()
    {
        _onToeCurl.Invoke();
    }

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        if (_stage == null || !_stage.IsPlaying ||
            _ring == null || !_ring.IsSpawned || !_ring.isActiveAndEnabled)
        {
            CancelRecovery();
            return;
        }

        if (_finishing)
        {
            _finishElapsed += Time.unscaledDeltaTime;
            float duration = Mathf.Max(0.01f, _cameraDuration);

            if (_success)
            {
                _ring.AnimateRecoveryPass(
                    Mathf.Clamp01(_finishElapsed / duration));
            }

            if (_finishElapsed >= duration)
            {
                Complete();
            }

            return;
        }

        _elapsed += Time.unscaledDeltaTime;
        float timeLimit = Mathf.Max(0.1f, _duration);

        if (_elapsed < timeLimit &&
            Time.frameCount > _startFrame &&
            Input.GetKeyDown(_key))
        {
            _presses++;
            _onPress.Invoke();

            if (!_isActive)
            {
                return;
            }
        }

        RefreshUI();

        if (_presses >= Mathf.Max(1, _requiredPresses) ||
            _elapsed >= timeLimit)
        {
            _success = _presses >= Mathf.Max(1, _requiredPresses);
            _finishing = true;
            _returnZoomAmount = Mathf.Clamp01(
                _elapsed / Mathf.Max(0.01f, _cameraDuration));

            if (_prompt != null)
            {
                _prompt.text = _success ? "RECOVERY!" : "HIT!";
            }
        }
    }

    private void LateUpdate()
    {
        if (!_isActive || _camera == null || _footPoint == null)
        {
            return;
        }

        Vector3 focus = _footPoint.position + (Vector3)_focusOffset;
        focus.z = _cameraPosition.z;

        float duration = Mathf.Max(0.01f, _cameraDuration);
        float amount;

        if (_finishing)
        {
            amount = _returnZoomAmount *
                     (1f - Mathf.Clamp01(_finishElapsed / duration));
        }
        else
        {
            amount = Mathf.Clamp01(_elapsed / duration);
        }

        amount = Mathf.SmoothStep(0f, 1f, amount);
        _camera.transform.position =
            Vector3.Lerp(_cameraPosition, focus, amount);

        if (_camera.orthographic)
        {
            _camera.orthographicSize = Mathf.Lerp(
                _cameraSize,
                _cameraSize * _zoomRatio,
                amount);
        }
    }

    private void RefreshUI()
    {
        int required = Mathf.Max(1, _requiredPresses);
        float amount = Mathf.Clamp01((float)_presses / required);

        if (_progress != null)
        {
            _progress.normalizedValue = amount;
        }

        if (_timeRemaining != null)
        {
            _timeRemaining.normalizedValue =
                1f - Mathf.Clamp01(_elapsed / Mathf.Max(0.1f, _duration));
        }

        if (_prompt != null)
        {
            _prompt.text = $"{_key} 연타! {_presses}/{required}";
        }

        if (_footImage == null || _curlFrames == null ||
            _curlFrames.Length == 0)
        {
            return;
        }

        int index = Mathf.FloorToInt(amount * (_curlFrames.Length - 1));

        if (_curlFrames[index] != null)
        {
            _footImage.sprite = _curlFrames[index];
        }
    }

    private void Complete()
    {
        FireRing ring = _ring;
        bool success = _success;

        Restore(false);

        if (ring == null || _stage == null || !_stage.IsPlaying)
        {
            return;
        }

        ring.ResolveRecovery(success, _health);

        if (success)
        {
            _onSuccess.Invoke();
        }
        else
        {
            _onFailure.Invoke();
        }
    }

    public void CancelRecovery()
    {
        Restore(true);
    }

    private void Restore(bool cancelled)
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;

        if (cancelled && _ring != null)
        {
            _ring.CancelPendingRecovery();
        }

        Time.timeScale = _oldScale;

        if (_stage != null)
        {
            _stage.SetRecovering(false);
        }

        bool playing = _stage != null && _stage.IsPlaying;

        if (_move != null)
        {
            _move.SetInputEnabled(playing && _inputEnabled);
            _move.enabled = playing && _moveEnabled;
        }

        if (_rigid != null)
        {
            _rigid.simulated = playing && _simulated;
            _rigid.linearVelocity = playing ? _velocity : Vector2.zero;
        }

        if (_animator != null)
        {
            _animator.enabled = _animatorEnabled;
        }

        if (_camera != null)
        {
            _camera.transform.position = _cameraPosition;
            _camera.orthographicSize = _cameraSize;
        }

        if (_follow != null)
        {
            _follow.enabled = _followEnabled;
        }

        ShowPanel(false);
        _ring = null;
    }

    private void ShowPanel(bool visible)
    {
        if (_recoveryPanel == null ||
            _recoveryPanel == gameObject ||
            transform.IsChildOf(_recoveryPanel.transform))
        {
            return;
        }

        _recoveryPanel.SetActive(visible);
    }

    private void OnDisable()
    {
        CancelRecovery();
    }
}