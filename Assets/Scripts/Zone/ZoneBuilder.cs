using UnityEngine;
using PetSimLite.Data;
using PetSimLite.Zone;

namespace PetSimLite.ZoneGeneration
{
    /// <summary>
    /// Dynamically builds floor/walls/gate/egg and spawner based on ZoneData.
    /// Can be configured with entry/exit directions to open walls and place gates accordingly.
    /// </summary>
    public class ZoneBuilder : MonoBehaviour
    {
        [SerializeField] private ZoneData zoneData;
        [SerializeField] private bool buildOnAwake = false;

        private ZoneDirection _entryDirection = ZoneDirection.None;
        private ZoneDirection _exitDirection = ZoneDirection.None;
        private Vector3 _gateBaseSize = new Vector3(4f, 3f, 0.5f);
        private string _gateLabel = "UNLOCK NEXT ZONE";
        private float _nextZoneWidth;
        private float _nextZoneLength;

        private void Awake()
        {
            if (buildOnAwake && zoneData != null)
            {
                BuildWithConnections(_entryDirection, zoneData != null ? zoneData.NextDirection : ZoneDirection.None);
            }
        }

        public void Configure(ZoneData data, ZoneDirection entry, ZoneDirection exit, Vector3 gateSizeOverride, string gateLabelOverride, float nextZoneWidth, float nextZoneLength)
        {
            zoneData = data;
            _entryDirection = entry;
            _exitDirection = exit;
            _gateBaseSize = gateSizeOverride;
            _gateLabel = gateLabelOverride;
            _nextZoneWidth = nextZoneWidth;
            _nextZoneLength = nextZoneLength;
        }

        public void BuildWithConnections(ZoneDirection entryDirection, ZoneDirection exitDirection)
        {
            if (zoneData == null)
            {
                Debug.LogWarning("[ZoneBuilder] No ZoneData assigned.");
                return;
            }

            _entryDirection = entryDirection;
            _exitDirection = exitDirection;
            if (_nextZoneWidth <= 0f) _nextZoneWidth = zoneData.Width;
            if (_nextZoneLength <= 0f) _nextZoneLength = zoneData.Length;

            BuildFloor();
            BuildWalls();
            BuildGate();
            BuildEgg();
            EnsureBreakableSpawner();
        }

        private void BuildFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = $"{zoneData.ZoneId}_Floor";
            floor.transform.SetParent(transform, false);
            floor.transform.localScale = new Vector3(zoneData.Width / 10f, 1f, zoneData.Length / 10f); // Plane is 10x10
            floor.transform.localPosition = Vector3.zero;

            var renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateSimpleMaterial(zoneData.FloorColor);
            }

