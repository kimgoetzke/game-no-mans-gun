using UnityEngine;

namespace CaptainHindsight
{
    [RequireComponent(typeof(PlayerSettingsController))]
    [DisallowMultipleComponent]
    public class PlayerManagement : BPlayerAndGameStateDependent, IDamageable, IHealable
    {
        public static PlayerManagement Instance;

        [Header("Settings")]
        [Range(100, 999)][SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;
        [SerializeField] private Transform effectPopup;
        private bool touchingBlocks;
        public bool PlayerIsDead { get; private set; }
        private HealthBar healthBar;
        private Animator animator;

        [Header("Network healing")]
        private float healthCooldownTimer = Mathf.Infinity;
        private bool healingInProgress;
        private float healthTimer = 1f;
        private int healthBoost;

        [Header("Network damage")]
        private float damageCooldownTimer = Mathf.Infinity;
        private bool damageInProgress;
        private float damageTimer = 0.5f;
        private int damage;
        private string objectCausingDamage;
        private bool canBeAffected;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            healthBar = FindObjectOfType<HealthBar>();
            GetReferences();
        }

        private void GetReferences() => animator = GetComponentInChildren<Animator>();

        #region Start & Update
        private void Start()
        {
            PlayerIsDead = false;
            ResetHealth();
            healthBar.SetMaxHealth(maxHealth);
        }

        private void Update()
        {
            // Mark player as dead and skip remaining method if player is dead
            if (currentHealth <= 0)
            {
                PlayerIsDead = true;
                return;
            }

            // Logic for health boost triggered by members of a network (trigger 'cooldown' timer, then apply health boost)
            if (touchingBlocks && healingInProgress && healthCooldownTimer < healthTimer) healthCooldownTimer += Time.deltaTime;
            else if (touchingBlocks && healthCooldownTimer >= healthTimer && healingInProgress)
            {
                AddHealth(healthBoost);
                touchingBlocks = false;
            }

            // Logic for damage caused by members of a network (apply damage immediately, then trigger cooldown timer)
            if (touchingBlocks && damageInProgress && damageCooldownTimer == 0f) TakeDamage(damage, objectCausingDamage);
            else if (touchingBlocks == false && damageInProgress && damageCooldownTimer < damageTimer) damageCooldownTimer += Time.deltaTime;
            else if (damageInProgress && damageCooldownTimer >= damageTimer) damageInProgress = false;
        }
        #endregion

        #region Managing damage
        public void TryToDamagePlayer(int damageValue, string damageObject, bool isSourceMemberOfNetwork)
        {
            animator.SetTrigger("isHurt");

            if (isSourceMemberOfNetwork && damageInProgress == false)
            {
                damageInProgress = true;
                damage = damageValue;
                objectCausingDamage = damageObject;
                damageCooldownTimer = 0f;
                touchingBlocks = true;
                return;
            }
            else if (isSourceMemberOfNetwork == false) TakeDamage(damageValue, damageObject);
        }

        private void TakeDamage(int damage, string objectCausingDamage)
        {
            // Don't do anything if game state isn't Play
            if (canBeAffected == false) return;

            // Apply and manage damage/results of damage
            currentHealth -= damage;
            EventManager.Instance.CountEvent(ScoreEventType.DamageTaken, damage);
            healthBar.SetHealth(currentHealth);
            AudioManager.Instance.Play("Hurt");
            Helper.Log(objectCausingDamage + " damage(s) player. Damage: " + damage + " - remaining health: " + currentHealth + ".", this);
            //if (currentHealth <= 0) EventManager.Instance.PlayerDeath();
            if (currentHealth <= 0) GameStateManager.Instance.SwitchState(GameState.GameOver);

            // Instantiate effect popup showing amount of damage taken
            Transform damagePopup = Instantiate(effectPopup, transform);
            damagePopup.GetComponent<MEffectPopup>().Initialisation(ActionType.Damage, "-" + damage.ToString());

            // Resets contact status of player so that network can damage player again
            touchingBlocks = false;
        }
        #endregion

        #region Managing health boosts
        public void TryToHealPlayer(int boost, bool isSourceMemberOfNetwork)
        {
            if (isSourceMemberOfNetwork && healingInProgress == false)
            {
                healingInProgress = true;
                healthBoost = boost;
                healthCooldownTimer = 0f;
                touchingBlocks = true;
                return;
            }
            else if (isSourceMemberOfNetwork == false) AddHealth(boost);
        }

        private void AddHealth(int healthBoost)
        {
            // Allow the network to trigger a health boost again
            healingInProgress = false;

            // Don't do anything if game state isn't Play
            if (canBeAffected == false) return;

            // Add health
            var initialHealth = currentHealth;
            if (currentHealth < maxHealth) currentHealth += healthBoost;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            var actualBoost = currentHealth - initialHealth;
            if (actualBoost > 0)
            {
                Helper.Log("PlayerManagement: Player received health boost: " + actualBoost + " - new health: " + currentHealth + ".");
                healthBar.SetHealth(currentHealth);
                Transform healthPopup = Instantiate(effectPopup, transform);
                healthPopup.GetComponent<MEffectPopup>().Initialisation(ActionType.Health, "+" + actualBoost.ToString());
                AudioManager.Instance.Play("Heal");
            }
            //else if (actualBoost <= 0) Helper.Log("Player is at max health, no health boost received. Health: " + currentHealth + ".", this);
        }
        #endregion

        #region Managing health reset
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            healthBar.SetHealth(currentHealth);
        }
        #endregion

        #region Managing death
        private void Die()
        {
            Helper.Log("PlayerManagement: Player dies.", this);
            PlayerIsDead = true;
            AudioManager.Instance.Play("Die");
        }
        #endregion

        #region Managing events and game states
        protected override void ActionPlayerSettingChange(PlayerSettings settings, bool modelChange)
        {
            // Update currentHealth, maxHealth, and healthBar
            float healthModifier = (float)currentHealth / (float)maxHealth;
            maxHealth = settings.BaseHealth;
            healthBar.SetMaxHealth(maxHealth);
            currentHealth = (int)(maxHealth * healthModifier);
            healthBar.SetHealth(currentHealth);
            Helper.Log("PlayerController: Player settings (new max health: " + maxHealth + ") received and applied.");

            // Instantiate text pop-up if state changed to Charged
            if (modelChange == false && settings.State == PlayerState.Default)
            {
                Transform popup = Instantiate(effectPopup, transform);
                popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Text, "Charge over");
            }

            // Get references again if model as changed
            if (modelChange) GetReferences();
        }

        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            if (state == GameState.GameOver) Die();

            canBeAffected = settings.PlayerAffected;
        }
        #endregion
    }
}