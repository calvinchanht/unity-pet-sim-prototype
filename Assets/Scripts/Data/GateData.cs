using UnityEngine;

namespace PetSimLite.Data
{
    [CreateAssetMenu(fileName = "GateData", menuName = "PetSimLite/Data/Gate")]
    public class GateData : ScriptableObject
    {
        [SerializeField] private string gateId;
        [SerializeField] private int requiredCoins = 500;

        public string GateId => gateId;
        public int RequiredCoins => requiredCoins;
    }
}
