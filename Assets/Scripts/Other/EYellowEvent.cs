namespace CaptainHindsight
{
    public enum YellowButtonType
    {
        SingleUse,
        SingleState,
        OnOff
    }

    public enum YellowEventType
    {
        Move_Always,
        Move_WaitAndReturn,
        Move_BackAndForth,
        Move_OnlyOnce
    }

    public enum YellowEventState
    {
        Inactive,
        Move,
        Pause,
    }
}