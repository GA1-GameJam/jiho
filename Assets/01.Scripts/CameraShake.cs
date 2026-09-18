using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class CameraShake : MonoBehaviour
{
    [Header("풍선 피격 흔들림")]
    [SerializeField, Min(0f)]
    private float _duration = 0.12f;

    [SerializeField, Min(0f)]
    private float _strength = 0.08f;

    private Vector3 _originalLocalPosition;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        _originalLocalPosition =
            transform.localPosition;
    }

    internal void Play()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);

            transform.localPosition =
                _originalLocalPosition;
        }

        _shakeCoroutine =
            StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            float progress =
                elapsedTime / _duration;

            float currentStrength =
                _strength * (1f - progress);

            Vector2 offset =
                Random.insideUnitCircle
                * currentStrength;

            transform.localPosition =
                _originalLocalPosition
                + new Vector3(
                    offset.x,
                    offset.y,
                    0f);

            elapsedTime +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        transform.localPosition =
            _originalLocalPosition;

        _shakeCoroutine = null;
    }

    private void OnDisable()
    {
        transform.localPosition =
            _originalLocalPosition;

        _shakeCoroutine = null;
    }
}