# Pet Sim Lite - Requirements v0.2 - 2025-12-04

## 1. Platform & Controls (Phase 1)

* Target platform: PC for initial prototype (mouse + keyboard), with controls chosen to **simulate mobile-style gestures** for later porting.

### Player Movement

* Primary scheme:

  * **Left mouse button drag**: drag in a direction on the ground plane to move the player in that direction (simulating touch drag-to-move).
* Optional fallback (can be implemented later if desired):

  * WASD keyboard input for desktop testing.

### Camera Controls (PC, gesture-inspired)

* Camera: top-down or slightly angled 3D camera, always following the player.
* Controls:

  * **Hold Shift + left mouse drag**: rotate the camera around the player (simulates a two-finger rotate gesture).
  * **Hold Ctrl + left mouse drag up/down**: zoom the camera in and out (simulates pinch-zoom). Horizontal Ctrl-drag can be ignored or treated as no-op for MVP.
* These mappings let us debug a mobile-like camera on PC without multi-touch.

Mobile-style true multi-touch gestures (drag to move, two-finger rotate, pinch zoom) remain **Phase 2** features for an actual mobile build, but the above PC mappings should make adding real touch input straightforward later.

## 2. Core Loop (Gacha-Centric)

The core game loop is a simplified, single-player version of Pet Simulator 99 with a strong gacha focus.

1. Player farms coins by breaking coin/chest objects.
2. Player uses coins to auto-purchase egg rolls in the current zone.
3. Egg rolls give pets of different rarities and powers.
4. Stronger pets → higher total team power → faster breaking of coins/chests → more coins.
5. Accumulate enough coins to auto-unlock the next zone via a price gate (wall).
6. New zone provides a new egg with stronger pets and tougher coins/chests.

The system should be data-driven so additional zones, eggs, pets, and coin/chest configurations can be added through configuration (ScriptableObjects / data files) without changing code.

## 3. Zones & Level Layout

* Each zone is a simple square arena.
* Every zone contains:

  * A single **Egg** placed in the center.
  * Multiple **coin/chest** breakable objects scattered around.
  * A **Zone Gate/Wall** that leads to the next zone.
* For MVP:

  * Implement **Zone 1** and **Zone 2** only.
  * Zone 1: cheaper egg, weaker pets, weaker coins/chests, cheaper gate.
  * Zone 2: more expensive egg, stronger pets, tougher coins/chests, more expensive gate.

## 4. Coins / Chests (Breakables)

* Breakables (coins/chests) have:

  * HP value.
  * Coin reward amount.
* When “attacked” by pets:

  * Their HP is reduced based on pets’ combined damage per second.
  * When HP reaches 0, the breakable is destroyed and the player gains the configured coin reward.
* For MVP, a single type of breakable per zone is enough (can still use different prefabs/models later).

## 5. Currency & Economy

* Single soft currency: **coins**.
* **CurrencyManager** responsibilities:

  * Store current coin amount.
  * Provide methods to add and spend coins with success/fail result.
  * Raise an event when coin amount changes so UI can update.

## 6. Eggs & Auto-Roll System

* Each zone has exactly one egg with:

  * `eggCost` (coins per roll).
  * A drop table defining pets and their rarity/weights.
* An invisible **egg circle trigger** surrounds the egg.
* Behaviour:

  * When the player enters the circle and has at least `eggCost` coins, the game **automatically spends coins and rolls an egg**.
  * Rolling continues automatically at a fixed interval (e.g. 0.7–1.0 seconds) as long as the player is inside the circle and has enough coins.
  * When the player leaves the circle or runs out of coins, rolling stops.

### Egg Roll Feedback

* On each roll:

  * Show an overlay with a 3D egg that blinks/shakes a few times.
  * After the short animation, reveal the pet obtained:

    * Show pet name.
    * Show rarity (e.g. Common, Rare, Epic, Legendary).
    * Optionally tint the egg/FX by rarity.

## 7. Pets – Data & Behaviour

### Pet Data

* Pets are defined as data objects (e.g. ScriptableObjects) with:

  * ID / name.
  * Rarity.
  * Base power (used to compute damage).
  * Optional: movement speed or other stats if needed later.

### Pet Appearance

* Pets can be very simple: a cube with minimal attachments/decoration.
* Visual variety can be mostly via color, small accessories, and VFX.
* Performance is not expected to be an issue even with many pets, given their simplicity.

### Pet Spawning

* When the player obtains a new pet that belongs in the **top 10 strongest pets**:

  * That pet should **spawn into the world**.
  * Spawn position: random point within a small radius around the player, avoiding overlaps if possible.
* Pets not in the top 10 are kept in inventory and do not appear in the world.

### Active Team Rules

* The player can own unlimited pets.
* At any time, **up to 10 highest-power pets** are considered "active" and are visible in the world.
* If a new pet is rolled that is stronger than at least one active pet:

  * Recalculate the top 10.
  * Promote the new pet to active status, demote the weakest, and update the world visuals accordingly.

### Pet Following Behaviour

* Each active pet should follow the player using very simple logic:

  * If the distance to the player is greater than a threshold `followRange`:

    * Move toward the player's position until within `followRange`.
  * If within `followRange` and not currently attacking, idle near the player (e.g. slight wandering or idle animation is optional).
* Movement can be simple (no complex pathfinding required for MVP).

### Auto-Attack Behaviour

* There is an **aggro radius** around the player.
* When the player moves close enough to a coin/chest (within this radius):

  * A central **PetManager** selects one or more free pets to attack that target.
  * Assignment logic should be simple for MVP (e.g., closest idle pet to target).
