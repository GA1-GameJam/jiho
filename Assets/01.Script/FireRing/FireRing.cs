using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireRing : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _returnDistance = 12f;

    [Header("아래쪽 피해 영역")]
    [SerializeField] private Collider2D _damageCollider;

    private Rigidbody2D _rigid;
    private FireRingPool _pool;
    private StageManager _stageManager;
    private StuntJudge _stuntJudge;
    private Vector2 _previousRelativePosition;
    private float _crossingClearance;
    private bool _isSpawned;
    private bool _hasHit;
    private bool _hasTouchedDamage;
    private bool _hasTriggeredToeCurl;
    private bool _hasPreviousSample;
    private bool _hasCrossed;
    private bool _isJudgmentResolved;

    public bool IsSpawned => _isSpawned;

    public void Initialize(FireRingPool pool, StageManager stageManager)
    {
        _rigid = GetComponent<Rigidbody2D>();
        _pool = pool;
        _stageManager = stageManager;
        _stuntJudge = _stageManager.Player.GetComponent<StuntJudge>();

        if (_damageCollider == null || _stuntJudge == null)
        {
            Debug.LogError("FireRing의 Damage Collider와 플레이어의 StuntJudge를 확인하세요.", this);
        }
    }

    public void Spawn(Vector3 position)
    {
        ResetRing();

        transform.position = position;
        _rigid.position = new Vector2(position.x, position.y);
        _isSpawned = true;

        gameObject.SetActive(true);
    }

    public void ResetRing()
    {
        _isSpawned = false;
        _hasHit = false;
        _hasTouchedDamage = false;
        _hasTriggeredToeCurl = false;
        _hasPreviousSample = false;
        _hasCrossed = false;
        _isJudgmentResolved = false;
        _previousRelativePosition = Vector2.zero;
        _crossingClearance = 0f;
    }

    private void FixedUpdate()
    {
        if (!_isSpawned || !_stageManager.IsPlaying)
        {
            return;
        }

        CheckPassage();

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

    private void CheckPassage()
    {
        if (_isJudgmentResolved || _damageCollider == null || _stuntJudge == null || !_stuntJudge.isActiveAndEnabled)
        {
            return;
        }

        Bounds damageBounds = _damageCollider.bounds;
        Vector3 footPosition = _stuntJudge.FootPoint.position;
        Vector2 relativePosition = new Vector2(footPosition.x - damageBounds.center.x, footPosition.y - damageBounds.max.y);

        // 두 물리 프레임 사이에서 고리 중심선을 지나는 순간의 발 높이를 계산한다.
        if (_hasPreviousSample && _previousRelativePosition.x <= 0f && relativePosition.x > 0f)
        {
            float crossingRatio = -_previousRelativePosition.x / (relativePosition.x - _previousRelativePosition.x);
            float clearance = Mathf.Lerp(_previousRelativePosition.y, relativePosition.y, crossingRatio);

            _crossingClearance = _hasCrossed ? Mathf.Min(_crossingClearance, clearance) : clearance;
            _hasCrossed = true;
        }

        _previousRelativePosition = relativePosition;
        _hasPreviousSample = true;

        if (!_hasCrossed || _stuntJudge.BodyCollider.bounds.min.x <= damageBounds.max.x)
        {
            return;
        }

        if (_hasTriggeredToeCurl)
        {
            return;
        }

        _isJudgmentResolved = true;

        if (!_hasTouchedDamage && _crossingClearance >= 0f)
        {
            _stuntJudge.JudgeClearance(_crossingClearance);
        }
    }

    public void TryDamage(PlayerHealth playerHealth)
    {
        if (!_isSpawned || _hasHit || !_stageManager.IsPlaying || playerHealth == null)
        {
            return;
        }

        _hasTouchedDamage = true;

        int previousLife = playerHealth.Life;
        playerHealth.TakeDamage();

        if (playerHealth.Life < previousLife)
        {
            _hasHit = true;

            if (!_isJudgmentResolved)
            {
                _isJudgmentResolved = true;

                if (_stuntJudge != null)
                {
                    _stuntJudge.PublishJudgment(StuntGrade.Hit);
                }
            }
        }
    }

    public void TryToeCurl(PlayerToeCurl playerToeCurl)
    {
        if (!_isSpawned || _hasHit || _hasTriggeredToeCurl || _isJudgmentResolved || !_stageManager.IsPlaying || playerToeCurl == null)
        {
            return;
        }

        _hasTriggeredToeCurl = true;

        Debug.Log("RECOVERY 진입: 일반 통과 판정을 보류합니다.", this);
        playerToeCurl.TriggerToeCurl();
    }
}