using UnityEngine;

namespace CaptainHindsight
{
    public class Blue : MonoBehaviour
    {
        private void OnDestroy()
        {
            EventManager.Instance.CountEvent(ScoreEventType.BreakableBlocksBlue, 1);
        }
    }
}
