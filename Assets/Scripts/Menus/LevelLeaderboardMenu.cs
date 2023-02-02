using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CaptainHindsight
{
    public class LevelLeaderboardMenu : MonoBehaviour
    {
        [Header("Title")]
        [SerializeField] private TextMeshProUGUI title;
        private int level;

        [Header("Leaderboard")]
        [SerializeField] private TextMeshProUGUI[] rankEntries;
        [SerializeField] private TextMeshProUGUI[] nameEntries;
        [SerializeField] private TextMeshProUGUI[] scoreEntries;
        [SerializeField] private GameObject leaderboardError;
        [SerializeField] private GameObject playerError;

        [Header("Player")]
        [SerializeField] private TextMeshProUGUI playerRank;
        [SerializeField] private TextMeshProUGUI playerScore;
        [SerializeField] private TextMeshProUGUI playerExplanation;

        //private List<LootLockerManager.LeaderboardEntry> entryList = new List<LootLockerManager.LeaderboardEntry>();

        public void LeaderboardInitialisation(int levelNumber, int leaderboardIDStaging, int leaderboardIDLive)
        {    
            // Identify correct leaderboard ID based on whether dev mode is on or off
            int leaderboardID;
            if (LootLockerManager.Instance.DevModeOn == false) leaderboardID = leaderboardIDLive;
            else leaderboardID = leaderboardIDStaging;

            // Set int level so that correct scene can be loaded when pressing play
            level = levelNumber;

            // Set leaderboard canvas title
            title.text = "Leaderboard for Level " + levelNumber.ToString();

            // Request the top scores and update UI - but show error message in UI if LootLocker is turned off/not returning anything
            List<LootLockerManager.LeaderboardEntry> entryList = LootLockerManager.Instance.RequestHighestScores(leaderboardID, rankEntries.Length);
            if (entryList == null)
            {
                nameEntries[0].text = ""; // Removes "Loading..." default text
                leaderboardError.SetActive(true);
            }
            else InitialiseLeaderboard(entryList);

            // Request the player score and update UI - but show error message in UI if LootLocker is turned off/not returning anything
            LootLockerManager.LeaderboardEntry playerScoreEntry = LootLockerManager.Instance.RequestPlayerScore(leaderboardID);
            if (playerScoreEntry == null)
            {
                playerRank.text = "";
                playerScore.text = "";
                playerExplanation.text = "";
                playerError.SetActive(true);
                return;
            }
            else InitialisePlayerRank(playerScoreEntry);
        }

        private async void InitialiseLeaderboard(List<LootLockerManager.LeaderboardEntry> entryList)
        {
            // Wait to maximise probability of LootLocker having responded
            await Task.Delay(System.TimeSpan.FromSeconds(1.2f));

            // Populate the UI
            for (int i = 0; i < entryList.Count; i++)
            {
                rankEntries[i].text = entryList[i].rank.ToString();
                nameEntries[i].text = entryList[i].name;
                scoreEntries[i].text = entryList[i].score.ToString("N0");

                // Apply additional formatting
                if (nameEntries[i].text == "(Not claimed yet)") nameEntries[i].fontSize = 26;
            }
        }

        private async void InitialisePlayerRank(LootLockerManager.LeaderboardEntry playerScoreEntry)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(1.2f));

            if (playerScoreEntry.name == "Unavailable")
            {
                playerRank.text = "-";
                playerScore.text = "-";
                playerExplanation.text = "Complete level to see your own score above";
                Helper.LogWarning("LootLockerManager: A top score response was returned but is faulty (rank: " + playerScoreEntry.rank + ", score: " + playerScoreEntry.score + "). You may want to investigate if this is normal/expected.");
                return;
            }
            else if (playerScoreEntry.rank > 0 && playerScoreEntry.score > 0)
            {
                playerRank.text = playerScoreEntry.rank.ToString();
                playerScore.text = playerScoreEntry.score.ToString("N0");
                playerExplanation.text = "";
            }
            else
            {
                playerRank.text = "-";
                playerScore.text = "-";
                Helper.LogWarning("LootLockerManager: A top score response (containing rank: " + playerScoreEntry.rank + ", score: " + playerScoreEntry.score + ") was returned but something unexpected happened. You need to investigate this but it may only happen during testing (caused by changing your player name, creating new builds, and deleting player prefs.");
            }
        }

        public void Play()
        {
            string levelString = "Level-" + level.ToString();
            TransitionManager.Instance.FadeToNextScene(levelString);
        }
    }
}
