using UnityEngine;

namespace CaptainHindsight
{
    public class MMoveBackground : MonoBehaviour
    {
        [HideInInspector] private Vector2 startVector;
        [HideInInspector] private GameObject myCamera;
        [Range(-1, 0)][SerializeField] private float parallexEffect;

        private void Start()
        {
            myCamera = GameObject.Find("Cinemachine");
            startVector = transform.position;
        }

        private void Update()
        {
            Vector2 temp = (myCamera.transform.position * (1 - parallexEffect));
            Vector2 distance = (myCamera.transform.position * parallexEffect);

            transform.position = new Vector3(startVector.x + distance.x, startVector.y + distance.y, transform.position.z);
        }
    }
}