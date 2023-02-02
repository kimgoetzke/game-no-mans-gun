using UnityEngine;

namespace CaptainHindsight
{
    public class Speed : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Transform effectPopup;
        [SerializeField] private string textOnEnter;
        [SerializeField] private float maxSpeed;
        private float countdown;
        private bool blockEffectPopup;

        private void Update()
        {
            // Block effect popup for a short period of time
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
            if (collision.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false)
            {
                // Helps to stop effect popup from being triggered too often (because the player has multiple colliders)
                if (blockEffectPopup) return;
                blockEffectPopup = true;

                // Instantiate pop up message
                Transform winPopup = Instantiate(effectPopup, transform);
                winPopup.GetComponent<MEffectPopup>().Initialisation(ActionType.Text, textOnEnter);

                // Play audio
                AudioManager.Instance.Play("AreaEffect");
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false)
            {
                // Clamp speed
                PlayerController.Instance.ClampSpeed(maxSpeed);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                // Helps to stop effect popup from being triggered too often (because the player has multiple colliders)
                blockEffectPopup = true;

                // Stop the audio effect
                AudioManager.Instance.StopPlaying("AreaEffect");
            }
        }
    }
}
