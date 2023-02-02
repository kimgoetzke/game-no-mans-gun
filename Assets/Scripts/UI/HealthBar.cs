using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private GameObject fullHealthParticlePrefab;
        private bool particlesUnlocked;

        public void SetMaxHealth(int health)
        {
            slider.maxValue = health;
            slider.value = health;
        }

        public void SetHealth(int health)
        {
            slider.value = health;

            if (slider.value == slider.maxValue)
            {
                if (particlesUnlocked == false)
                {
                    particlesUnlocked = true;
                    return;
                }

                GameObject prefabInstance = Instantiate(fullHealthParticlePrefab, new Vector3(0, 0, 20), Quaternion.identity);
                Destroy(prefabInstance.gameObject, 2f);
            }
        }
    }
}