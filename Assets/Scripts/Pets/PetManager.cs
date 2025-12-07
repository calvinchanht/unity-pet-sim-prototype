using System.Collections.Generic;
using UnityEngine;
using PetSimLite.Data;
using PetSimLite.Zone;
using PetSimLite.Player;
using PetSimLite.Resources;

namespace PetSimLite.Pets
{
    /// <summary>
    /// Tracks owned pets, spawns top N active, and assigns them to nearby breakables.
    /// </summary>
    public class PetManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject petPrefab;

        [Header("Limits")]
        [SerializeField] private int maxActivePets = 10;
        [SerializeField] private float spawnRadius = 2f;

        [Header("Aggro")]
        [SerializeField] private float aggroRadius = 12f;
        [SerializeField] private LayerMask breakableMask = ~0;

        private readonly List<PetData> _ownedPets = new List<PetData>();
        private readonly List<PetAgent> _activeAgents = new List<PetAgent>();
        private readonly HashSet<Breakable> _claimedTargets = new HashSet<Breakable>();

        private void OnEnable()
        {
            EggRoller.EggRolled += OnEggRolled;
        }

        private void OnDisable()
        {
            EggRoller.EggRolled -= OnEggRolled;
        }

        private void Awake()
        {
            if (player == null)
            {
                var pc = FindObjectOfType<PlayerController>();
                if (pc != null) player = pc.transform;
            }
        }

        private void Update()
        {
            CleanupAgents();
            CleanupClaims();
            AssignPetsToBreakables();
        }

        private void OnEggRolled(EggRollResult result)
        {
            if (result.Pet == null) return;

            _ownedPets.Add(result.Pet);
            RebuildActivePets();
        }

        private void RebuildActivePets()
        {
            // Sort owned by descending power.
            _ownedPets.Sort((a, b) => b.BasePower.CompareTo(a.BasePower));

            // Desired active list (top N).
            var desired = new HashSet<PetData>();
            int count = Mathf.Min(maxActivePets, _ownedPets.Count);
            for (int i = 0; i < count; i++)
            {
                desired.Add(_ownedPets[i]);
            }

            // Remove agents whose pet is no longer desired.
            for (int i = _activeAgents.Count - 1; i >= 0; i--)
            {
                var agent = _activeAgents[i];
                if (agent == null || agent.PetData == null || !desired.Contains(agent.PetData))
                {
                    if (agent != null)
                    {
                        Destroy(agent.gameObject);
                    }
                    _activeAgents.RemoveAt(i);
                }
            }

            // Spawn missing desired pets.
            for (int i = 0; i < _ownedPets.Count && _activeAgents.Count < maxActivePets; i++)
            {
                var pet = _ownedPets[i];
                bool alreadyActive = false;
                for (int j = 0; j < _activeAgents.Count; j++)
                {
                    if (_activeAgents[j].PetData == pet)
                    {
                        alreadyActive = true;
                        break;
                    }
                }

                if (alreadyActive) continue;

                SpawnPet(pet);
            }
        }

        private void SpawnPet(PetData pet)
        {
            if (petPrefab == null || player == null) return;

            Vector3 offset = Random.insideUnitSphere;
            offset.y = 0f;
            offset = offset.normalized * Random.Range(0.5f, spawnRadius);
            Vector3 spawnPos = player.position + offset;

            GameObject obj = Instantiate(petPrefab, spawnPos, Quaternion.identity);
            var agent = obj.GetComponent<PetAgent>();
            if (agent == null)
            {
                agent = obj.AddComponent<PetAgent>();
            }

            agent.Initialize(pet, player);
            _activeAgents.Add(agent);
        }

        private void CleanupAgents()
        {
            for (int i = _activeAgents.Count - 1; i >= 0; i--)
            {
                if (_activeAgents[i] == null)
                {
                    _activeAgents.RemoveAt(i);
                }
            }
        }

        private void CleanupClaims()
        {
            _claimedTargets.RemoveWhere(t =>
                t == null || !_activeAgents.Exists(a => a != null && a.CurrentTarget == t));
        }

        private void AssignPetsToBreakables()
        {
            if (player == null) return;

            Collider[] hits = Physics.OverlapSphere(player.position, aggroRadius, breakableMask);
            if (hits == null || hits.Length == 0) return;

            foreach (var hit in hits)
            {
                var breakable = hit.GetComponentInParent<Breakable>();
                if (breakable == null || _claimedTargets.Contains(breakable))
                {
                    continue;
                }

                var pet = FindIdlePet();
                if (pet != null)
                {
                    pet.AssignTarget(breakable);
                    _claimedTargets.Add(breakable);
                }
            }
        }

        private PetAgent FindIdlePet()
        {
            for (int i = 0; i < _activeAgents.Count; i++)
            {
                var agent = _activeAgents[i];
                if (agent != null && agent.IsIdle)
                {
                    return agent;
                }
            }

            return null;
        }
    }
}
