using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PetSimLite.Data;
using UnityEditor;
using UnityEngine;

namespace PetSimLite.Editor
{
    /// <summary>
    /// Simple CSV -> ScriptableObject importer for pet templates, eggs (with drop table), gates, breakable templates, and zones.
    /// Expected CSV files in Assets/Data/CSV:
    /// petTemplates.csv, eggs.csv, eggDrops.csv, gates.csv, breakableTemplates.csv, zones.csv, zoneBreakables.csv
    /// Output assets are written to Assets/Data/Generated/.
    /// </summary>
    public static class CSVImporter
    {
        private const string CsvFolder = "Assets/Data/CSV";
        private const string GeneratedFolder = "Assets/Data/Generated";

        [MenuItem("Tools/PetSimLite/Import CSV Data")]
        public static void ImportAll()
        {
            EnsureFolder(GeneratedFolder);

            var petAssets = ImportPetTemplates();
            var gateAssets = ImportGates();
            var breakableTemplates = ImportBreakableTemplates();
            var eggAssets = ImportEggs(petAssets);
            ImportZones(eggAssets, gateAssets, breakableTemplates);

            AssetDatabase.SaveAssets();
            Debug.Log("[CSV Importer] Import completed.");
        }

        #region Importers

        private static Dictionary<string, PetData> ImportPetTemplates()
        {
            string path = Path.Combine(CsvFolder, "petTemplates.csv");
            var rows = ReadCsv(path);
            var pets = new Dictionary<string, PetData>();

            foreach (var row in rows)
            {
                if (!row.TryGetValue("petTemplateId", out var id) || string.IsNullOrWhiteSpace(id))
                {
                    Debug.LogWarning($"[CSV Importer] Skipping pet row without petTemplateId.");
                    continue;
                }

                var asset = GetOrCreateAsset<PetData>($"{GeneratedFolder}/Pet_{id}.asset");
                var so = new SerializedObject(asset);
                so.FindProperty("petId").stringValue = id.Trim();
                so.FindProperty("displayName").stringValue = Get(row, "displayName", id);
                so.FindProperty("basePower").floatValue = ParseFloat(Get(row, "basePower", "1"), 1f);

                string prefabPath = Get(row, "prefabPath", string.Empty);
                GameObject petPrefab = !string.IsNullOrWhiteSpace(prefabPath)
                    ? AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                    : null;
                if (petPrefab == null && !string.IsNullOrWhiteSpace(prefabPath))
                {
                    Debug.LogWarning($"[CSV Importer] Pet template '{id}' prefab not found at '{prefabPath}'.");
                }
                so.FindProperty("prefab").objectReferenceValue = petPrefab;
                so.FindProperty("prefabScale").floatValue = ParseFloat(Get(row, "prefabScale", "1"), 1f);

                var rarityStr = Get(row, "rarity", "Common");
                if (Enum.TryParse(rarityStr, true, out PetRarity rarity))
                {
                    so.FindProperty("rarity").enumValueIndex = (int)rarity;
                }
                else
                {
                    Debug.LogWarning($"[CSV Importer] Unknown rarity '{rarityStr}' for pet '{id}', defaulting to Common.");
                    so.FindProperty("rarity").enumValueIndex = (int)PetRarity.Common;
                }

                so.ApplyModifiedProperties();
                pets[id] = asset;
            }

            return pets;
        }

        private static Dictionary<string, GateData> ImportGates()
        {
            string path = Path.Combine(CsvFolder, "gates.csv");
            var rows = ReadCsv(path);
            var gates = new Dictionary<string, GateData>();

            foreach (var row in rows)
            {
                if (!row.TryGetValue("gateId", out var id) || string.IsNullOrWhiteSpace(id))
                {
                    Debug.LogWarning("[CSV Importer] Skipping gate row without gateId.");
                    continue;
                }

                var asset = GetOrCreateAsset<GateData>($"{GeneratedFolder}/Gate_{id}.asset");
                var so = new SerializedObject(asset);
                so.FindProperty("gateId").stringValue = id.Trim();
                so.FindProperty("requiredCoins").intValue = ParseInt(Get(row, "requiredCoins", "0"), 0);
                so.ApplyModifiedProperties();
                gates[id] = asset;
            }

            return gates;
        }

