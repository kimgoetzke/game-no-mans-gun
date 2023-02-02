using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight
{
    public class MAutoScrollBackground : MonoBehaviour
    {
        [SerializeField] private RawImage image;
        [SerializeField] private float xAxis, yAxis;

        private void Update()
        {
            image.uvRect = new Rect(image.uvRect.position + new Vector2(xAxis, yAxis) * Time.deltaTime, image.uvRect.size);
        }
    }
}