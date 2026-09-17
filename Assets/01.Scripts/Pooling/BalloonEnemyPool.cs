using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

internal sealed class BalloonEnemyPool
{
    private readonly Transform _parent;
    private readonly GameObject[] _prefabs;
    private readonly ObjectPool<EnemyController> _pool;
    private readonly HashSet<EnemyController> _leased = new();
    private int _variation;

    internal BalloonEnemyPool(Transform parent, GameObject[] prefabs, int capacity)
    {
        _parent = parent;
        _prefabs = prefabs;
        capacity = Mathf.Max(1, capacity);
        _pool = new ObjectPool<EnemyController>(CreateEnemy, OnGet, OnRelease, OnDestroyEnemy, true, capacity, capacity);
        List<EnemyController> enemies = new();
        for (int index = 0; index < capacity; index++) enemies.Add(_pool.Get());
        foreach (EnemyController enemy in enemies) _pool.Release(enemy);
    }

    internal EnemyController Get(Vector2 position)
    {
        EnemyController enemy = _pool.Get();
        _leased.Add(enemy);
        enemy.transform.position = position;
        return enemy;
    }

    internal void Release(EnemyController enemy)
    {
        if (enemy != null && _leased.Remove(enemy)) _pool.Release(enemy);
    }

    internal void ReleaseAll(IEnumerable<EnemyController> enemies)
    {
        foreach (EnemyController enemy in enemies) Release(enemy);
    }

    internal void Clear()
    {
        _leased.Clear();
        _pool.Clear();
    }

    private EnemyController CreateEnemy()
    {
        EnemyController enemy = FighterFactory.CreateEnemy(_parent, _prefabs, _variation++);
        enemy.ConfigurePool(this);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    private static void OnGet(EnemyController enemy) => enemy.OnSpawned();
    private static void OnRelease(EnemyController enemy) => enemy.OnDespawned();
    private static void OnDestroyEnemy(EnemyController enemy) { if (enemy != null) Object.Destroy(enemy.gameObject); }
}
