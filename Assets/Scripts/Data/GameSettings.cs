using UnityEngine;

namespace PetSimLite.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "PetSimLite/Data/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private int startingCoins = 500;
        [SerializeField] private string startingZoneId = "zone1";
        [SerializeField] private GameObject defaultPetPrefab;

        [Header("Gate Defaults")]
        [SerializeField] private Vector3 gateSize = new Vector3(4f, 3f, 0.5f);
        [SerializeField] private string gateLabel = "UNLOCK NEXT ZONE";

        public int StartingCoins => startingCoins;
        public string StartingZoneId => startingZoneId;
        public GameObject DefaultPetPrefab => defaultPetPrefab;
        public Vector3 GateSize => gateSize;
        public string GateLabel => gateLabel;
    }
}
