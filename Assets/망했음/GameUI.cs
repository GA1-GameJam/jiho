using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Image[] _lifeIcons;

    private void Update()
    {
        for (int i = 0; i < _lifeIcons.Length; i++)
        {
            _lifeIcons[i].enabled = i < _playerHealth.Life;
        }
    }
}