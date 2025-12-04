using System;
using UnityEngine;

namespace PetSimLite.Data
{
    [CreateAssetMenu(fileName = "ZoneData", menuName = "PetSimLite/Data/Zone")]
    public class ZoneData : ScriptableObject
    {
        [SerializeField] private string zoneId;
        [SerializeField] private string displayName;
        [SerializeField] private EggData egg;
        [SerializeField] private GateData gate;
        [SerializeField] private BreakableConfig breakableConfig;

        public string ZoneId => zoneId;
        public string DisplayName => displayName;
        public EggData Egg => egg;
        public GateData Gate => gate;
        public BreakableConfig Breakable => breakableConfig;
    }

    [Serializable]
    public struct BreakableConfig
    {
        public GameObject prefab;
        public int maxHealth;
        public int coinReward;
    }
}
