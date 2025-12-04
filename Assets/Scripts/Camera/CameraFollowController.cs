using UnityEngine;
using UnityEngine.InputSystem;

namespace PetSimLite.CameraSystem
{
    /// <summary>
    /// Follows the player with gesture-inspired rotate (Shift+drag) and zoom (Ctrl+drag).
    /// </summary>
    public class CameraFollowController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followLerp = 10f;
        [SerializeField] private float distance = 10f;
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 18f;
        [SerializeField] private float pitch = 45f;
        [SerializeField] private float minPitch = 20f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private float yaw = 0f;
        [SerializeField] private float rotateSensitivity = 0.15f;
        [SerializeField] private float zoomSensitivity = 0.03f;

        private Vector3 _currentVelocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            HandleInput();
            UpdateCameraPosition();
        }

        private void HandleInput()
        {
            if (Mouse.current == null || Keyboard.current == null)
            {
                return;
            }

            bool leftMouseHeld = Mouse.current.leftButton.isPressed;
            bool shiftHeld = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            bool ctrlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float mouseX = mouseDelta.x;
            float mouseY = mouseDelta.y;

            if (leftMouseHeld && shiftHeld)
            {
                yaw += mouseX * rotateSensitivity * 200f;
                pitch -= mouseY * rotateSensitivity * 200f;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            if (leftMouseHeld && ctrlHeld)
            {
                distance -= mouseY * zoomSensitivity * distance;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }

        private void UpdateCameraPosition()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredOffset = rotation * (Vector3.back * distance);
            Vector3 desiredPosition = target.position + desiredOffset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, followLerp * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), followLerp * Time.deltaTime);
        }
    }
}
