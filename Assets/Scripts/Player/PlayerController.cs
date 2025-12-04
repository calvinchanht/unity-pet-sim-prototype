using UnityEngine;
using UnityEngine.InputSystem;

namespace PetSimLite.Player
{
    /// <summary>
    /// Handles drag-to-move (mobile-style) and optional WASD for desktop testing.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private bool enableKeyboardFallback = true;
        [SerializeField] private float dragDeadzonePixels = 4f;

        private CharacterController _characterController;
        private Camera _mainCamera;
        private Vector3 _dragStart;
        private bool _isDragging;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            Vector3 moveDirection = Vector3.zero;

            // Gesture-inspired drag input (ignored while using camera modifier keys).
            if (!IsCameraModifierHeld())
            {
                moveDirection = GetDragDirection();
            }

            // Optional desktop fallback.
            if (enableKeyboardFallback)
            {
                Vector3 keyboard = GetKeyboardInput();
                if (keyboard.sqrMagnitude > 0f)
                {
                    moveDirection = keyboard;
                }
            }

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                moveDirection.Normalize();
                _characterController.SimpleMove(moveDirection * moveSpeed);
                transform.forward = Vector3.Lerp(transform.forward, moveDirection, 10f * Time.deltaTime);
            }
        }

        private Vector3 GetKeyboardInput()
        {
            if (Keyboard.current == null)
            {
                return Vector3.zero;
            }

            float h = 0f;
            float v = 0f;
            if (Keyboard.current.aKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed) h += 1f;
            if (Keyboard.current.sKey.isPressed) v -= 1f;
            if (Keyboard.current.wKey.isPressed) v += 1f;

            return new Vector3(h, 0f, v);
        }

        private Vector3 GetDragDirection()
        {
            if (Mouse.current == null)
            {
                return Vector3.zero;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _dragStart = Mouse.current.position.ReadValue();
                _isDragging = true;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }

            if (!_isDragging || _mainCamera == null)
            {
                return Vector3.zero;
            }

            Vector2 delta = Mouse.current.position.ReadValue() - (Vector2)_dragStart;
            if (delta.sqrMagnitude < dragDeadzonePixels * dragDeadzonePixels)
            {
                return Vector3.zero;
            }

            Vector3 camForward = _mainCamera.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = _mainCamera.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            Vector3 worldDirection = camRight * delta.x + camForward * delta.y;
            return worldDirection;
        }

        private bool IsCameraModifierHeld()
        {
            if (Keyboard.current == null)
            {
                return false;
            }

            return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed ||
                   Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
        }
    }
}
