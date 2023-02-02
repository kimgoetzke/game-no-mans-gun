using UnityEngine;
using System.Collections.Generic;

namespace CaptainHindsight
{
    public class GhostRecorder : BStateDependent
    {
        [SerializeField] private bool recordingOn = true;
        public Ghost ghost;
        private float timer;
        private float timeValue;

        #region Start & Update to initialise
        private void Awake()
        {
            ghost.ResetData();
            timeValue = 0;
            timer = 0;
            //DeleteAllRecordings();
        }

        private void Update()
        {
            // Guard clause - only continue if recording is turned on
            if (recordingOn == false || ghost.IsRecording == false) return;

            timer += Time.unscaledDeltaTime;
            timeValue += Time.unscaledDeltaTime;

            if (timer >= 1 / ghost.RecordFrequency)
            {
                ghost.TotalTime = timeValue;
                ghost.Timestamp.Add(timeValue);
                ghost.Position.Add(transform.position);
                ghost.Rotation.Add(transform.rotation);

                timer = 0;
            }
        }
        #endregion

        #region Methods triggered by events
        private void SetRecording(bool status)
        {
            if (status)
            {
                Helper.Log("GhostManager: Recording has started.");

                // Configure scriptable object
                int levelNumber = Helper.ReturnLevelNumberFromSceneName();
                ghost.LevelNumber = levelNumber;
                ghost.IsRecording = true;
            }
            else if (status == false)
            {
                // Configure scriptable object
                ghost.IsRecording = false;
                ghost.TotalTime = timeValue;
            }
        }

        private void PauseRecording(bool status) => recordingOn = status;

        private void SetTotalScoreAndAttemptToSaveRecording(int finalScore)
        {
            // Update final score once it's been calculated
            ghost.TotalScore = finalScore;
            //Helper.Log("GhostRecorder: Final score of " + finalScore + " received.");

            // Save the new recording if it's better than the previous one or it's the first one
            AttemptToAddNewRecording();
        }
        #endregion

        #region Methods used by GhostRecorder
        private void AttemptToAddNewRecording()
        {
            // Create new recording entry
            RecordingEntry recordingEntry = new RecordingEntry
            {
                levelNumber = ghost.LevelNumber,
                totalTime = ghost.TotalTime,
                totalScore = ghost.TotalScore,
                recordingFrequency = ghost.RecordFrequency,
                timeStamp = ghost.Timestamp,
                position = ghost.Position,
                rotation = ghost.Rotation
            };
            Helper.Log("GhostManager: Recording being processed. Level: " + recordingEntry.levelNumber + ", run time: " + recordingEntry.totalTime + ", score: " + recordingEntry.totalScore + ".");

            // Create new, empty JSON file if it doesn't exist already
            CheckIfJSONExists();

            // Load existing list from PlayerPrefs
            string jsonString = PlayerPrefs.GetString("ghostRecordings");
            Recordings recordings = JsonUtility.FromJson<Recordings>(jsonString);

            // Add recording to JSON if current recording is best run to date
            bool levelIsAlreadyInList = false;
            for (int i = 0; i < recordings.recordingEntryList.Count; i++)
            {
                if (recordings.recordingEntryList[i].levelNumber == recordingEntry.levelNumber)
                {
                    if (recordings.recordingEntryList[i].totalScore < recordingEntry.totalScore)
                    {
                        // Update if new highest score
                        recordings.recordingEntryList[i].totalTime = recordingEntry.totalTime;
                        recordings.recordingEntryList[i].totalScore = recordingEntry.totalScore;
                        recordings.recordingEntryList[i].recordingFrequency = recordingEntry.recordingFrequency;
                        recordings.recordingEntryList[i].timeStamp = recordingEntry.timeStamp;
                        recordings.recordingEntryList[i].position = recordingEntry.position;
                        recordings.recordingEntryList[i].rotation = recordingEntry.rotation;
                        Helper.Log("GhostManager: New score record. Recording on file was updated.");
                    }
                    else if (recordings.recordingEntryList[i].totalScore == recordingEntry.totalScore)
                    {
                        // If same score, update if current run was faster
                        if (recordings.recordingEntryList[i].totalTime > recordingEntry.totalTime)
                        {
                            recordings.recordingEntryList[i].totalTime = recordingEntry.totalTime;
                            recordings.recordingEntryList[i].totalScore = recordingEntry.totalScore;
                            recordings.recordingEntryList[i].recordingFrequency = recordingEntry.recordingFrequency;
                            recordings.recordingEntryList[i].timeStamp = recordingEntry.timeStamp;
                            recordings.recordingEntryList[i].position = recordingEntry.position;
                            recordings.recordingEntryList[i].rotation = recordingEntry.rotation;
                            Helper.Log("GhostManager: New record. Same score but faster run. Recording on file was updated.");
                        }
                        else Helper.Log("GhostManager: Same score as previous record but slower time. Current run will be discarded.");
                    }
                    else Helper.Log("GhostManager: A better run is already on file (score: " + recordings.recordingEntryList[i].totalScore + ", run time: " + recordings.recordingEntryList[i].totalTime + "). Current run will be discarded.");
                    levelIsAlreadyInList = true;
                }
            }

            // Add the new entry if no recording for current level exists
            if (levelIsAlreadyInList == false)
            {
                recordings.recordingEntryList.Add(recordingEntry);
                Helper.Log("GhostManager: No previous runs on file. Added current run.");
            }

            // Save list with new/updated entry in it
            string json = JsonUtility.ToJson(recordings);
            PlayerPrefs.SetString("ghostRecordings", json);
            PlayerPrefs.Save();
        }