            var col = floor.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        private void BuildWalls()
        {
            float w = zoneData.Width;
            float l = zoneData.Length;
            float thickness = 0.5f;
            float height = 2f;

            if (_entryDirection != ZoneDirection.North && _exitDirection != ZoneDirection.North)
                CreateWall(new Vector3(0, height / 2f, l / 2f), new Vector3(w, height, thickness)); // north
            if (_entryDirection != ZoneDirection.South && _exitDirection != ZoneDirection.South)
                CreateWall(new Vector3(0, height / 2f, -l / 2f), new Vector3(w, height, thickness)); // south
            if (_entryDirection != ZoneDirection.East && _exitDirection != ZoneDirection.East)
                CreateWall(new Vector3(w / 2f, height / 2f, 0), new Vector3(thickness, height, l)); // east
            if (_entryDirection != ZoneDirection.West && _exitDirection != ZoneDirection.West)
                CreateWall(new Vector3(-w / 2f, height / 2f, 0), new Vector3(thickness, height, l)); // west

            // Handle exit side with gate and fillers
            if (_exitDirection != ZoneDirection.None)
            {
                float boundarySpan = (_exitDirection == ZoneDirection.East || _exitDirection == ZoneDirection.West) ? l : w;
                float neighborSpan = (_exitDirection == ZoneDirection.East || _exitDirection == ZoneDirection.West) ? _nextZoneLength : _nextZoneWidth;
                float gateSpan = Mathf.Min(boundarySpan, neighborSpan);
                float remaining = Mathf.Max(0f, boundarySpan - gateSpan);
                float sideSpan = remaining * 0.5f;
                float heightHalf = height * 0.5f;
                float thicknessHalf = thickness * 0.5f;
                if (_exitDirection == ZoneDirection.North || _exitDirection == ZoneDirection.South)
                {
                    float z = (_exitDirection == ZoneDirection.North) ? l * 0.5f : -l * 0.5f;
                    // left filler
                    if (sideSpan > 0.001f)
                        CreateWall(new Vector3(-gateSpan * 0.5f - sideSpan * 0.5f, heightHalf, z), new Vector3(sideSpan, height, thickness));
                    // right filler
                    if (sideSpan > 0.001f)
                        CreateWall(new Vector3(gateSpan * 0.5f + sideSpan * 0.5f, heightHalf, z), new Vector3(sideSpan, height, thickness));
                }
                else
                {
                    float x = (_exitDirection == ZoneDirection.East) ? w * 0.5f : -w * 0.5f;
                    if (sideSpan > 0.001f)
                        CreateWall(new Vector3(x, heightHalf, -gateSpan * 0.5f - sideSpan * 0.5f), new Vector3(thickness, height, sideSpan));
                    if (sideSpan > 0.001f)
                        CreateWall(new Vector3(x, heightHalf, gateSpan * 0.5f + sideSpan * 0.5f), new Vector3(thickness, height, sideSpan));
                }
            }
        }

        private void CreateWall(Vector3 localPos, Vector3 size)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = localPos;
            wall.transform.localScale = size;

