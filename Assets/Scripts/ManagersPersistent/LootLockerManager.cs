using UnityEngine;
using LootLocker.Requests;
using LootLocker;
using System.Reflection;
using System.Collections.Generic;

namespace CaptainHindsight
{
    public class LootLockerManager : MonoBehaviour
    {
        public static LootLockerManager Instance;

        [SerializeField] private bool lootLockerOff;
        public bool OfflineMode;
        public bool DevModeOn;

        #region Awake & Start to initialise LootLocker
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // For trouble shooting only
            //PlayerPrefs.DeleteKey("PlayerID");
            //PlayerPrefs.DeleteKey("PlayerName");
            //PlayerPrefs.DeleteKey("PlayerIdentifier");

            // Establish whether this is live or development mode
            DevModeOn = LootLockerConfig.Get().developmentMode;
            Helper.Log("LootLockerManager: Status of development mode: " + DevModeOn + ".");

            // Guard clause to allow keeping LootLocker turned off
            if (CheckIfToContinue(MethodBase.GetCurrentMethod().Name) == false) return;

            // Connect to LootLocker
            ConnectToLootLocker();
        }
        #endregion

        #region Submit a new high score
        public void SubmitScore(int leaderboardID, int score)
        {
            // Guard clause to allow keeping LootLocker turned off
            if (CheckIfToContinue(MethodBase.GetCurrentMethod().Name) == false) return;

            // Get player ID from PlayerPrefs (stored on authentication)
            string memberID = PlayerPrefs.GetInt("PlayerID").ToString();

            // Submit the score
            LootLockerSDKManager.SubmitScore(memberID, score, leaderboardID, (response) =>
            {
                if (response.statusCode == 200)
                {
                    Helper.Log("LootLockerManager: Score (" + score + ") successfully submitted for " + response.member_id + " to leaderboard '" + leaderboardID + "'.");
                }
                else Helper.Log("LootLockerManager: Failed to submit score. Error message: " + response.Error);
            });
        }
        #endregion

        #region Request top scores to build leaderboard
        public List<LeaderboardEntry> RequestHighestScores(int leaderboardID, int maxEntries)
        {
            // Guard clause to allow keeping LootLocker turned off
            if (CheckIfToContinue(MethodBase.GetCurrentMethod().Name) == false) return null;

            // Number of entries (in descending order) that should be requested
            int count = maxEntries;

            // Set success bool to get success status of anonymous function
            bool success = true;

            // Create an new list for the entries
            List<LeaderboardEntry> leaderboardEntryList = new List<LeaderboardEntry>();

            // Request score list from LootLocker
            LootLockerSDKManager.GetScoreList(leaderboardID, count, (response) =>
            {
                if (response.statusCode == 200)
                {
                    Helper.Log("LootLockerManager: Successfully loaded top " + count + " for leaderboard '" + leaderboardID + "'.");

                    LootLockerLeaderboardMember[] scores = response.items;

                    for (int i = 0; i < scores.Length; i++)
                    {
                        LeaderboardEntry leaderboardEntry = new LeaderboardEntry 
                        { 
                            rank = scores[i].rank, 
                            name = scores[i].player.name, 
                            score = scores[i].score 
                        };
                        leaderboardEntryList.Add(leaderboardEntry);
                    }

                    if (scores.Length < count)
                    {
                        for (int i = scores.Length; i < count; i++)
                        {
                            LeaderboardEntry leaderboardEntry = new LeaderboardEntry 
                            { 
                                rank = i + 1, 
                                name = "(Not claimed yet)", 
                                score = 0 
                            };
                            leaderboardEntryList.Add(leaderboardEntry);
                        }
                    }
                }
                else
                {
                    success = false;
                    Helper.LogWarning("LootLockerManager: Failed to load top " + count + " for leaderboard '" + leaderboardID + "'. Error message: " + response.Error);
                }
            });

            if (success) return leaderboardEntryList;
            else return null;
        }
        #endregion

        #region Request top player score
        public LeaderboardEntry RequestPlayerScore(int leaderboardID)
        {
            // Guard clause to allow keeping LootLocker turned off
            if (CheckIfToContinue(MethodBase.GetCurrentMethod().Name) == false) return null;

            // Set success bool to get success status of anonymous function
            bool success = true;

            // Get PlayerID from PlayerPrefs
            int memberID = PlayerPrefs.GetInt("PlayerID");

            // Create new, empty entry
            LeaderboardEntry playerScore = new();

            LootLockerSDKManager.GetMemberRank(leaderboardID, memberID, (response) =>
            {
                if (!response.success || response.member_id == "")
                {
                    playerScore.name = "Unavailable";
                    success = false;
                    Helper.LogWarning("LootLockerManager: No score found for '" + memberID + "'. Error message: " + response.Error);
                }
                else
                { 
                    Helper.Log("LootLockerManager: Successfully loaded top score for ID: " + memberID + ".");
                    playerScore.name = response.player.name;
                    playerScore.rank = response.rank;
                    playerScore.score = response.score;
                }
            });

            if (success) return playerScore;
            else return null;
        }
        #endregion

