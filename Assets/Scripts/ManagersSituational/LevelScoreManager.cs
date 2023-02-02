using UnityEngine;
using System.Collections.Generic;

namespace CaptainHindsight
{
    public class LevelScoreManager : MonoBehaviour
    {
        public static LevelScoreManager Instance;
        private Dictionary<string, int> scoringEvents = new Dictionary<string, int>();

        [Header("Positive event count")]
        [SerializeField] private int timeBonus;
        [SerializeField] private int breakableBlocksYellow;
        [SerializeField] private int breakableBlocksRed;
        [SerializeField] private int trianglesRed;
        [SerializeField] private int breakableBlocksBlue;

        [Header("Negative event count")]
        [SerializeField] private int breakableBlocksGreen;
        [SerializeField] private int damageTaken;
        [SerializeField] private int touchingWalls;

        [Header("Positive event multiplier")]
        [SerializeField] private int timeBonusMultiplier; // per second
        [SerializeField] private int breakableBlocksYellowMultiplier; // per destroyed object
        [SerializeField] private int breakableBlocksRedMultiplier; // per destroyed object
        [SerializeField] private int trianglesRedMultiplier; // per destroyed object
        [SerializeField] private int breakableBlocksBlueMultiplier; // per destroyed object

        [Header("Negative event multiplier")]
        [SerializeField] private int breakableBlocksGreenMultiplier; // per destroyed object
        [SerializeField] private int damageTakenMultiplier; // per damage point
        [SerializeField] private int touchingWallsMultiplier; // per second

        // Note that if you want to add a new scoring event, it will have to be added in several places:
        // a) In EffectTypes (enum) so that it can be triggered as an event
        // b) In this script as new count and multiplier variables
        // c) In this script under ShareScoringDataAsDictionary() so that it is made part of the list

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        // Used to record events whenever a score event is triggered
        private void CountScoreEvent(ScoreEventType scoreEventType, int value)
        {
            switch (scoreEventType)
            {
                case ScoreEventType.BreakableBlocksBlue:
                    breakableBlocksBlue++;
                    break;
                case ScoreEventType.BreakableBlocksRed:
                    breakableBlocksRed++;
                    break;
                case ScoreEventType.BreakableBlocksGreen:
                    breakableBlocksGreen++;
                    break;
                case ScoreEventType.BreakableBlocksYellow:
                    breakableBlocksYellow++;
                    break;
                case ScoreEventType.TrianglesRed:
                    trianglesRed++;
                    break;
                case ScoreEventType.Time:
                    timeBonus = value;
                    break;
                case ScoreEventType.TouchingWall:
                    touchingWalls += value;
                    break;
                case ScoreEventType.DamageTaken:
                    damageTaken += value;
                    break;
                default:
                    Helper.Log("An unknown scoring event was triggered: " + scoreEventType);
                    break;
            }
        }

        // Used to visualise and calculate the total score elsewhere
        public Dictionary<string, int> ShareScoringDataAsDictionary()
        {
            // Clear the list before it is assembled to ensure that it can be called anytime
            scoringEvents.Clear();

            if (timeBonus > 0) scoringEvents.Add("Time bonus:", timeBonus * timeBonusMultiplier);
            if (breakableBlocksYellow > 0) scoringEvents.Add("Yellow blocks bonus:", breakableBlocksYellow * breakableBlocksYellowMultiplier);
            if (breakableBlocksRed > 0) scoringEvents.Add("Red blocks bonus:", breakableBlocksRed * breakableBlocksRedMultiplier);
            if (trianglesRed > 0) scoringEvents.Add("Triangle bonus:", trianglesRed * trianglesRedMultiplier);
            if (breakableBlocksBlue > 0) scoringEvents.Add("Blue blocks bonus:", breakableBlocksBlue * breakableBlocksBlueMultiplier);
            if (breakableBlocksGreen > 0) scoringEvents.Add("Green block penalty:", breakableBlocksGreen * breakableBlocksGreenMultiplier);
            if (damageTaken > 0) scoringEvents.Add("Damage taken penalty:", damageTaken * damageTakenMultiplier);
            if (touchingWalls > 0) scoringEvents.Add("Touching walls penalty:", touchingWalls * touchingWallsMultiplier);

            return scoringEvents;
        }

        #region Managing events
        private void OnEnable() => EventManager.Instance.OnCountEvent += CountScoreEvent;

        private void OnDestroy() =>  EventManager.Instance.OnCountEvent -= CountScoreEvent;
        #endregion
    }
}
