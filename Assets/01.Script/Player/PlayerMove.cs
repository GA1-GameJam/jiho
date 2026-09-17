using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMove : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _airMoveSpeed = 4f;
    [Header("점프")]
    [SerializeField] private float _jumpSpeed = 8f;
    [Header("바닥 확인")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody2D _rigid;
    private Animator _animator;
    private float _moveInput;
    private bool _jumpRequested;
    private bool _isGrounded;
    private bool _isInputEnabled = true;
    private int _inputResumeFrame = -1;

    public float MoveInput => _moveInput;
    public bool IsInputEnabled => _isInputEnabled;
    public bool IsGrounded => _isGrounded;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_isInputEnabled && Time.frameCount > _inputResumeFrame)
        {
            GetMoveInput();
            RequestJump();
        }
        else
        {
            _moveInput = 0f;
            _jumpRequested = false;
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        CheckGround();
        Jump();
        Move();
    }

    public void SetInputEnabled(bool isInputEnabled)
    {
        _isInputEnabled = isInputEnabled;
        _inputResumeFrame = Time.frameCount;
        _moveInput = 0f;
        _jumpRequested = false;
    }

    private void GetMoveInput()
    {
        _moveInput = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _moveInput -= 1f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            _moveInput += 1f;
        }
    }

    private void RequestJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpRequested = true;
        }
    }

    private void CheckGround()
    {
        bool isTouchingGround = Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _groundLayer) != null;

        _isGrounded = isTouchingGround &&
                      _rigid.linearVelocity.y <= 0.01f;
    }

    private void Jump()
    {
        if (_jumpRequested && _isGrounded)
        {
            Vector2 velocity = _rigid.linearVelocity;
            velocity.y = _jumpSpeed;
            _rigid.linearVelocity = velocity;
            _isGrounded = false;

            _animator.SetBool("IsGrounded", false);
            _animator.SetTrigger("Jump");
        }

        _jumpRequested = false;
    }

    private void Move()
    {
        float speed = _isGrounded ? _moveSpeed : _airMoveSpeed;
        Vector2 velocity = _rigid.linearVelocity;
        velocity.x = _moveInput * speed;
        _rigid.linearVelocity = velocity;
    }

    private void UpdateAnimation()
    {
        bool isRunning = Mathf.Abs(_moveInput) > 0.01f;
        _animator.SetBool("IsRunning", isRunning);
        _animator.SetBool("IsGrounded", _isGrounded);
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }
}