using UnityEngine;

/// <summary>
/// 플레이어 메인 클래스
/// 책임: 각 시스템의 조율 및 생명 주기 관리 (SRP 준수)
/// 
/// SOLID 원칙 적용:
/// - 단일책임의 원칙(SRP): 각 기능별로 별도 클래스로 분리
///   * WireSystem: 와이어 로직만
///   * WireVisualizer: 와이어 시각화만
///   * GasBoostSystem: 가스 분출만
///   * PlayerInputHandler: 입력만
///   * Player: 시스템 조율만
/// </summary>
public class Player : MonoBehaviour
{
    [Header("와이어 시스템")]
    [SerializeField] private WireSystem leftWireSystem;
    [SerializeField] private WireSystem rightWireSystem;
    [SerializeField] private WireVisualizer leftWireVisualizer;
    [SerializeField] private WireVisualizer rightWireVisualizer;
    [SerializeField] private float wireSpeed = 0.05f;

    [Header("가스 분출 시스템")]
    [SerializeField] private GasBoostSystem gasBoostSystem;

    [Header("입력 처리")]
    [SerializeField] private PlayerInputHandler inputHandler;

    private Rigidbody rb;

    private void Awake()
    {
        // Rigidbody 자동 찾기 또는 생성
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("Rigidbody 컴포넌트가 자동으로 추가되었습니다.");
        }
        
        // Rigidbody 설정 (안정성 개선)
        rb.linearDamping = 1.0f;        // 선형 감속 (더 강함 = 덜 떨림)
        rb.angularDamping = 1.0f;       // 회전 감속 (더 강함)
        rb.freezeRotation = true;       // 회전 완전 제약 (화면 떨림 방지)

        // PlayerInputHandler 자동 찾기 또는 생성
        if (inputHandler == null)
        {
            inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler == null)
            {
                inputHandler = gameObject.AddComponent<PlayerInputHandler>();
                Debug.Log("PlayerInputHandler 컴포넌트가 자동으로 추가되었습니다.");
            }
        }

        // GasBoostSystem 자동 찾기 또는 생성
        if (gasBoostSystem == null)
        {
            gasBoostSystem = GetComponent<GasBoostSystem>();
            if (gasBoostSystem == null)
            {
                gasBoostSystem = gameObject.AddComponent<GasBoostSystem>();
                Debug.Log("GasBoostSystem 컴포넌트가 자동으로 추가되었습니다.");
            }
        }

        // WireSystem 자동 찾기 (좌/우 와이어 별도 시스템)
        WireSystem[] wireSystems = GetComponentsInChildren<WireSystem>();
        if (wireSystems.Length >= 1)
        {
            leftWireSystem = wireSystems[0];
            Debug.Log("leftWireSystem을 찾았습니다.");
        }
        else if (leftWireSystem == null)
        {
            // 왼쪽 와이어시스템이 없으면 생성
            leftWireSystem = CreateWireSystemChild("LeftWire");
            Debug.Log("leftWireSystem을 새로 생성했습니다.");
        }

        if (wireSystems.Length >= 2)
        {
            rightWireSystem = wireSystems[1];
            Debug.Log("rightWireSystem을 찾았습니다.");
        }
        else if (rightWireSystem == null)
        {
            // 오른쪽 와이어시스템이 없으면 생성
            rightWireSystem = CreateWireSystemChild("RightWire");
            Debug.Log("rightWireSystem을 새로 생성했습니다.");
        }

        // 와이어 시각화 자동 생성
        if (leftWireSystem != null && leftWireVisualizer == null)
            leftWireVisualizer = CreateWireVisualizer("LeftWire", new Color(0.8f, 0f, 1f, 1f));
        if (rightWireSystem != null && rightWireVisualizer == null)
            rightWireVisualizer = CreateWireVisualizer("RightWire", new Color(0f, 0.6f, 1f, 1f));

        if (leftWireSystem != null)
            leftWireSystem.SetVisualizer(leftWireVisualizer, wireSpeed);
        if (rightWireSystem != null)
            rightWireSystem.SetVisualizer(rightWireVisualizer, wireSpeed);
    }

    private void Start()
    {
        // 입력 이벤트 구독
        if (inputHandler != null)
        {
            Debug.Log("입력 이벤트 구독됨");
            inputHandler.OnLeftWireInput += () => 
            {
                Debug.Log("왼쪽 와이어 이벤트 발생, leftWireSystem: " + (leftWireSystem != null));
                leftWireSystem?.ProgressState();
            };
            inputHandler.OnRightWireInput += () => 
            {
                Debug.Log("오른쪽 와이어 이벤트 발생, rightWireSystem: " + (rightWireSystem != null));
                rightWireSystem?.ProgressState();
            };
        }
        else
        {
            Debug.LogError("inputHandler가 null입니다!");
        }
    }

    private void Update()
    {
        // 가스 분출 입력 처리
        if (gasBoostSystem != null)
        {
            gasBoostSystem.HandleInput();
        }

        // 와이어 충돌 감지
        if (leftWireSystem != null)
            leftWireSystem.CheckCollision();
        if (rightWireSystem != null)
            rightWireSystem.CheckCollision();

        // 와이어 시각화 업데이트
        if (leftWireSystem != null)
            leftWireSystem.UpdateVisuals();
        if (rightWireSystem != null)
            rightWireSystem.UpdateVisuals();
    }

    private void FixedUpdate()
    {
        // 물리 적용
        if (leftWireSystem != null)
            leftWireSystem.ApplyForce();
        if (rightWireSystem != null)
            rightWireSystem.ApplyForce();

        if (gasBoostSystem != null)
            gasBoostSystem.ApplyForce();
    }

    private WireVisualizer CreateWireVisualizer(string name, Color color)
    {
        GameObject visualizerObj = new GameObject(name + "Visualizer");
        visualizerObj.transform.SetParent(transform);
        WireVisualizer visualizer = visualizerObj.AddComponent<WireVisualizer>();
        visualizer.Initialize(name, color, wireSpeed);
        return visualizer;
    }

    private WireSystem CreateWireSystemChild(string name)
    {
        GameObject wireObj = new GameObject(name);
        wireObj.transform.SetParent(transform);
        wireObj.transform.localPosition = Vector3.zero;
        
        // Rigidbody 공유 (플레이어의 Rigidbody 사용)
        WireSystem wireSystem = wireObj.AddComponent<WireSystem>();
        Debug.Log(name + " WireSystem을 생성했습니다.");
        return wireSystem;
    }
}