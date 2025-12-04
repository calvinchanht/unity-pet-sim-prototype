using System;
using UnityEngine;

namespace PetSimLite.Data
{
    [CreateAssetMenu(fileName = "EggData", menuName = "PetSimLite/Data/Egg")]
    public class EggData : ScriptableObject
    {
        [SerializeField] private string eggId;
        [SerializeField] private int eggCost = 100;
        [SerializeField] private DropEntry[] dropTable;

        public string EggId => eggId;
        public int EggCost => eggCost;
        public DropEntry[] DropTable => dropTable;
    }

    [Serializable]
    public struct DropEntry
    {
        public PetData pet;
        public int weight;
    }
}
