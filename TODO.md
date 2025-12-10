# TODO

- [ ] Gate & zone progression: gate MonoBehaviour to check coins/spend/unlock; wire Zone 1→2+ for prototype scene.
- [ ] Breakable spawner: use ZoneData.breakableSpawns (count/respawn, area size) to place and refill breakables within each zone’s area.
- [ ] Prefab hookup: ensure prefab paths in CSVs exist (pets, eggs, breakables) or adjust paths before running CSV importer.
- [ ] UI: simple pet inventory modal (top-10 with name/rarity/power, storage count); polish coin HUD positioning.
- [ ] Egg roll feedback: lightweight overlay/FX hook on EggRolled event (can reuse egg prefab/scale data).
- [ ] Scene assembly: Prototype scene with Systems (CurrencyManager, PetManager), zones, gates, eggs, breakables, HUD/log configured from generated data.
- [ ] Zone visuals: floor per zone with tiling/gradient material; low wall bounds with emissive strip; gate pillars/arch + price sign; egg on plinth; spawn-area decal/quad.
- [ ] VFX polish: minimal gate/egg particles and emissive materials (works well with mild bloom).
- [ ] Postponed: lighting/post stack/skies cleanup once geometry and colors are in place.