        private static Dictionary<string, EggData> ImportEggs(Dictionary<string, PetData> petsById)
        {
            string eggsPath = Path.Combine(CsvFolder, "eggs.csv");
            string dropsPath = Path.Combine(CsvFolder, "eggDrops.csv");

            var eggRows = ReadCsv(eggsPath);
            var dropRows = ReadCsv(dropsPath);

            // Group drop rows by eggId.
            var dropsByEgg = dropRows
                .Where(r => r.ContainsKey("eggId") && !string.IsNullOrWhiteSpace(r["eggId"]))
                .GroupBy(r => r["eggId"].Trim())
                .ToDictionary(g => g.Key, g => g.ToList());

            var eggs = new Dictionary<string, EggData>();

            foreach (var row in eggRows)
            {
                if (!row.TryGetValue("eggId", out var eggId) || string.IsNullOrWhiteSpace(eggId))
                {
                    Debug.LogWarning("[CSV Importer] Skipping egg row without eggId.");
                    continue;
                }

                var asset = GetOrCreateAsset<EggData>($"{GeneratedFolder}/Egg_{eggId}.asset");
                var so = new SerializedObject(asset);
                so.FindProperty("eggId").stringValue = eggId.Trim();
                so.FindProperty("eggCost").intValue = ParseInt(Get(row, "cost", "0"), 0);
                so.FindProperty("rollIntervalSeconds").floatValue = ParseFloat(Get(row, "rollIntervalSeconds", "0.8"), 0.8f);

                string prefabPath = Get(row, "prefabPath", string.Empty);
                GameObject eggPrefab = !string.IsNullOrWhiteSpace(prefabPath)
                    ? AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                    : null;
                if (eggPrefab == null && !string.IsNullOrWhiteSpace(prefabPath))
                {
                    Debug.LogWarning($"[CSV Importer] Egg '{eggId}' prefab not found at '{prefabPath}'.");
                }
                so.FindProperty("prefab").objectReferenceValue = eggPrefab;
                so.FindProperty("prefabScale").floatValue = ParseFloat(Get(row, "prefabScale", "1"), 1f);

                var dropTableProp = so.FindProperty("dropTable");
                if (dropsByEgg.TryGetValue(eggId.Trim(), out var eggDropRows))
                {
                    dropTableProp.arraySize = eggDropRows.Count;
                    for (int i = 0; i < eggDropRows.Count; i++)
                    {
                        var dr = eggDropRows[i];
                        string petId = Get(dr, "petTemplateId", string.Empty);
                        int weight = ParseInt(Get(dr, "weight", "0"), 0);

                        var element = dropTableProp.GetArrayElementAtIndex(i);
                        element.FindPropertyRelative("weight").intValue = Mathf.Max(0, weight);

                        if (petsById.TryGetValue(petId, out var petAsset))
                        {
                            element.FindPropertyRelative("pet").objectReferenceValue = petAsset;
                        }
                        else
                        {
                            element.FindPropertyRelative("pet").objectReferenceValue = null;
                            Debug.LogWarning($"[CSV Importer] Egg '{eggId}' drop references unknown petTemplateId '{petId}'.");
                        }
                    }
                }
                else
                {
                    dropTableProp.arraySize = 0;
                    Debug.LogWarning($"[CSV Importer] Egg '{eggId}' has no drop entries.");
                }

                so.ApplyModifiedProperties();
                eggs[eggId.Trim()] = asset;
            }

            return eggs;
        }

        private static Dictionary<string, BreakableTemplateData> ImportBreakableTemplates()
        {
            string path = Path.Combine(CsvFolder, "breakableTemplates.csv");
            var rows = ReadCsv(path);
            var templates = new Dictionary<string, BreakableTemplateData>();

            foreach (var row in rows)
            {
                if (!row.TryGetValue("breakableTemplateId", out var id) || string.IsNullOrWhiteSpace(id))
                {
                    Debug.LogWarning("[CSV Importer] Skipping breakable row without breakableTemplateId.");
                    continue;
                }

                var asset = GetOrCreateAsset<BreakableTemplateData>($"{GeneratedFolder}/Breakable_{id}.asset");
                var so = new SerializedObject(asset);
                so.FindProperty("templateId").stringValue = id.Trim();
                so.FindProperty("maxHealth").intValue = ParseInt(Get(row, "maxHealth", "0"), 0);
                so.FindProperty("coinReward").intValue = ParseInt(Get(row, "coinReward", "0"), 0);

                string prefabPath = Get(row, "prefabPath", string.Empty);
                GameObject breakablePrefab = !string.IsNullOrWhiteSpace(prefabPath)
                    ? AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                    : null;
                if (breakablePrefab == null && !string.IsNullOrWhiteSpace(prefabPath))
                {
                    Debug.LogWarning($"[CSV Importer] Breakable template '{id}' prefab not found at '{prefabPath}'.");
                }
                so.FindProperty("prefab").objectReferenceValue = breakablePrefab;
                so.FindProperty("prefabScale").floatValue = ParseFloat(Get(row, "prefabScale", "1"), 1f);

                so.ApplyModifiedProperties();
                templates[id.Trim()] = asset;
            }

            return templates;
        }

