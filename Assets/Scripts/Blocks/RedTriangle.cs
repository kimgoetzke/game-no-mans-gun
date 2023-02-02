using UnityEngine;
using System.Threading.Tasks;

namespace CaptainHindsight
{
    [RequireComponent(typeof(MKnockback))]
    public class RedTriangle : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private int damage;

        [Header("Component references")]
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private bool explosionTriggered;
        private CircleCollider2D explosionCollider;
        private Rigidbody2D rb;
        private Animator animator;

        private bool PlayerIsBelowTriangle()
        {
            //Helper.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - 2f), Color.green, 100f);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f, playerLayer);
            if (hit.collider != null) return true;
            return false;
        }

        private void Awake()
        {
            // Get all components & do initial setup
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            animator = GetComponent<Animator>();
            explosionCollider = GetComponentInChildren<CircleCollider2D>();
            explosionCollider.enabled = false;
        }

        private void Update()
        {
            if (PlayerIsBelowTriangle())
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        private void OnTriggerEnter2D(Collider2D trigger)
        {
            // Guard clause to stop explosion when trigger is blue blocks or an area effect
            if (trigger.CompareTag("AreaEffect")) return;

            // Count this as a score event
            if (trigger.CompareTag("Bullet")) EventManager.Instance.CountEvent(ScoreEventType.TrianglesRed, 1);

            if (explosionTriggered == false)
            {
                // Play dying animation
                animator.SetTrigger("isDying");

                //Helper.Log("Triangle collides with " + trigger.gameObject + ".", this);

                // Set this bool to true so that the explosion is only triggered once
                explosionTriggered = true;

                // Damage and kock back the player
                if (trigger.CompareTag("Player") && PlayerManagement.Instance.PlayerIsDead == false)
                {
                    PlayerManagement.Instance.TryToDamagePlayer(damage, gameObject.name.ToString(), true);
                }

                // Activate circle collidor in child object to damage neighbouring damagable items
                explosionCollider.enabled = true;

                // Play random explosion audio
                var impactSound = Random.Range(1, 6);
                AudioManager.Instance.Play("Impact" + impactSound);

                //Instantiate particles and then destroy object
                InstantiateParticelsAndDestroyObject();
            }
        }

        private async void InstantiateParticelsAndDestroyObject()
        {
            // Instantiate explosion particles
            await Task.Delay(System.TimeSpan.FromMilliseconds(200));
            Instantiate(particlePrefab, transform.position, Quaternion.identity);

            // Destroy game object with slight delay so that explosion can be triggered in other
            // objects & animation can be played
            await Task.Delay(System.TimeSpan.FromMilliseconds(300));
            Destroy(gameObject);
        }

        // Triggered by animation events at the end of each idle/shaking animation
        private void PlayNextShakingAnimation()
        {
            animator.SetInteger("shakeIndex", Random.Range(0, 3));
            animator.SetTrigger("isShaking");
        }
    }
}