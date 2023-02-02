using System.Collections.Generic;
using UnityEngine;

namespace CaptainHindsight
{
    public class TotalScoreManager : MonoBehaviour
    {
        //private void Awake() => AddHighscoreEntry(2, 4000); // Create a test entry (level, score)

        private static void CheckIfJSONExists()
        {
            // Create empty list if there's no JSON file on local drive
            if (!PlayerPrefs.HasKey("scoreTable"))
            {
                string emptyJson = JsonUtility.ToJson("");
                PlayerPrefs.SetString("scoreTable", emptyJson);
                PlayerPrefs.Save();
            }
        }

        #region Method used by other classes to add/update/delete scores
        public static int AddHighscoreEntry(int level, int score)
        {
            int differenceToPreviousScore = 0;

            // Create new score entry
            LevelScoreEntry levelScoreEntry = new LevelScoreEntry { level = level, score = score };
            //Helper.Log("TotalScoreManager: Data received. Level: " + level + ", score: " + score);

            // Create new, empty JSON file if it doesn't exist already
            CheckIfJSONExists();

            // Load existing list from PlayerPrefs
            string jsonString = PlayerPrefs.GetString("scoreTable");
            LevelScores levelScores = JsonUtility.FromJson<LevelScores>(jsonString);

            // Update score if level exists
            bool levelIsAlreadyInList = false;
            for (int i = 0; i < levelScores.levelScoreEntryList.Count; i++)
            {
                if (levelScores.levelScoreEntryList[i].level == level)
                {
                    //Helper.Log("TotalScoreManager: Level was already in list with score: " + levelScores.levelScoreEntryList[i].score);
                    if (levelScores.levelScoreEntryList[i].score < score)
                    {
                        differenceToPreviousScore = score - levelScores.levelScoreEntryList[i].score;
                        levelScores.levelScoreEntryList[i].score = score;
                    }
                    levelIsAlreadyInList = true;
                    //Helper.Log("TotalScoreManager: Score in list post-operation is: " + levelScores.levelScoreEntryList[i].score);
                }
            }

            // Add the new entry
            if (levelIsAlreadyInList == false) levelScores.levelScoreEntryList.Add(levelScoreEntry);

            // Save list with new/updated entry in it
            string json = JsonUtility.ToJson(levelScores); // Replace 'levelScores' with "" to purge list
            PlayerPrefs.SetString("scoreTable", json);
            PlayerPrefs.Save();

            return differenceToPreviousScore;
        }

        public static void DeleteAllProgress()
        {
            string json = JsonUtility.ToJson(0);
            PlayerPrefs.SetString("scoreTable", json);
            PlayerPrefs.Save();
        }
        #endregion

        #region Methods used by other classes in order to access score data
        public static bool CheckIfHighestScoreToDate(int level, int score)
        {
            bool highestScoreToDate;

            // Load existing list from PlayerPrefs
            string jsonString = PlayerPrefs.GetString("scoreTable");
            LevelScores levelScores = JsonUtility.FromJson<LevelScores>(jsonString);

            // Check if score for level already exists
            for (int i = 0; i < levelScores.levelScoreEntryList.Count; i++)
            {
                if (levelScores.levelScoreEntryList[i].level == level)
                {
                    //Helper.Log("TotalScoreManager: Level was already in list with score: " + levelScores.levelScoreEntryList[i].score + " (vs new score of " + score + ").");
                    if (levelScores.levelScoreEntryList[i].score < score)
                    {
                        highestScoreToDate = true;
                        return highestScoreToDate;
                    }
                    else if (levelScores.levelScoreEntryList[i].score >= score)
                    {
                        highestScoreToDate = false;
                        return highestScoreToDate;
                    }
                }
            }

            highestScoreToDate = true;
            return highestScoreToDate;
        }

        public List<LevelScoreEntry> RetrieveListOfScores()
        {
            // Create new, empty JSON file if it doesn't exist already
            CheckIfJSONExists();

            // Load existing list from PlayerPrefs
            string jsonString = PlayerPrefs.GetString("scoreTable");
            LevelScores levelScores = JsonUtility.FromJson<LevelScores>(jsonString);

            return levelScores.levelScoreEntryList;
        }

        public static int CalculateTotalScore()
        {
            // Create new, empty JSON file if it doesn't exist already
            CheckIfJSONExists();

            // Load existing list from PlayerPrefs
            string jsonString = PlayerPrefs.GetString("scoreTable");
            LevelScores levelScores = JsonUtility.FromJson<LevelScores>(jsonString);

            // Reset total score then sum up all entries in levelScoreEntryList to get total score
            int totalScore = 0;
            foreach (LevelScoreEntry entry in levelScores.levelScoreEntryList)
            {
                totalScore += entry.score;
            }

            return totalScore;
        }
        #endregion

        #region Sub-classes used by this clasee
        private class LevelScores
        {
            public List<LevelScoreEntry> levelScoreEntryList;
        }

        // Represents a single score entry
        [System.Serializable]
        public class LevelScoreEntry
        {
            public int level;
            public int score;
        }
        #endregion
    }
}
