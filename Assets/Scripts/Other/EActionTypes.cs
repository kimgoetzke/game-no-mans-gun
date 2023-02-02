namespace CaptainHindsight
{
    public enum ActionType
    {
        // Only used for pop-up messages
        Damage, 
        Health,
        Text,
        // Only by power-ups
        Time,
        // Used by power-ups and to change player
        Player
    }
}