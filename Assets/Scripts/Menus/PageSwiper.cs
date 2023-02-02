using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CaptainHindsight
{
    public class PageSwiper : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        private Vector3 panelLocation;
        public float PercentThreshold = 0.2f;
        public float Easing = 0.5f;
        public int TotalPages = 1;
        public int CurrentPage = 1;

        private void Start() => panelLocation = transform.position;

        public void OnDrag(PointerEventData data)
        {
            float difference = data.pressPosition.x - data.position.x;
            transform.position = panelLocation - new Vector3(difference, 0, 0);
        }
        public void OnEndDrag(PointerEventData data)
        {
            float percentage = (data.pressPosition.x - data.position.x) / Screen.width;
            if (Mathf.Abs(percentage) >= PercentThreshold)
            {
                Vector3 newLocation = panelLocation;
                if (percentage > 0 && CurrentPage < TotalPages)
                {
                    CurrentPage++;
                    newLocation += new Vector3(-Screen.width, 0, 0);
                }
                else if (percentage < 0 && CurrentPage > 1)
                {
                    CurrentPage--;
                    newLocation += new Vector3(Screen.width, 0, 0);
                }
                StartCoroutine(SmoothMove(transform.position, newLocation, Easing));
                panelLocation = newLocation;
                EventManager.Instance.CheckStatusOfLevelSelectPanels();
            }
            else
            {
                StartCoroutine(SmoothMove(transform.position, panelLocation, Easing));
            }
        }

        IEnumerator SmoothMove(Vector3 startpos, Vector3 endpos, float seconds)
        {
            float t = 0f;
            while (t <= 1.0)
            {
                t += Time.deltaTime / seconds;
                transform.position = Vector3.Lerp(startpos, endpos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
        }

        public bool NextPage()
        {
            if (CurrentPage >= TotalPages) return false;

            CurrentPage++;
            Vector3 newLocation = panelLocation;
            newLocation += new Vector3(-Screen.width, 0, 0);
            StartCoroutine(SmoothMove(transform.position, newLocation, Easing));
            panelLocation = newLocation;
            return true;
        }

        public bool PreviousPage()
        {
            if (CurrentPage <= 1) return false;

            CurrentPage--;
            Vector3 newLocation = panelLocation;
            newLocation += new Vector3(Screen.width, 0, 0);
            StartCoroutine(SmoothMove(transform.position, newLocation, Easing));
            panelLocation = newLocation;
            return true;
        }

        public void StatusOfPanels(out bool panelLeft, out bool panelRight)
        {
            panelLeft = true;
            panelRight = true;

            if (CurrentPage <= 1)
            {
                panelLeft = false;
            }

            if (CurrentPage >= TotalPages)
            {
                panelRight = false;
            }
        }
    }
}