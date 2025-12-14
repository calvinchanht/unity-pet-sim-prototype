using UnityEngine;
using PetSimLite.Data;

namespace PetSimLite.Player
{
    public class PlayerAnimatorDriver : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Animator animator;

        [Header("Parameters")]
        [SerializeField] private string horParam = "Hor";
        [SerializeField] private string vertParam = "Vert";
        [SerializeField] private string stateParam = "State";
        [SerializeField] private string isJumpParam = "IsJump";

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        private int _horHash;
        private int _vertHash;
        private int _stateHash;
        private int _jumpHash;

        public void Configure(GameSettings settings, PlayerController playerController, Animator targetAnimator)
        {
            if (settings != null)
            {
                horParam = settings.AnimatorHorParam;
                vertParam = settings.AnimatorVertParam;
                stateParam = settings.AnimatorStateParam;
                isJumpParam = settings.AnimatorIsJumpParam;
            }

            player = playerController;
            animator = targetAnimator;
            CacheHashes();
        }

        private void Awake()
        {
            if (player == null) player = GetComponent<PlayerController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            CacheHashes();
        }

        private void CacheHashes()
        {
            _horHash = Animator.StringToHash(horParam);
            _vertHash = Animator.StringToHash(vertParam);
            _stateHash = Animator.StringToHash(stateParam);
            _jumpHash = Animator.StringToHash(isJumpParam);
        }

        private void Update()
        {
            if (player == null || animator == null) return;

            // Roblox-style: character turns to movement direction, so we drive "forward" motion in the animator
            // even when the input was A/D (no strafing animation).
            float speed01 = Mathf.Clamp01(player.MoveInput.magnitude);
            animator.SetFloat(_horHash, 0f);
            animator.SetFloat(_vertHash, speed01);
            animator.SetFloat(_stateHash, player.IsRunning ? 1f : 0f);
            animator.SetBool(_jumpHash, !player.IsGrounded);
        }
    }
}
