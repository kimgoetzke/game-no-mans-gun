using UnityEngine;
using TMPro;
using DG.Tweening;

namespace CaptainHindsight
{
    public class StartCountdown : BStateDependent
    {
        private TextMeshProUGUI textMesh;
        private int countdownToStart = 5;
        private float updateInterval = 1f;
        private double lastInterval;
        private bool gameStateIsActive;

        private void Start()
        {
            // Get reference for TMPro text mesh component on StartCountdown
            textMesh = transform.GetComponent<TextMeshProUGUI>();

            // Set reference time stamp
            lastInterval = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            // Prevent any action unless game state Countdown is active
            if (gameStateIsActive == false) return;

            // Set action timer (updated every frame)
            float timeNow = Time.realtimeSinceStartup;

            // Update text, animate it, etc. every updateInterval
            if (timeNow > lastInterval + updateInterval)
            {
                StartCountdownToStart();
                lastInterval = timeNow;
            }
        }

        private void StartCountdownToStart()
        {
            if (countdownToStart > 1) AudioManager.Instance.Play("Tick");

            if (countdownToStart > 0)
            {
                textMesh.text = (countdownToStart - 2).ToString();
                textMesh.transform.DOPunchScale(new Vector3(5, 5, 5), 1, 1, 0.2f).SetUpdate(UpdateType.Normal, true);
                countdownToStart--;
            }

            if (countdownToStart == 1)
            {
                textMesh.text = "GO!";
                AudioManager.Instance.Play("Positive");
                textMesh.transform.DOScale(new Vector3(10, 10, 10), 0.5f).SetUpdate(UpdateType.Normal, true).OnComplete(() => textMesh.gameObject.SetActive(false));
                GameStateManager.Instance.SwitchState(GameState.Play);
            }
        }

        #region Managing events and game states
        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            if (state == GameState.Countdown) gameStateIsActive = true;
            else gameStateIsActive = false;
        }
        #endregion
    }
}