using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace CaptainHindsight
{
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance;

        [SerializeField] private Image blackImage;

        private void Awake()
        {
            blackImage.gameObject.SetActive(true);

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void FadeToNextScene(string levelName) => LoadLevelAfterFade(levelName, 999);

        public void FadeToNextScene(int levelNumber) => LoadLevelAfterFade("", levelNumber);

        private void Start()
        {
            // Fade out black image
            blackImage.DOFade(0f, 1f).SetUpdate(UpdateType.Normal, true);

            // Check if there's an object with a state priority tag in the scence;
            // if yes, switch to it - if no, switch to Countdown state
            GameObject[] priorityStates = GameObject.FindGameObjectsWithTag("StartHere");
            if (priorityStates.Length == 0)
            {
                GameStateManager.Instance.SwitchState(GameState.Countdown);
                return;
            }
            else for (int i = 0; i < priorityStates.Length; i++)
                {
                    //Helper.Log("TransitionManager: Priority game state found: " + priorityStates[i].name + ".");
                    if (priorityStates[i].name == "GameHelper")
                    {
                        GameStateManager.Instance.SwitchState(GameState.Menu);
                        return;
                    }
                    else if (priorityStates[i].name == "Tutorial")
                    {
                        GameStateManager.Instance.SwitchState(GameState.Tutorial);
                        return;
                    }
                    else Helper.LogError("TransitionManager: Priority stage in scene but couldn't be recognised.");
                }
        }

        private async void LoadLevelAfterFade(string name, int number)
        {
            // Fade in black overlay
            blackImage.DOFade(1f, 1f).SetUpdate(UpdateType.Normal, true);

            // Switch state
            GameStateManager.Instance.SwitchState(GameState.Transition);

            await Task.Delay(System.TimeSpan.FromSeconds(2f));

            // Kill all DOTweens to prevent errors/warnings
            DOTween.KillAll();

            // Load next scene by number or, if set to 999 (loaded by name), load by name
            if (number == 999) SceneManager.LoadScene(name);
            else SceneManager.LoadScene(number);
        }
    }
}
