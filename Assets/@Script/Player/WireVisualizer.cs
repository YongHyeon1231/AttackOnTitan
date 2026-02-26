using UnityEngine;

/// <summary>
/// 와이어의 시각화를 담당하는 클래스
/// 책임: LineRenderer를 사용한 와이어 렌더링만 담당
/// </summary>
public class WireVisualizer : MonoBehaviour
{
    private LineRenderer wireRenderer;
    private float wireSpeed;
    private Color wireColor;

    public void Initialize(string name, Color color, float speed)
    {
        wireColor = color;
        wireSpeed = speed;
        CreateWireVisual(name, color);
    }

    private void CreateWireVisual(string name, Color color)
    {
        GameObject wireObj = new GameObject(name);
        wireObj.transform.SetParent(transform);

        wireRenderer = wireObj.AddComponent<LineRenderer>();
        wireRenderer.material = new Material(Shader.Find("Sprites/Default"));
        wireRenderer.startColor = color;
        wireRenderer.endColor = color;
        wireRenderer.startWidth = 0.15f;
        wireRenderer.endWidth = 0.15f;
        wireRenderer.positionCount = 0;
    }

    /// <summary>
    /// 와이어를 표시합니다
    /// </summary>
    public void RenderWire(Vector3 startPoint, Vector3 endPoint, WireState.State wireState, bool isHooked, float launchTime)
    {
        if (wireState == WireState.State.Inactive)
        {
            wireRenderer.positionCount = 0;
            return;
        }

        Vector3 displayTarget = endPoint;

        // 발사 중이고 아직 고정되지 않으면 애니메이션
        if (wireState == WireState.State.Launched && !isHooked)
        {
            float elapsedTime = Time.time - launchTime;
            float progress = Mathf.Clamp01(elapsedTime / Mathf.Max(wireSpeed, 0.01f));
            displayTarget = Vector3.Lerp(startPoint, endPoint, progress);
        }

        wireRenderer.positionCount = 2;
        wireRenderer.SetPosition(0, startPoint);
        wireRenderer.SetPosition(1, displayTarget);
    }

    public void Hide()
    {
        wireRenderer.positionCount = 0;
    }
}
