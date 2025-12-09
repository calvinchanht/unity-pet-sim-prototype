using UnityEngine;

namespace PetSimLite.Data
{
    [CreateAssetMenu(fileName = "BreakableTemplate", menuName = "PetSimLite/Data/Breakable Template")]
    public class BreakableTemplateData : ScriptableObject
    {
        [SerializeField] private string templateId;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private int coinReward = 10;
        [SerializeField] private float prefabScale = 1f;

        public string TemplateId => templateId;
        public GameObject Prefab => prefab;
        public int MaxHealth => maxHealth;
        public int CoinReward => coinReward;
        public float PrefabScale => prefabScale;
    }
}
