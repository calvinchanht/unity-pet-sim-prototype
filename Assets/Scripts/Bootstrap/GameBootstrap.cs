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

            EnsureCurrencyManager(gameSettings.StartingCoins);
            EnsureUI();
            BuildZonesSequence();

            var player = EnsurePlayer();
            EnsureCamera(player);
            EnsurePetManager(player, gameSettings.DefaultPetPrefab);
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
                player = pgo.GetComponent<PlayerController>() ?? pgo.AddComponent<PlayerController>();
            }

            if (player == null && gameSettings.PlayerPrefab != null)
            {
                var pgo = Instantiate(gameSettings.PlayerPrefab);
                pgo.name = "Player";
                player = pgo.GetComponent<PlayerController>() ?? pgo.AddComponent<PlayerController>();
            }

            if (player != null)
            {
                if (player.GetComponent<CharacterController>() == null)
                {
                    player.gameObject.AddComponent<CharacterController>();
                }
                player.Configure(gameSettings);

                // Ensure animator driver (drives Base_Mesh animator parameters).
                var driver = player.GetComponent<PlayerAnimatorDriver>();
                if (driver == null)
                {
                    driver = player.gameObject.AddComponent<PlayerAnimatorDriver>();
                }
                driver.Configure(gameSettings, player, player.GetComponentInChildren<Animator>());

                DisableThirdPartyControllers(player.gameObject);

                // Prefer controller-driven movement.
                var animator = player.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.applyRootMotion = false;
                }
            }

            return player;
        }

        private void EnsureUI()
        {
            var prefab = uiCanvasPrefab != null ? uiCanvasPrefab : gameSettings.UiCanvasPrefab;
            if (prefab != null && FindFirstObjectByType<PetSimLite.UI.CoinHUD>() == null)
            {
                Instantiate(prefab);
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

        private void EnsureCamera(PlayerController player)
        {
            if (player == null) return;

            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                cam = go.AddComponent<Camera>();
                go.tag = "MainCamera";
            }

            // Disable old camera controller if present.
            var old = cam.GetComponent<PetSimLite.CameraSystem.CameraFollowController>();
            if (old != null) old.enabled = false;

            var tp = cam.GetComponent<PetSimLite.CameraSystem.RobloxThirdPersonCamera>();
            if (tp == null)
            {
                tp = cam.gameObject.AddComponent<PetSimLite.CameraSystem.RobloxThirdPersonCamera>();
            }
            tp.Configure(gameSettings, player.transform);
        }

        private static void DisableThirdPartyControllers(GameObject playerRoot)
        {
            // The ithappy prefab generator can add old-input movement scripts that conflict with our new Input System setup.
            var behaviours = playerRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                var mb = behaviours[i];
                if (mb == null) continue;
                string fullName = mb.GetType().FullName;
                if (fullName == "Controller.MovePlayerInput" || fullName == "Controller.CharacterMover")
                {
                    mb.enabled = false;
                }
            }
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
