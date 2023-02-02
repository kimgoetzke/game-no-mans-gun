using UnityEngine;

namespace CaptainHindsight
{
    public class GameHelper : MonoBehaviour
    {
        [SerializeField] private bool scriptActive;
        [SerializeField] private bool cheatsActive;
        [SerializeField] private bool resetMenus;
        [SerializeField] private GameObject[] menusToActivate;
        [SerializeField] private GameObject[] menusToDeactivate;
        [SerializeField] private GameObject[] cheats;

        private void Awake()
        {
            if (scriptActive != true) return;

            if (resetMenus)
            {
                foreach (var item in menusToActivate)
                {
                    item.SetActive(true);
                }

                foreach (var item in menusToDeactivate)
                {
                    item.SetActive(false);
                }

                Helper.Log("GameHelper: Main menu game objects configured for play.");
            }

            if (cheatsActive == false)
            {
                if (cheats.Length > 0) foreach (var item in cheats) item.SetActive(false);
                EventManager.Instance.ToggleCheats(false);
            }
            else
            {
                if (cheats.Length > 0) foreach (var item in cheats) item.SetActive(true);
                EventManager.Instance.ToggleCheats(true);
            }

            Helper.Log("GameHelper: Cheats = " + cheatsActive + ".");
        }
    }
}
