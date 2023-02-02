using UnityEngine;

namespace CaptainHindsight
{
    public class LevelLoad : MonoBehaviour
    {
        public bool SpecialLevel;
        public GameObject LeaderboardCanvas;
        public int LevelNumber;
        public int LeaderboardIDStaging;
        public int LeaderboardIDLive;

        public void LoadLevelOfSameNameAsObject()
        {
            // Loads the actual level or, if special level, load the leaderboard canvas instead
            if (SpecialLevel == false) TransitionManager.Instance.FadeToNextScene(gameObject.name);
            else if (SpecialLevel) LoadLeaderboard();
        }

        public void LoadLeaderboard()
        {
            GameObject levelSelect = GameObject.Find("LevelSelect");
            if (levelSelect != null) levelSelect.SetActive(false);
            else
            {
                Helper.LogWarning("LevelLoad: Level select menu wasn't found and couldn't be deactivated.", this);
                return;
            }

            if (LeaderboardCanvas != null)
            {
                LeaderboardCanvas.SetActive(true);
                LeaderboardCanvas.GetComponent<LevelLeaderboardMenu>().LeaderboardInitialisation(LevelNumber, LeaderboardIDStaging, LeaderboardIDLive);
            }
            else
            {
                levelSelect.SetActive(true);
                Helper.LogWarning("LevelLoad: Level select menu wasn't found and couldn't be deactivated.", this);
                return;
            }
        }

        public void PlayButtonPressSound()
        {
            AudioManager.Instance.Play("Click");
        }

        public void PlayButtonHighlightSound()
        {
            AudioManager.Instance.Play("Select");
        }
    }
}
