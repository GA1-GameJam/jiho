using UnityEngine;

[CreateAssetMenu(fileName = "ComboSettingSO", menuName = "Circus/Combo Settings")]
public class ComboSettingSO : ScriptableObject
{
    [Header("판정별 콤보 증가량")]
    [SerializeField, Min(0)] private int _safeCombo = 0;
    [SerializeField, Min(0)] private int _goodCombo = 1;
    [SerializeField, Min(0)] private int _dangerCombo = 1;
    [SerializeField, Min(0)] private int _extremeCombo = 1;
    [SerializeField, Min(0)] private int _recoveryCombo = 1;

    [Header("불고리 속도")]
    [SerializeField, Min(1)] private int _comboPerSpeedStep = 3;
    [SerializeField, Min(0f)] private float _speedIncreasePerStep = 0.2f;
    [SerializeField, Min(1f)] private float _maxSpeedMultiplier = 1.6f;

    public int GetComboIncrease(StuntGrade grade)
    {
        switch (grade)
        {
            case StuntGrade.Safe:
                return Mathf.Max(0, _safeCombo);
            case StuntGrade.Good:
                return Mathf.Max(0, _goodCombo);
            case StuntGrade.Danger:
                return Mathf.Max(0, _dangerCombo);
            case StuntGrade.Extreme:
                return Mathf.Max(0, _extremeCombo);
            case StuntGrade.Recovery:
                return Mathf.Max(0, _recoveryCombo);
            default:
                return 0;
        }
    }

    public float GetSpeedMultiplier(int combo)
    {
        int speedStep = Mathf.Max(0, combo) / Mathf.Max(1, _comboPerSpeedStep);
        float multiplier = 1f + speedStep * Mathf.Max(0f, _speedIncreasePerStep);
        return Mathf.Min(multiplier, Mathf.Max(1f, _maxSpeedMultiplier));
    }
}