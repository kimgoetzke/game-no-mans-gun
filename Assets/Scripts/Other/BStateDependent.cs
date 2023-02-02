using UnityEngine;

namespace CaptainHindsight
{
    public abstract class BStateDependent : MonoBehaviour
    {
        abstract protected void ActionGameStateChange(GameState state, GameStateSettings settings);

        protected virtual void OnEnable() => GameStateManager.OnGameStateChange += ActionGameStateChange;

        protected virtual void OnDestroy() => GameStateManager.OnGameStateChange -= ActionGameStateChange;
    }

    public abstract class BPlayerAndGameStateDependent : BStateDependent
    {
        abstract protected void ActionPlayerSettingChange(PlayerSettings settings, bool modelChange);

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.OnUpdatePlayerSettings += ActionPlayerSettingChange;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventManager.Instance.OnUpdatePlayerSettings += ActionPlayerSettingChange;
        }
    }
}