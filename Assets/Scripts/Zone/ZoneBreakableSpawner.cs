using System.Collections.Generic;
using UnityEngine;
using PetSimLite.Data;
using PetSimLite.Resources;

namespace PetSimLite.ZoneGeneration
{
    /// <summary>
    /// Spawns and respawns breakables within a rectangular area based on ZoneData.breakableSpawns.
    /// Attach to same GameObject as ZoneBuilder.
    /// </summary>
    public class ZoneBreakableSpawner : MonoBehaviour
    {
        private class SpawnTracker
        {
            public BreakableSpawnConfig Config;
            public List<Breakable> Active = new List<Breakable>();
            public float RespawnTimer;
        }

        private readonly List<SpawnTracker> _trackers = new List<SpawnTracker>();
        private ZoneData _zoneData;

        public void Configure(ZoneData zoneData)
        {
            _zoneData = zoneData;
            _trackers.Clear();

            if (_zoneData == null || _zoneData.BreakableSpawns == null) return;

            foreach (var config in _zoneData.BreakableSpawns)
            {
                if (config.template == null) continue;
                _trackers.Add(new SpawnTracker
                {
                    Config = config,
                    RespawnTimer = 0f
                });
            }

            // initial spawn
            foreach (var tracker in _trackers)
            {
                for (int i = 0; i < tracker.Config.count; i++)
                {
                    Spawn(tracker);
                }
            }
        }

        private void Update()
        {
            for (int i = 0; i < _trackers.Count; i++)
            {
                var tracker = _trackers[i];
                // cleanup destroyed
                tracker.Active.RemoveAll(b => b == null);

                int deficit = tracker.Config.count - tracker.Active.Count;
                if (deficit <= 0) continue;

                tracker.RespawnTimer -= Time.deltaTime;
                if (tracker.RespawnTimer <= 0f)
                {
                    Spawn(tracker);
                    tracker.RespawnTimer = tracker.Config.respawnSeconds > 0f
                        ? tracker.Config.respawnSeconds
                        : 0.1f;
                }
            }
        }

        private void Spawn(SpawnTracker tracker)
        {
            var template = tracker.Config.template;
            if (template == null || template.Prefab == null) return;

            Vector3 localPos = GetRandomLocalPointInArea(_zoneData.BreakableAreaWidth, _zoneData.BreakableAreaLength);

            var instance = Instantiate(template.Prefab, transform);
            instance.transform.localPosition = localPos;
            instance.transform.localScale *= template.PrefabScale;

            var breakable = instance.GetComponent<Breakable>();
            if (breakable == null)
            {
                breakable = instance.AddComponent<Breakable>();
            }
            breakable.InitializeTemplate(template);

            tracker.Active.Add(breakable);
        }

        private static Vector3 GetRandomLocalPointInArea(float width, float length)
        {
            float x = Random.Range(-width * 0.5f, width * 0.5f);
            float z = Random.Range(-length * 0.5f, length * 0.5f);
            return new Vector3(x, 0f, z);
        }
    }
}
