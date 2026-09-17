using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _offsetX = 3f;

    private void LateUpdate()
    {
        Vector3 position = transform.position;
        position.x = _target.position.x + _offsetX;
        transform.position = position;
    }
}