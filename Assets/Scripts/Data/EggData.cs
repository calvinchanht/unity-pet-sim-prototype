using System;
using UnityEngine;

namespace PetSimLite.Data
{
    [CreateAssetMenu(fileName = "EggData", menuName = "PetSimLite/Data/Egg")]
    public class EggData : ScriptableObject
    {
        [SerializeField] private string eggId;
        [SerializeField] private int eggCost = 100;
        [SerializeField] private float rollIntervalSeconds = 0.8f;
        [SerializeField] private DropEntry[] dropTable;
        [SerializeField] private GameObject prefab;
        [SerializeField] private float prefabScale = 1f;

        public string EggId => eggId;
        public int EggCost => eggCost;
        public float RollIntervalSeconds => rollIntervalSeconds;
        public DropEntry[] DropTable => dropTable;
        public GameObject Prefab => prefab;
        public float PrefabScale => prefabScale;
    }

    [Serializable]
    public struct DropEntry
    {
        public PetData pet;
        public int weight;
    }
}
