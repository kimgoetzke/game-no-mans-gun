using UnityEngine;

namespace CaptainHindsight
{
    public class YellowButton : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string eventName;
        [SerializeField] private YellowButtonType buttonType;

        [Header("References")]
        private bool buttonPressed;
        private float lastTriggerTimestamp;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (buttonType == YellowButtonType.SingleState) animator.SetBool("release", true);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.CompareTag("Bullet"))
            {
                // Ensure that button can only be pressed once a second
                if (Time.time - lastTriggerTimestamp <= 1f) return;
                lastTriggerTimestamp = Time.time;

                // Reset button if single state button
                if (buttonType == YellowButtonType.SingleState && buttonPressed) buttonPressed = false;

                // Gatekeeper to make sure only single use buttons that are pressed cannot be triggered again
                if (buttonType == YellowButtonType.SingleUse && buttonPressed) return;

                // Main logic
                if (buttonPressed == false && buttonType != YellowButtonType.SingleUse)
                {
                    animator.SetTrigger("pressButton");
                    buttonPressed = true;
                    AudioManager.Instance.Play("Button");
                    EventManager.Instance.TriggerYellowEvent(eventName, buttonPressed);
                }
                else if (buttonPressed == false && buttonType == YellowButtonType.SingleUse)
                {
                    animator.SetTrigger("pressButtonOneWay");
                    buttonPressed = true;
                    AudioManager.Instance.Play("Button");
                    EventManager.Instance.TriggerYellowEvent(eventName, buttonPressed);
                }
                else if (buttonPressed && buttonType != YellowButtonType.SingleState)
                {
                    animator.SetTrigger("unpressButton");
                    buttonPressed = false;
                    AudioManager.Instance.Play("Button");
                    EventManager.Instance.TriggerYellowEvent(eventName, buttonPressed);
                }
            }
        }
    }
}