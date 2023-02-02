using UnityEngine;

namespace CaptainHindsight
{
    public class ScriptableObjectsLedger : MonoBehaviour
    {
        public static ScriptableObjectsLedger Instance;

        [SerializeField] public LevelSettings[] LevelSettings;
        [SerializeField] public PlayerSettings[] PlayerSettings;

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
    }
}
