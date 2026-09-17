using System;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [Header("콤보 설정")]
    [SerializeField] private ComboSettingSO _comboSettings;

    [Header("연결")]
    [SerializeField] private StageManager _stageManager;
    [SerializeField] private StuntJudge _stuntJudge;
    [SerializeField] private PlayerHealth _playerHealth;

    private int _combo;
    private float _speedMultiplier = 1f;

    public int Combo => _combo;
    public float SpeedMultiplier => _speedMultiplier;

    public event Action<int, float> ComboChanged;

    private void Awake()
    {
        if (_comboSettings == null || _stageManager == null || _stuntJudge == null || _playerHealth == null)
        {
            Debug.LogError("ComboManager의 Combo Settings, Stage Manager, Stunt Judge, Player Health를 연결하세요.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_stuntJudge != null)
        {
            _stuntJudge.JudgmentCompleted += HandleJudgmentCompleted;
        }

        if (_playerHealth != null)
        {
            _playerHealth.Damaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (_stuntJudge != null)
        {
            _stuntJudge.JudgmentCompleted -= HandleJudgmentCompleted;
        }

        if (_playerHealth != null)
        {
            _playerHealth.Damaged -= HandleDamaged;
        }
    }

    private void Start()
    {
        ResetCombo();
    }

    private void HandleJudgmentCompleted(StuntGrade grade)
    {
        if (!_stageManager.IsPlaying || grade == StuntGrade.Hit)
        {
            return;
        }

        int increase = _comboSettings.GetComboIncrease(grade);

        if (increase <= 0)
        {
            return;
        }

        _combo += increase;
        _speedMultiplier = _comboSettings.GetSpeedMultiplier(_combo);
        _stageManager.SetSpeedMultiplier(_speedMultiplier);
        NotifyComboChanged();
    }

    private void HandleDamaged()
    {
        ResetCombo();
    }

    private void ResetCombo()
    {
        _combo = 0;
        _speedMultiplier = 1f;
        _stageManager.ResetSpeed();
        NotifyComboChanged();
    }

    private void NotifyComboChanged()
    {
        Debug.Log($"콤보: {_combo}, 불고리 속도: {_speedMultiplier:F1}배", this);
        ComboChanged?.Invoke(_combo, _speedMultiplier);
    }
}