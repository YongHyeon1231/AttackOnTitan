using UnityEngine;

/// <summary>
/// 와이어의 상태와 데이터를 관리하는 클래스
/// 책임: 한쪽 와이어의 상태 정보 저장
/// </summary>
public class WireState
{
    public enum State { Inactive, Launched, Pulling }

    public State CurrentState { get; set; } = State.Inactive;
    public bool IsHooked { get; set; } = false;
    public Vector3 TargetPosition { get; set; }
    public Vector3 StartPosition { get; set; }
    public float LaunchTime { get; set; }

    public void Reset()
    {
        CurrentState = State.Inactive;
        IsHooked = false;
    }
}
