using UnityEngine;

namespace PetSimLite.Data
{
    public enum PetRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [CreateAssetMenu(fileName = "PetData", menuName = "PetSimLite/Data/Pet")]
    public class PetData : ScriptableObject
    {
        [SerializeField] private string petId;
        [SerializeField] private string displayName;
        [SerializeField] private PetRarity rarity;
        [SerializeField] private float basePower = 1f;

        public string PetId => petId;
        public string DisplayName => displayName;
        public PetRarity Rarity => rarity;
        public float BasePower => basePower;
    }
}
