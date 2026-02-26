using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("카메라 위치 설정 (플레이어 기준)")]
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0, 3f, -7f);  // X, Y, Z 오프셋

    [Header("카메라 회전 설정")]
    [SerializeField] private Vector3 cameraLocalRotation = new Vector3(10f, 0, 0);   // X, Y, Z 회전

    [Header("마우스 시점 제어")]
    [SerializeField] private float horizontalSensitivity = 0.25f;   // 좌우 감도
    [SerializeField] private float minHorizontalAngle = -45f;    // 좌우 최소 (-45도)
    [SerializeField] private float maxHorizontalAngle = 45f;     // 좌우 최대 (45도)
    [SerializeField] private float minVerticalAngle = -45f;      // 아래 제한 (-45도까지 아래)
    [SerializeField] private float maxVerticalAngle = 45f;       // 위 제한 (45도까지 위)

    private Transform target;                 // 플레이어
    private float horizontalAngle = 0f;      // 좌우 각도
    private float verticalAngle = 0f;        // 상하 각도 (위만 가능)

    void Start()
    {
        // 플레이어 찾기 (자동 할당)
        target = FindObjectOfType<Player>().transform;
        
        // 카메라를 플레이어의 자식 오브젝트로 설정 (따라다니게)
        transform.SetParent(target);
        
        // 커서 잠금 (게임 중 마우스 커서 숨김)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        HandleMouseInput();
        UpdateCameraPosition();
    }

    private void HandleMouseInput()
    {
        // 마우스 입력 감지
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // 좌우 회전 (마우스 X) - -45 ~ 45도
        horizontalAngle += mouseDelta.x * horizontalSensitivity;
        horizontalAngle = Mathf.Clamp(horizontalAngle, minHorizontalAngle, maxHorizontalAngle);

        // 상하 회전 (마우스 Y) - 위아래 모두 볼 수 있음 (-45 ~ 45도)
        verticalAngle -= mouseDelta.y * horizontalSensitivity * 0.5f;
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

        // ESC 키로 커서 해제/잠금 토글
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
        }
    }

    private void UpdateCameraPosition()
    {
        // 플레이어를 카메라의 좌우 각도에 맞춰 회전
        target.localRotation = Quaternion.Euler(0, horizontalAngle, 0);
        
        // 기본 카메라 위치 설정 (Inspector에서 수정 가능)
        transform.localPosition = cameraLocalPosition;

        // 기본 회전에 마우스 입력 각도 추가
        Vector3 finalRotation = cameraLocalRotation;
        finalRotation.y += horizontalAngle;  // 좌우 회전
        finalRotation.x += verticalAngle;    // 상하 회전 (위만)

        // 카메라 회전 적용
        transform.localRotation = Quaternion.Euler(finalRotation);
    }
}



