using UnityEngine;
using DG.Tweening;

namespace CaptainHindsight
{
    public class Tutorial : MonoBehaviour
    {
        [SerializeField] private GameObject desktopControls;
        [SerializeField] private GameObject mobileControls;
        [SerializeField] private GameObject uiTutorial;
        [SerializeField] private GameObject uiArrows;

        private void Start()
        {
            uiTutorial.SetActive(false);
            BeginTutorial();
        }

        private void BeginTutorial()
        {

            #if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_WEBGL_API

            desktopControls.SetActive(true);
            mobileControls.SetActive(false);

            #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

            desktopControls.SetActive(false);
            mobileControls.SetActive(true);

            #endif

        }

        public void NextTip()
        {
            desktopControls.SetActive(false);
            mobileControls.SetActive(false);
            uiTutorial.SetActive(true);
            uiArrows.transform.DOMove(new Vector3(uiArrows.transform.position.x, uiArrows.transform.position.y + 70, uiArrows.transform.position.z), 1).SetUpdate(UpdateType.Normal, true).SetLoops(-1);
        }

        public void StartPlaying()
        {
            uiTutorial.SetActive(false);
            GameStateManager.Instance.SwitchState(GameState.Countdown);
        }

        public void PlayButtonHighlightSound()
        {
            AudioManager.Instance.Play("Select");
        }

        public void PlayButtonPressSound()
        {
            AudioManager.Instance.Play("Click");
        }
    }
}
