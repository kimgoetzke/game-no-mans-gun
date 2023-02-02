using UnityEngine;

namespace CaptainHindsight
{
    public class PlayerAnimatorOverrider : MonoBehaviour
    {
        private Animator animator;

        private void Awake() => animator = GetComponentInChildren<Animator>();

        public void UpdateSettings(AnimatorOverrideController controller, PlayerModel model)
        {
            GameObject modelObject = GameObject.Find(model.ToString());
            animator = modelObject.GetComponent<Animator>();
            Helper.Log("PlayerAnimatorOverrider: Found '" + model + "' and updated animator.");
            animator.runtimeAnimatorController = controller;
        }
    }
}
