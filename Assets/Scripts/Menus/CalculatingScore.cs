using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaptainHindsight
{
    public class CalculatingScore : BStateDependent
    {
        [Header("General")]
        private Canvas canvas;
        private Camera cam;

        [Header("Final score")]
        [SerializeField] private TextMeshProUGUI finalScoreMesh;
        private int finalScore;
        private List<int> finalScoreComponents = new List<int>();
        [SerializeField] private Image starLeft;
        [SerializeField] private Image starRight;
        [SerializeField] private Sprite[] starTiers;
        [SerializeField] private ParticleSystem starParticlesGold;
        [SerializeField] private ParticleSystem starParticlesSilver;

        [Header("Configuration")]
        private LevelScoreManager scoreManager;
        private LootLockerManager lootLockerManager;
        private Dictionary<string, int> scoreEventsList = new Dictionary<string, int>();
        private List<Transform> scoreEventRows = new List<Transform>();

        [Header("Colours")]
        [SerializeField] private Color bonusColour;
        [SerializeField] private Color penaltyColour;

        [Header("Templates")]
        [SerializeField] private Transform entryContainer;
        [SerializeField] private Transform entryTemplate;
        [SerializeField] private GameObject newRecordHolder;
        [SerializeField] private TextMeshProUGUI newRecordMesh;

        private void Start()
        {
            starLeft.gameObject.SetActive(false);
            starRight.gameObject.SetActive(false);
            entryTemplate.gameObject.SetActive(false);
            scoreManager = LevelScoreManager.Instance;
            lootLockerManager = LootLockerManager.Instance;
            finalScoreMesh.text = 0.ToString();

            // Set canvas camera to UI camera
            canvas = GetComponent<Canvas>();
            if (GameObject.Find("UICam") == null) Helper.LogError("CalculatingScore: UI camera not found. Consider changing CalculatingScore and assigning cam in the editor.", this);
            else
            {
                cam = GameObject.Find("UICam").GetComponent<Camera>();
                canvas.worldCamera = cam;
            }
        }

        private async void TriggerScoreCalculation()
        {
            // Reduce background music volume
            AudioManager.Instance.ReduceBackgroundMusicVolume();

            // Reset everything so it can events can be requested multiple times
            finalScore = 0;
            scoreEventRows.Clear();

            // Request score events
            RequestEventsFromScoreManager();

            // Prepare the rows on the UI for each score event
            CreateEventRowsAndListsforAnimation();

            // Calculate final score and trigger event that calculation is complete
            CalculateFinalScore();
            EventManager.Instance.EndOfLevelScoreCalculated(finalScore);

            // Show each row on UI and animate it as well as the final score counter
            await ShowAndAnimateEachScoreEventRow(scoreEventRows, finalScoreComponents);

            // Read level number for current level
            int levelNumber = Helper.ReturnLevelNumberFromSceneName();

            // Trigger stars and particles if score is a silver or gold achievement
            SetAndAnimateStars(levelNumber);

            // Get data to make decisions on saving score and submitting to leaderboard
            bool specialLevel = ScriptableObjectsLedger.Instance.LevelSettings[levelNumber].SpecialLevel;
            bool highestScoreToDate = TotalScoreManager.CheckIfHighestScoreToDate(levelNumber, finalScore);

            // Save score as new highscore and show new record banner, if applicable
            await AddHighscoreAndShowNewRecordBanner(levelNumber);

            // Update online leaderboard, if applicable
            await UpdateLeaderboard(levelNumber, specialLevel, highestScoreToDate);

            // Ping event so that buttons on end of level menu can be moved into the screen
            EventManager.Instance.EndOfLevelAnimationComplete();

            // Increase background music volume
            AudioManager.Instance.IncreaseBackgroundMusicVolume();
        }

        #region Calculating and animating level score
        private void RequestEventsFromScoreManager()
        {
            scoreEventsList = scoreManager.ShareScoringDataAsDictionary();
            //scoreEventsCount = scoreEventsList.Count;
            //Helper.Log(scoringEventsCount + " scoring events found.");
            //foreach (var entry in scoringEventsList) Helper.Log("Entry received: " + entry.Key + " " + entry.Value);
        }

        private void CreateEventRowsAndListsforAnimation()
        {
            // To be moved into separate method once it works
            foreach (var entry in scoreEventsList)
            {
                // Instatiate UI transform and set active
                Transform entryTransform = Instantiate(entryTemplate, entryContainer);

                // Set description to dictionary key
                string name = entry.Key;
                entryTransform.Find("Description").GetComponent<TextMeshProUGUI>().text = name;

                // Set score to dictionary value
                int score = entry.Value;
                TextMeshProUGUI scoreMesh = entryTransform.Find("Score").GetComponent<TextMeshProUGUI>();
                scoreMesh.text = score.ToString("N0");
                if (score > 0) scoreMesh.color = bonusColour;
                else if (score < 0) scoreMesh.color = penaltyColour;

                // Deactivate object for now
                entryTransform.gameObject.SetActive(false);

                // Create lists for animations later
                scoreEventRows.Add(entryTransform);
                finalScoreComponents.Add(entry.Value);
            }
        }

        private void CalculateFinalScore()
        {
            foreach (int value in finalScoreComponents)
            {
                finalScore += value;
            }
            //Helper.Log("CalculatingScore: Your final score is " + finalScore);
        }

        private async Task ShowAndAnimateEachScoreEventRow(List<Transform> scoreEventRows, List<int> finalScoreComponents)
        {
            // Initial delay to ensure that UI has faded in properly
            await Task.Delay(System.TimeSpan.FromSeconds(0.5f));

            // Set index so that final score can be build/animated component by component
            int index = 0;
            int animatedFinalScore = 0;

            foreach (Transform row in scoreEventRows)
            {
                // Set score item row/transform active
                row.gameObject.SetActive(true);

                // Animate the score item
                row.DOPunchScale(new Vector3(1, 1, 1), 1, 1, 0.2f).SetUpdate(UpdateType.Normal, true);

                if (finalScoreComponents[index] > 0) AudioManager.Instance.Play("PositiveEvent");
                else if (finalScoreComponents[index] < 0) AudioManager.Instance.Play("NegativeEvent");

                // Animate final score for this score item
                DOVirtual.Int(animatedFinalScore, animatedFinalScore += finalScoreComponents[index], 1.5f, valueNow =>
                {
                    finalScoreMesh.text = valueNow.ToString("N0");
                    AudioManager.Instance.Play("Counting");
                });
                index++;

                // Wait before moving on on the next row or finishing
                await Task.Delay(System.TimeSpan.FromSeconds(1.7f));
            }
            await Task.Yield();
        }

        private void SetAndAnimateStars(int levelNumber)
        {
            if (finalScore >= ScriptableObjectsLedger.Instance.LevelSettings[levelNumber].Gold)
            {
                starLeft.gameObject.SetActive(true);
                starLeft.transform.DOScale(new Vector3(0, 0, 0), 1.5f).From().SetEase(Ease.OutElastic).SetUpdate(UpdateType.Normal, true);
                //Instantiate(starParticlesGold, new Vector3(-5.2f, 4f, 10f), transform.rotation);
                Instantiate(starParticlesGold, starLeft.transform.position, transform.rotation);

                starRight.gameObject.SetActive(true);
                starRight.transform.DOScale(new Vector3(0, 0, 0), 1.5f).From().SetEase(Ease.OutElastic).SetUpdate(UpdateType.Normal, true);
                //Instantiate(starParticlesGold, new Vector3(5.2f, 4f, 10f), transform.rotation);
                Instantiate(starParticlesGold, starRight.transform.position, transform.rotation);

                AudioManager.Instance.Play("AchievementGold");
            }
            else if (finalScore >= ScriptableObjectsLedger.Instance.LevelSettings[levelNumber].Silver)
            {
                starLeft.gameObject.SetActive(true);
                starLeft.sprite = starTiers[1];
                starLeft.transform.DOScale(new Vector3(0, 0, 0), 1.5f).From().SetEase(Ease.OutElastic).SetUpdate(UpdateType.Normal, true);
                //Instantiate(starParticlesSilver, new Vector3(-5.2f, 4f, 10f), transform.rotation);
                Instantiate(starParticlesSilver, starLeft.transform.position, transform.rotation);

                starRight.gameObject.SetActive(true);
                starRight.sprite = starTiers[1];
                starRight.transform.DOScale(new Vector3(0, 0, 0), 1.5f).From().SetEase(Ease.OutElastic).SetUpdate(UpdateType.Normal, true);
                //Instantiate(starParticlesSilver, new Vector3(5.2f, 4f, 10f), transform.rotation);
                Instantiate(starParticlesSilver, starRight.transform.position, transform.rotation);
            }
        }
        #endregion

        #region Checking and setting/updating highscore and leaderboard
        private async Task AddHighscoreAndShowNewRecordBanner(int levelNumber)
        {
            // Add highscore if applicable and return difference to previous score
            int differenceToPreviousScore = TotalScoreManager.AddHighscoreEntry(levelNumber, finalScore);

            // Trigger new record animation and save score if this is the highest score ever achieved by this player
            if (differenceToPreviousScore > 0)
            {
                newRecordHolder.SetActive(true);
                newRecordHolder.GetComponent<Image>().DOFade(0.85f, 0.3f).SetUpdate(UpdateType.Normal, true)
                    .OnComplete(() => newRecordHolder.transform.DOPunchScale(new Vector3(1, 1, 1), 1, 1, 0.2f).SetUpdate(UpdateType.Normal, true));
                newRecordMesh.text = "You beat your previous score by " + differenceToPreviousScore.ToString("N0");

                //yield return new WaitForSeconds(0.3f);
                await Task.Delay(System.TimeSpan.FromSeconds(0.3f));
                AudioManager.Instance.Play("Record");
            }
        }

        private async Task UpdateLeaderboard(int levelNumber, bool specialLevel, bool highestScoreToDate)
        {
            // If this score is the highest score to date, submit score to online leaderboard
            Helper.Log("CalculatingScore: Special level = " + specialLevel + ", highest score: " + highestScoreToDate + ".", this);
            if (specialLevel && highestScoreToDate)
            {
                // Use leaderboard live ID if LootLocker's development mode is off, otherwise use leaderboard staging ID
                int leaderboardID;
                if (lootLockerManager.DevModeOn == false) leaderboardID = ScriptableObjectsLedger.Instance.LevelSettings[levelNumber].LeaderboardIDLive;
                else leaderboardID = ScriptableObjectsLedger.Instance.LevelSettings[levelNumber].LeaderboardIDStaging;

                // Submit score to the correct leaderboard
                lootLockerManager.SubmitScore(leaderboardID, finalScore);
            }

            await Task.Yield();
        }
        #endregion

        #region Managing events and game states
        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            if (state == GameState.EndOfLevel) TriggerScoreCalculation();
        }
        #endregion
    }
}