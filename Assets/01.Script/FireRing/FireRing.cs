using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireRing : MonoBehaviour
{
    [SerializeField] private float _returnDistance = 12f;

    private Rigidbody2D _rigid;
    private FireRingPool _pool;
    private StageManager _stageManager;
    private bool _isSpawned;
    private bool _hasHit;

    public bool IsSpawned => _isSpawned;

    public void Initialize(FireRingPool pool, StageManager stageManager)
    {
        _rigid = GetComponent<Rigidbody2D>();
        _pool = pool;
        _stageManager = stageManager;
    }

    public void Spawn(Vector3 position)
    {
        transform.position = position;
        _rigid.position = new Vector2(position.x, position.y);
        _hasHit = false;
        _isSpawned = true;
        gameObject.SetActive(true);
    }

    public void ResetRing()
    {
        _isSpawned = false;
        _hasHit = false;
    }

    private void FixedUpdate()
    {
        if (!_isSpawned || !_stageManager.IsPlaying)
        {
            return;
        }

        float returnPositionX = _stageManager.Player.position.x - _returnDistance;

        if (_rigid.position.x < returnPositionX)
        {
            _pool.ReturnRing(this);
            return;
        }

        Vector2 position = _rigid.position;
        position.x -= _stageManager.ObstacleSpeed * Time.fixedDeltaTime;
        _rigid.MovePosition(position);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isSpawned || _hasHit || !_stageManager.IsPlaying)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        _hasHit = true;
        playerHealth.TakeDamage();
    }
   
}