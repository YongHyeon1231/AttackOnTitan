using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // 와이어 상태 정의
    private enum WireState { Inactive, Launched, Pulling }

    [Header("입체기동장치")]
    [SerializeField] private Transform leftGear;        // 왼쪽 발사부
    [SerializeField] private Transform rightGear;       // 오른쪽 발사부
    [SerializeField] private float pullForce = 5f;     // 당기는 힘의 크기 (기본값 20, 작을수록 느림)
    [SerializeField] private float maxWireDistance = 100f; // 와이어 최대 거리
    [SerializeField] private float wireSpeed = 0.05f;    // 와이어 채워지는 속도 (초, 작을수록 빠름. 기본값 0.05)
    [SerializeField] private LayerMask hookableLayer;   // 와이어가 고정될 수 있는 레이어

    [Header("가스 분출")]
    [SerializeField] private float gasBoostForce = 5f;   // 가스 분출 힘
    [SerializeField] private float gasBoostCooldown = 0.2f; // 가스 분출 쿨타임 (초)
    [SerializeField] private float maxGasBoostDuration = 2f; // 최대 가스 분출 지속 시간 (초)

    private Rigidbody rb;
    private LineRenderer leftWireVisual;
    private LineRenderer rightWireVisual;

    // 왼쪽 와이어
    private WireState leftWireState = WireState.Inactive;
    private bool leftWireHooked = false;  // 특정 Layer에 고정되었는지
    private Vector3 leftWireTarget;
    private float leftWireLaunchTime = 0f; // 발사 시작 시간
    private Vector3 leftWireStart;         // 발사 시작 위치

    // 오른쪽 와이어
    private WireState rightWireState = WireState.Inactive;
    private bool rightWireHooked = false; // 특정 Layer에 고정되었는지
    private Vector3 rightWireTarget;
    private float rightWireLaunchTime = 0f; // 발사 시작 시간
    private Vector3 rightWireStart;        // 발사 시작 위치

    // 가스 분출
    private float lastGasBoostTime = -999f; // 마지막 가스 분출 시간
    private float gasBoostStartTime = 0f;   // 현재 가스 분출 시작 시간
    private bool isGasBoosting = false;     // 현재 가스 분출 중인지

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        leftWireVisual = CreateWireVisual("LeftWire", new Color(0.8f, 0f, 1f, 1f)); // 보라색
        rightWireVisual = CreateWireVisual("RightWire", new Color(0f, 0.6f, 1f, 1f)); // 파랑색
    }

    void Update()
    {
        HandleInput();
        HandleGasBoost();
        CheckWireCollision();
        UpdateVisuals();
    }

    void FixedUpdate()
    {
        ApplyWireForce();
        ApplyGasBoost();
    }

    private void HandleInput()
    {
        // 마우스 왼쪽(1번) - 왼쪽 와이어 상태 전환
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("왼쪽 클릭 감지!");
            ProgressLeftWireState();
        }

        // 마우스 오른쪽(2번) - 오른쪽 와이어 상태 전환
        // InputSystem 대체 - 레거시 Input 사용
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("오른쪽 클릭 감지! (레거시 Input)");
            ProgressRightWireState();
        }
    }

    private void HandleGasBoost()
    {
        // 스페이스바 입력 감지 (눌린 상태 확인)
        if (Keyboard.current.spaceKey.isPressed)
        {
            // 가스 분출 중이 아니면 시작
            if (!isGasBoosting)
            {
                // 쿨타임 확인
                if (Time.time - lastGasBoostTime >= gasBoostCooldown)
                {
                    isGasBoosting = true;
                    gasBoostStartTime = Time.time;
                    lastGasBoostTime = Time.time;
                    Debug.Log("가스 분출 시작!");
                }
            }
        }
        else
        {
            // 스페이스바 떼어짐
            if (isGasBoosting)
            {
                isGasBoosting = false;
                Debug.Log("가스 분출 종료!");
            }
        }

        // 최대 지속 시간 초과 시 종료
        if (isGasBoosting && Time.time - gasBoostStartTime >= maxGasBoostDuration)
        {
            isGasBoosting = false;
            Debug.Log("가스 분출 최대 시간 도달!");
        }
    }

    private void ProgressLeftWireState()
    {
        if (leftGear == null) return;

        switch (leftWireState)
        {
            case WireState.Inactive:
                // 상태 1: 발사
                leftWireState = WireState.Launched;
                leftWireHooked = false;
                leftWireStart = leftGear.position;
                leftWireTarget = GetWireTarget();
                leftWireLaunchTime = Time.time; // 발사 시간 기록
                Debug.Log("왼쪽 와이어 발사!");
                break;

            case WireState.Launched:
                // 상태 2: 당기기
                leftWireState = WireState.Pulling;
                Debug.Log("왼쪽 와이어 당기기 시작!");
                break;

            case WireState.Pulling:
                // 상태 3: 해제
                leftWireState = WireState.Inactive;
                leftWireHooked = false;
                Debug.Log("왼쪽 와이어 해제!");
                break;
        }
    }

    private void ProgressRightWireState()
    {
        if (rightGear == null) return;

        switch (rightWireState)
        {
            case WireState.Inactive:
                // 상태 1: 발사
                rightWireState = WireState.Launched;
                rightWireHooked = false;
                rightWireStart = rightGear.position;
                rightWireTarget = GetWireTarget();
                rightWireLaunchTime = Time.time; // 발사 시간 기록
                Debug.Log("오른쪽 와이어 발사!");
                break;

            case WireState.Launched:
                // 상태 2: 당기기
                rightWireState = WireState.Pulling;
                Debug.Log("오른쪽 와이어 당기기 시작!");
                break;

            case WireState.Pulling:
                // 상태 3: 해제
                rightWireState = WireState.Inactive;
                rightWireHooked = false;
                Debug.Log("오른쪽 와이어 해제!");
                break;
        }
    }

    private Vector3 GetWireTarget()
    {
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        return ray.origin + ray.direction * maxWireDistance;
    }

    private void ApplyWireForce()
    {
        // 왼쪽 와이어로 당기기 (Pulling 상태일 때만)
        if (leftWireState == WireState.Pulling && leftGear != null)
        {
            Vector3 direction = (leftWireTarget - leftGear.position).normalized;
            rb.linearVelocity += direction * pullForce * Time.fixedDeltaTime;
        }

        // 오른쪽 와이어로 당기기 (Pulling 상태일 때만)
        if (rightWireState == WireState.Pulling && rightGear != null)
        {
            Vector3 direction = (rightWireTarget - rightGear.position).normalized;
            rb.linearVelocity += direction * pullForce * Time.fixedDeltaTime;
        }
    }

    private void ApplyGasBoost()
    {
        // 가스 분출 중일 때만 힘 적용
        if (isGasBoosting)
        {
            // 카메라 방향으로 가스 분출 (앞쪽)
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // 카메라의 forward 방향 (Y축 성분 일부만 포함해서 앞쪽으로)
                Vector3 boostDirection = mainCamera.transform.forward;
                boostDirection.y *= 0.3f; // Y축 성분을 30%로 줄임 (위보다 앞쪽 중심)
                boostDirection = boostDirection.normalized;

                rb.linearVelocity += boostDirection * gasBoostForce * Time.fixedDeltaTime;
            }
        }
    }

    private void ReleaseAllWires()
    {
        leftWireState = WireState.Inactive;
        rightWireState = WireState.Inactive;
        leftWireHooked = false;
        rightWireHooked = false;
        Debug.Log("모든 와이어 해제!");
    }

    private void CheckWireCollision()
    {
        // 왼쪽 와이어 충돌 감지 (Launched 상태일 때만, 고정되지 않았을 때만)
        if (leftWireState == WireState.Launched && !leftWireHooked && leftGear != null)
        {
            CheckWireHit(true);
        }

        // 오른쪽 와이어 충돌 감지 (Launched 상태일 때만, 고정되지 않았을 때만)
        if (rightWireState == WireState.Launched && !rightWireHooked && rightGear != null)
        {
            CheckWireHit(false);
        }
    }

    private void CheckWireHit(bool isLeft)
    {
        Transform gear = isLeft ? leftGear : rightGear;
        Vector3 wireTarget = isLeft ? leftWireTarget : rightWireTarget;
        Vector3 wireDirection = (wireTarget - gear.position).normalized;
        float wireDistance = Vector3.Distance(gear.position, wireTarget);

        // Raycast로 와이어 경로상의 충돌 감지
        if (Physics.Raycast(gear.position, wireDirection, out RaycastHit hit, wireDistance))
        {
            // 충돌한 오브젝트의 Layer가 Hookable Layer에 포함되는지 확인
            if (((1 << hit.collider.gameObject.layer) & hookableLayer) != 0)
            {
                // Hookable Layer에 고정!
                if (isLeft)
                {
                    leftWireTarget = hit.point;
                    leftWireHooked = true;
                    Debug.Log("왼쪽 와이어가 고정되었습니다!");
                }
                else
                {
                    rightWireTarget = hit.point;
                    rightWireHooked = true;
                    Debug.Log("오른쪽 와이어가 고정되었습니다!");
                }
            }
            else
            {
                // Hookable Layer가 아니므로 자동 해제
                if (isLeft)
                {
                    leftWireState = WireState.Inactive;
                    leftWireHooked = false;
                    leftWireVisual.positionCount = 0; // 즉시 와이어 시각 숨김
                    Debug.Log("왼쪽 와이어가 대상 레이어에 닿지 않아 해제되었습니다!");
                }
                else
                {
                    rightWireState = WireState.Inactive;
                    rightWireHooked = false;
                    rightWireVisual.positionCount = 0; // 즉시 와이어 시각 숨김
                    Debug.Log("오른쪽 와이어가 대상 레이어에 닿지 않아 해제되었습니다!");
                }
            }
        }
    }

    private void UpdateVisuals()
    {
        // 왼쪽 와이어 표시 (발사되었거나 당기는 중일 때)
        if (leftWireState != WireState.Inactive && leftGear != null)
        {
            Vector3 displayTarget = leftWireTarget;
            
            // 발사 중이고 아직 고정되지 않으면 애니메이션
            if (leftWireState == WireState.Launched && !leftWireHooked)
            {
                float elapsedTime = Time.time - leftWireLaunchTime;
                float progress = Mathf.Clamp01(elapsedTime / Mathf.Max(wireSpeed, 0.01f));
                displayTarget = Vector3.Lerp(leftWireStart, leftWireTarget, progress);
            }
            // 고정되었으면 바로 목표점까지 표시 (색이 즉시 보이게)
            
            leftWireVisual.positionCount = 2;
            leftWireVisual.SetPosition(0, leftGear.position);
            leftWireVisual.SetPosition(1, displayTarget);
        }
        else
        {
            leftWireVisual.positionCount = 0;
        }

        // 오른쪽 와이어 표시 (발사되었거나 당기는 중일 때)
        if (rightWireState != WireState.Inactive && rightGear != null)
        {
            Vector3 displayTarget = rightWireTarget;
            
            // 발사 중이고 아직 고정되지 않으면 애니메이션
            if (rightWireState == WireState.Launched && !rightWireHooked)
            {
                float elapsedTime = Time.time - rightWireLaunchTime;
                float progress = Mathf.Clamp01(elapsedTime / Mathf.Max(wireSpeed, 0.01f));
                displayTarget = Vector3.Lerp(rightWireStart, rightWireTarget, progress);
            }
            // 고정되었으면 바로 목표점까지 표시 (색이 즉시 보이게)
            
            rightWireVisual.positionCount = 2;
            rightWireVisual.SetPosition(0, rightGear.position);
            rightWireVisual.SetPosition(1, displayTarget);
        }
        else
        {
            rightWireVisual.positionCount = 0;
        }
    }

    private LineRenderer CreateWireVisual(string name, Color color)
    {
        GameObject wireObj = new GameObject(name);
        wireObj.transform.SetParent(transform);

        LineRenderer lr = wireObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.positionCount = 2;

        return lr;
    }
}