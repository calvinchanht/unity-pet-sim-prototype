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
        private struct OwnedPet
        {
            public PetData Data;
            public int InstanceId;
        }

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject petPrefab;

        [Header("Limits")]
        [SerializeField] private int maxActivePets = 10;
        [SerializeField] private float spawnRadius = 2f;

        [Header("Aggro")]
        [SerializeField] private float aggroRadius = 12f;
        [SerializeField] private LayerMask breakableMask = ~0;

        private readonly List<PetAgent> _activeAgents = new List<PetAgent>();
        private readonly HashSet<Breakable> _claimedTargets = new HashSet<Breakable>();
        private readonly List<OwnedPet> _ownedPetInstances = new List<OwnedPet>();
        private int _nextInstanceId = 1;

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
                var pc = FindFirstObjectByType<PlayerController>();
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

            _ownedPetInstances.Add(new OwnedPet
            {
                Data = result.Pet,
                InstanceId = _nextInstanceId++
            });
            RebuildActivePets();
        }

        private void RebuildActivePets()
        {
            // Sort owned by descending power, then by instance id for stable ordering.
            _ownedPetInstances.Sort((a, b) =>
            {
                int powerCompare = b.Data.BasePower.CompareTo(a.Data.BasePower);
                if (powerCompare != 0) return powerCompare;
                return a.InstanceId.CompareTo(b.InstanceId);
            });

            // Desired active list (top N).
            var desiredIds = new HashSet<int>();
            int count = Mathf.Min(maxActivePets, _ownedPetInstances.Count);
            for (int i = 0; i < count; i++)
            {
                desiredIds.Add(_ownedPetInstances[i].InstanceId);
            }

            // Remove agents whose pet is no longer desired.
            for (int i = _activeAgents.Count - 1; i >= 0; i--)
            {
                var agent = _activeAgents[i];
                if (agent == null || !desiredIds.Contains(agent.InstanceId))
                {
                    if (agent != null)
                    {
                        Destroy(agent.gameObject);
                    }
                    _activeAgents.RemoveAt(i);
                }
            }

            // Spawn missing desired pets.
            for (int i = 0; i < _ownedPetInstances.Count && _activeAgents.Count < maxActivePets; i++)
            {
                var petInstance = _ownedPetInstances[i];
                bool alreadyActive = _activeAgents.Exists(a => a != null && a.InstanceId == petInstance.InstanceId);
                if (alreadyActive) continue;

                SpawnPet(petInstance);
            }
        }

        private void SpawnPet(OwnedPet petInstance)
        {
            if (petPrefab == null || player == null || petInstance.Data == null) return;

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

            agent.Initialize(petInstance.Data, petInstance.InstanceId, player);
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
