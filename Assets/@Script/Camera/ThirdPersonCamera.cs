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

    [Header("카메라 추적 설정")]
    [SerializeField] private Vector3 dampingFactor = new Vector3(5f, 5f, 5f);  // 카메라가 얼마나 부드럽게 따라다닐지 (클수록 빠름)
    [SerializeField] private Vector3 hardLimitsMin = new Vector3(-20f, -5f, -30f);  // 카메라 위치 최소값
    [SerializeField] private Vector3 hardLimitsMax = new Vector3(20f, 20f, 10f);    // 카메라 위치 최대값
    [SerializeField] private Vector3 deadZone = new Vector3(2f, 1.5f, 2f);          // 불감대 (이 범위 내면 카메라 움직이지 않음)
    [SerializeField] private bool useHardLimits = true;  // Hard Limits 활성화 여부
    [SerializeField] private bool useDeadZone = true;    // Dead Zone 활성화 여부

    private Transform target;                 // 플레이어
    private Vector3 cameraTargetPosition;     // 카메라 목표 위치
    private float horizontalAngle = 0f;      // 좌우 각도
    private float verticalAngle = 0f;        // 상하 각도 (위만 가능)

    void Start()
    {
        // 플레이어 찾기 (자동 할당)
        target = FindObjectOfType<Player>().transform;
        
        // 카메라 목표 위치 초기화
        cameraTargetPosition = transform.position;
        
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
        // 플레이어를 기준으로 목표 위치 계산
        Vector3 playerPos = target.position;
        
        // 플레이어의 회전을 고려하여 오프셋 위치 계산 (플레이어의 현재 회전만 사용)
        Vector3 offsetPosition = target.TransformDirection(cameraLocalPosition);
        Vector3 desiredPosition = playerPos + offsetPosition;
        
        // Dead Zone 적용
        if (useDeadZone)
        {
            Vector3 cameraDifference = desiredPosition - cameraTargetPosition;
            
            // Dead Zone 범위 내면 이동하지 않음
            if (Mathf.Abs(cameraDifference.x) < deadZone.x)
                desiredPosition.x = cameraTargetPosition.x;
            if (Mathf.Abs(cameraDifference.y) < deadZone.y)
                desiredPosition.y = cameraTargetPosition.y;
            if (Mathf.Abs(cameraDifference.z) < deadZone.z)
                desiredPosition.z = cameraTargetPosition.z;
        }
        
        // Damping 적용 (부드러운 따라다니기) - 축별로 개별 적용
        cameraTargetPosition.x = Mathf.Lerp(cameraTargetPosition.x, desiredPosition.x, dampingFactor.x * Time.deltaTime);
        cameraTargetPosition.y = Mathf.Lerp(cameraTargetPosition.y, desiredPosition.y, dampingFactor.y * Time.deltaTime);
        cameraTargetPosition.z = Mathf.Lerp(cameraTargetPosition.z, desiredPosition.z, dampingFactor.z * Time.deltaTime);
        
        // Hard Limits 적용 (카메라 이동 범위 제한)
        if (useHardLimits)
        {
            cameraTargetPosition.x = Mathf.Clamp(cameraTargetPosition.x, playerPos.x + hardLimitsMin.x, playerPos.x + hardLimitsMax.x);
            cameraTargetPosition.y = Mathf.Clamp(cameraTargetPosition.y, playerPos.y + hardLimitsMin.y, playerPos.y + hardLimitsMax.y);
            cameraTargetPosition.z = Mathf.Clamp(cameraTargetPosition.z, playerPos.z + hardLimitsMin.z, playerPos.z + hardLimitsMax.z);
        }
        
        // 카메라 위치 업데이트
        transform.position = cameraTargetPosition;
        
        // 기본 회전에 마우스 입력 각도 추가 (카메라 회전만)
        Vector3 finalRotation = cameraLocalRotation;
        finalRotation.y += horizontalAngle;  // 좌우 회전
        finalRotation.x += verticalAngle;    // 상하 회전

        // 카메라 회전 적용
        transform.localRotation = Quaternion.Euler(finalRotation);
    }
}



