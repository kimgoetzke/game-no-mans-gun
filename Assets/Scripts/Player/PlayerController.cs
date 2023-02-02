using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading.Tasks;

namespace CaptainHindsight
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PolygonCollider2D), typeof(PlayerSettingsController))]
    [RequireComponent(typeof(PlayerAnimatorOverrider))]
    [DisallowMultipleComponent]
    public class PlayerController : BPlayerAndGameStateDependent, IInteractable
    {
        public static PlayerController Instance;

        [Header("Attack ability")]
        private float attackCooldown; // Controlled by PlayerSettings
        public bool IsAttacking { get; private set; }
        private float attackCooldownTimer = Mathf.Infinity;

        [Header("Special ability")]
        private float durationOfSpecial; // Controlled by PlayerSettings
        private int rechargeThrottle; // Controlled by PlayerSettings
        private float slowDownFactor; // Controlled by PlayerSettings - the higher, the faster the slowdown
        private float specialCooldown;
        public bool IsUsingSpecial { get; private set; }
        private float incomingGravity;
        private float gravityOnStart;
        private SpecialBar specialBar;
        [SerializeField] private MShaderShockwave shaderShockwave;
        private float specialLastTriggerTimestamp; // Only used on mobile to allow context.canceled

        [Header("Gun components")]
        [SerializeField] private GameObject bulletSpawnPoint;
        [SerializeField] private GameObject firePoint;
        [SerializeField] private GameObject backOfGun;
        [SerializeField] private GameObject handleOfGun;
        [SerializeField] private ParticleSystem playerParticles;
        [SerializeField] private ParticleSystem deathParticles;
        private int numberOfBullets;
        private float bulletSpread;
        private bool particlesUnlocked;
        private PolygonCollider2D polygonCollider2D;
        private CapsuleCollider2D capsuleCollider2D; // Used when player dies

        [Header("Ground collision")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float groundContactTime;
        public bool onGround;
        private bool wasOnGround;
        private Vector3 lastVelocity;

        [Header("Movement - General")]
        private bool canMove = true;
        private bool canRotate = true;
        private float forceAmount; // Controlled by PlayerSettings, default: 400
        private float maxUpAssist; // Controlled by PlayerSettings, default: 50
        private float maxY = 10;
        private float knockbackModifier; // Controlled by PlayerSettings, default: 1 - reduced knockback if <1; increases knockback if >1
        private bool isFacingRight;

        [Header("Movement - Desktop build")]
        private Camera cam;
        private Vector2 mousePosition;

        [Header("Movement - Mobile build")]
        [SerializeField] private GameObject aimCircle;
        [SerializeField] private GameObject aimPoint;
        [SerializeField] private GameObject oppositePoint;
        private GameObject touchControllerCanvas;

        [Header("General")]
        private ObjectPoolManager objectPoolManager;
        private Rigidbody2D rb;
        private Animator animator;
        private float lastKnockbackTimestamp;
        private PlayerInputActions playerInputActions;
        private InputAction rotationInput;

        #region Awake & Start
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

            // Find model-related (swappable) references
            //GetModelReferences();
            animator = GetComponentInChildren<Animator>();
            capsuleCollider2D = GetComponentInChildren<CapsuleCollider2D>();
            capsuleCollider2D.enabled = false;

            // Find internal references
            rb = GetComponent<Rigidbody2D>();
            polygonCollider2D = GetComponent<PolygonCollider2D>();

            // Find external references
            // - Special bar
            specialBar = FindObjectOfType<SpecialBar>();

            // - MainCam
            if (GameObject.Find("MainCam") == null) Helper.LogWarning("Main camera not found. Consider changing PlayerController and assigning cam in the editor.", this);
            else cam = GameObject.Find("MainCam").GetComponent<Camera>();

            // - TouchController
            if (GameObject.Find("TouchController") == null) Helper.LogWarning("No touch controller found. Please add one to the scene.", this);
            else touchControllerCanvas = GameObject.Find("TouchController");
        }

        private void Start()
        {
            // Set objectPoolManager to the singleton instance (cannot be moved to Awake())
            objectPoolManager = ObjectPoolManager.Instance;

            // Reset special ability bar and start with empty charge
            specialBar.ResetSpecialBar(0, durationOfSpecial);

            // Switch on touch controller if mobile, switch off if desktop
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_WEBGL_API

            touchControllerCanvas.SetActive(false);

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

            touchControllerCanvas.SetActive(true);

#endif

            // Store gravity as configured in Inspector to allow resetting special when initiating attack
            gravityOnStart = rb.gravityScale;
        }
        #endregion

        #region Fixed Update & Update
        private void FixedUpdate()
        {
            // Make sure that both bools have the same value at the beginning of this frame
            // so that differences in values can be checked and used later in the frame
            wasOnGround = onGround;

            // Stop gun from rotating when too close to the ground or touching the ground
            RaycastHit2D hitFront = Physics2D.Raycast(firePoint.transform.position, Vector2.down, 0.15f, groundLayers);
            RaycastHit2D hitBack = Physics2D.Raycast(backOfGun.transform.position, Vector2.down, 0.1f, groundLayers);
            RaycastHit2D hitHandle = Physics2D.Raycast(handleOfGun.transform.position, Vector2.down, 0.1f, groundLayers);
            if (hitFront.collider != null || hitBack.collider != null || hitHandle.collider != null)
            {
                onGround = true;
                canRotate = false;
            }
            else
            {
                canRotate = true;
                onGround = false;
            }

            if (canMove && canRotate)
            {
                // Rotate the gun
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_WEBGL_API

                // Mouse control: Rotate gun towards mouse position
                Vector2 targetLookDirection = mousePosition - rb.position;

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

                // Mobile/touch controls: Rotate the gun towards the aim point of the aim circle canvas
                Vector2 targetLookDirection = aimPoint.transform.position - oppositePoint.transform.position;

#endif

                float angle = Mathf.Atan2(targetLookDirection.y, targetLookDirection.x) * Mathf.Rad2Deg - 180f;
                Quaternion targetQuaternion = Quaternion.Euler(new Vector3(0, 0, angle));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetQuaternion, 100f);

                // Flip sprite depending on the direction the gun is facing (to prevent gun from being upside down)?
                Vector2 currentLookDirection = firePoint.transform.position - backOfGun.transform.position;
                float currentAngle = Mathf.Atan2(currentLookDirection.y, currentLookDirection.x) * Mathf.Rad2Deg - 180f;

                // There's a few degrees left in between each slice to prevent the gun from flipping wildly back and forth
                if (currentAngle > -88 && isFacingRight == true) FlipGunOnYAxis();
                if (currentAngle < -92 && currentAngle > -178 && isFacingRight == false) FlipGunOnYAxis();
                if (currentAngle < -182 && currentAngle > -268 && isFacingRight == false) FlipGunOnYAxis();
                if (currentAngle < -272 && currentAngle > -358 && isFacingRight == true) FlipGunOnYAxis();
            }

            // Bring player to a halt when using special ability 
            if (IsUsingSpecial & rb.velocity.magnitude > 0f)
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.ClampMagnitude(rb.velocity, 0f), slowDownFactor);

            // How long has the gun been touching the floor?
            if (onGround) groundContactTime += Time.deltaTime;

            // Detect a change in ground contract vs non-contact
            if (wasOnGround == false && onGround)
            {
                // Play thud sound
                AudioManager.Instance.Play("Thud");
            }
            else if (wasOnGround && onGround == false)
            {
                // Send off touching wall event to score manager
                if (groundContactTime < 1) return;
                EventManager.Instance.CountEvent(ScoreEventType.TouchingWall, (int)Math.Floor(groundContactTime));
                groundContactTime = 0;
            }
        }

        private void Update()
        {

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_WEBGL_API

            // Get mouse position for aiming
            mousePosition = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

            // Stick control for gun: Rotate aim circle canvas based on Move input incl. WASD and (touch) joystick
            Vector2 input = rotationInput.ReadValue<Vector2>();
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg - 180f;
            //Helper.Log(input + " - " + angleJoystick);
            if (angle != -180f) aimCircle.transform.rotation = Quaternion.Euler(aimCircle.transform.rotation.x,
                aimCircle.transform.rotation.y, angle); // If statement is temp fix; check if I can change how I read the playerInput value (returning last value until there's a change)
            
#endif

            // Manage attack cooldown timer
            if (attackCooldownTimer < attackCooldown) attackCooldownTimer += Time.deltaTime;
            else if (attackCooldownTimer >= attackCooldown) IsAttacking = false;

            // Manage special cooldown timer
            if (IsUsingSpecial && specialCooldown <= 0) StopSpecial();
            else if (IsUsingSpecial)
            {
                specialCooldown -= Time.deltaTime;
                specialBar.UpdateSpecialBar(specialCooldown, IsUsingSpecial);
            }
            else if (IsUsingSpecial == false && specialCooldown < durationOfSpecial)
            {
                specialCooldown += Time.deltaTime / rechargeThrottle;
                specialBar.UpdateSpecialBar(specialCooldown, IsUsingSpecial);
            }

            lastVelocity = rb.velocity;
        }
        #endregion

        #region Flip gun on Y axis
        private void FlipGunOnYAxis()
        {
            isFacingRight = !isFacingRight;
            Vector3 flippedLocalScale = transform.localScale;
            flippedLocalScale.y *= -1;
            transform.localScale = flippedLocalScale;
        }
        #endregion

        #region Pause
        private async void Pause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                await Task.Yield();
                EventManager.Instance.RequestPauseMenu();
            }
        }
        #endregion

        #region Attack
        public void Attack(InputAction.CallbackContext context)
        {
            if (attackCooldownTimer >= attackCooldown && canMove)
            {
                // Restart timer, set attack state and trigger animation
                attackCooldownTimer = 0;
                IsAttacking = true;
                animator.SetTrigger("attack");

                // Move gun through recoil
                if (IsUsingSpecial == false)
                {
                    var assistPoint = Mathf.InverseLerp(0, maxY, rb.position.y);
                    var assistAmount = Mathf.Lerp(maxUpAssist, 0, assistPoint);
                    var forceDirection = (backOfGun.transform.position - bulletSpawnPoint.transform.position).normalized;
                    rb.AddForce(forceDirection * forceAmount + Vector3.up * assistAmount);
                }

                // Instantiate bullets and point in the right direction
                for (int i = 0; i < numberOfBullets; i++)
                {
                    if (numberOfBullets == 1)
                    {
                        Vector3 shootingDirection = (bulletSpawnPoint.transform.position - backOfGun.transform.position).normalized;
                        GameObject bulletObject = objectPoolManager.SpawnFromPool("bullet", bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
                        bulletObject.GetComponent<Bullet>().InitialiseBullet(shootingDirection);
                    }
                    else
                    {
                        float temp = ((bulletSpread * 2) / (numberOfBullets - 1)) * i;
                        Vector3 shootingDirection = (bulletSpawnPoint.transform.position + bulletSpawnPoint.transform.TransformDirection(new Vector3(0, bulletSpread - temp, 0)) - backOfGun.transform.position).normalized;
                        GameObject bulletObject = objectPoolManager.SpawnFromPool("pellet", bulletSpawnPoint.transform.position + bulletSpawnPoint.transform.TransformDirection(new Vector3(0, bulletSpread - temp, 0)), bulletSpawnPoint.transform.rotation);
                        bulletObject.GetComponent<Bullet>().InitialisePellet(shootingDirection);
                    }
                }

                // Instantiate muzzle flash, randomise size, and point in right direction
                Vector3 direction = (backOfGun.transform.position - bulletSpawnPoint.transform.position).normalized;
                GameObject muzzleFlashObject = objectPoolManager.SpawnFromPool("muzzleFlash", firePoint.transform.position, firePoint.transform.rotation);
                muzzleFlashObject.transform.parent = firePoint.transform;
                muzzleFlashObject.transform.localScale = transform.localScale;
                muzzleFlashObject.transform.eulerAngles = new Vector3(0, 0, Helper.GetAngelFromVectorFloat(direction));
                DeactivateMuzzleFlash(muzzleFlashObject);

                // Shake camera
                MCameraShake.Instance.ShakeCamera(1.5f, 0.25f);

                // Play random audio
                var gunshot = UnityEngine.Random.Range(1, 4);
                AudioManager.Instance.Play("9mm_shot" + gunshot);
            }
        }

        private async void DeactivateMuzzleFlash(GameObject muzzleFlash)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(0.05f));
            muzzleFlash.SetActive(false);
        }
        #endregion

        #region Special
        public void Special(InputAction.CallbackContext context)
        {

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_WEBGL_API

            if (context.started) StartSpecial();
            if (context.canceled) StopSpecial();

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

            if (context.started && IsUsingSpecial) StopSpecial();
            else if (context.started) StartSpecial();
            else if (IsUsingSpecial && Time.time - specialLastTriggerTimestamp >= 1f) StopSpecial();

#endif

        }

        public void StartSpecial()
        {
            // Don't allow triggering special if canMove is true or no charge is available
            if (specialCooldown <= 0 || canMove == false) return;

            // Store incoming parameters & disable gravity
            incomingGravity = rb.gravityScale;
            rb.gravityScale = 0f;

            // Create shockwave effect
            shaderShockwave.InitiateShockwaveEffect(0.3f);

            // Play sound effect
            AudioManager.Instance.Play("Special_On");

            // Start IsUsingSpecial (used in update)
            IsUsingSpecial = true;

            // Record time stamp (currently only used to stop special on mobile)
            specialLastTriggerTimestamp = Time.time;
        }

        private void StopSpecial()
        {
            // Only run code when special is still being used
            if (IsUsingSpecial == false) return;
            IsUsingSpecial = false;

            // Play sound effect
            AudioManager.Instance.Play("Special_Off");

            // Play the reverse shockwave to signal the end of the special
            shaderShockwave.EndShockwaveEffect(0.3f);

            // Reset gravity to previous gravity value
            rb.gravityScale = incomingGravity;
        }
        #endregion

        #region Effect: Change gravity
        public void ChangeGravity(float newGravity)
        {
            incomingGravity = rb.gravityScale;
            rb.gravityScale = rb.gravityScale * newGravity;
            Helper.Log("PlayerController: Gravity changed. Old value: " + incomingGravity + " - new value: " + rb.gravityScale +  ".");
        }

        public void ResetGravity()
        {
            Helper.Log("PlayerController: Gravity reset. Old value: " + rb.gravityScale + " - new value: " + gravityOnStart + ".");
            rb.gravityScale = gravityOnStart;
        }
    #endregion

        #region Effect: Clamp speed
        public void ClampSpeed(float maxSpeed)
        {
            if (rb.velocity.magnitude > maxSpeed) rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    #endregion

        #region Effect: One way movement
        public void OneWayMovement(bool leftForbidden, bool rightForbidden)
        {
            // Cancel any movement to the left and add small force in the opposite direction
            if (leftForbidden && rb.velocity.x < 0)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                rb.AddForce(new Vector2(7f, 0f));
            }

            // Cancel any movement to the right and add small force in the opposite direction
            if (rightForbidden && rb.velocity.x > 0)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                rb.AddForce(new Vector2(-7f, 0f));
            }
        }
    #endregion

        #region Managing death
        // Used by GameStateManager to disable player on death
        private void Die()
        {
            // Increase gravity because it looks better
            rb.gravityScale = rb.gravityScale * 3;

            // Trigger dying animation and death particles
            animator.SetTrigger("isDying");
            Instantiate(deathParticles, transform.position, transform.rotation.normalized);

            // Disable outline collider and enable collider for death shape
            polygonCollider2D.enabled = false;
            capsuleCollider2D.enabled = true;

            // Shake camera
            MCameraShake.Instance.ShakeCamera(2f, 0.75f);
        }
        #endregion

        #region Applying a knockback
        private void OnCollisionEnter2D(Collision2D collision)
        {
            MKnockback knockback = collision.gameObject.GetComponent<MKnockback>();
            if (knockback != null)
            {
                var speed = lastVelocity.magnitude;
                var direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
                rb.velocity = direction * speed;
                rb.AddForce(direction * knockback.KnockbackStrength);

                Helper.Log("PlayerController: Knockback triggered by " + collision.transform.name + ".");
            }
        }
        #endregion

        #region Triggering particles 
        // Win particles triggered when game changes into Win state
        public async void TriggerParticles(int value)
        {
            for (int i = 0; i < value; i++)
            {
                Instantiate(playerParticles, transform.position, transform.rotation.normalized);
                AudioManager.Instance.Play("Positive");
                await Task.Delay(System.TimeSpan.FromSeconds(1));
            }
        }
        #endregion

        #region Swapping player model
        private void SwapModel(PlayerSettings settings)
        {
            PlayerModel currentModel = GetComponent<PlayerSettingsController>().CurrentSettings().Model;
            GameObject currentModelObject = GameObject.Find(currentModel.ToString());
            if (currentModel == settings.Model && currentModelObject == isActiveAndEnabled)
            {
                return;
            }
            else if (currentModel != settings.Model && currentModelObject != isActiveAndEnabled)
            {
                // Deactivate current model
                currentModelObject.SetActive(false);

                // Re-activate existing model
                GameObject newModel = GameObject.Find(settings.Model.ToString());
                newModel.SetActive(true);
                GetModelReferences(newModel);
            }
            else
            {
                // Deactivate current model
                currentModelObject.SetActive(false);

                // Instantiate and activate new model
                GameObject newModel = Instantiate(settings.Prefab, transform.position, transform.rotation);
                newModel.transform.parent = gameObject.transform;
                newModel.name = settings.Model.ToString();
                GetModelReferences(newModel);
            }
        }

        private void GetModelReferences(GameObject model)
        {
            animator = model.GetComponentInChildren<Animator>();
            capsuleCollider2D = model.GetComponentInChildren<CapsuleCollider2D>();
            capsuleCollider2D.enabled = false;
        }

        private void ReadPolygonColliderPointsFromSettings(PlayerSettings settings)
        {
            polygonCollider2D.points = settings.PolygonColliderPoints;
            Helper.Log("PlayerController: Main polygon collider has been updated.");
        }
        #endregion

        #region Debug/editor mode keys
        public void DebugS(InputAction.CallbackContext context)
        {

#if UNITY_EDITOR

            Helper.Log("PlayerController: Editor/debug mode key 'S' pressed (won't work in build). Currently no function assigned.");

#endif

        }

        public void DebugL(InputAction.CallbackContext context)
        {

#if UNITY_EDITOR

            Helper.Log("PlayerController: Editor/debug key 'L' pressed (won't work in build). Currently no function assigned.");

#endif

        }
        #endregion

        #region Managing events and game states
        protected override void ActionPlayerSettingChange(PlayerSettings settings, bool modelChange)
        {
            // Update general states that don't change depending on platform
            attackCooldown = settings.AttackCooldown;
            slowDownFactor = settings.SlowDownFactor;
            knockbackModifier = settings.KnockbackModifier;
            numberOfBullets = settings.NumberOfBullets;
            bulletSpread = settings.BulletsSpread;

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_WEBGL_API

            // Update desktop specific settings
            durationOfSpecial = settings.D_durationOfSpecial;
            rechargeThrottle = settings.D_rechargeThrottle;
            rb.gravityScale = settings.D_gravityScale;
            forceAmount = settings.D_forceAmount;
            maxUpAssist = settings.D_maxUpAssist;

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

            // Update mobile specific settings
            durationOfSpecial = settings.M_durationOfSpecial;
            rechargeThrottle = settings.M_rechargeThrottle;
            rb.gravityScale = settings.M_gravityScale;
            forceAmount = settings.M_forceAmount;
            maxUpAssist = settings.M_maxUpAssist;

#endif

            // Instantiate particles but not the first change in model/state
            if (particlesUnlocked == true) TriggerParticles(1);
            particlesUnlocked = true;

            // Update special bar
            specialBar.ResetSpecialBar(durationOfSpecial, durationOfSpecial);

            // Change (prefab) model, colliders, and other attached game objects
            if (modelChange)
            {
                // Check if model is currently flipped and if so, 'unflip' it before
                // changing the model - otherwise collider points will be upside down
                bool isFlipped = false;
                if (isFacingRight)
                {
                    FlipGunOnYAxis();
                    isFlipped = true;
                }

                // Swap model component and polycon collider model
                SwapModel(settings);
                ReadPolygonColliderPointsFromSettings(settings);

                // Reverse any 'unflipping', if required
                if (isFlipped) FlipGunOnYAxis();

                // Update all other game objects attached to the player
                bulletSpawnPoint.transform.localPosition = settings.BulletSpawnPoint_position;
                bulletSpawnPoint.transform.localRotation = settings.BulletSpawnPoint_rotation;
                firePoint.transform.localPosition = settings.FirePoint;
                backOfGun.transform.localPosition = settings.BackOfGun;
                handleOfGun.transform.localPosition = settings.HandleOfGun;
                Helper.Log("PlayerController: Player model was changed to '" + settings.Model + "'.");
            }

            // Update animator controller
            GetComponent<PlayerAnimatorOverrider>().UpdateSettings(settings.AnimatorController, settings.Model);
        }

        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            if (state == GameState.GameOver) Die();
            else if (state == GameState.Win) TriggerParticles(4);

            canMove = settings.PlayerCanMove;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Use PlayerInputActions.cs and subscribe to relevant events
            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Enable();
            playerInputActions.Player.Fire.performed += Attack;
            playerInputActions.Player.Special.started += Special;
            playerInputActions.Player.Special.canceled += Special;
            playerInputActions.Player.Pause.performed += Pause;
            playerInputActions.Player.DebugLoad.performed += DebugL;
            playerInputActions.Player.DebugSave.performed += DebugS;
            rotationInput = playerInputActions.Player.Move;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from all player input events
            playerInputActions.Player.Fire.performed -= Attack;
            playerInputActions.Player.Special.started -= Special;
            playerInputActions.Player.Special.canceled -= Special;
            playerInputActions.Player.Pause.performed -= Pause;
            playerInputActions.Player.DebugLoad.performed -= DebugL;
            playerInputActions.Player.DebugSave.performed -= DebugS;
            rotationInput.Disable();
            playerInputActions.Player.Disable();
        }
        #endregion
    }
}