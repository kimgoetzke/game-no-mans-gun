using UnityEngine;

namespace CaptainHindsight
{
    public class MainMenu : MonoBehaviour
    {
        public void NewGame()
        {
            TransitionManager.Instance.FadeToNextScene(1);
        }

        public void GiveFeedback()
        {
            Application.OpenURL("https://forms.gle/8paFzArRGK8F6sso7");
        }

        public void QuitGame()
        {
            Helper.Log("Application closed.");
            Application.Quit();
        }

        public void PlayButtonHighlightSound()
        {
            AudioManager.Instance.Play("Select");
        }

        public void PlayButtonPressSound()
        {
            AudioManager.Instance.Play("Click");
        }

        public void PlayTypingSound()
        {
            AudioManager.Instance.Play("Typing");
        }

        public void PlayScrollingSound()
        {
            AudioManager.Instance.Play("Scrolling");
        }
    }
}
