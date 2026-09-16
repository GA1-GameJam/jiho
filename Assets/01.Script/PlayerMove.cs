using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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
    private float _moveInput;
    private bool _jumpRequested;
    private bool _isGrounded;

    public float MoveInput => _moveInput;
    public bool IsGrounded => _isGrounded;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }

    private void Update()
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        CheckGround();
        Jump();
        Move();
    }

    private void CheckGround()
    {
        bool isTouchingGround = Physics2D.OverlapCircle(
            _groundCheck.position, _groundCheckRadius, _groundLayer) != null;

        _isGrounded = isTouchingGround && _rigid.linearVelocity.y <= 0.01f;
    }

    private void Jump()
    {
        if (_jumpRequested && _isGrounded)
        {
            Vector2 velocity = _rigid.linearVelocity;
            velocity.y = _jumpSpeed;
            _rigid.linearVelocity = velocity;
            _isGrounded = false;
        }

        _jumpRequested = false;
    }

    private void Move()
    {
        float speed = _moveSpeed;

        if (!_isGrounded)
        {
            speed = _airMoveSpeed;
        }

        Vector2 velocity = _rigid.linearVelocity;
        velocity.x = _moveInput * speed;
        _rigid.linearVelocity = velocity;
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