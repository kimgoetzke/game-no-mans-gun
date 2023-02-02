using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight
{
    public class SpecialBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Image fill;
        private Color specialActiveColour = new Color(0.6226f, 0.5831f, 0.1086f, 0.7f);
        private Color specialChargingColour = new Color(0.1098f, 0.4359f, 0.6235f, 0.7f);
        [SerializeField] private GameObject specialChargedParticlePrefab;
        private bool isAtMax;

        public void ResetSpecialBar(float currentCharge, float fullyCharged)
        {
            slider.value = currentCharge;
            slider.maxValue = fullyCharged;
            fill.color = specialChargingColour;
        }

        public void UpdateSpecialBar(float timer, bool specialActive)
        {
            slider.value = timer;

            if (slider.value < slider.maxValue && specialActive)
            {
                if (isAtMax == true) isAtMax = false;
                fill.color = specialActiveColour;
            }
            else fill.color = specialChargingColour;

            if (slider.value == slider.maxValue && isAtMax == false)
            {
                isAtMax = true;
                GameObject prefabInstance = Instantiate(specialChargedParticlePrefab, new Vector3(0, 0, 10), Quaternion.identity);
                Destroy(prefabInstance.gameObject, 1f);
            }
        }
    }
}