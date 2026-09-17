public enum PlayerNumber
{
    One,
    Two
}

public static class PlayerRoster
{
    public const int Count = (int)PlayerNumber.Two + 1;
}
