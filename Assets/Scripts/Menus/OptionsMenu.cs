using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CaptainHindsight
{
    public class OptionsMenu : MonoBehaviour
    {
        [Header("General References")]
        [SerializeField] private AudioMixer mixer;
        private float multiplier = 30f;

        [Header("Graphics References")]
        [SerializeField] RenderPipelineAsset[] qualityLevels;
        [SerializeField] TMP_Dropdown qualityDropdown;

        [Header("SFX References")]
        [SerializeField] private Slider soundVolumeSlider;
        [SerializeField] private TextMeshProUGUI soundVolumeText;

        [Header("Music References")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeText;

        [Header("Player Name References")]
        [SerializeField] private TMP_InputField nameInput;

        [Header("Cheat & Delete Progress References")]
        [SerializeField] private GameObject notice;
        [SerializeField] private bool needToReloadScene;
        private int countToCheat;
        private TotalScoreManager totalScoreManager;

        private void Awake()
        {
            soundVolumeSlider.onValueChanged.AddListener(SoundSliderValueChanged);
            musicVolumeSlider.onValueChanged.AddListener(MusicSliderValueChanged);
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            totalScoreManager = gameObject.AddComponent<TotalScoreManager>();
        }

        public void ChangeGraphicsQualityLevel(int value)
        {
            QualitySettings.SetQualityLevel(value);
            QualitySettings.renderPipeline = qualityLevels[value];
        }

        private void SoundSliderValueChanged(float value)
        {
            mixer.SetFloat("SFX", Mathf.Log10(value) * multiplier);

            float volume = (value * 100) / soundVolumeSlider.maxValue;
            if (volume > 1) soundVolumeText.text = volume.ToString("0");
            if (volume.ToString("0") == "1") soundVolumeText.text = "OFF";
        }

        private void MusicSliderValueChanged(float value)
        {
            mixer.SetFloat("Music", Mathf.Log10(value) * multiplier);

            float volume = (value * 100) / musicVolumeSlider.maxValue;
            if (volume > 1) musicVolumeText.text = volume.ToString("0");
            if (volume.ToString("0") == "1") musicVolumeText.text = "OFF";
        }

        public void DeleteAllProgress(int value)
        {
            if (value != 2) return;
            TotalScoreManager.DeleteAllProgress();
            GhostRecorder.DeleteAllRecordings();
            //if (PlayerPrefs.HasKey("PlayerName")) PlayerPrefs.DeleteKey("PlayerName");
            AudioManager.Instance.Play("Die");
            notice.SetActive(true);
            Helper.Log("OptionsMenu: The local progress save files and recordings have been deleted but any changes in to the games audio and graphic settings remain saved unaffected. ");
            needToReloadScene = true;
        }

        public void EnableCheats()
        {
            countToCheat += 1;
            Helper.Log("OptionsMenu: What are you doing? - " + countToCheat + "...");

            if (countToCheat == 3)
            {
                int score = TotalScoreManager.CalculateTotalScore();
                if (score == 0) TotalScoreManager.AddHighscoreEntry(1, 80000);
                if (score >= 1000) TotalScoreManager.AddHighscoreEntry(2, 20000);
                AudioManager.Instance.Play("Win");
                notice.SetActive(true);
                Helper.Log("OptionsMenu: You're a cheater.");
                needToReloadScene = true;
                countToCheat = 0;
            }
        }

        #region Save/load all changes when leaving/entering the options menu
        public void OnDisable()
        {
            PlayerPrefs.SetInt("GraphicsQuality", qualityDropdown.value);
            PlayerPrefs.SetFloat("SoundVolume", soundVolumeSlider.value);
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            PlayerPrefs.SetString("PlayerName", nameInput.text);

            //Helper.Log("Graphics quality level saved as: " + QualitySettings.GetQualityLevel());
            //Helper.Log("SFX value saved as: " + soundVolumeSlider.value);
            //Helper.Log("Music value saved as: " + musicVolumeSlider.value);
            Helper.Log("Player name saved as: " + nameInput.text);

            if (needToReloadScene)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                Helper.Log("OptionsMenu: Scene had to be reloaded. You probably deleted your progress files or enabled cheats.");
            }
        }

        private void OnEnable()
        {
            qualityDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", 4);
            soundVolumeSlider.value = PlayerPrefs.GetFloat("SoundVolume", soundVolumeSlider.value);
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", musicVolumeSlider.value);
            nameInput.text = PlayerPrefs.GetString("PlayerName");
        }
        #endregion
    }
}
