using UnityEngine;

namespace PetSimLite.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "PetSimLite/Data/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private int startingCoins = 500;
        [SerializeField] private string startingZoneId = "zone1";
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject uiCanvasPrefab;
        [SerializeField] private GameObject defaultPetPrefab;

        [Header("Gate Defaults")]
        [SerializeField] private Vector3 gateSize = new Vector3(4f, 3f, 0.5f);
        [SerializeField] private string gateLabel = "UNLOCK NEXT ZONE";

        [Header("Player Movement")]
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -20f;

        [Header("Player Animator Params")]
        [SerializeField] private string animatorHorParam = "Hor";
        [SerializeField] private string animatorVertParam = "Vert";
        [SerializeField] private string animatorStateParam = "State";
        [SerializeField] private string animatorIsJumpParam = "IsJump";

        [Header("Camera (Roblox-like)")]
        [SerializeField] private float cameraDistance = 6f;
        [SerializeField] private float cameraMinDistance = 2f;
        [SerializeField] private float cameraMaxDistance = 12f;
        [SerializeField] private float cameraPitch = 18f;
        [SerializeField] private float cameraMinPitch = -10f;
        [SerializeField] private float cameraMaxPitch = 60f;
        [SerializeField] private float cameraSensitivityX = 0.2f;
        [SerializeField] private float cameraSensitivityY = 0.15f;
        [SerializeField] private float cameraFollowLerp = 15f;
        [SerializeField] private float cameraLookOffsetY = 1.6f;
        [SerializeField] private bool cameraRotateOnlyWhileRmbHeld = true;

        public int StartingCoins => startingCoins;
        public string StartingZoneId => startingZoneId;
        public GameObject PlayerPrefab => playerPrefab;
        public GameObject UiCanvasPrefab => uiCanvasPrefab;
        public GameObject DefaultPetPrefab => defaultPetPrefab;
        public Vector3 GateSize => gateSize;
        public string GateLabel => gateLabel;

        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;

        public string AnimatorHorParam => animatorHorParam;
        public string AnimatorVertParam => animatorVertParam;
        public string AnimatorStateParam => animatorStateParam;
        public string AnimatorIsJumpParam => animatorIsJumpParam;

        public float CameraDistance => cameraDistance;
        public float CameraMinDistance => cameraMinDistance;
        public float CameraMaxDistance => cameraMaxDistance;
        public float CameraPitch => cameraPitch;
        public float CameraMinPitch => cameraMinPitch;
        public float CameraMaxPitch => cameraMaxPitch;
        public float CameraSensitivityX => cameraSensitivityX;
        public float CameraSensitivityY => cameraSensitivityY;
        public float CameraFollowLerp => cameraFollowLerp;
        public float CameraLookOffsetY => cameraLookOffsetY;
        public bool CameraRotateOnlyWhileRmbHeld => cameraRotateOnlyWhileRmbHeld;
    }
}
