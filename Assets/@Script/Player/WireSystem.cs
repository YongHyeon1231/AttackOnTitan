using UnityEngine;

/// <summary>
/// 와이어 시스템을 관리하는 클래스
/// 책임: 와이어의 상태 전환, 충돌 감지, 물리 적용
/// </summary>
public class WireSystem : MonoBehaviour
{
    [SerializeField] private Transform gearTransform;           // 발사부 위치
    [SerializeField] private float pullForce = 50f;             // 당기는 힘
    [SerializeField] private float maxWireDistance = 100f;      // 와이어 최대 거리
    [SerializeField] private LayerMask hookableLayer = 1 << 3;  // 고정 가능한 레이어

    private Rigidbody rb;
    private WireState wireState;
    private WireVisualizer wireVisualizer;
    private float wireSpeed;

    public WireState.State CurrentState => wireState.CurrentState;
    public bool IsHooked => wireState.IsHooked;
    public Vector3 TargetPosition => wireState.TargetPosition;

    private void Awake()
    {
        // 부모 오브젝트에서 Rigidbody 찾기 (자식 오브젝트에는 없음)
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody>();
            Debug.Log("부모에서 Rigidbody를 찾았습니다: " + (rb != null ? rb.gameObject.name : "없음"));
        }
        wireState = new WireState();

        // gearTransform 자동 설정 (부모=Player 또는 자신)
        if (gearTransform == null)
        {
            gearTransform = transform.parent != null ? transform.parent : transform;
            Debug.Log("gearTransform을 설정했습니다: " + gearTransform.gameObject.name);
        }
    }

    /// <summary>
    /// 와이어 시각화 설정 (Player에서 호출)
    /// </summary>
    public void SetVisualizer(WireVisualizer visualizer, float speed)
    {
        wireVisualizer = visualizer;
        wireSpeed = speed;
    }

    /// <summary>
    /// 와이어 상태를 다음 단계로 전환합니다
    /// </summary>
    public void ProgressState()
    {
        if (gearTransform == null) return;

        switch (wireState.CurrentState)
        {
            case WireState.State.Inactive:
                // 상태 1: 발사
                wireState.CurrentState = WireState.State.Launched;
                wireState.IsHooked = false;
                wireState.StartPosition = gearTransform.position;
                wireState.TargetPosition = GetWireTarget();
                wireState.LaunchTime = Time.time;
                Debug.Log("와이어 발사!");
                break;

            case WireState.State.Launched:
                // 상태 2: 당기기
                wireState.CurrentState = WireState.State.Pulling;
                Debug.Log("와이어 당기기 시작!");
                break;

            case WireState.State.Pulling:
                // 상태 3: 해제
                wireState.Reset();
                Debug.Log("와이어 해제!");
                break;
        }
    }

    /// <summary>
    /// 와이어 충돌을 감지하고 처리합니다
    /// </summary>
    public void CheckCollision()
    {
        // Launched 상태이고 아직 고정되지 않았을 때만
        if (wireState.CurrentState == WireState.State.Launched && !wireState.IsHooked && gearTransform != null)
        {
            Vector3 wireDirection = (wireState.TargetPosition - gearTransform.position).normalized;
            float wireDistance = Vector3.Distance(gearTransform.position, wireState.TargetPosition);

            if (Physics.Raycast(gearTransform.position, wireDirection, out RaycastHit hit, wireDistance))
            {
                // Hookable Layer인지 확인
                if (((1 << hit.collider.gameObject.layer) & hookableLayer) != 0)
                {
                    wireState.TargetPosition = hit.point;
                    wireState.IsHooked = true;
                    Debug.Log("와이어가 고정되었습니다!");
                }
                else
                {
                    // Hookable Layer가 아니면 해제
                    wireState.Reset();
                    wireVisualizer?.Hide();
                    Debug.Log("와이어가 대상 레이어에 닿지 않아 해제되었습니다!");
                }
            }
        }
    }

    /// <summary>
    /// 와이어의 당기는 힘을 적용합니다
    /// </summary>
    public void ApplyForce()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody가 없습니다!");
            return;
        }

        if (wireState.CurrentState == WireState.State.Pulling && gearTransform != null)
        {
            Vector3 direction = (wireState.TargetPosition - gearTransform.position).normalized;
            rb.AddForce(direction * pullForce, ForceMode.Acceleration);
            Debug.Log("와이어로 당기는 중! 방향: " + direction + " 힘: " + pullForce);
        }
    }

    /// <summary>
    /// 와이어를 시각화합니다
    /// </summary>
    public void UpdateVisuals()
    {
        if (wireVisualizer != null)
        {
            wireVisualizer.RenderWire(
                gearTransform.position,
                wireState.TargetPosition,
                wireState.CurrentState,
                wireState.IsHooked,
                wireState.LaunchTime
            );
        }
    }

    /// <summary>
    /// 와이어의 목표점을 계산합니다 (카메라 중앙 광선)
    /// </summary>
    private Vector3 GetWireTarget()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera를 찾을 수 없습니다!");
            return gearTransform.position + transform.forward * maxWireDistance;
        }

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        // 카메라 중앙 광선으로 충돌 감지 (Hookable Layer만)
        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, maxWireDistance))
        {
            // 닿은 오브젝트가 Hookable Layer인지 확인
            if (((1 << hit.collider.gameObject.layer) & hookableLayer) != 0)
            {
                Debug.Log("와이어 타겟 감지: " + hit.collider.gameObject.name + " Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
                return hit.point;
            }
            else
            {
                Debug.Log("카메라 중앙에 있지만 Hookable Layer가 아님. 충돌한 오브젝트 Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
            }
        }

        // Hookable Layer가 아니거나 닿은 것이 없으면 기본 거리만큼 떨어진 지점
        Debug.Log("카메라 중앙에 Hookable Layer 충돌 없음. 최대 거리로 발사");
        return ray.origin + ray.direction * maxWireDistance;
    }

    public void Release()
    {
        wireState.Reset();
    }
}
