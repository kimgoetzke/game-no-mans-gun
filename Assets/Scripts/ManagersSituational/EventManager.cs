using System;
using UnityEngine;

namespace CaptainHindsight
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance;

        public event Action<int> OnEndOfLevelScoreCalculated;
        public event Action OnEndOfLevelAnimationComplete;
        public event Action<ScoreEventType, int> OnCountEvent;
        public event Action OnLevelSelectSwipe;
        public event Action<string, bool> OnTriggerYellowEvent;
        public event Action OnRequestPauseMenu;
        public event Action<bool> OnToggleCheats;
        public event Action<ActionType, float> OnGivePowerUp;
        public event Action<PlayerSettings, bool> OnUpdatePlayerSettings;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void EndOfLevelAnimationComplete()
        {
            OnEndOfLevelAnimationComplete?.Invoke();
        }

        public void EndOfLevelScoreCalculated(int finalScore)
        {
            OnEndOfLevelScoreCalculated?.Invoke(finalScore);
        }

        // Used by all classes who track and submit scoreable events
        public void CountEvent(ScoreEventType scoreEventType, int value)
        {
            //Helper.Log("Event received at manager class: "  + scoreEventType);
            OnCountEvent?.Invoke(scoreEventType, value);
        }

        // Used by LevelSwiper and LevelSelect to update the arrow buttons
        public void CheckStatusOfLevelSelectPanels()
        {
            OnLevelSelectSwipe?.Invoke();
        }

        // Used to communicate from YellowButton to the relevant YellowEvent
        public void TriggerYellowEvent(string eventName, bool buttonPressed)
        {
            Helper.Log("EventManager: Yellow event was triggered - event name: " + eventName + " (" + buttonPressed + ").");
            OnTriggerYellowEvent?.Invoke(eventName, buttonPressed);
        }

        // Used by PlayerController when relevant input is detected
        public void RequestPauseMenu()
        {
            OnRequestPauseMenu?.Invoke();
        }

        // Used by GameHelper and allows for cheats to set in any other class
        public void ToggleCheats(bool status)
        {
            OnToggleCheats?.Invoke(status);
        }

        // Used by PowerUp to let classes who are changed know that the power-up was collected
        public void GivePowerUp(ActionType type, float value)
        {
            OnGivePowerUp?.Invoke(type, value);
        }

        // Used by player related classes to initiate request for changed player settings
        public void UpdatePlayerSettings(PlayerSettings settings, bool modelChange)
        {
            OnUpdatePlayerSettings?.Invoke(settings, modelChange);
        }
    }
}