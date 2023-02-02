using UnityEngine;
using UnityEngine.EventSystems;

namespace CaptainHindsight
{
    public class OnEnableHighlight : MonoBehaviour
    {
        // Use Unity Editor to select the object on which this script sits
        [SerializeField] private GameObject ButtonToHighlight;

        void OnEnable()
        {
            if (ButtonToHighlight) EventSystem.current.SetSelectedGameObject(ButtonToHighlight);
        }
    }
}
