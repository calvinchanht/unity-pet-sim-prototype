using System.Collections.Generic;
using UnityEngine;
using PetSimLite.Data;

namespace PetSimLite.ZoneGeneration
{
    /// <summary>
    /// Builds zones in sequence with automatic positioning based on ZoneData sizes and nextDirection.
    /// </summary>
    public class ZoneSequencer : MonoBehaviour
    {
        [SerializeField] private float gapBetweenZones = 2f;

        public void BuildFromList(List<ZoneData> zonesInOrder, Vector3 gateSizeBase, string gateLabel)
        {
            if (zonesInOrder == null || zonesInOrder.Count == 0)
            {
                Debug.LogWarning("[ZoneSequencer] No zones assigned.");
                return;
            }

            Vector3 currentPos = Vector3.zero;
            ZoneDirection entryDir = ZoneDirection.None;

            for (int i = 0; i < zonesInOrder.Count; i++)
            {
                var zone = zonesInOrder[i];
                if (zone == null) continue;

                var go = new GameObject($"Zone_{zone.ZoneId}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = currentPos;

                ZoneData next = (i < zonesInOrder.Count - 1) ? zonesInOrder[i + 1] : null;
                float nextWidth = next != null ? next.Width : zone.Width;
                float nextLength = next != null ? next.Length : zone.Length;

                var builder = go.AddComponent<ZoneBuilder>();
                builder.Configure(zone, entryDir, zone.NextDirection, gateSizeBase, gateLabel, nextWidth, nextLength);
                builder.BuildWithConnections(entryDir, zone.NextDirection);

                if (zone.NextDirection != ZoneDirection.None)
                {
                    float currentExtent = GetExtent(zone, zone.NextDirection);
                    float nextExtent = next != null ? GetExtent(next, zone.NextDirection) : currentExtent;
                    currentPos += DirectionToVector(zone.NextDirection) * (currentExtent + nextExtent + gapBetweenZones);
                    entryDir = Opposite(zone.NextDirection);
                }
            }
        }

        private float GetExtent(ZoneData zone, ZoneDirection dir)
        {
            return dir == ZoneDirection.East || dir == ZoneDirection.West ? zone.Width * 0.5f : zone.Length * 0.5f;
        }

        private Vector3 DirectionToVector(ZoneDirection dir)
        {
            switch (dir)
            {
                case ZoneDirection.North: return new Vector3(0, 0, 1);
                case ZoneDirection.South: return new Vector3(0, 0, -1);
                case ZoneDirection.East: return new Vector3(1, 0, 0);
                case ZoneDirection.West: return new Vector3(-1, 0, 0);
                default: return Vector3.zero;
            }
        }

        private ZoneDirection Opposite(ZoneDirection dir)
        {
            switch (dir)
            {
                case ZoneDirection.North: return ZoneDirection.South;
                case ZoneDirection.South: return ZoneDirection.North;
                case ZoneDirection.East: return ZoneDirection.West;
                case ZoneDirection.West: return ZoneDirection.East;
                default: return ZoneDirection.None;
            }
        }
    }
}
