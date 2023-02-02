using UnityEngine;
using DG.Tweening;

namespace CaptainHindsight
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MoveImage : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float xSpeed;
        [SerializeField] private float ySpeed;
        [SerializeField] private bool randomMovement;
        [SerializeField] private float timeUntilChange;
        private SpriteRenderer spriteRenderer;
        private Vector2 offset;
        private float timer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (randomMovement == false)
            {
                offset = new Vector2(Time.time * xSpeed, Time.time * ySpeed);
                spriteRenderer.material.mainTextureOffset = offset;
            }
            else if (randomMovement)
            {
                timer += Time.deltaTime;
                if (timer > timeUntilChange)
                {
                    timer = 0;
                    xSpeed = Random.Range(-0.2f, 0.3f);
                    ySpeed = Random.Range(-0.2f, 0.3f);
                    spriteRenderer.material.DOOffset(new Vector2(xSpeed, ySpeed), timeUntilChange - 1).SetEase(Ease.InOutSine);
                }
            }
        }

        private void OnDestroy() => spriteRenderer.DOKill();
    }
}