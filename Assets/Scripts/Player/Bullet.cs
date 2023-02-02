using UnityEngine;

namespace CaptainHindsight
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float bulletSpeed = 20f;
        private Rigidbody2D rb;

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        public void InitialiseBullet(Vector3 shootingDirection)
        {
            transform.eulerAngles = new Vector3(0, 0, Helper.GetAngelFromVectorFloat(shootingDirection));
            rb.AddForce(shootingDirection * bulletSpeed);
        }

        public void InitialisePellet(Vector3 shootingDirection) => rb.AddForce(shootingDirection * bulletSpeed);

        private void OnCollisionEnter2D(Collision2D trigger)
        {
            if (trigger.transform.CompareTag("Player") || trigger.transform.CompareTag("Bullet"))
            {
                Helper.Log("Bullet: Collision protection active. Collision with " + trigger.transform.name + " ignored.");
                return;
            }

            ObjectPoolManager.Instance.SpawnFromPool("hitParticles", transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
}