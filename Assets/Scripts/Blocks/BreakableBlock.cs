using UnityEngine;

namespace CaptainHindsight
{
    public class BreakableBlock : MonoBehaviour, INetworkMember
    {
        [Header("Components")]
        [SerializeField] private GameObject breakingBlockParticlePrefab;
        [SerializeField] private CircleCollider2D explosionCollider;
        private ObjectPoolManager objectPoolManager;
        private Animator animator;

        [Header("Explosion settings")]
        [SerializeField] private float explodeAfterSeconds;
        [SerializeField] private bool explosionInProgress;
        [SerializeField] private bool explosionTriggered;
        private float explosionDelay;

        [Header("Network settings")]
        [SerializeField] private bool isMemberOfNetwork; // Makes the status visible in the inspector
        [SerializeField] public bool IsMemberOfNetwork { get; private set; }

        // Audio
        private AudioSource audioSource; // Can be used to add spatial audio later

        private void Awake() => animator = GetComponent<Animator>();

        private void Start() => objectPoolManager = ObjectPoolManager.Instance;

        private void Update()
        {
            // Start timer when explosion is triggerd
            if (explosionTriggered == true && explosionDelay < explodeAfterSeconds) explosionDelay += Time.deltaTime;
            else if (explosionInProgress == false && explosionTriggered == true && explosionDelay >= explodeAfterSeconds) StartDestructionOfBlock();
            else if (explosionInProgress == true && explosionDelay >= explodeAfterSeconds) FinishDestructionOfBlock();
        }

        // Method to be called by child object with MemberOfNetwork script
        public void MemberOfNetwork()
        {
            IsMemberOfNetwork = true;
            isMemberOfNetwork = IsMemberOfNetwork;
        }

        #region OnTriggerEnter and destruction of block
        private void OnTriggerEnter2D(Collider2D trigger)
        {
            if (trigger.CompareTag("Bullet"))
            {
                //Helper.Log(trigger.gameObject + " hits breakable block.", this);
                StartDestructionOfBlock();
                FinishDestructionOfBlock();
            }
            else if (trigger.CompareTag("Explosion") && explosionInProgress == false)
            {
                // Initiate explosion counter
                explosionTriggered = true;

                // Play animation to indicate upcoming explosion
                animator.SetTrigger("isHurt");
            }
            return;
        }

        private void StartDestructionOfBlock()
        {
            // Activate circle collidor in child object to damage neighbouring damagable items
            explosionCollider.enabled = true;

            // Play random audio
            var impactSound = Random.Range(1, 6);
            AudioManager.Instance.Play("Impact" + impactSound);
            //AudioManager.Instance.RetrieveClipToPlayLocally("Impact1");

            // Take explosion particles from object pool
            objectPoolManager.SpawnFromPool("breakableBlockParticles", transform.position, Quaternion.identity);

            // Ensure this method is only played once
            explosionInProgress = true;
        }

        private void FinishDestructionOfBlock()
        {
            Destroy(gameObject, 0.1f);
        }
        #endregion
    }
}