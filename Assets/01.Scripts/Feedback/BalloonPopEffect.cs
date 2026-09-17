using UnityEngine;

public sealed class BalloonPopEffect : MonoBehaviour, IPoolable
{
    [SerializeField] private int _poolCapacity = 16;
    [SerializeField] private float _duration = 0.35f;
    [SerializeField] private int _fragmentCount = 12;
    [SerializeField] private float _speed = 2.5f;
    [SerializeField] private float _fragmentSize = 0.08f;
    [SerializeField] private float _gravity = 3f;
    [SerializeField] private int _sortingOrder = 30;
    [SerializeField] private Color _color = new Color32(255, 225, 120, 255);
    private BalloonPopPool _pool;
    private SpriteRenderer[] _fragments;
    private Vector3[] _velocities;
    private float _elapsed;

    internal int PoolCapacity => Mathf.Max(1, _poolCapacity);

    internal void CopySettingsTo(BalloonPopEffect effect)
    {
        effect._poolCapacity = _poolCapacity;
        effect._duration = _duration;
        effect._fragmentCount = _fragmentCount;
        effect._speed = _speed;
        effect._fragmentSize = _fragmentSize;
        effect._gravity = _gravity;
        effect._sortingOrder = _sortingOrder;
        effect._color = _color;
    }

    internal void Initialize(BalloonPopPool pool, Sprite sprite)
    {
        _pool = pool;
        _fragments = new SpriteRenderer[Mathf.Max(1, _fragmentCount)];
        _velocities = new Vector3[_fragments.Length];
        for (int index = 0; index < _fragments.Length; index++)
        {
            GameObject fragment = new("Fragment");
            fragment.transform.SetParent(transform, false);
            SpriteRenderer renderer = fragment.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = _sortingOrder;
            _fragments[index] = renderer;
        }
    }

    public void OnSpawned()
    {
        _elapsed = 0f;
        for (int index = 0; index < _fragments.Length; index++)
        {
            float angle = index * (Mathf.PI * 2f / _fragments.Length);
            _velocities[index] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * _speed;
            _fragments[index].transform.localPosition = Vector3.zero;
            _fragments[index].transform.localScale = Vector3.one * _fragmentSize;
            _fragments[index].color = _color;
        }

        gameObject.SetActive(true);
    }

    public void OnDespawned() => gameObject.SetActive(false);

    private void Update()
    {
        if (_pool == null)
        {
            return;
        }

        _elapsed += Time.deltaTime;
        if (_elapsed >= _duration) { _pool.Release(this); return; }
        Color color = _color;
        color.a *= 1f - _elapsed / _duration;
        for (int index = 0; index < _fragments.Length; index++)
        {
            _velocities[index] += Vector3.down * (_gravity * Time.deltaTime);
            _fragments[index].transform.localPosition += _velocities[index] * Time.deltaTime;
            _fragments[index].color = color;
        }
    }
}
