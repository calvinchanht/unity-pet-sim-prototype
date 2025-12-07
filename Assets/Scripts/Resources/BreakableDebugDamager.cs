// Debug-only helper for editor playtesting.
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

namespace PetSimLite.Resources
{
    /// <summary>
    /// Temporary helper: raycasts from camera on left click and applies damage to Breakable.
    /// </summary>
    public class BreakableDebugDamager : MonoBehaviour
    {
        [SerializeField] private int damagePerClick = 3;
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private LayerMask hitMask = ~0;

        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null || _cam == null)
            {
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out var hit, maxDistance, hitMask))
                {
                    var breakable = hit.collider.GetComponentInParent<Breakable>();
                    if (breakable != null)
                    {
                        breakable.ApplyDamage(damagePerClick);
                    }
                }
            }
        }
    }
}
#endif
