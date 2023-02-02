using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace CaptainHindsight
{
    public class EndOfLevelMenu : BStateDependent
    {
        [HideInInspector] public static bool GameIsPaused = false;
        [SerializeField] private GameObject levelEndMenuUI;
        [SerializeField] private RectTransform buttons;
        [SerializeField] private GameObject nextLevelButton;
        private bool buttonsInSight;

        private async void ActivateMenuCanvas()
        {
            levelEndMenuUI.SetActive(true);
            levelEndMenuUI.GetComponent<Image>().DOFade(0.8f, 1.5f);

            // Move buttons into sight after 10 seconds, just in case event didn't fire
            await Task.Delay(System.TimeSpan.FromSeconds(10));
            if (buttonsInSight == false) MoveButtonsIntoSight();
        }

        private void MoveButtonsIntoSight()
        {
            // This method is called by an event (triggered in CalculatingScore) or after 10 seconds
            buttons.DOAnchorPosY(120, 1f);
            buttonsInSight = true;
        }

        private void CheckStatus()
        {
            int levelNumber = Helper.ReturnLevelNumberFromSceneName();
            if (ScriptableObjectsLedger.Instance.LevelSettings.Length - 1 < levelNumber + 1)
            {
                // Deactivate resume button
                nextLevelButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
                TextMeshProUGUI textMesh = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
                textMesh.alpha = 0.2f;
                textMesh.text = "The End";
                Helper.Log("EndOfLevelMenu: This is level " + levelNumber + " out of " + (ScriptableObjectsLedger.Instance.LevelSettings.Length - 1) + ". Next level button has been deactivated.");
            }
            else if (ScriptableObjectsLedger.Instance.LevelSettings[levelNumber + 1].ScoreRequiredToUnlock > TotalScoreManager.CalculateTotalScore())
            {
                // Deactivate resume button
                nextLevelButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
                TextMeshProUGUI textMesh = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
                textMesh.alpha = 0.2f;
                textMesh.text = "Locked";
                Helper.Log("EndOfLevelMenu: Total score too low to unlock next level. Next level button has been deactivated.");
            }
        }

        #region Button functionality
        public void RestartLevel()
        {
            TransitionManager.Instance.FadeToNextScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void NextLevel()
        {
            TransitionManager.Instance.FadeToNextScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void MainMenu()
        {
            TransitionManager.Instance.FadeToNextScene("MainMenu");
        }

        public void QuitGame()
        {
            Helper.Log("Application closed.");
            Application.Quit();
        }
        #endregion

        #region Managing audio
        public void PlayButtonHighlightSound()
        {
            AudioManager.Instance.Play("Select");
        }

        public void PlayButtonPressSound()
        {
            AudioManager.Instance.Play("Click");
        }
        #endregion

        #region Managing events and game states
        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            if (state == GameState.EndOfLevel) ActivateMenuCanvas();
        }

        protected override void OnEnable()
        {
            EventManager.Instance.OnEndOfLevelAnimationComplete += MoveButtonsIntoSight;
            EventManager.Instance.OnEndOfLevelAnimationComplete += CheckStatus;
            GameStateManager.OnGameStateChange += ActionGameStateChange;
        }

        protected override void OnDestroy()
        {
            EventManager.Instance.OnEndOfLevelAnimationComplete -= MoveButtonsIntoSight;
            EventManager.Instance.OnEndOfLevelAnimationComplete -= CheckStatus;
            GameStateManager.OnGameStateChange -= ActionGameStateChange;
        }
        #endregion
    }
}
