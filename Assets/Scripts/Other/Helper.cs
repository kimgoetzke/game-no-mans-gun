using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaptainHindsight
{
    public static class Helper
    {
        public static float GetAngelFromVectorFloat(Vector3 direction)
        {
            // Get the angel from a vector and return it as a float
            direction = direction.normalized;
            float n = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;

            return n;
        }

        public static int ReturnLevelNumberFromSceneName()
        {
            // Extract number of level from scene name and convert to int
            string levelName = SceneManager.GetActiveScene().name;
            var everythingBeforeHyphen = levelName.LastIndexOf('-');
            var onlyTheNumber = levelName.Substring(everythingBeforeHyphen + 1);
            int levelNumber;
            int.TryParse(onlyTheNumber, out levelNumber);
            //Log("Helper: Index " + levelNumber + " for current level (used to find scene settings in SO ledger).");
            return levelNumber;
        }

        public static string ReturnLeaderboardKeyForActiveScene()
        {
            string leaderboardKey = SceneManager.GetActiveScene().name;
            Log("Helper: Leaderboard key '" + leaderboardKey + "' for current level.");
            return leaderboardKey;
        }

        public static void DeleteAllChildGameObjects(Transform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        // A safe way to get components
        public static T GetComponentSafely<T>(this GameObject obj) where T : MonoBehaviour
        {
            T component = obj.GetComponent<T>();

            if (component == null) Log("Helper: Expected to find component of type " + typeof(T) + " but found none.", obj);
            
            return component;
        }

        #region Logging
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void Log(object message, Object obj)
        {
            UnityEngine.Debug.Log(message, obj);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void LogWarning(object message, Object obj)
        {
            UnityEngine.Debug.Log(message, obj);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void LogError(object message, Object obj)
        {
            UnityEngine.Debug.Log(message, obj);
        }
        #endregion
    }
}