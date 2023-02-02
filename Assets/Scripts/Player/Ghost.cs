using UnityEngine;
using System.Collections.Generic;

namespace CaptainHindsight
{
    [CreateAssetMenu(fileName = "GhostFor", menuName = "Scriptable Object/New Ghost", order = 2)]
    public class Ghost : ScriptableObject
    {
        public bool IsRecording;
        public float RecordFrequency = 10;
        public int LevelNumber;
        public float TotalTime;
        public int TotalScore;

        public List<float> Timestamp;
        public List<Vector3> Position;
        public List<Quaternion> Rotation;

        public void ResetData()
        {
            IsRecording = false;
            Timestamp.Clear();
            Position.Clear();
            Rotation.Clear();
            TotalTime = 0;
            LevelNumber = 0;
            TotalScore = 0;
        }
    }
}
