using UnityEngine;

namespace CaptainHindsight
{
    [CreateAssetMenu(fileName = "Level-", menuName = "Scriptable Object/New Level Settings File", order = 1)]
    public class LevelSettings : ScriptableObject
    {
        [Header("Level scene file")]
        public Object Scene;

        [Header("Level settings")]
        public int ScoreRequiredToUnlock;
        public float timeToCompleteLevel = 60f;

        [Header("Ranking bands")]
        public int Gold;
        public int Silver;

        [Header("Special settings")]
        public bool SpecialLevel;
        public int LeaderboardIDStaging;
        public int LeaderboardIDLive;
    }
}
