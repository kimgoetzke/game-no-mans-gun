using System;
using UnityEngine;
using TMPro;

namespace CaptainHindsight
{
    public class Timer : BStateDependent
    {
        [SerializeField] private float countdownTimer;
        private TextMeshProUGUI textMesh;
        private float throttleTimer;
        private bool timerStopped;

        private void Awake()
        {
            textMesh = transform.GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            int activeSceneNumber = Helper.ReturnLevelNumberFromSceneName();
            float timeToCompleteLevel = ScriptableObjectsLedger.Instance.LevelSettings[activeSceneNumber].timeToCompleteLevel;
            countdownTimer = timeToCompleteLevel + 1f;
            UpdateTimerUI(countdownTimer - 1f);
        }

        private void Update()
        {
            // End game if time runds out
            if (countdownTimer <= 0 && PlayerManagement.Instance.PlayerIsDead == false) GameStateManager.Instance.SwitchState(GameState.GameOver);
            else if (PlayerManagement.Instance.PlayerIsDead) return;

            // Bool set by event to pause timer
            if (timerStopped) return;

            countdownTimer -= Time.deltaTime;

            // Update UI every one second-ish
            throttleTimer += Time.deltaTime;
            if (throttleTimer >= 1)
            {
                throttleTimer = 0;
                UpdateTimerUI(countdownTimer);
                if (countdownTimer <= 3) AudioManager.Instance.Play("Negative");
            }
        }

        private void StopTimer()
        {
            timerStopped = true;
            int timeRemaining = (int)Math.Floor(countdownTimer);
            EventManager.Instance.CountEvent(ScoreEventType.Time, timeRemaining);
        }

        private void AddTime(ActionType type, float t)
        {
            if (type == ActionType.Time) countdownTimer += t;
        }

        private void UpdateTimerUI(float t)
        {
            float minutes = Mathf.FloorToInt(t / 60);
            float seconds = Mathf.FloorToInt(t % 60);
            textMesh.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        #region Managing events and game states
        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            if (state == GameState.Win || state == GameState.GameOver) StopTimer();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.OnGivePowerUp += AddTime;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventManager.Instance.OnGivePowerUp -= AddTime;
        }
        #endregion
    }
}