        public static void DeleteAllRecordings()
        {
            if (!PlayerPrefs.HasKey("ghostRecordings")) return;

            Helper.Log("GhostManager: All recordings on file have been deleted.");
            string json = JsonUtility.ToJson("");
            PlayerPrefs.SetString("ghostRecordings", json);
            PlayerPrefs.Save();
        }

        private void CheckIfJSONExists()
        {
            // Create empty list if there's no JSON file on local drive
            if (!PlayerPrefs.HasKey("ghostRecordings"))
            {
                string emptyJson = JsonUtility.ToJson("");
                PlayerPrefs.SetString("ghostRecordings", emptyJson);
                PlayerPrefs.Save();
            }
        }
        #endregion

        #region Methods used by GhostPlayer
        public static bool CheckIfRecordingForCurrentLevelExists(int level)
        {
            bool recordingExists = false;

            // Return false if there is no JSON file for ghost recordings
            if (!PlayerPrefs.HasKey("ghostRecordings")) return recordingExists;

            // Load existing list from PlayerPrefs
            string jsonString = PlayerPrefs.GetString("ghostRecordings");
            Recordings recordings = JsonUtility.FromJson<Recordings>(jsonString);

            // Check if recording for level already exists
            for (int i = 0; i < recordings.recordingEntryList.Count; i++)
            {
                if (recordings.recordingEntryList[i].levelNumber == level)
                {
                    Helper.Log("GhostManager: Recording for level " + recordings.recordingEntryList[i].levelNumber + " exists (run time: " + recordings.recordingEntryList[i].totalTime + " seconds).");
                    recordingExists = true;
                    return recordingExists;
                }
            }

            // If not, return false
            return recordingExists;
        }

        public static RecordingEntry RetrieveRecordingForCurrentLevel(int level)
        {
            // Return null if no JSON file for ghost recordings exists
            if (!PlayerPrefs.HasKey("ghostRecordings")) return null;

            // Load existing list from PlayerPrefs
            string jsonString = PlayerPrefs.GetString("ghostRecordings");
            Recordings recordings = JsonUtility.FromJson<Recordings>(jsonString);

            // Return recording for level requested
            for (int i = 0; i < recordings.recordingEntryList.Count; i++)
            {
                if (recordings.recordingEntryList[i].levelNumber == level) return recordings.recordingEntryList[i];

            }

            // Throw error if no recording retrieved
            Helper.LogError("GhostRecorder: Recording for level " + level + " requested but it doesn't exist. Always call 'CheckIfRecordingForCurrentLevelExists(" + level + ")' prior to RetrieveRecordingForCurrentLevel().");
            return null;
        }
        #endregion

        #region Recording sub-classes (used by GhostRecorder and GhostPlayer)
        private class Recordings
        {
            public List<RecordingEntry> recordingEntryList;
        }

        // Represents a ghost recording entry
        [System.Serializable]
        public class RecordingEntry
        {
            public int levelNumber;
            public float totalTime;
            public int totalScore;

            public float recordingFrequency;

            public List<float> timeStamp;
            public List<Vector3> position;
            public List<Quaternion> rotation;
        }
        #endregion

        #region Managing events and game states
        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            // Pause recording temporarily depending on game state
            PauseRecording(settings.PlayGhostRecording);
            
            if (ghost.IsRecording)
            {
                // Stop recording entirely depending on game state, if it's currently on
                if (state == GameState.EndOfLevel || state == GameState.GameOver || state == GameState.Transition) SetRecording(false);
            }
            else if (ghost.IsRecording == false && state == GameState.Play)
            {
                // Start recording if not on and player enters play state (SetRecording is
                // the only function that can set the IsRecording on the ghost)
                SetRecording(true);
            }
        }

        protected override void OnEnable()
        {
            EventManager.Instance.OnEndOfLevelScoreCalculated += SetTotalScoreAndAttemptToSaveRecording;
            GameStateManager.OnGameStateChange += ActionGameStateChange;
        }

        protected override void OnDestroy()
        {
            EventManager.Instance.OnEndOfLevelScoreCalculated -= SetTotalScoreAndAttemptToSaveRecording;
            GameStateManager.OnGameStateChange -= ActionGameStateChange;
        }
        #endregion
    }
}