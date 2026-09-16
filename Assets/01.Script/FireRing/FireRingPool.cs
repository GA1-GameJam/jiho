using System.Collections.Generic;
using UnityEngine;

public class FireRingPool : MonoBehaviour
{
    [SerializeField] private FireRing _fireRingPrefab;
    [SerializeField] private StageManager _stageManager;
    [SerializeField] private int _initialCount = 8;

    private readonly Queue<FireRing> _availableRings = new Queue<FireRing>();

    private void Awake()
    {
        for (int i = 0; i < _initialCount; i++)
        {
            FireRing fireRing = CreateRing();
            _availableRings.Enqueue(fireRing);
        }
    }

    private FireRing CreateRing()
    {
        FireRing fireRing = Instantiate(_fireRingPrefab, transform);
        fireRing.Initialize(this, _stageManager);
        fireRing.gameObject.SetActive(false);

        return fireRing;
    }

    public FireRing GetRing(Vector3 position)
    {
        FireRing fireRing;

        if (_availableRings.Count > 0)
        {
            fireRing = _availableRings.Dequeue();
        }
        else
        {
            fireRing = CreateRing();
        }

        fireRing.Spawn(position);

        return fireRing;
    }

    public void ReturnRing(FireRing fireRing)
    {
        if (!fireRing.IsSpawned)
        {
            return;
        }

        fireRing.ResetRing();
        fireRing.gameObject.SetActive(false);
        _availableRings.Enqueue(fireRing);
    }
}