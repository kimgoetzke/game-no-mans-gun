using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

namespace CaptainHindsight
{
    public class LevelSelect : MonoBehaviour
    {
        // Level select canvas references to be set in inspector
        [Header("Level select canvas")]
        [SerializeField] private GameObject levelHolder;
        [SerializeField] private GameObject levelIcon;
        [SerializeField] private GameObject thisCanvas;
        private int numberOfLevels; // Setting number of levels automatically for now
        [SerializeField] private Vector2 iconSpacing;
        [SerializeField] private Color activeTextColour;
        [SerializeField] private Color inactiveTextColour;
        [SerializeField] private Color specialColour;
        [SerializeField] private TextMeshProUGUI totalScoreMesh;
        [SerializeField] private GameObject leaderboardMenu;
        [SerializeField] private Sprite starGold;
        [SerializeField] private Sprite StarSilver;

        // Other references to generate panel layout
        private Rect panelDimensions;
        private Rect iconDimensions;
        private int amountPerPage;
        private int currentLevelCount;

        // References for TotalScoreManager
        private TotalScoreManager scores;
        private List<TotalScoreManager.LevelScoreEntry> scoreList = new List<TotalScoreManager.LevelScoreEntry>();
        private bool isLevelUnlocked;
        private int totalScore;

        // References for level properties/values
        private ScriptableObjectsLedger ledger;

        // Buttons
        [Header("Left/right arrows")]
        [SerializeField] private TextMeshProUGUI buttonLeft;
        [SerializeField] private TextMeshProUGUI buttonRight;
        private PageSwiper swiper;
        private int countToLoadPlayground;
        private bool cheatsOn;

        private void Start()
        {
            // Subscribe to events
            EventManager.Instance.OnLevelSelectSwipe += UpdateArrowImages;
            EventManager.Instance.OnToggleCheats += ToggleCheats;

            // Initialise total score manager, ledger, and components to build grid
            scores = GetComponent<TotalScoreManager>();
            ledger = ScriptableObjectsLedger.Instance;
            panelDimensions = levelHolder.GetComponent<RectTransform>().rect;
            iconDimensions = levelIcon.GetComponent<RectTransform>().rect;

            // Retrieve list of scores
            scoreList = scores.RetrieveListOfScores();
            //Helper.Log(scoreList.Count + " scores received.", this);
            //foreach (var entry in scoreList) Helper.Log("LevelSelect: Entry received - Level: " + entry.level + ", score: " + entry.score);

            // Request total score and update text mesh
            totalScore = TotalScoreManager.CalculateTotalScore();
            totalScoreMesh.text = "Your Score: " + totalScore.ToString("N0");
            //Helper.Log("Total score received: " + totalScore, this);

            numberOfLevels = ledger.LevelSettings.Length - 1;
            // Log error if number of levels <> number of level setting files in list
            //if (numberOfLevels < ledger.LevelSettings.Length - 1 || numberOfLevels > ledger.LevelSettings.Length - 1) Helper.LogError("You have " + (ledger.LevelSettings.Length - 1) + " level setting files but assume " + numberOfLevels + " levels. Make sure that you have a) the as many levels as you have specified in the level select class and b) setting files for each level. Note that The last level is the playground and ignored.");

            // Set basic variables to visualise panel(s) correctly
            //int maxInARow = Mathf.FloorToInt((panelDimensions.width + iconSpacing.x) / (iconDimensions.width + iconSpacing.x)); // Used to calculate panels that fit automatically but I think it's too many
            //int maxInACol = Mathf.FloorToInt((panelDimensions.height + iconSpacing.y) / (iconDimensions.height + iconSpacing.y)); // Used to calculate panels that fit automatically but I think it's too many
            int maxInARow = 3;
            int maxInACol = 2;
            amountPerPage = maxInARow * maxInACol;
            int totalPages = Mathf.CeilToInt((float)numberOfLevels / amountPerPage);

            // Create and load as many panels as required
            LoadPanels(totalPages);
        }

        public void LoadPlayground()
        {
            if (cheatsOn == false) return;

            countToLoadPlayground += 1;
            Helper.Log("LevelSelect: What are you doing? - " + countToLoadPlayground + "...");

            if (countToLoadPlayground == 5)
            {
                TransitionManager.Instance.FadeToNextScene("Level-99");
                Helper.Log("LevelSelect: Loading playground.");
                countToLoadPlayground = 0;
            }
        }

        #region Set up the grid, panels and icons
        private void LoadPanels(int numberOfPanels)
        {
            GameObject panelClone = Instantiate(levelHolder);
            swiper = levelHolder.AddComponent<PageSwiper>();
            swiper.TotalPages = numberOfPanels;

            for (int i = 1; i <= numberOfPanels; i++)
            {
                GameObject panel = Instantiate(panelClone);
                panel.transform.SetParent(thisCanvas.transform, false);
                panel.transform.SetParent(levelHolder.transform);
                panel.name = "Page-" + i;
                panel.GetComponent<RectTransform>().localPosition = new Vector2(panelDimensions.width * (i - 1), 0);
                SetUpGrid(panel);
                int numberOfIcons = i == numberOfPanels ? numberOfLevels - currentLevelCount : amountPerPage;
                LoadIcons(numberOfIcons, panel);
            }
            Destroy(panelClone);
        }

        private void SetUpGrid(GameObject panel)
        {
            GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(iconDimensions.width, iconDimensions.height);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.spacing = iconSpacing;
        }

