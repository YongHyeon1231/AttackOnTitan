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
        rb = GetComponent<Rigidbody>();

        // 자동 찾기
        if (inputHandler == null)
            inputHandler = GetComponent<PlayerInputHandler>();
        if (gasBoostSystem == null)
            gasBoostSystem = GetComponent<GasBoostSystem>();

        // 와이어 시스템 초기화
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
            inputHandler.OnLeftWireInput += () => leftWireSystem?.ProgressState();
            inputHandler.OnRightWireInput += () => rightWireSystem?.ProgressState();
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
}