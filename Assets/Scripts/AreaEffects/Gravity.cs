using UnityEngine;

namespace CaptainHindsight
{
    public class Gravity : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Transform effectPopup;
        [SerializeField] private string textOnEnter;
        [SerializeField] private float modifier;
        private bool triggeredBefore;
        private float lastTriggerTimestamp;
        private float countdown;
        private bool blockEffectPopup;

        private void Update()
        {
            // The below resets the trigger should the player be removed without triggering OnTriggerExit
            if (triggeredBefore && PlayerManagement.Instance.PlayerIsDead)
            {
                // lastTriggerTimestamp will be 0 so this will be immediately triggered but only once
                // as the lastTriggerTimestamp will then be set
                if (Time.time - lastTriggerTimestamp <= 1f) return;

                lastTriggerTimestamp = Time.time;
                PlayerController.Instance.ResetGravity();
                triggeredBefore = false;
                Helper.Log("Gravity effect automatically reset.", this);
            }

            if (blockEffectPopup)
            {
                countdown += Time.deltaTime;

                if (countdown >= 0.2f)
                {
                    blockEffectPopup = false;
                    countdown = 0;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false && triggeredBefore == false)
            {
                // Increased gravity on
                PlayerController.Instance.ChangeGravity(modifier);
                triggeredBefore = true;

                // Stop effect popup from being triggered too often (because the player has multiple colliders)
                if (blockEffectPopup) return;
                blockEffectPopup = true;

                // Instantiate pop up message 
                Transform winPopup = Instantiate(effectPopup, transform);
                winPopup.GetComponent<MEffectPopup>().Initialisation(ActionType.Text, textOnEnter);

                // Play audio
                AudioManager.Instance.Play("AreaEffect");
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && triggeredBefore)
            {
                // Increased gravity off
                PlayerController.Instance.ResetGravity();
                triggeredBefore = false;

                // Stop code from being triggered too often (because the player has multiple colliders)
                blockEffectPopup = true;

                // Stop the audio effect
                AudioManager.Instance.StopPlaying("AreaEffect");
            }
        }
    }
}
