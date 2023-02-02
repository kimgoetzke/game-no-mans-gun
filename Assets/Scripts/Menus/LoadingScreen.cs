using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace CaptainHindsight
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private Transform loadingObject;
        [SerializeField] private Transform logo;
        [SerializeField] private GameObject continueButton;

        private void Start()
        {

#if UNITY_WEBGL

            continueButton.SetActive(true);

            logo.GetComponent<Image>().DOFade(1f, 0.5f).OnComplete(() =>
            {
                logo.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 3).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            });

#else

            continueButton.SetActive(false);

            loadingObject.GetComponent<TextMeshProUGUI>().DOFade(0.6f, 1f).SetUpdate(UpdateType.Normal).OnComplete(() => { LoadGame(); });
            logo.GetComponent<Image>().DOFade(1f, 0.5f).OnComplete(() =>
            {
                logo.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 3).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            });

#endif

        }

        public async void LoadGame()
        {
            var scene = SceneManager.LoadSceneAsync("MainMenu");
            scene.allowSceneActivation = false;

            await Task.Delay(System.TimeSpan.FromSeconds(2f));

            if (scene.progress >= 0.9f)
            {
                logo.GetComponent<Image>().DOFade(0f, 1f);
                loadingObject.GetComponent<TextMeshProUGUI>().DOFade(0f, 1f);
                await Task.Delay(System.TimeSpan.FromSeconds(1.5f));
                
                logo.DOKill();
                loadingObject.DOKill();
                scene.allowSceneActivation = true;
            }
        }

        // Reference by button on screen. This is required because otherwise all audio is being
        // blocked from loading by modern browsers.
        public void StartWebGL()
        {
            continueButton.transform.DOScale(0f, 0.5f);
            loadingObject.GetComponent<TextMeshProUGUI>().alpha = 1f;
            loadingObject.GetComponent<TextMeshProUGUI>().text = "Starting now...";
            continueButton.transform.DOKill();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
