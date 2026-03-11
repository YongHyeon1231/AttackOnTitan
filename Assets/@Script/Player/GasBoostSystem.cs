using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 가스 분출 시스템을 관리하는 클래스
/// 책임: 가스 분출의 상태 관리, 입력 처리, 물리 적용
/// </summary>
public class GasBoostSystem : MonoBehaviour
{
    [SerializeField] private float boostForce = 25f;              // 가스 분출 힘
    [SerializeField] private float cooldown = 0.2f;              // 쿨타임
    [SerializeField] private float maxDuration = 2f;             // 최대 지속 시간

    private Rigidbody rb;
    private bool isBoosting = false;
    private float lastBoostTime = -999f;
    private float boostStartTime = 0f;

    public bool IsBoosting => isBoosting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 가스 분출 입력을 처리합니다
    /// </summary>
    public void HandleInput()
    {
        if (Keyboard.current.spaceKey.isPressed)
        {
            if (!isBoosting && Time.time - lastBoostTime >= cooldown)
            {
                StartBoost();
            }
        }
        else
        {
            if (isBoosting)
            {
                StopBoost();
            }
        }

        // 최대 지속 시간 초과 시 종료
        if (isBoosting && Time.time - boostStartTime >= maxDuration)
        {
            StopBoost();
            Debug.Log("가스 분출 최대 시간 도달!");
        }
    }

    /// <summary>
    /// 가스 분출을 시작합니다
    /// </summary>
    private void StartBoost()
    {
        isBoosting = true;
        boostStartTime = Time.time;
        lastBoostTime = Time.time;
        Debug.Log("가스 분출 시작!");
    }

    /// <summary>
    /// 가스 분출을 중지합니다
    /// </summary>
    private void StopBoost()
    {
        isBoosting = false;
        Debug.Log("가스 분출 종료!");
    }

    /// <summary>
    /// 가스 분출의 힘을 적용합니다
    /// </summary>
    public void ApplyForce()
    {
        if (!isBoosting) return;

        Camera mainCamera = Camera.main;
        if (mainCamera != null && rb != null)
        {
            // 카메라의 forward 방향으로 가스 분출
            Vector3 boostDirection = mainCamera.transform.forward;
            boostDirection.y *= 0.3f; // Y축 성분을 30%로 줄임
            boostDirection = boostDirection.normalized;

            // AddForce를 사용하여 Player의 velocity 덮어쓰기를 피함
            rb.AddForce(boostDirection * boostForce, ForceMode.Acceleration);
        }
    }
}
