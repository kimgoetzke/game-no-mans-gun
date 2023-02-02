using UnityEngine;

namespace CaptainHindsight
{
    [RequireComponent(typeof(MKnockback))]
    public class Yellow : MonoBehaviour
    {
        [Header("Damage settings")]
        [SerializeField] private int damage;
        [SerializeField] private bool touchingBlocks;
        [SerializeField] private float damageTimer = 0f;
        [SerializeField] private float cooldownTimer = 1f;

        [Header("Network settings")]
        [SerializeField] private bool isPartOfBreakableNetwork;
        private BreakableBlock breakableBlock;

        private void Start()
        {
            if (TryGetComponent(out BreakableBlock breakableBlock))
            {
                breakableBlock = GetComponent<BreakableBlock>();
                if (breakableBlock.IsMemberOfNetwork == true) isPartOfBreakableNetwork = true;
            }
            else
            {
                Helper.Log(gameObject.name + " is not attached to breakable blocks.", this);
            }
        }

        private void Update()
        {
            // Start timer when player is touching blocks
            if (touchingBlocks) damageTimer -= Time.deltaTime;

            // Reset touching blocks status if player dies
            if (touchingBlocks && PlayerManagement.Instance.PlayerIsDead == true) touchingBlocks = false;

            // Damage player while touching blocks
            if (touchingBlocks & damageTimer <= 0f)
            {
                // Reset damage timer
                damageTimer = 1f;

                // Damage player
                PlayerManagement.Instance.TryToDamagePlayer(damage, gameObject.name.ToString(), isPartOfBreakableNetwork);

                // Play audio
                AudioManager.Instance.Play("ZapYellow");
            }

            // Start cooldown timer when no longer touching blocks
            if (touchingBlocks == false && damageTimer > 0f) cooldownTimer -= Time.deltaTime;

            // Reset both timers once cooldown is over
            if (cooldownTimer <= 0f)
            {
                damageTimer = 0f;
                cooldownTimer = 1f;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.transform.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false)
            {
                touchingBlocks = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.transform.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false)
            {
                touchingBlocks = false;
            }
        }

        private void OnDestroy()
        {
            EventManager.Instance.CountEvent(ScoreEventType.BreakableBlocksYellow, 1);
        }
    }
}
