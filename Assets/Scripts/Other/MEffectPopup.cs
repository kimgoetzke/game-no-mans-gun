using UnityEngine;
using TMPro;

namespace CaptainHindsight
{
    public class MEffectPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private GameObject textObject;
        [SerializeField] private Color damageColour = new Color(168, 25, 25, 204);
        [SerializeField] private Color healthColour = new Color(75, 176, 57, 204);

        private void Awake()
        {
            textMesh = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }

        public void Initialisation(ActionType effectType, string text)
        {
            textMesh.text = text;

            switch (effectType)
            {
                case ActionType.Damage:
                    textMesh.color = damageColour;
                    break;
                case ActionType.Health:
                    textMesh.color = healthColour;
                    break;
                default:
                    textMesh.color = Color.white;
                    break;
            }
        }
    }
}