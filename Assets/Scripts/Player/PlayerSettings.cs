using UnityEngine;

namespace CaptainHindsight
{
    [CreateAssetMenu(fileName = "PlayerSettings-", menuName = "Scriptable Object/New Player Settings", order = 4)]
    public class PlayerSettings : ScriptableObject
    {
        public PlayerModel Model;
        public PlayerState State;
        public float Duration = Mathf.Infinity;

        [Header("PlayerManagement")]
        [Range(100, 999)] public int BaseHealth = 100;

        [Header("PlayerController - General")]
        public float AttackCooldown = 0.1f;
        public float SlowDownFactor = 0.1f;
        public float KnockbackModifier = 1f;
        public int NumberOfBullets = 1;
        public float BulletsSpread = 0f;
    
        [Header("PlayerController - Desktop")]
        public float D_durationOfSpecial = 1.5f;
        public int D_rechargeThrottle = 3;
        public float D_gravityScale = 0.2f;
        public float D_forceAmount = 400f;
        public float D_maxUpAssist = 50f;

        [Header("PlayerController - Mobile")]
        public float M_durationOfSpecial = 3f;
        public int M_rechargeThrottle = 2;
        public float M_gravityScale = 0.1f;
        public float M_forceAmount = 200f;
        public float M_maxUpAssist = 12.5f;

        [Header("PlayerController - Model")]
        public GameObject Prefab;
        public Vector3 BulletSpawnPoint_position;
        public Quaternion BulletSpawnPoint_rotation;
        public Vector2 FirePoint;
        public Vector2 BackOfGun;
        public Vector2 HandleOfGun;
        public Vector2[] PolygonColliderPoints;

        [Header("PlayerAnimatorOverrider")]
        public AnimatorOverrideController AnimatorController;
    }
}
