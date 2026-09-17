using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

internal sealed class BalloonPopPool
{
    private readonly Transform _parent;
    private readonly BalloonPopEffect _template;
    private readonly Sprite _sprite;
    private readonly ObjectPool<BalloonPopEffect> _pool;
    private readonly HashSet<BalloonPopEffect> _active = new();

    internal BalloonPopPool(Transform parent, BalloonPopEffect template, Sprite sprite)
    {
        _parent = parent;
        _template = template;
        _sprite = sprite;
        int capacity = template.PoolCapacity;
        _pool = new ObjectPool<BalloonPopEffect>(Create, null, effect => effect.OnDespawned(), effect => Object.Destroy(effect.gameObject), true, capacity, capacity);
    }

    internal void Play(Vector3 position)
    {
        if (_active.Count >= _template.PoolCapacity) return;
        BalloonPopEffect effect = _pool.Get();
        _active.Add(effect);
        effect.transform.position = position;
        effect.OnSpawned();
    }

    internal void Release(BalloonPopEffect effect)
    {
        if (effect != null && _active.Remove(effect)) _pool.Release(effect);
    }

    internal void Reset()
    {
        foreach (BalloonPopEffect effect in new List<BalloonPopEffect>(_active)) Release(effect);
    }

    internal void Clear() { Reset(); _pool.Clear(); }

    private BalloonPopEffect Create()
    {
        GameObject instance = new(nameof(BalloonPopEffect));
        instance.transform.SetParent(_parent, false);
        BalloonPopEffect effect = instance.AddComponent<BalloonPopEffect>();
        _template.CopySettingsTo(effect);
        effect.Initialize(this, _sprite);
        effect.gameObject.SetActive(false);
        return effect;
    }
}
