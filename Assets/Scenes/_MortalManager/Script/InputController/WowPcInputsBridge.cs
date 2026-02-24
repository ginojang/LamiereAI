using UnityEngine;

[RequireComponent(typeof(StarterAssetsInputs))]
public class WowPcInputsBridge : MonoBehaviour
{
    [SerializeField]
    public MyCharacterCore parentCharacterCore;


    StarterAssetsInputs inputs;
    ThirdPersonController controller; // optional (to toggle isUseCameraAngle)

    [Header("WoW-like Settings")]
    public float turnSpeedDegPerSec = 180f;   // A/D 회전 속도
    public float mouseSensitivity = 1.0f;     // 마우스 감도 (StarterAssets look delta)
    public bool holdRightMouseToLook = true;  // 우클릭 드래그 시 회전


    public void SetParentCharacterCore(MyCharacterCore parentCore)
    {
        parentCharacterCore = parentCore;
    }


    void Start()
    {
        inputs = GetComponent<StarterAssetsInputs>();
        controller = GetComponent<ThirdPersonController>();
    }

    [SerializeField] private float toggleCooldown = 0.2f;
    private float _lastToggleTime = -999f;


    void Update()
    {
        if (!inputs) return;

        if (Input.GetKeyDown(KeyCode.C) && parentCharacterCore != null)
        {
            if (Time.time - _lastToggleTime >= toggleCooldown)
            {
                _lastToggleTime = Time.time;
                parentCharacterCore.ToggleCombatMode();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && parentCharacterCore != null)
        {
            if (Time.time - _lastToggleTime >= toggleCooldown)
            {
                _lastToggleTime = Time.time;
                parentCharacterCore.SetTriggerNormalAttack();
            }
        }



        // =========================
        // 1) 기본 이동 (W/S 전후)
        // =========================
        float forward = 0f;
        if (Input.GetKey(KeyCode.W)) forward += 1f;
        if (Input.GetKey(KeyCode.S)) forward -= 1f;

        // =========================
        // 2) 평행이동 (Q/E 좌우)
        // =========================
        float strafe = 0f;
        if (Input.GetKey(KeyCode.E)) strafe += 1f;
        if (Input.GetKey(KeyCode.Q)) strafe -= 1f;

        // StarterAssetsInputs.move: (x=좌우, y=전후)
        Vector2 move = new Vector2(strafe, forward);
        move = Vector2.ClampMagnitude(move, 1f);
        inputs.MoveInput(move);

        // =========================
        // 3) 점프 (Space) - 누른 프레임만
        // =========================
        inputs.JumpInput(Input.GetKeyDown(KeyCode.Space));

        // (선택) 스프린트: Shift (WoW 기본은 NumLock 자동달리기/토글도 있지만 일단 Shift)
        //inputs.SprintInput(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        // =========================
        // 4) A/D: 좌/우 회전 (키보드 턴)
        // =========================
        float turn = 0f;
        if (Input.GetKey(KeyCode.D)) turn += 1f;
        if (Input.GetKey(KeyCode.A)) turn -= 1f;

        if (Mathf.Abs(turn) > 0.001f)
        {
            float yaw = turn * turnSpeedDegPerSec * Time.deltaTime;
            transform.Rotate(0f, yaw, 0f, Space.World);
        }

        // =========================
        // 5) 마우스 우클릭 드래그:
        //    카메라 + 캐릭터 방향 함께 회전 (WoW 느낌)
        // =========================
        bool allowLook = !holdRightMouseToLook || Input.GetMouseButton(1);

        if (allowLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mx = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            // Cinemachine 타겟 회전용 look delta 제공
            inputs.LookInput(new Vector2(mx, my));

            // 우클릭 중에는 "카메라 각도 기준 이동"을 쓰는 편이 WoW 느낌
            if (controller) controller.isUseCameraAngle = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            inputs.LookInput(Vector2.zero);

            // 우클릭이 아닐 땐 카메라각 참조를 끄면 더 WoW스러움(선택)
            if (controller) controller.isUseCameraAngle = false;
        }
    }
}
