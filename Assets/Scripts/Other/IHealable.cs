namespace CaptainHindsight
{
    public interface IHealable
    {
        void TryToHealPlayer(int healthBoost, bool isSourceMemberOfNetwork);
    }
}