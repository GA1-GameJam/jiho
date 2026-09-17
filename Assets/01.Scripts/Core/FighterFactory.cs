using UnityEngine;

internal static class FighterFactory
{
    internal static PlayerController CreatePlayer(Transform parent, GameObject[] prefabs, PlayerNumber playerNumber)
    {
        GameObject prefab = GetPrefab(prefabs, (int)playerNumber);
        if (prefab != null) return PrepareInstance(Object.Instantiate(prefab, parent)).GetComponent<PlayerController>();
        GameObject fighter = CreateBody(parent, $"Player {(int)playerNumber + 1}");
        PlayerController player = fighter.AddComponent<PlayerController>();
        fighter.GetComponent<FighterBody>().SetOwner(player);
        RetroFactory.CreateFighter(fighter.transform, true, player.BalloonLimit, (int)playerNumber);
        return player;
    }

    internal static EnemyController CreateEnemy(Transform parent, GameObject[] prefabs, int variation)
    {
        GameObject prefab = GetPrefab(prefabs, variation);
        if (prefab != null) return PrepareInstance(Object.Instantiate(prefab, parent)).GetComponent<EnemyController>();
        GameObject fighter = CreateBody(parent, $"Enemy {variation + 1}");
        EnemyController enemy = fighter.AddComponent<EnemyController>();
        fighter.GetComponent<FighterBody>().SetOwner(enemy);
        RetroFactory.CreateFighter(fighter.transform, false, enemy.BalloonLimit, variation);
        return enemy;
    }

    private static GameObject PrepareInstance(GameObject instance)
    {
        Fighter fighter = instance.GetComponent<Fighter>();
        instance.GetComponent<FighterBody>().SetOwner(fighter);
        return instance;
    }

    private static GameObject CreateBody(Transform parent, string objectName)
    {
        GameObject fighter = new(objectName);
        fighter.transform.SetParent(parent);
        Rigidbody2D body = fighter.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        fighter.AddComponent<CapsuleCollider2D>();
        fighter.AddComponent<FighterBody>();
        return fighter;
    }

    private static GameObject GetPrefab(GameObject[] prefabs, int index)
    {
        return prefabs == null || prefabs.Length == 0 ? null : prefabs[index % prefabs.Length];
    }
}
