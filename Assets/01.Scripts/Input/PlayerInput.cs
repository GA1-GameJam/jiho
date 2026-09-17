using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInput : MonoBehaviour
{
    [Header("1P")]
    [SerializeField] private Key[] _playerOneLeft = { Key.A };
    [SerializeField] private Key[] _playerOneRight = { Key.D };
    [SerializeField] private Key[] _playerOneFlap = { Key.Space, Key.W };
    [Header("2P")]
    [SerializeField] private Key[] _playerTwoLeft = { Key.LeftArrow };
    [SerializeField] private Key[] _playerTwoRight = { Key.RightArrow };
    [SerializeField] private Key[] _playerTwoFlap = { Key.UpArrow, Key.Enter };
    [SerializeField] private Key _restart = Key.R;

    internal Key Restart => _restart;
    internal bool RestartPressed => Keyboard.current != null && Keyboard.current[_restart].wasPressedThisFrame;
    internal float GetHorizontal(PlayerNumber playerNumber) => (IsHeld(GetRightKeys(playerNumber)) ? 1f : 0f) - (IsHeld(GetLeftKeys(playerNumber)) ? 1f : 0f);
    internal bool IsFlapPressed(PlayerNumber playerNumber) => WasPressed(GetFlapKeys(playerNumber));
    internal Key[] GetLeftKeys(PlayerNumber playerNumber) => playerNumber == PlayerNumber.One ? _playerOneLeft : _playerTwoLeft;
    internal Key[] GetRightKeys(PlayerNumber playerNumber) => playerNumber == PlayerNumber.One ? _playerOneRight : _playerTwoRight;
    internal Key[] GetFlapKeys(PlayerNumber playerNumber) => playerNumber == PlayerNumber.One ? _playerOneFlap : _playerTwoFlap;

    private static bool IsHeld(Key[] keys)
    {
        if (Keyboard.current == null || keys == null) return false;
        foreach (Key key in keys) if (Keyboard.current[key].isPressed) return true;
        return false;
    }

    private static bool WasPressed(Key[] keys)
    {
        if (Keyboard.current == null || keys == null) return false;
        foreach (Key key in keys) if (Keyboard.current[key].wasPressedThisFrame) return true;
        return false;
    }
}
