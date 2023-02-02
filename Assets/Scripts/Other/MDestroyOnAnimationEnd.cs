using UnityEngine;

namespace CaptainHindsight
{
    public class MDestroyOnAnimationEnd : MonoBehaviour
    {
        public void DestroyParentAfterAnimation()
        {
            GameObject parent = gameObject.transform.parent.gameObject;
            Destroy(parent);
        }

        public void DestroyAfterAnimation()
        {
            Destroy(gameObject);
        }
    }
}