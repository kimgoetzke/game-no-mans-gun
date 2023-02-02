using UnityEngine;
using UnityEngine.Rendering;

namespace CaptainHindsight
{
    public class GraphicsManager : MonoBehaviour
    {
        // Note that this singleton is private so it cannot be accessed by other scripts
        private static GraphicsManager Instance;

        [SerializeField] private RenderPipelineAsset[] qualityLevelsURP;

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

        private void OnEnable()
        {
            int value = PlayerPrefs.GetInt("GraphicsQuality", 4);
            QualitySettings.SetQualityLevel(value);
            QualitySettings.renderPipeline = qualityLevelsURP[value];
            Helper.Log("GraphicsManager: Graphics quality loaded at settings level: " + QualitySettings.GetQualityLevel() + ".");
        }
    }
}
