using UnityEngine;

public sealed class MapBoundary : MonoBehaviour
{
    [SerializeField] private float _topLimit = 4.8f;
    [SerializeField] private float _wrapPadding = 0.45f;
    [SerializeField] private float _ceilingBounce = -0.25f;
    private Camera _camera;

    internal void SetCamera(Camera gameCamera) => _camera = gameCamera;

    internal void Wrap(Transform target)
    {
        float edge = _camera.orthographicSize * _camera.aspect + _wrapPadding;
        Vector3 position = target.position;
        if (position.x > edge) position.x = -edge;
        else if (position.x < -edge) position.x = edge;
        target.position = position;
    }

    internal void ClampVertical(Transform target, Rigidbody2D body)
    {
        if (target.position.y <= _topLimit) return;
        Vector3 position = target.position;
        position.y = _topLimit;
        target.position = position;
        Vector2 velocity = body.linearVelocity;
        if (velocity.y > 0f)
        {
            velocity.y *= _ceilingBounce;
            body.linearVelocity = velocity;
        }
    }
}
