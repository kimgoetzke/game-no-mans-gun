using UnityEngine;
using System;

namespace CaptainHindsight
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance;

        [Header("States")]
        [SerializeField] private GameState previousState;
        [SerializeField] private GameState currentState = GameState.Transition;

        [Header("Settings")]
        [SerializeField] private GameStateSettings[] stateSettings; // Error state must be 0 in the list

        public static event Action<GameState, GameStateSettings> OnGameStateChange;

        private void Awake()
        {
            // Make this class a singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Set previous state to current state to avoid errors
            previousState = currentState;
        }

        #region Switching state method which is called by other classes
        public void SwitchState(GameState state)
        {
            if (state == currentState)
            {
                Helper.LogWarning("GameStateManager: Game is already in '" + currentState + "' state. No action taken.");
                return;
            }

            // Store previous state before changing so it can be reverted
            previousState = currentState;

            // Read settings for state
            GameStateSettings stateSettings = ReadStateSettings(state);
            if (stateSettings == null)
            {
                state = GameState.Error;
                stateSettings = ReadStateSettings(state);
                Helper.LogError("GameStateManager: State settings do not exist. Error state will be triggered.");
            }

            // For trouble shooting only
            //else DebugStateSettings(stateSettings);

            // Change time scale
            UpdateTimeScale(stateSettings.FreezeTime);

            // Exit current game state...
            switch (currentState)
            {
                case GameState.Tutorial: break;
                case GameState.Countdown: break;
                case GameState.Play: break;
                case GameState.Pause: break;
                case GameState.GameOver: break;
                case GameState.Win: break;
                case GameState.EndOfLevel: break;
                case GameState.Transition: break;
                case GameState.Menu: break;
                case GameState.Error: break;
            }

            // Enter the new game state...
            switch (state)
            {
                case GameState.Tutorial: break;
                case GameState.Countdown: break;
                case GameState.Play: break;
                case GameState.Pause: break;
                case GameState.GameOver: break;
                case GameState.Win: break;
                case GameState.EndOfLevel: break;
                case GameState.Transition: break;
                case GameState.Menu: break;
                case GameState.Error: break;
                default:
                    Helper.LogError("GameStateManager: An unknown state was triggered: " + state + ". Switching to Error state.");
                    state = GameState.Error;
                    break;
            }

            // Set current state to allow next change
            currentState = state;

            // Trigger state change event for anyone who's listening
            OnGameStateChange?.Invoke(state, stateSettings);
            Helper.Log("GameStateManager: State changed from '" + previousState + "' to '" + currentState + "'.");
        }
        #endregion

        public GameStateSettings ReadStateSettings(GameState state)
        {
            for (int i = 0; i < stateSettings.Length; i++)
            {
                if (stateSettings[i].Name == state)
                {
                    return stateSettings[i];
                }
            }

            return null;
        }

        private void UpdateTimeScale(bool freezeTime)
        {
            if (freezeTime) Time.timeScale = 0f;
            else Time.timeScale = 1f;
        }

        public GameState CurrentState()
        {
            return currentState;
        }

        // Only used for trouble shooting
        private void DebugStateSettings(GameStateSettings currentState)
        { 
            for (int i = 0; i < stateSettings.Length; i++)
            {
                if (stateSettings[i].Name == previousState)
                {
                    Helper.Log("GameStateManager: Settings for state '" + currentState.Name.ToString() + "' requested. Details below.");
                    Helper.Log(" - Show in-game UI: " + currentState.ShowInGameUI + ".");
                    Helper.Log(" - Freeze time: " + currentState.FreezeTime + ".");
                    Helper.Log(" - Show in-game UI: " + currentState.ShowInGameUI + ".");
                    Helper.Log(" - Player can move: " + currentState.PlayerCanMove + ".");
                    Helper.Log(" - Play ghost recording: " + currentState.PlayGhostRecording + ".");
                }
            }
        }
    }
}