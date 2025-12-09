using System;
using UnityEngine;

namespace PetSimLite.Data
{
    [CreateAssetMenu(fileName = "ZoneData", menuName = "PetSimLite/Data/Zone")]
    public class ZoneData : ScriptableObject
    {
        [SerializeField] private string zoneId;
        [SerializeField] private string displayName;
        [SerializeField] private float width = 50f;
        [SerializeField] private float length = 50f;
        [SerializeField] private float breakableAreaWidth = 40f;
        [SerializeField] private float breakableAreaLength = 40f;
        [SerializeField] private EggData egg;
        [SerializeField] private GateData gate;
        [SerializeField] private BreakableSpawnConfig[] breakableSpawns;

        public string ZoneId => zoneId;
        public string DisplayName => displayName;
        public float Width => width;
        public float Length => length;
        public float BreakableAreaWidth => breakableAreaWidth;
        public float BreakableAreaLength => breakableAreaLength;
        public EggData Egg => egg;
        public GateData Gate => gate;
        public BreakableSpawnConfig[] BreakableSpawns => breakableSpawns;
    }

    [System.Serializable]
    public struct BreakableSpawnConfig
    {
        public BreakableTemplateData template;
        public int count;
        public float respawnSeconds;
    }
}