        private static void ImportZones(Dictionary<string, EggData> eggsById, Dictionary<string, GateData> gatesById, Dictionary<string, BreakableTemplateData> breakablesById)
        {
            string path = Path.Combine(CsvFolder, "zones.csv");
            var rows = ReadCsv(path);
            var breakableRows = ReadCsv(Path.Combine(CsvFolder, "zoneBreakables.csv"));

            // group breakables per zone
            var breakablesPerZone = breakableRows
                .Where(r => r.ContainsKey("zoneId") && !string.IsNullOrWhiteSpace(r["zoneId"]))
                .GroupBy(r => r["zoneId"].Trim())
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var row in rows)
            {
                if (!row.TryGetValue("zoneId", out var zoneId) || string.IsNullOrWhiteSpace(zoneId))
                {
                    Debug.LogWarning("[CSV Importer] Skipping zone row without zoneId.");
                    continue;
                }

                var asset = GetOrCreateAsset<ZoneData>($"{GeneratedFolder}/Zone_{zoneId}.asset");
                var so = new SerializedObject(asset);
                so.FindProperty("zoneId").stringValue = zoneId.Trim();
                so.FindProperty("displayName").stringValue = Get(row, "displayName", zoneId);
                so.FindProperty("width").floatValue = ParseFloat(Get(row, "width", "50"), 50f);
                so.FindProperty("length").floatValue = ParseFloat(Get(row, "length", "50"), 50f);
                so.FindProperty("breakableAreaWidth").floatValue = ParseFloat(Get(row, "breakableAreaWidth", "40"), 40f);
                so.FindProperty("breakableAreaLength").floatValue = ParseFloat(Get(row, "breakableAreaLength", "40"), 40f);

                // Egg link
                var eggId = Get(row, "eggId", string.Empty);
                so.FindProperty("egg").objectReferenceValue = eggsById.TryGetValue(eggId, out var eggAsset) ? eggAsset : null;
                if (!string.IsNullOrWhiteSpace(eggId) && !eggsById.ContainsKey(eggId))
                {
                    Debug.LogWarning($"[CSV Importer] Zone '{zoneId}' references unknown eggId '{eggId}'.");
                }

                // Gate link
                var gateId = Get(row, "gateId", string.Empty);
                so.FindProperty("gate").objectReferenceValue = gatesById.TryGetValue(gateId, out var gateAsset) ? gateAsset : null;
                if (!string.IsNullOrWhiteSpace(gateId) && !gatesById.ContainsKey(gateId))
                {
                    Debug.LogWarning($"[CSV Importer] Zone '{zoneId}' references unknown gateId '{gateId}'.");
                }

                // Breakable spawn configs
                var spawnProp = so.FindProperty("breakableSpawns");
                if (breakablesPerZone.TryGetValue(zoneId.Trim(), out var spawns))
                {
                    spawnProp.arraySize = spawns.Count;
                    for (int i = 0; i < spawns.Count; i++)
                    {
                        var sr = spawns[i];
                        string breakId = Get(sr, "breakableTemplateId", string.Empty);
                        int count = ParseInt(Get(sr, "count", "0"), 0);
                        float respawn = ParseFloat(Get(sr, "respawnSeconds", "0"), 0f);

                        var element = spawnProp.GetArrayElementAtIndex(i);
                        element.FindPropertyRelative("count").intValue = Mathf.Max(0, count);
                        element.FindPropertyRelative("respawnSeconds").floatValue = Mathf.Max(0f, respawn);

                        if (breakablesById.TryGetValue(breakId, out var breakAsset))
                        {
                            element.FindPropertyRelative("template").objectReferenceValue = breakAsset;
                        }
                        else
                        {
                            element.FindPropertyRelative("template").objectReferenceValue = null;
                            Debug.LogWarning($"[CSV Importer] Zone '{zoneId}' spawn references unknown breakableTemplateId '{breakId}'.");
                        }
                    }
                }
                else
                {
                    spawnProp.arraySize = 0;
                }

                so.ApplyModifiedProperties();
            }
        }

        #endregion

        #region CSV Helpers

        private static List<Dictionary<string, string>> ReadCsv(string path)
        {
            var result = new List<Dictionary<string, string>>();
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[CSV Importer] CSV not found: {path}");
                return result;
            }

            var lines = File.ReadAllLines(path);
            if (lines.Length == 0) return result;

            var headers = SplitCsvLine(lines[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = SplitCsvLine(line);
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int h = 0; h < headers.Length && h < cols.Length; h++)
                {
                    row[headers[h]] = cols[h];
                }
                result.Add(row);
            }

            return result;
        }

        private static string[] SplitCsvLine(string line)
        {
            // Simple CSV splitter (no escaped quotes support). Good enough for prototype.
            return line.Split(',').Select(s => s.Trim()).ToArray();
        }

        private static string Get(Dictionary<string, string> row, string key, string fallback)
        {
            return row.TryGetValue(key, out var value) ? value : fallback;
        }

        private static int ParseInt(string value, int fallback)
        {
            return int.TryParse(value, out var v) ? v : fallback;
        }

        private static float ParseFloat(string value, float fallback)
        {
            return float.TryParse(value, out var v) ? v : fallback;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path);
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent.Replace("\\", "/"), folderName);
            }
        }

        private static T GetOrCreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }
            return asset;
        }

        #endregion
    }
}
