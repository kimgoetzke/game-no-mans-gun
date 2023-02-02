using UnityEngine;
using System;
using System.Collections;

namespace CaptainHindsight
{
    public class YellowMoveEvent : YellowEvent 
    {
        [Header("Move Event Configuration")]
        [SerializeField] private float speed;
        [SerializeField] private float delayWhenReachingPoint;
        [SerializeField] [Range(1, 25)] private Transform[] points;
        private readonly int firstDestination = 1;
        private readonly int startPoint = 0;
        private int destination;
        private float timer;
        private int countdown = 4;
        private WaitForSeconds wait;

        [Header("Move Event References")]
        [SerializeField] private Transform eventLayer;

        private void Start()
        {
            // Set position of platform to startingPoint
            SetFirstDestination();

            // Set eventState to idle unless it's always moving
            if (EventType != YellowEventType.Move_Always) EventState = YellowEventState.Inactive;
            else
            {
                EventState = YellowEventState.Move;
                EventActive = true;
            }

            // Get specific timer required here ready to minimise impact on memory
            wait = new WaitForSeconds(delayWhenReachingPoint);
        }

        private void Update()
        {
            if (EventType == YellowEventType.Move_Always)
            {
                switch (EventState)
                {
                    case YellowEventState.Move:
                        DoMove();
                        if (DestinationReached())
                        {
                            StartCoroutine(WaitBeforeContinuing());
                            EventState = YellowEventState.Pause;
                        }
                        break;
                    case YellowEventState.Pause:
                        break;
                    default:
                        Helper.LogWarning("YellowMoveEvent: An unknown state was triggered: " + EventState);
                        GameStateManager.Instance.SwitchState(GameState.Error);
                        break;
                }
            }
            else if (EventType == YellowEventType.Move_WaitAndReturn)
            {
                switch (EventState)
                {
                    case YellowEventState.Inactive:
                        if (EventActive == true) EventState = YellowEventState.Move;
                        break;
                    case YellowEventState.Move:
                        DoMove();
                        if (DestinationReached())
                        {
                            if (destination == firstDestination)
                            {
                                StartCoroutine(WaitBeforeReturning());
                                EventState = YellowEventState.Pause;
                            }
                            else if (destination == startPoint)
                            {
                                EventActive = false;
                                SetFirstDestination();
                                EventState = YellowEventState.Inactive;
                                Helper.Log("YellowMoveEvent: Move_WaitAndReturn event returned back to the start. Event now inactive. It can be triggered again.");
                            }
                        }
                        break;
                    case YellowEventState.Pause:
                        DoCountDown();
                        break;
                    default:
                        Helper.LogWarning("YellowMoveEvent: An unknown state was triggered: " + EventState);
                        GameStateManager.Instance.SwitchState(GameState.Error);
                        break;
                }
            }
            else if (EventType == YellowEventType.Move_BackAndForth)
            {
                switch (EventState)
                {
                    case YellowEventState.Inactive:
                        if (EventActive == true) EventState = YellowEventState.Move;
                        break;
                    case YellowEventState.Move:
                        DoMove();
                        if (DestinationReached())
                        {
                            if (destination == firstDestination)
                            {
                                EventActive = false;
                                destination = startPoint;
                                EventState = YellowEventState.Inactive;
                            }
                            else if (destination == startPoint)
                            {
                                EventActive = false;
                                SetFirstDestination();
                                EventState = YellowEventState.Inactive;
                            }
                        }
                        break;
                    default:
                        Helper.LogWarning("YellowMoveEvent: An unknown state was triggered: " + EventState);
                        GameStateManager.Instance.SwitchState(GameState.Error);
                        break;
                }
            }
            else if (EventType == YellowEventType.Move_OnlyOnce)
            {
                switch (EventState)
                {
                    case YellowEventState.Inactive:
                        if (EventActive == true) EventState = YellowEventState.Move;
                        break;
                    case YellowEventState.Move:
                        DoMove();
                        if (DestinationReached())
                        {
                            EventActive = false;
                            EventState = YellowEventState.Pause;
                            Helper.Log("YellowMoveEvent: Move_OnlyOnce event has reached the destination. Event remains active so that it can't be triggered again.");
                        }
                        break;
                    case YellowEventState.Pause:
                        break;
                    default:
                        Helper.LogWarning("YellowMoveEvent: An unknown state was triggered: " + EventState);
                        GameStateManager.Instance.SwitchState(GameState.Error);
                        break;
                }
            }
        }

        private void DoMove()
        {
            eventLayer.transform.position = Vector2.MoveTowards(eventLayer.transform.position, points[destination].position, speed * Time.deltaTime);
            AudioManager.Instance.Play("Move");
        }

        private void DoCountDown()
        {
            // Don't do anything if game state isn't Play
            if (PauseEvent) return;

            timer += Time.deltaTime;

            if (delayWhenReachingPoint - timer <= countdown)
            {
                // Don't do anything if count down is <= 0
                if (countdown <= 0) return;

                // Instantiate popup but without setting text and colour
                Transform popup = Instantiate(EffectPopup, PlayerController.Instance.transform);

                // Reduce countdown by 1
                countdown--;

                // Set text and colour depending on countdown
                if (countdown > 0)
                {
                    popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Text, countdown + "...");
                    AudioManager.Instance.Play("EventCountdown");
                }
                else if (countdown == 0)
                {
                    popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Text, "Time is up!");
                    AudioManager.Instance.Play("EventOver");
                }
            }
        }

        private bool DestinationReached()
        {
            if (Vector2.Distance(eventLayer.transform.position, points[destination].position) < 0.02f) return true;
            else return false;
        }

        private void SetFirstDestination() => destination = firstDestination;

        private IEnumerator WaitBeforeReturning()
        {
            // Using an IEnumerator because it is impacted by Time.timeScale
            Helper.Log("YellowMoveEvent: Now pausing for " + delayWhenReachingPoint + " seconds.");
            yield return wait;
            EventState = YellowEventState.Move;
            destination = startPoint;
            countdown = 4;
            timer = 0;
            Helper.Log("YellowMoveEvent: Now en route back to point " + destination + ".");
        }

        private IEnumerator WaitBeforeContinuing()
        {
            // Using an IEnumerator because it is impacted by Time.timeScale
            Helper.Log("YellowMoveEvent: Now pausing for " + delayWhenReachingPoint + " seconds.");
            yield return wait;
            EventState = YellowEventState.Move;
            destination++;
            if (destination == points.Length) destination = 0;
            Helper.Log("YellowMoveEvent: Now resuming. Next up - point " + destination + ".");
        }
    }
}