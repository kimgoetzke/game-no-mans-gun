using UnityEngine;
using System.Threading.Tasks;
using TMPro;

namespace CaptainHindsight
{
    public class OfflineManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        private bool offlineMode;

        private void Start()
        {
            text.gameObject.SetActive(false);

            // Request and set offline mode with a delay to make sure that
            // LootLocker had the time to respond
            DelayedStatusCheck(3f);
        }

        private async void DelayedStatusCheck(float delay)
        {
            // Wait a moment
            await Task.Delay(System.TimeSpan.FromSeconds(delay));

            // Request status
            offlineMode = LootLockerManager.Instance.RequestStatus();
            Helper.Log("OfflineManager: Offline mode = " + offlineMode + ".");

            // Update UI accordingly
            if (offlineMode) text.gameObject.SetActive(true);
            else text.gameObject.SetActive(false);
        }
    }
}
