using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace CaptainHindsight
{
    public class MChangeImageOnPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image onScreenImage;
        [SerializeField] private Sprite buttonPressed;
        [SerializeField] private Sprite buttonUnpressed;
        [SerializeField] private bool changeText;
        [SerializeField] private TextMeshProUGUI text;

        public void OnPointerDown(PointerEventData eventData)
        {
            onScreenImage.sprite = buttonPressed;
            if (changeText) text.alpha = 1f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onScreenImage.sprite = buttonUnpressed;
            if (changeText) text.alpha = 0.3f;
        }
    }
}