            var renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateSimpleMaterial(zoneData.WallColor);
            }
        }

        private void BuildGate()
        {
            if (zoneData.Gate == null || _exitDirection == ZoneDirection.None)
            {
                return;
            }

            float boundarySpan = (_exitDirection == ZoneDirection.East || _exitDirection == ZoneDirection.West) ? zoneData.Length : zoneData.Width;
            float neighborSpan = (_exitDirection == ZoneDirection.East || _exitDirection == ZoneDirection.West) ? _nextZoneLength : _nextZoneWidth;
            float gateSpan = Mathf.Min(boundarySpan, neighborSpan);

            var gateObj = new GameObject($"{zoneData.ZoneId}_Gate");
            gateObj.transform.SetParent(transform, false);
            gateObj.transform.localPosition = GetBoundaryPosition(_exitDirection, _gateBaseSize.y * 0.5f);

            var collider = gateObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            Vector3 colliderSize = (_exitDirection == ZoneDirection.East || _exitDirection == ZoneDirection.West)
                ? new Vector3(_gateBaseSize.z, _gateBaseSize.y, gateSpan)
                : new Vector3(gateSpan, _gateBaseSize.y, _gateBaseSize.z);
            collider.size = colliderSize;

            BuildGateVisuals(gateObj.transform, colliderSize, _exitDirection);

            var controller = gateObj.AddComponent<GateController>();
            controller.SetGateData(zoneData.Gate);
        }

        private void BuildGateVisuals(Transform parent, Vector3 colliderSize, ZoneDirection dir)
        {
            // pillars
            bool vertical = dir == ZoneDirection.East || dir == ZoneDirection.West;
            float height = colliderSize.y;
            float span = vertical ? colliderSize.z : colliderSize.x;
            float thickness = vertical ? colliderSize.x : colliderSize.z;
            float pillarThickness = Mathf.Max(0.2f, span * 0.1f);
            float halfSpan = span * 0.5f;

            Vector3 pillarScale = vertical
                ? new Vector3(thickness, height, pillarThickness)
                : new Vector3(pillarThickness, height, thickness);
            Vector3 beamScale = vertical
                ? new Vector3(thickness, pillarThickness, span)
                : new Vector3(span, pillarThickness, thickness);

            if (vertical)
            {
                CreateGatePart("Pillar_L", parent, new Vector3(0f, height * 0.5f, -halfSpan + pillarThickness * 0.5f), pillarScale);
                CreateGatePart("Pillar_R", parent, new Vector3(0f, height * 0.5f, halfSpan - pillarThickness * 0.5f), pillarScale);
                CreateGatePart("Beam", parent, new Vector3(0f, height, 0f), beamScale);
            }
            else
            {
                CreateGatePart("Pillar_L", parent, new Vector3(-halfSpan + pillarThickness * 0.5f, height * 0.5f, 0f), pillarScale);
                CreateGatePart("Pillar_R", parent, new Vector3(halfSpan - pillarThickness * 0.5f, height * 0.5f, 0f), pillarScale);
                CreateGatePart("Beam", parent, new Vector3(0f, height, 0f), beamScale);
            }

            // sign quad with text
            var sign = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sign.name = "GateSign";
            sign.transform.SetParent(parent, false);
            float signDepth = thickness * 0.6f;
            sign.transform.localPosition = new Vector3(0f, height * 0.6f, vertical ? signDepth : signDepth);
            sign.transform.localScale = new Vector3(span * 0.5f, height * 0.3f, 1f);

            if (dir == ZoneDirection.North)
                sign.transform.localRotation = Quaternion.identity;
            else if (dir == ZoneDirection.South)
                sign.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            else if (dir == ZoneDirection.East)
                sign.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            else if (dir == ZoneDirection.West)
                sign.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            var signRenderer = sign.GetComponent<Renderer>();
            if (signRenderer != null)
            {
                signRenderer.sharedMaterial = CreateSimpleMaterial(Color.black * 0.3f);
            }

#if TMP_PRESENT
            var tmp = sign.AddComponent<TMPro.TextMeshPro>();
            tmp.text = $"{_gateLabel}\n{zoneData.Gate.RequiredCoins} COINS";
            tmp.fontSize = 4;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.rectTransform.sizeDelta = Vector2.zero;
#endif
        }

        private void CreateGatePart(string name, Transform parent, Vector3 localPos, Vector3 scale)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPos;
            part.transform.localScale = scale;
            var renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateSimpleMaterial(zoneData.WallColor);
            }
            var col = part.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        private void BuildEgg()
        {
            if (zoneData.Egg == null)
            {
                return;
            }

            GameObject eggObj = null;
            if (zoneData.Egg.Prefab != null)
            {
                eggObj = Instantiate(zoneData.Egg.Prefab, transform);
                eggObj.transform.localScale *= zoneData.Egg.PrefabScale;
            }
            else
            {
                eggObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eggObj.transform.SetParent(transform, false);
                eggObj.transform.localScale = Vector3.one * 1.5f;
            }

            eggObj.name = $"{zoneData.ZoneId}_Egg";
            eggObj.transform.localPosition = Vector3.zero;

            var eggCollider = eggObj.GetComponent<Collider>();
            if (eggCollider == null)
            {
                eggCollider = eggObj.AddComponent<SphereCollider>();
            }
            eggCollider.isTrigger = true;

            var roller = eggObj.GetComponent<EggRoller>();
            if (roller == null)
            {
                roller = eggObj.AddComponent<EggRoller>();
            }
            roller.SetEggData(zoneData.Egg);
        }

        private void EnsureBreakableSpawner()
        {
            var spawner = GetComponent<ZoneBreakableSpawner>();
            if (spawner == null)
            {
                spawner = gameObject.AddComponent<ZoneBreakableSpawner>();
            }
            spawner.Configure(zoneData);
        }

        private Vector3 GetBoundaryPosition(ZoneDirection dir, float y)
        {
            float halfW = zoneData.Width * 0.5f;
            float halfL = zoneData.Length * 0.5f;
            switch (dir)
            {
                case ZoneDirection.North:
                    return new Vector3(0, y, halfL);
                case ZoneDirection.South:
                    return new Vector3(0, y, -halfL);
                case ZoneDirection.East:
                    return new Vector3(halfW, y, 0);
                case ZoneDirection.West:
                    return new Vector3(-halfW, y, 0);
                default:
                    return Vector3.zero;
            }
        }

        private Material CreateSimpleMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Glossiness", 0f);
            return mat;
        }
    }
}
