using UnityEngine;

namespace CaptainHindsight
{
    public class PowerUp : MonoBehaviour
    {
        [SerializeField] private ActionType type;
        [SerializeField] private float bonus;
        [SerializeField] private GameObject effectPopup;
        [SerializeField] private string text;
        [SerializeField] private AnimatorOverrideController controller;
        private Animator animator;
        private bool collected;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
        }

        private void OnTriggerEnter2D(Collider2D trigger)
        {
            if (trigger.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false && collected == false)
            {
                // Mark as collected (to prevent double triggering), initiate
                // event, play animation and sound
                collected = true;
                EventManager.Instance.GivePowerUp(type, bonus);
                animator.SetTrigger("isCollected");
                AudioManager.Instance.Play("PositiveEvent");

                // If action type != Player, then create a pop-up message
                if (type == ActionType.Player) return;
                GameObject popup = Instantiate(effectPopup, PlayerController.Instance.transform);
                popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Text, text);
            }
        }
    }
}
