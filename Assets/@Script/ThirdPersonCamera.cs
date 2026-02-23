using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private Transform target;                  // 추적 대상 (플레이어)
    [SerializeField] private float distance = 7f;               // 플레이어와의 거리
    [SerializeField] private float height = 1.5f;               // 카메라 높이 오프셋
    [SerializeField] private float smoothSpeed = 5f;            // 카메라 추적 부드러움 (작을수록 빠름)

    [Header("마우스 시점 제어")]
    [SerializeField] private float mouseSensitivity = 0.5f;     // 마우스 감도
    [SerializeField] private float minVerticalAngle = -30f;     // 최소 수직 각도 (아래)
    [SerializeField] private float maxVerticalAngle = 60f;      // 최대 수직 각도 (위)

    private float horizontalAngle = 0f;  // 수평 각도 (좌우)
    private float verticalAngle = 15f;   // 수직 각도 (위아래)

    void Start()
    {
        // 플레이어가 할당되지 않았으면 자동 찾기
        if (target == null)
        {
            target = FindObjectOfType<Player>().transform;
        }

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

        // 좌우 회전 (마우스 X) - 부호 반전으로 자연스러운 방향
        horizontalAngle -= mouseDelta.x * mouseSensitivity;

        // 위아래 회전 (마우스 Y)
        verticalAngle -= mouseDelta.y * mouseSensitivity;
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
        // 플레이어 위치 기준점
        Vector3 targetPosition = target.position + Vector3.up * height;

        // 극좌표를 이용한 카메라 위치 계산
        float radHorizontal = horizontalAngle * Mathf.Deg2Rad;
        float radVertical = verticalAngle * Mathf.Deg2Rad;

        // 카메라 오프셋 계산
        Vector3 cameraOffset = new Vector3(
            Mathf.Sin(radHorizontal) * Mathf.Cos(radVertical),
            Mathf.Sin(radVertical),
            -Mathf.Cos(radHorizontal) * Mathf.Cos(radVertical)
        ) * distance;

        Vector3 desiredPosition = targetPosition + cameraOffset;

        // 부드럽게 카메라 이동 (Lerp 사용)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 플레이어를 바라보기 (약간 위 지점)
        transform.LookAt(targetPosition + Vector3.up * 0.5f);
    }
}
