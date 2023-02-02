namespace CaptainHindsight
{
    public interface IDamageable
    {
        void TryToDamagePlayer(int damage, string gameObject, bool isSourceMemberOfNetwork);
    }
}