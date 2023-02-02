using UnityEngine;

namespace CaptainHindsight
{
    public class Green : MonoBehaviour
    {
        [SerializeField] private int healthBoost = 10;
        [SerializeField] private float cooldownTimer = 0;
        [SerializeField] private bool touchingBlocks;
        //[HideInInspector] private BreakableBlock breakableBlock;
        [SerializeField] private bool isPartOfBreakableNetwork;

        private void Start()
        {
            if (TryGetComponent(out BreakableBlock breakableBlock))
            {
                breakableBlock = GetComponent<BreakableBlock>();
                if (breakableBlock.IsMemberOfNetwork == true) isPartOfBreakableNetwork = true;
            }
            /* else
             {
                 Helper.Log(gameObject + " is not attached to breakable blocks.", this);
             }*/
        }

        private void Update()
        {
            // Start timer when player is touching blocks
            if (touchingBlocks) cooldownTimer += Time.deltaTime;
            
            // Reset touching blocks status if player dies
            if (touchingBlocks && PlayerManagement.Instance.PlayerIsDead == true) touchingBlocks = false;

            // Heal player while touching blocks
            if (touchingBlocks && cooldownTimer > 1f)
            {
                PlayerManagement.Instance.TryToHealPlayer(healthBoost, isPartOfBreakableNetwork);
                cooldownTimer = 0f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false)
            {
                cooldownTimer = 0f;
                touchingBlocks = true;
            }
        }

        private void OnTriggerExit2D(Collider2D trigger)
        {
            if (trigger.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false)
            {
                touchingBlocks = false;
            }
        }

        private void OnDestroy()
        {
            EventManager.Instance.CountEvent(ScoreEventType.BreakableBlocksGreen, 1);
        }
    }
}
