using System;
using UnityEngine;

public class StuntJudge : MonoBehaviour
{
    [Header("판정 설정")]
    [SerializeField] private StuntSettingSO _stuntSettings;

    [Header("플레이어 판정 기준")]
    [SerializeField] private Transform _footPoint;
    [SerializeField] private Collider2D _bodyCollider;

    private StuntGrade _lastGrade;

    public Transform FootPoint => _footPoint;
    public Collider2D BodyCollider => _bodyCollider;
    public StuntGrade LastGrade => _lastGrade;

    public event Action<StuntGrade> JudgmentCompleted;

    private void Awake()
    {
        if (_stuntSettings == null || _footPoint == null || _bodyCollider == null)
        {
            Debug.LogError("StuntJudge의 Stunt Settings, Foot Point, Body Collider를 연결하세요.", this);
            enabled = false;
        }
    }

    public void JudgeClearance(float clearance)
    {
        if (_stuntSettings == null || clearance < 0f)
        {
            return;
        }

        StuntGrade grade;

        if (clearance <= _stuntSettings.ExtremeDistance)
        {
            grade = StuntGrade.Extreme;
        }
        else if (clearance <= _stuntSettings.DangerDistance)
        {
            grade = StuntGrade.Danger;
        }
        else if (clearance <= _stuntSettings.GoodDistance)
        {
            grade = StuntGrade.Good;
        }
        else
        {
            grade = StuntGrade.Safe;
        }

        Debug.Log($"통과 간격: {clearance:F3}", this);
        PublishJudgment(grade);
    }

    public void PublishJudgment(StuntGrade grade)
    {
        _lastGrade = grade;
        Debug.Log($"스턴트 판정: {grade.ToString().ToUpperInvariant()}", this);
        JudgmentCompleted?.Invoke(grade);
    }
}
