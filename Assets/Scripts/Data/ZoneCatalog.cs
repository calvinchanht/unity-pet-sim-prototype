using System.Collections.Generic;
using UnityEngine;

namespace PetSimLite.Data
{
    [CreateAssetMenu(fileName = "ZoneCatalog", menuName = "PetSimLite/Data/Zone Catalog")]
    public class ZoneCatalog : ScriptableObject
    {
        [SerializeField] private List<ZoneData> zones = new List<ZoneData>();

        public IReadOnlyList<ZoneData> Zones => zones;

        public void SetZones(List<ZoneData> list)
        {
            zones = list;
        }
    }
}
