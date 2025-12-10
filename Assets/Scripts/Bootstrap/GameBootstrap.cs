using System.Collections.Generic;
using UnityEngine;
using PetSimLite.Data;
using PetSimLite.Economy;
using PetSimLite.Pets;
using PetSimLite.Player;
using PetSimLite.ZoneGeneration;

namespace PetSimLite.Bootstrap
{
    /// <summary>
    /// Single entry point to spin up systems, UI, player, and zones.
    /// Configure with GameSettings and a list of ZoneData assets (unordered).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private List<ZoneData> zones = new List<ZoneData>();
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject uiCanvasPrefab; // should contain CoinHUD/EggRollLog

        private void Start()
        {
            if (gameSettings == null)
            {
                gameSettings = UnityEngine.Resources.Load<GameSettings>("GameSettings");
                if (gameSettings == null)
                {
                    Debug.LogError("[GameBootstrap] Missing GameSettings.");
                    return;
                }
            }

            var currency = EnsureCurrencyManager(gameSettings.StartingCoins);
            var player = EnsurePlayer();
            EnsureUI();
            var petManager = EnsurePetManager(player, gameSettings.DefaultPetPrefab);
            BuildZonesSequence();
        }

        private CurrencyManager EnsureCurrencyManager(int startingCoins)
        {
            var currency = FindFirstObjectByType<CurrencyManager>();
            if (currency == null)
            {
                var go = new GameObject("CurrencyManager");
                currency = go.AddComponent<CurrencyManager>();
            }
            currency.InitializeCoins(startingCoins);
            return currency;
        }

        private PlayerController EnsurePlayer()
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player == null && playerPrefab != null)
            {
                var pgo = Instantiate(playerPrefab);
                pgo.name = "Player";
                player = pgo.GetComponent<PlayerController>();
            }

            return player;
        }

        private void EnsureUI()
        {
            if (uiCanvasPrefab != null && FindFirstObjectByType<PetSimLite.UI.CoinHUD>() == null)
            {
                Instantiate(uiCanvasPrefab);
            }
        }

        private PetManager EnsurePetManager(PlayerController player, GameObject defaultPetPrefab)
        {
            var petManager = FindFirstObjectByType<PetManager>();
            if (petManager == null)
            {
                var go = new GameObject("PetManager");
                petManager = go.AddComponent<PetManager>();
            }

            petManager.SetPlayer(player != null ? player.transform : null);
            petManager.SetDefaultPetPrefab(defaultPetPrefab);
            return petManager;
        }

        private void BuildZonesSequence()
        {
            var zoneSource = zones != null && zones.Count > 0 ? zones : LoadZonesFromCatalog();
            if (zoneSource == null || zoneSource.Count == 0)
            {
                Debug.LogWarning("[GameBootstrap] No zones provided or found in catalog.");
                return;
            }

            // Map by id
            var dict = new Dictionary<string, ZoneData>();
            foreach (var z in zoneSource)
            {
                if (z != null && !string.IsNullOrWhiteSpace(z.ZoneId))
                {
                    dict[z.ZoneId] = z;
                }
            }

            // Build ordered list
            var ordered = new List<ZoneData>();
            string currentId = gameSettings.StartingZoneId;
            var visited = new HashSet<string>();

            while (!string.IsNullOrWhiteSpace(currentId) && dict.TryGetValue(currentId, out var z) && !visited.Contains(currentId))
            {
                ordered.Add(z);
                visited.Add(currentId);
                currentId = z.NextZoneId;
            }

            if (ordered.Count == 0)
            {
                Debug.LogWarning("[GameBootstrap] Could not resolve zone sequence from StartingZoneId.");
                return;
            }

            var sequencerGO = new GameObject("ZoneSequencer");
            var sequencer = sequencerGO.AddComponent<ZoneSequencer>();
            sequencer.BuildFromList(ordered, gameSettings.GateSize, gameSettings.GateLabel);
        }

        private List<ZoneData> LoadZonesFromCatalog()
        {
            var catalog = UnityEngine.Resources.Load<ZoneCatalog>("ZoneCatalog");
            if (catalog != null && catalog.Zones != null && catalog.Zones.Count > 0)
            {
                return new List<ZoneData>(catalog.Zones);
            }
            return null;
        }
    }
}
