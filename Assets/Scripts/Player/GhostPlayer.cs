using UnityEngine;

namespace CaptainHindsight
{
    public class GhostPlayer : BStateDependent
    {
        [Header("Ghost management")]
        public Ghost ghost;
        private float timeValue;
        private int index1;
        private int index2;
        private bool replayOn;
        private bool recordingFound;

        // The new way of doing things
        private GhostRecorder.RecordingEntry ghostEntry;

        [Header("Player management")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform backOfGun;
        [SerializeField] private bool isFacingRight;

        private void Awake()
        {
            timeValue = 0;
            int currentLevel = Helper.ReturnLevelNumberFromSceneName();
            bool recordingExists = GhostRecorder.CheckIfRecordingForCurrentLevelExists(currentLevel);
            if (recordingExists)
            {
                ghostEntry = GhostRecorder.RetrieveRecordingForCurrentLevel(currentLevel);
                recordingFound = true;
                Helper.Log("GhostPlayer: Received recording for currently level with " + ghostEntry.timeStamp.Count + " timestamps (total run time: " + ghostEntry.totalTime + ", score: " + ghostEntry.totalScore + ") received.");
                if (ghostEntry.timeStamp.Count <= 1 || ghostEntry.totalScore == 0)
                {
                    Helper.LogWarning("GhostPlayer: Recording is compromised. Replay aborted.");
                    recordingFound = false;
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Helper.Log("GhostPlayer: No recording for current level received.");
                recordingFound = false;
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // Guard clause - only continue of replayOn is true
            if (recordingFound == false || replayOn == false) return;



            // Run timer
            timeValue += Time.unscaledDeltaTime;

            // Update position and rotation of ghost prefab
            GetIndex();
            SetTransform();
        }

        #region Playback
        private void SetRecording(bool status)
        {
            if (status) replayOn = true;
            else if (status == false) replayOn = false;
        }

        private void GetIndex()
        {
            for (int i = 0; i < ghostEntry.timeStamp.Count - 2; i++)
            {
                if (ghostEntry.timeStamp[i] == timeValue)
                {
                    index1 = i;
                    index2 = i;
                    return;
                }
                else if (ghostEntry.timeStamp[i] < timeValue & timeValue < ghostEntry.timeStamp[i + 1])
                {
                    index1 = i;
                    index2 = i + 1;
                    return;
                }
            }
            index1 = ghostEntry.timeStamp.Count - 1;
            index2 = ghostEntry.timeStamp.Count - 1;
        }

        private void SetTransform()
        {
            if (index1 == index2)
            {
                // Use stored position and rotation
                this.transform.position = ghostEntry.position[index1];
                this.transform.rotation = ghostEntry.rotation[index1];
            }
            else
            {
                // Interpolate between previous and next value
                float interpolationFactor = (timeValue - ghostEntry.timeStamp[index1]) / (ghostEntry.timeStamp[index2] - ghostEntry.timeStamp[index1]);
                this.transform.position = Vector3.Lerp(ghostEntry.position[index1], ghostEntry.position[index2], interpolationFactor);
                this.transform.rotation = Quaternion.Slerp(ghostEntry.rotation[index1], ghostEntry.rotation[index2], interpolationFactor);
            }
        }
        #endregion

        #region Player sprite management, esp. flipping sprite on Y axis
        private void FixedUpdate()
        {
            // Flip sprite depending on the direction the gun is facing (to prevent gun from being upside down)?
            Vector2 currentLookDirection = firePoint.position - backOfGun.position;
            float currentAngle = Mathf.Atan2(currentLookDirection.y, currentLookDirection.x) * Mathf.Rad2Deg - 180f;

            // There's a few degrees left in between each slice to prevent the gun from flipping wildly back and forth
            if (currentAngle > -88 && isFacingRight == true) FlipGunOnYAxis();
            if (currentAngle < -92 && currentAngle > -178 && isFacingRight == false) FlipGunOnYAxis();
            if (currentAngle < -182 && currentAngle > -268 && isFacingRight == false) FlipGunOnYAxis();
            if (currentAngle < -272 && currentAngle > -358 && isFacingRight == true) FlipGunOnYAxis();
        }

        private void FlipGunOnYAxis()
        {
            isFacingRight = !isFacingRight;
            Vector3 flippedLocalScale = transform.localScale;
            flippedLocalScale.y *= -1;
            transform.localScale = flippedLocalScale;
        }
        #endregion

        #region Managing events and game states
        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            SetRecording(settings.PlayGhostRecording);
        }
        #endregion
    }
}