* While attacking:

  * Pet moves toward the assigned target if not already close.
  * When in attack range, apply damage over time based on pet power.
  * Once the target is destroyed, pet returns to follow behaviour.

## 8. Pet Inventory & UI

* There is a **Pet Inventory button** on the HUD.
* When clicked, it opens a **Pet Inventory UI** which shows:

  * The top 10 active pets with their key info (name, rarity, power).
  * Optionally, a summary line like "Extra pets in storage: X".
* For MVP, pets are automatically chosen based on power (no manual equip/unequip yet).

## 9. Zone Gates / Walls

* Each zone (except the first) is blocked by a gate/wall that leads to the next zone.
* Gate data:

  * `requiredCoins` to unlock.
* Behaviour:

  * The gate displays its price visually (e.g., floating text or sign).
  * When the player touches the gate:

    * If the player has **at least `requiredCoins`**:

      * Automatically spend the coins.
      * Play an open animation or simply disable collider/mesh to allow passage.
    * If the player does not have enough coins:

      * The wall remains solid and optionally shows a short "Need X coins" message.

## 10. UI Summary (MVP)

* Screen-space UI elements (HUD-style overlay, not in-world 3D objects):

  * Current coin count (e.g. top of screen).
  * Pet Inventory button (e.g. corner button that opens the inventory modal).
* Pet Inventory UI:

  * Modal or panel that lists the top 10 active pets with their name, rarity, and power.
  * Optionally shows a summary count of extra pets in storage.
* Egg roll overlay:

  * Egg shake animation + pet reveal when rolling.
* Keep layout simple and readable; styling/polish can be improved later.

## 11. Technical & Architectural Preferences

* Use C# with standard Unity best practices.
* Prefer:

  * Small, focused MonoBehaviours.
  * ScriptableObjects for data (pets, eggs, zones, walls, etc.), even when data is initially authored in Google Sheets and imported via CSV.
  * Events/delegates or simple observer patterns for updating UI on data changes.
* Keep code organised into clear folders under `Assets/Scripts/`, for example:

  * `Assets/Scripts/Player`
  * `Assets/Scripts/Pets`
  * `Assets/Scripts/Resources` (coins/chests and related logic)
  * `Assets/Scripts/Economy`
  * `Assets/Scripts/UI`
  * `Assets/Scripts/Zone`
  * Additional subfolders can be added as needed, but they should follow this pattern and remain consistent with the requirements document.
* Aim for readable, maintainable code that can be extended with:

  * More zones.
  * More eggs and pets.
  * Additional mechanics (rebirth, upgrades, etc.) later.

### Data Flow & Bootstrap (Runtime)

* Author/tune data in CSVs under `Assets/Data/CSV/` (PascalCase filenames). Importer generates ScriptableObjects under `Assets/Data/Generated/` (e.g., `GameSettings.asset`, `ZoneData`, `ZoneCatalog.asset`). Code reads generated assets, not raw CSV at runtime.
* Auto-import on Play is enabled via editor hook; you can also run `Tools/PetSimLite/Import CSV Data` manually to regenerate assets after CSV changes.
* `GameBootstrap` is the single scene entry point: it loads `GameSettings`, resolves zone order via `startingZoneId`/`nextZoneId`, spawns systems (CurrencyManager/PetManager), player, UI, and builds zones procedurally (floors/walls/gates/eggs/breakables). Gates are procedural; no gate prefab needed.

## 12. Data Authoring with Google Sheets / CSV

* Core game data (zones, eggs, pets, player settings, general game settings) can be authored in a shared Google Sheets document.
* Recommended tabs:

  * **Zones**: zone ID, display name, egg ID, wall price, coin/chest prefab IDs, HP/reward modifiers.
  * **Pets**: pet ID, name, rarity, base power, optional visual/style fields.
  * **Eggs**: egg ID, zone ID, cost, drop table entries (pet ID + weight/rarity).
  * **PlayerSettings**: base move speed, camera settings, max active pets (e.g. 10), aggro radius, follow range.
  * **GameSettings**: global tuning values that may be tweaked often.
* The sheet is periodically exported as CSV files (one CSV per tab) and placed into a Unity `Data/` folder.
* A small **CSV import/parsing layer** will:

  * Load these CSVs at startup (or via an editor tool).
  * Convert rows into ScriptableObjects or in-memory data structures.
  * Allow re-importing when columns are added or tuning changes.
* Even for a small number of zones and pets, this approach makes it easier to iterate on balancing and add new columns over time without touching code.

## 13. Future Extensions (Not Required for MVP)

* Multiple eggs per zone.
* Manual pet management (equip/unequip, locking favourites).
* More elaborate pet stats/abilities.
* Rebirth/prestige mechanics.
* Full mobile control scheme with drag movement and gesture-based camera.
* Visual polish: better VFX, animations, sound, and UI styling.

## Edit log

* v0.2 – 2025-12-04: Added versioned title, clarified screen-space UI wording, added Google Sheets/CSV data authoring section, and introduced an edit log. Previous content retained; only section numbering adjusted near the end.
* v0.3 – 2025-12-04: Clarified control scheme to use left-drag for movement, Shift+drag for camera rotation, and Ctrl+drag for zoom on PC (gesture-inspired controls). Added note that filename in the repo should stay stable (e.g., `Docs/PetSimLite-Requirements.md`) while version/date are tracked inside the document and edit log.
