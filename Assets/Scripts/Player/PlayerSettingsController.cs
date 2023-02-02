using UnityEngine;
using System.Collections;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    public class PlayerSettingsController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private PlayerModel currentModel;
        [SerializeField] private PlayerState currentState;
        private ScriptableObjectsLedger ledger;
        private bool tempStateActive;

        private void Awake() => ledger = ScriptableObjectsLedger.Instance;

        private void Start()
        {
            // Go through each setting and find the one specified in the editor for
            // this instance of Player
            PlayerSettings newSettings = FindPlayerSettings(currentModel, currentState);
            if (newSettings != null) EventManager.Instance.UpdatePlayerSettings(newSettings, true); // Remove true later
            else Helper.LogError("PlayerSettingsController: No action taken.");
        }

        #region Managing player settings
        private void SetStateAndModel(PlayerModel model, PlayerState state, bool overrideChecks)
        {
            // Check if model is changed
            bool modelChange = false;
            if (model != currentModel) modelChange = true;

            // Return null if requested and current model are the same, unless override is true
            if (overrideChecks == false && modelChange == false && state == currentState)
            {
                Helper.Log("PlayerSettingsController: Requested and current model/state are the same (" + model + ", " + state + "). No action necessary.");
                return;
            }

            // Find settings for requested state and trigger update, if required
            PlayerSettings newSettings = FindPlayerSettings(model, state);
            if (newSettings != null)
            {
                // Trigger event for all relevant classes to update themselves with the new settings
                EventManager.Instance.UpdatePlayerSettings(newSettings, modelChange);

                // If a temporary state (e.g. Charged) is currently active, stop it (allowing
                // for it to be immediately restarted again)
                if (tempStateActive == true)
                {
                    StopAllCoroutines();
                    Helper.Log("PlayerSettingsController: A coroutine was active but it was stopped to reset the timer.");
                }

                // Update current model and state
                if (modelChange) currentModel = model;
                currentState = state;

                // If required, start the timer to revert back to the Default state
                StartCoroutine(StartTimerToRevertStateIfRequired(newSettings.Duration));
            }
        }

        private PlayerSettings FindPlayerSettings(PlayerModel model, PlayerState state)
        {
            // Cycle through each known setting and return the requested one
            for (int i = 0; i < ledger.PlayerSettings.Length; i++)
            {
                //Helper.Log("Player: Cycling through '" + ledger.PlayerSettings[i].name + "'.");
                if (ledger.PlayerSettings[i].Model == model && ledger.PlayerSettings[i].State == state)
                {
                    //Helper.Log("PlayerSettingsController: Settings for model '" + ledger.PlayerSettings[i].Model + "' (" + ledger.PlayerSettings[i].State + " state) requested.");
                    return ledger.PlayerSettings[i];
                }
            }

            // Log error and return null if requested settings couldn't be found
            Helper.LogError("PlayerSettingsController: Requested settings (model: " + model + ", state: " + state +") not found.");
            return null;
        }

        public PlayerSettings CurrentSettings()
        {
            PlayerSettings settings = FindPlayerSettings(currentModel, currentState);
            return settings;
        }

        private IEnumerator StartTimerToRevertStateIfRequired(float duration)
        {
            if (duration < Mathf.Infinity)
            {
                tempStateActive = true;

                PlayerState stateAtStart = currentState;
                //Helper.Log("PlayerSettingsController: Coroutine (" + duration + " seconds) has started.");
                yield return new WaitForSeconds(duration);

                // Check if a) game is in GameState.Play and b) the temporary state is still active,
                // otherwise don't do anything
                if (GameStateManager.Instance.CurrentState() == GameState.Play && stateAtStart == currentState)
                {
                    ActionEvent(ActionType.Player, 0); // Revert to Default state
                    AudioManager.Instance.Play("NegativeEvent");
                }
                else Helper.Log("PlayerSettingsController: Coroutine to revert back to previous state is redundant. No action taken.");

                tempStateActive = false;
            }
        }
        #endregion

        #region Managing events
        private void ActionEvent(ActionType type, float value)
        {
            if (type == ActionType.Player)
            {
                Helper.Log("PlayerSettingsController: Player event triggered.");

                switch (value)
                {
                    case 0:
                        SetStateAndModel(currentModel, PlayerState.Default, false);
                        break;
                    case 1:
                        if (currentModel == PlayerModel.Handgun) SetStateAndModel(currentModel, PlayerState.Charged, true);
                        else SetStateAndModel(PlayerModel.Handgun, currentState, false);
                        break;
                    case 2:
                        if (currentModel == PlayerModel.Shotgun) SetStateAndModel(currentModel, PlayerState.Charged, true);
                        else SetStateAndModel(PlayerModel.Shotgun, currentState, false);
                        break;
                    default:
                        Helper.LogError("PlayerSettingsController: An unknown ActionType.Player was triggered: " + value + ". No action taken.");
                        break;
                }
            }
        }

        private void OnEnable()
        {
            EventManager.Instance.OnGivePowerUp += ActionEvent;
        }

        private void OnDestroy()
        {
            EventManager.Instance.OnGivePowerUp -= ActionEvent;
            StopAllCoroutines();
        }
        #endregion
    }
}
