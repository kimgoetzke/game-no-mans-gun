using UnityEngine;

namespace CaptainHindsight
{
    public class MemberOfNetwork : MonoBehaviour
    {
        [HideInInspector] private BreakableBlock breakableBlock;
        [SerializeField] private LayerMask networkLayers;

        private void Awake() => breakableBlock = GetComponentInParent<BreakableBlock>();

        private void Start()
        {
            if (CheckVicinityForNetworkMembers()) breakableBlock.MemberOfNetwork();
            else if (!CheckVicinityForNetworkMembers()) Helper.LogWarning(this + " is not a member of a network.");
        }

        private bool CheckVicinityForNetworkMembers()
        {
            //Helper.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - 0.1f), Color.green, 10f);
            //Helper.DrawLine(transform.position, new Vector3(transform.position.x + 0.1f, transform.position.y), Color.green, 10f);
            //Helper.DrawLine(transform.position, new Vector3(transform.position.x , transform.position.y + 0.1f), Color.green, 10f);
            RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, 0.1f, networkLayers);
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 0.1f, networkLayers);
            RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, networkLayers);
            if (hitUp.collider != null || hitRight.collider != null || hitDown.collider != null) return true;
            else return false;
        }
    }
}
