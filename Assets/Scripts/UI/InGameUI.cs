using UnityEngine;

namespace CaptainHindsight
{
    public class InGameUI : BStateDependent
    {
        [SerializeField] private GameObject holder;

        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            holder.SetActive(settings.ShowInGameUI);
        }
    }
}