        #region Sub-classes used by this and other classes
        private class LeaderboardEntryList
        {
            public List<LeaderboardEntry> leaderboardEntries;
        }

        // Represents a single score entry
        [System.Serializable]
        public class LeaderboardEntry
        {
            public int rank;
            public string name;
            public int score;
        }
        #endregion

        #region Connection management
        private void ConnectToLootLocker()
        {
            // Get the player identifier (or create one if none exists)
            string playerIdentifier = GetPlayerIdentifier();

            // Start guest session
            LootLockerSDKManager.StartSession(playerIdentifier, (response) =>
            {
                if (!response.success)
                {
                    Helper.Log("LootLockerManager: Error starting LootLocker session.");
                    OfflineMode = true;
                    return;
                }

                Helper.Log("LootLockerManager: Successfully started LootLocker session.");
                OfflineMode = false;

                // Update local player ID
                PlayerPrefs.SetInt("PlayerID", response.player_id);
            });

            // Get player name from LootLocker or set/update if not already set
            FetchOrSetPlayerName(playerIdentifier);

            Helper.Log("LootLockerManager: Player authenticated with identifier '" + playerIdentifier + "', ID '" + PlayerPrefs.GetInt("PlayerID") + "', and name '" + PlayerPrefs.GetString("PlayerName") + "'.");
        }

        public void AttemptToReConnectToServer()
        {
            if (lootLockerOff)
            {
                Helper.Log("LootLockerManager: Re-connection request received but LootLocker is turned off, so request will be ignored.");
                return;
            }

            ConnectToLootLocker();
        }
        #endregion

        #region Status management
        public bool RequestStatus()
        { 
            if (lootLockerOff || OfflineMode) return true;
            else return false;
        }

        private bool CheckIfToContinue(string methodName)
        {
            // Use this method with the below:
            // if (CheckToContinue(MethodInfo.GetCurrentMethod().Name) == false) return;
            if (lootLockerOff || OfflineMode)
            {
                Helper.Log("LootLockerManager: '" + methodName + "' was called but LootLocker is turned off (" + lootLockerOff + ") or you are offline (" + OfflineMode + "). The request will be ignored.");
                return false;
            }
            else return true;
        }
        #endregion

        #region Player management
        public string GetPlayerIdentifier()
        {
            string identifier;

            // Return PlayerID or PlayerName, should ID not exist
            if (PlayerPrefs.HasKey("PlayerIdentifier") == false)
            {
                identifier = Random.Range(100000000000, 999999999999).ToString() + "-" + Random.Range(100000000000, 999999999999).ToString();
                PlayerPrefs.SetString("PlayerIdentifier", identifier);
                Helper.Log("LootLockerManager: New local player identifier created: " + identifier + ". PlayerPrefs have been updated.", this);
            }
            else
            {
                identifier = PlayerPrefs.GetString("PlayerIdentifier");
                Helper.Log("LootLockerManager: Local player identifier exists in PlayerPrefs: " + identifier + ".", this);
            }

            return identifier;
        }

        public void FetchOrSetPlayerName(string identifier)
        {
            // Guard clause to allow keeping LootLocker turned off
            if (CheckIfToContinue(MethodBase.GetCurrentMethod().Name) == false) return;

            // Get local player name from PlayerPrefs
            string playerName = PlayerPrefs.GetString("PlayerName");

            // Fetch name and update if it's empty or different to locally stored name
            LootLockerSDKManager.GetPlayerName((response) =>
            {
                Helper.Log("LootLockerManager: LootLocker returned the player name '" + response.name + "' for player identifier '" + identifier + "'.'");

                if (string.IsNullOrEmpty(response.name))
                {
                    LootLockerSDKManager.SetPlayerName(playerName, (response) => {
                        if (!response.success) Helper.LogError("LootLockerManager: LootLocker didn't accept the set player name. Error message received: " + response.Error);
                        else Helper.Log("LootLockerManager: LootLocker set the player name to '" + playerName + "' and linked it to the player identifier '" + identifier + "'.'");
                    });
                }
                
                if (playerName != response.name)
                {
                    Helper.LogWarning("LootLockerManager: The player name on LootLocker is different to the player name on the local device. Attempting to update LootLocker.");
                    LootLockerSDKManager.SetPlayerName(playerName, (response) =>
                    {
                        if (!response.success) Helper.LogError("LootLockerManager: LootLocker didn't accept the set player name. Error message received: " + response.Error);
                        else Helper.Log("LootLockerManager: LootLocker updated the player name to '" + playerName + "' and linked it to the player identifier '" + identifier + "'.'");
                    });
                }
            });
        }
        #endregion
    }
}
