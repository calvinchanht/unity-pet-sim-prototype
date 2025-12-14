using UnityEngine;
using UnityEngine.InputSystem;
using PetSimLite.Data;

namespace PetSimLite.CameraSystem
{
    public class RobloxThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float yaw;
        [SerializeField] private float pitch;
        [SerializeField] private float distance;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        private float _minDistance;
        private float _maxDistance;
        private float _minPitch;
        private float _maxPitch;
        private float _sensitivityX;
        private float _sensitivityY;
        private float _followLerp;
        private float _lookOffsetY;
        private bool _rotateOnlyWhileRmbHeld;

        public void Configure(GameSettings settings, Transform followTarget)
        {
            if (settings == null) return;

            target = followTarget;
            distance = settings.CameraDistance;
            pitch = settings.CameraPitch;

            _minDistance = settings.CameraMinDistance;
            _maxDistance = settings.CameraMaxDistance;
            _minPitch = settings.CameraMinPitch;
            _maxPitch = settings.CameraMaxPitch;
            _sensitivityX = settings.CameraSensitivityX;
            _sensitivityY = settings.CameraSensitivityY;
            _followLerp = settings.CameraFollowLerp;
            _lookOffsetY = settings.CameraLookOffsetY;
            _rotateOnlyWhileRmbHeld = settings.CameraRotateOnlyWhileRmbHeld;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            HandleInput();
            UpdateTransform();
        }

        private void HandleInput()
        {
            if (Mouse.current == null)
            {
                return;
            }

            bool allowRotate = !_rotateOnlyWhileRmbHeld || Mouse.current.rightButton.isPressed;
            if (allowRotate)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                yaw += delta.x * _sensitivityX;
                pitch -= delta.y * _sensitivityY;
                pitch = Mathf.Clamp(pitch, _minPitch, _maxPitch);
            }

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance -= scroll * 0.01f;
                distance = Mathf.Clamp(distance, _minDistance, _maxDistance);
            }
        }

        private void UpdateTransform()
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 lookPoint = target.position + Vector3.up * _lookOffsetY;
            Vector3 desiredPos = lookPoint + rot * (Vector3.back * distance);

            transform.position = Vector3.Lerp(transform.position, desiredPos, _followLerp * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookPoint - transform.position), _followLerp * Time.deltaTime);
        }
    }
}

