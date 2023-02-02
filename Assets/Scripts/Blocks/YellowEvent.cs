using UnityEngine;

namespace CaptainHindsight
{
    [RequireComponent(typeof(YellowEventType))]
    public class YellowEvent : BStateDependent
    {
        [Header("Yellow Event Configuration")]
        [SerializeField] protected YellowEventType EventType;
        [SerializeField] protected string EventName;
        [SerializeField] protected string OnMessage;
        [SerializeField] protected string OffMessage;

        [Header("Yellow Event References")]
        [SerializeField] protected Transform EffectPopup;
        protected YellowEventState EventState;
        protected bool EventActive;
        protected bool PauseEvent;

        private void ReadEvents(string name, bool buttonPressed)
        {
            if (EventName != name) return;

            if (EventActive)
            {
                // Send 'off message' and do nothing - e.g. when button pressed during an active Move_WaitAndReturn event
                if (buttonPressed)
                {
                    InstatiateMessageToPlayer(OffMessage);
                    AudioManager.Instance.Play("Deny");
                }
            }
            else
            {
                if (buttonPressed)
                {
                    // Activate event & send 'on message' - this is the main activation path
                    EventActive = true;
                    InstatiateMessageToPlayer(OnMessage);
                }
                else
                {
                    // Activate event & send 'off message' - used for Move_BackAndForth and this is 'back', for example
                    EventActive = true;
                    InstatiateMessageToPlayer(OffMessage);
                }
            }
        }

        private void InstatiateMessageToPlayer(string message)
        {
            Transform popup = Instantiate(EffectPopup, PlayerController.Instance.transform);
            popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Text, message);
        }

        protected override void ActionGameStateChange(GameState state, GameStateSettings settings) 
        {
            if (state != GameState.Play) PauseEvent = true;
            else PauseEvent = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.OnTriggerYellowEvent += ReadEvents;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventManager.Instance.OnTriggerYellowEvent -= ReadEvents;
            StopAllCoroutines();
        }
    }
}
