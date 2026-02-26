using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 입력을 관리하는 클래스
/// 책임: 입력 감지 및 이벤트 발생만 담당
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    public delegate void WireInputAction();
    public event WireInputAction OnLeftWireInput;
    public event WireInputAction OnRightWireInput;

    private void Update()
    {
        // 마우스 왼쪽 클릭 - 왼쪽 와이어
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnLeftWireInput?.Invoke();
        }

        // 마우스 오른쪽 클릭 - 오른쪽 와이어
        if (Input.GetMouseButtonDown(1))
        {
            OnRightWireInput?.Invoke();
        }
    }
}