        private void LoadIcons(int numberOfIcons, GameObject parentObject)
        {
            for (int i = 1; i <= numberOfIcons; i++)
            {
                // Instantiate 'icon', set parent and name of game object (name must be identical with file
                // name because the button on the object will load a scene of the same name (icon.name)
                currentLevelCount++;
                GameObject icon = Instantiate(levelIcon);
                icon.transform.SetParent(thisCanvas.transform, false);
                icon.transform.SetParent(parentObject.transform);
                icon.name = "Level-" + currentLevelCount;

                // Set variables in instatiated object
                LevelLoad iconSettings = icon.GetComponent<LevelLoad>();
                iconSettings.LevelNumber = currentLevelCount;
                iconSettings.LeaderboardCanvas = leaderboardMenu;
                iconSettings.SpecialLevel = ledger.LevelSettings[currentLevelCount].SpecialLevel;
                iconSettings.LeaderboardIDStaging = ledger.LevelSettings[currentLevelCount].LeaderboardIDStaging;
                iconSettings.LeaderboardIDLive = ledger.LevelSettings[currentLevelCount].LeaderboardIDLive;

                // Highlight if special level
                if (ledger.LevelSettings[currentLevelCount].SpecialLevel)
                {
                    icon.GetComponent<Image>().color = specialColour;
                }

                // Level - set references for transform and text
                TextMeshProUGUI levelLabelTMPro = icon.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                levelLabelTMPro.SetText("LEVEL " + currentLevelCount);

                // Stats - set references for transform and text
                TextMeshProUGUI statsLabelTMPro = icon.transform.Find("Stats").GetComponent<TextMeshProUGUI>();

                // Status (for unlock) - set references for transform and text
                TextMeshProUGUI statusLabelTMPro = icon.transform.Find("Status").GetComponent<TextMeshProUGUI>();
                
                // Unlock level if score is higher than unlock requirement
                if (totalScore >= ledger.LevelSettings[currentLevelCount].ScoreRequiredToUnlock) isLevelUnlocked = true;
                else isLevelUnlocked = false;

                if (isLevelUnlocked)
                {
                    // Deactivate padlock image
                    icon.transform.Find("Padlock").gameObject.SetActive(false);

                    // Update stats with highest score and status text, too
                    bool scoreInScoreList = false;
                    for (int j = 0; j < scoreList.Count; j++)
                    {
                        if (scoreList[j].level == currentLevelCount)
                        {
                            statsLabelTMPro.SetText("Best score: " + scoreList[j].score.ToString("N0"));
                            statusLabelTMPro.text = "";
                            scoreInScoreList = true;

                            if (scoreList[j].score >= ledger.LevelSettings[currentLevelCount].Gold)
                            {
                                icon.transform.Find("Star").gameObject.SetActive(true);
                            }
                            else if (scoreList[j].score >= ledger.LevelSettings[currentLevelCount].Silver)
                            {
                                GameObject star = icon.transform.Find("Star").gameObject;
                                star.SetActive(true);
                                star.GetComponent<Image>().sprite = StarSilver;
                            }
                            else
                            {
                                icon.transform.Find("Star").gameObject.SetActive(false);
                            }
                        }
                    }

                    if (scoreInScoreList == false)
                    {
                        statsLabelTMPro.SetText("Not played yet");
                        statusLabelTMPro.text = "";
                        icon.transform.Find("Star").gameObject.SetActive(false);
                    }
                }
                else if (isLevelUnlocked == false)
                {
                    // Deactivate star
                    icon.transform.Find("Star").gameObject.SetActive(false);

                    // Update text and colour
                    levelLabelTMPro.color = inactiveTextColour;
                    statsLabelTMPro.text = "Unlock and play to get score";
                    statsLabelTMPro.color = inactiveTextColour;

                    // Set unlock requirements from with scriptable object
                    statusLabelTMPro.text = "UNLOCKED AT " + ledger.LevelSettings[currentLevelCount].ScoreRequiredToUnlock.ToString("N0");

                    // Disable button
                    icon.GetComponent<UnityEngine.UI.Button>().interactable = false;
                }
            }
        }
        #endregion

        #region Managing buttons and arrow keys/images
        public void NextPage()
        {
            // Swipe, if possible
            bool successful = swiper.NextPage();

            // Animate button
            buttonRight.transform.DORestart();
            buttonRight.transform.DOKill();
            buttonRight.transform.DOPunchScale(new Vector3(1, 1, 1), 1, 1, 0.2f);

            // Play relevant audio
            if (successful) AudioManager.Instance.Play("Click");
            else AudioManager.Instance.Play("Negative");

            // Check status of arrow images and update if necessary
            EventManager.Instance.CheckStatusOfLevelSelectPanels();
        }

        public void PreviousPage()
        {
            // Swipe, if possible
            bool successful = swiper.PreviousPage();

            // Animate button
            buttonLeft.transform.DORestart();
            buttonLeft.transform.DOKill();
            buttonLeft.transform.DOPunchScale(new Vector3(1, 1, 1), 1, 1, 0.2f);

            // Play relevant audio
            if (successful) AudioManager.Instance.Play("Click");
            else AudioManager.Instance.Play("Negative");

            // Check status of arrow images and update if necessary
            EventManager.Instance.CheckStatusOfLevelSelectPanels();
        }

        private void UpdateArrowImages()
        {
            swiper.StatusOfPanels(out bool thereIsAPanelToTheLeft, out bool thereIsAPanelToTheRight);

            if (thereIsAPanelToTheLeft) buttonLeft.alpha = 1f;
            else buttonLeft.alpha = 0.2f;

            if (thereIsAPanelToTheRight) buttonRight.alpha = 1f;
            else buttonRight.alpha = 0.2f;
        }
        #endregion

        #region Managing events
        private void ToggleCheats(bool status) => cheatsOn = status;

        private void OnDestroy()
        {
            EventManager.Instance.OnLevelSelectSwipe -= UpdateArrowImages;
            EventManager.Instance.OnToggleCheats -= ToggleCheats;
        }
        #endregion
    }
}