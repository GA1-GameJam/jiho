using UnityEngine;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private StageManager _stageManager;

    [Header("생명")]
    [SerializeField] private int _maxLife = 3;

    [Header("피격")]
    [SerializeField] private float _invincibleDuration = 1.5f;

    private PlayerMove _playerMove;
    private Rigidbody2D _rigid;
    private int _life;
    private float _invincibleEndTime;

    public int Life => _life;

    private void Awake()
    {
        _playerMove = GetComponent<PlayerMove>();
        _rigid = GetComponent<Rigidbody2D>();
        _life = _maxLife;
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

        _life--;
        _invincibleEndTime = Time.time + _invincibleDuration;
        _stageManager.ResetSpeed();

        Debug.Log($"피격! 남은 LIFE: {_life}");

        if (_life <= 0)
        {
            _playerMove.enabled = false;
            _rigid.linearVelocity = Vector2.zero;
            _rigid.simulated = false;
            _stageManager.FailStage();
        }
    }
}