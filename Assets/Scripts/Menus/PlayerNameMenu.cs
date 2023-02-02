using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace CaptainHindsight
{
    public class PlayerNameMenu : MonoBehaviour
    {
        [SerializeField] private GameObject playerNamePrompt;
        [SerializeField] private TMP_InputField inputField;

        private void Start()
        {
            //PlayerPrefs.DeleteKey("PlayerName"); // For testing purposes only
            if (PlayerPrefs.HasKey("PlayerName")) playerNamePrompt.SetActive(false);
        }

        public void SetPlayerName()
        {
            name = inputField.text;
            if (name == "") name = "The One Who Did Not Set A Name";
            Helper.Log("Player name set to: " + name + ".", this);

            // Update PlayerPrefs
            PlayerPrefs.SetString("PlayerName", name);

            // Update LootLocker
            string localPlayerIdentifier = LootLockerManager.Instance.GetPlayerIdentifier();
            LootLockerManager.Instance.FetchOrSetPlayerName(localPlayerIdentifier);
        }

        public void ValidatInput()
        {
            // Limit player name to 30 characters
            string text = inputField.text;
            if (text.Length > 30)
            {
                text = text.Substring(0, 30);
                inputField.text = text;
            }
        }

        public string LoadPlayerName()
        {
            string name;
            name = PlayerPrefs.GetString("PlayerName", "Unknown");
            return name;
        }

        public void PlayTypingSound()
        {
            AudioManager.Instance.Play("Typing");
        }
    }
}
