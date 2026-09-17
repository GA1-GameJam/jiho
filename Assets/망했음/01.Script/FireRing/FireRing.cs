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
    private PlayerToeCurl _pendingToeCurl;

    private Vector2 _previousRelativePosition;
    private Vector2 _recoveryStart;
    private Vector2 _recoveryEnd;
    private float _crossingClearance;

    private bool _isSpawned;
    private bool _hasHit;
    private bool _hasTouchedDamage;
    private bool _hasTriggeredToeCurl;
    private bool _hasPreviousSample;
    private bool _hasCrossed;
    private bool _isJudgmentResolved;
    private bool _recovered;

    public bool IsSpawned => _isSpawned;

    public void Initialize(FireRingPool pool, StageManager stageManager)
    {
        _rigid = GetComponent<Rigidbody2D>();
        _pool = pool;
        _stageManager = stageManager;
        _stuntJudge = _stageManager.Player.GetComponent<StuntJudge>();

        if (_damageCollider == null || _stuntJudge == null)
        {
            Debug.LogError(
                "FireRing의 Damage Collider와 플레이어의 StuntJudge를 확인하세요.",
                this);
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
        _pendingToeCurl = null;
        _recovered = false;
        _isSpawned = false;
        _hasHit = false;
        _hasTouchedDamage = false;
        _hasTriggeredToeCurl = false;
        _hasPreviousSample = false;
        _hasCrossed = false;
        _isJudgmentResolved = false;
        _previousRelativePosition = Vector2.zero;
        _recoveryStart = Vector2.zero;
        _recoveryEnd = Vector2.zero;
        _crossingClearance = 0f;
    }

    private void FixedUpdate()
    {
        if (!_isSpawned || !_stageManager.IsPlaying ||
            _stageManager.IsRecovering)
        {
            return;
        }

        CheckPassage();

        float returnPositionX =
            _stageManager.Player.position.x - _returnDistance;

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
        if (_isJudgmentResolved || _damageCollider == null ||
            _stuntJudge == null || !_stuntJudge.isActiveAndEnabled)
        {
            return;
        }

        Bounds damageBounds = _damageCollider.bounds;
        Vector3 footPosition = _stuntJudge.FootPoint.position;
        Vector2 relativePosition = new Vector2(
            footPosition.x - damageBounds.center.x,
            footPosition.y - damageBounds.max.y);

        if (_hasPreviousSample &&
            _previousRelativePosition.x <= 0f &&
            relativePosition.x > 0f)
        {
            float crossingRatio = -_previousRelativePosition.x /
                                  (relativePosition.x -
                                   _previousRelativePosition.x);

            float clearance = Mathf.Lerp(
                _previousRelativePosition.y,
                relativePosition.y,
                crossingRatio);

            _crossingClearance = _hasCrossed
                ? Mathf.Min(_crossingClearance, clearance)
                : clearance;

            _hasCrossed = true;
        }

        _previousRelativePosition = relativePosition;
        _hasPreviousSample = true;

        if (!_hasCrossed ||
            _stuntJudge.BodyCollider.bounds.min.x <= damageBounds.max.x ||
            _hasTriggeredToeCurl || _pendingToeCurl != null)
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
        if (!_isSpawned || _hasHit || _recovered ||
            _stageManager.IsRecovering ||
            !_stageManager.IsPlaying || playerHealth == null)
        {
            return;
        }

        _hasTouchedDamage = true;

        int previousLife = playerHealth.Life;
        playerHealth.TakeDamage();

        if (playerHealth.Life >= previousLife)
        {
            return;
        }

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

    public void TryToeCurl(PlayerToeCurl playerToeCurl)
    {
        if (!_isSpawned || _hasHit || _hasTriggeredToeCurl ||
            _isJudgmentResolved || !_stageManager.IsPlaying ||
            _stageManager.IsRecovering || playerToeCurl == null)
        {
            return;
        }

        _pendingToeCurl = playerToeCurl;
    }

    private void LateUpdate()
    {
        PlayerToeCurl candidate = _pendingToeCurl;
        _pendingToeCurl = null;

        if (candidate == null || _hasHit || _isJudgmentResolved ||
            !_isSpawned || !_stageManager.IsPlaying ||
            _stageManager.IsRecovering || _damageCollider == null ||
            _stuntJudge == null || !_stuntJudge.isActiveAndEnabled)
        {
            return;
        }

        // 플레이어 물리가 꺼지기 전에 통과 목표 위치를 저장한다.
        _recoveryStart = _rigid.position;

        float passDistance = Mathf.Max(
            0f,
            _damageCollider.bounds.max.x -
            _stuntJudge.BodyCollider.bounds.min.x + 0.15f);

        _recoveryEnd = _recoveryStart + Vector2.left * passDistance;
        _hasTriggeredToeCurl = true;

        if (!candidate.TryBegin(this, _stageManager))
        {
            _hasTriggeredToeCurl = false;
        }
    }

    public void AnimateRecoveryPass(float amount)
    {
        if (!_isSpawned || !_hasTriggeredToeCurl)
        {
            return;
        }

        float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(amount));
        Vector2 position =
            Vector2.Lerp(_recoveryStart, _recoveryEnd, progress);

        _rigid.position = position;
        transform.position =
            new Vector3(position.x, position.y, transform.position.z);
    }

    public void CancelPendingRecovery()
    {
        if (!_isJudgmentResolved)
        {
            _hasTriggeredToeCurl = false;
        }
    }

    public void ResolveRecovery(bool success, PlayerHealth health)
    {
        if (!_isSpawned || !_hasTriggeredToeCurl ||
            _isJudgmentResolved || !_stageManager.IsPlaying)
        {
            return;
        }

        if (!success)
        {
            TryDamage(health);
            return;
        }

        _recovered = true;
        _isJudgmentResolved = true;
        _stuntJudge.PublishJudgment(StuntGrade.Recovery);
    }
}