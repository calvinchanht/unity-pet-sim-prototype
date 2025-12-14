using UnityEngine;
using UnityEngine.InputSystem;
using PetSimLite.Data;

namespace PetSimLite.Player
{
    /// <summary>
    /// Roblox-style movement: WASD + Space jump + Shift run. Moves relative to the camera.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float turnSpeed = 18f;

        private CharacterController _characterController;
        private Camera _mainCamera;
        private float _verticalVelocity;
        private bool _jumpRequested;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        public Vector2 MoveInput { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsGrounded => _characterController != null && _characterController.isGrounded;

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

            ReadInput();
            Move(Time.deltaTime);
        }

        public void Configure(GameSettings settings)
        {
            if (settings == null) return;
            walkSpeed = settings.WalkSpeed;
            runSpeed = settings.RunSpeed;
            jumpHeight = settings.JumpHeight;
            gravity = settings.Gravity;
        }

        private void ReadInput()
        {
            if (Keyboard.current == null)
            {
                MoveInput = Vector2.zero;
                IsRunning = false;
                _jumpRequested = false;
                return;
            }

            float x = 0f;
            float y = 0f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;
            if (Keyboard.current.wKey.isPressed) y += 1f;

            MoveInput = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            IsRunning = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            _jumpRequested = Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        private void Move(float deltaTime)
        {
            Vector3 camForward = Vector3.forward;
            Vector3 camRight = Vector3.right;
            if (_mainCamera != null)
            {
                camForward = _mainCamera.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();

                camRight = _mainCamera.transform.right;
                camRight.y = 0f;
                camRight.Normalize();
            }

            Vector3 desiredMove = camRight * MoveInput.x + camForward * MoveInput.y;
            if (desiredMove.sqrMagnitude > 0.0001f)
            {
                desiredMove.Normalize();
                var targetRotation = Quaternion.LookRotation(desiredMove, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * deltaTime);
            }

            float speed = IsRunning ? runSpeed : walkSpeed;
            Vector3 horizontal = desiredMove * speed;

            if (IsGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f; // small downward force to keep grounded
            }

            if (IsGrounded && _jumpRequested)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (enableDebugLogs) Debug.Log("[Player] Jump");
            }

            _verticalVelocity += gravity * deltaTime;

            Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;
            _characterController.Move(velocity * deltaTime);

        }
    }
}
