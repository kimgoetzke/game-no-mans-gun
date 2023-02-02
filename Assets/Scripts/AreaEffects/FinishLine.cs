using UnityEngine;
using Cinemachine;
using System.Threading.Tasks;

namespace CaptainHindsight
{
    public class FinishLine : MonoBehaviour
    {
        [SerializeField] private Transform effectPopup;
        [SerializeField] private string text;
        private CinemachineVirtualCamera cinemachineCamera;
        private bool triggeredBefore;
        private GameObject[] elementsUI;

        private void Awake() =>  cinemachineCamera = FindObjectOfType<CinemachineVirtualCamera>();

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false && triggeredBefore == false)
            {
                // Switch game state to 'Win' state
                GameStateManager.Instance.SwitchState(GameState.Win);

                // Turn off camera and deactivate player controls
                cinemachineCamera.gameObject.SetActive(false);

                // Instantiate pop up message and trigger win particles
                Transform winPopup = Instantiate(effectPopup, transform);
                winPopup.GetComponent<MEffectPopup>().Initialisation(ActionType.Text, text);
                PlayerController.Instance.TriggerParticles(4);

                // Play win sound
                AudioManager.Instance.Play("Win");

                // Prevent this code being triggered again
                triggeredBefore = true;

                // Send signal to turn ghost recording off, if currently being recorded
                TriggerNextStateAfterDelay();
            }
            else if (collision.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead && triggeredBefore == false)
            {
                //Turn off camera and deactivate player controls
                cinemachineCamera.gameObject.SetActive(false);

                // Prevent this code being triggered again
                triggeredBefore = true;
            }
        }

        private async void TriggerNextStateAfterDelay()
        {
            await Task.Delay(System.TimeSpan.FromSeconds(2f));
            //EventManager.Instance.SetGhostRecording(false);

            // Switch game state to 'Win' state
            GameStateManager.Instance.SwitchState(GameState.EndOfLevel);
        }
    }
}
