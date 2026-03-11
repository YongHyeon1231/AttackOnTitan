# Attack on Titan - Wire System

Unity에서 구현한 진격의 거인 스타일의 입체기동장치 시스템.

**핵심 아키텍처 원칙: Update 분리를 통한 입력/물리 설계**

## 주요 기능

- **양쪽 와이어 시스템**: 좌클릭(왼쪽), 우클릭(오른쪽) 독립적 발사
- **Damping 기반 카메라 추적**: LateUpdate에서 부드럽게 따라다니기
- **물리 기반 이동**: Rigidbody AddForce로 와이어/가스 힘 적용
- **입력 기반 회전/이동**: 마우스+WASD 조작

## 아키텍처 (Update 분리 설계)

```
게임 루프

1️⃣  Update (입력 처리)
    ├── Player.HandleMovement()        → WASD 입력
    ├── Player.HandleRotation()        → 마우스 좌우 입력
    ├── GasBoostSystem.HandleInput()   → 스페이스바 입력
    ├── WireSystem.CheckCollision()    → 와이어 충돌 감지
    └── WireSystem.UpdateVisuals()     → 와이어 시각화

2️⃣  FixedUpdate (물리 적용)
    ├── WireSystem.ApplyForce()        → 와이어 당기는 힘
    ├── GasBoostSystem.ApplyForce()    → 가스 분출 힘
    └── Player.Velocity 업데이트      → 수평 이동만 적용

3️⃣  LateUpdate (카메라 추적)
    └── ThirdPersonCamera.UpdateCameraPosition()
        ├── Damping 적용 (부드러운 추적)
        ├── Dead Zone 적용 (불감대)
        └── Hard Limits 적용 (범위 제한)
```

### 클래스 구조 (SOLID 원칙)

```
Player (시스템 조율 + 입력/이동)
  ├── WireSystem × 2 (좌/우 와이어)
  │   ├── WireVisualizer (시각화)
  │   └── WireState (상태 관리)
  ├── GasBoostSystem (가스 분출)
  ├── PlayerInputHandler (마우스 입력)
  └── [Camera로부터 관리됨]
      └── ThirdPersonCamera (카메라 추적)
```

| 클래스 | 책임 | Update 타입 |
|--------|------|-----------|
| **Player** | 시스템 조율, WASD 이동, 마우스 회전 | Update |
| **WireSystem** | 와이어 상태/충돌/시각화 | Update/FixedUpdate |
| **GasBoostSystem** | 입력 처리, 가스 분출 힘 적용 | Update/FixedUpdate |
| **ThirdPersonCamera** | 카메라 추적/Damping/제약 | LateUpdate |

## 조작

| 입력 | 기능 | 처리 |
|------|------|------|
| **W** | 앞으로 이동 | Update → FixedUpdate |
| **A** | 좌측 이동 | Update → FixedUpdate |
| **S** | 뒤로 이동 | Update → FixedUpdate |
| **D** | 오른쪽 이동 | Update → FixedUpdate |
| **마우스 좌우** | 플레이어/카메라 회전 | Update (플레이어) / LateUpdate (카메라) |
| **스페이스바** | 가스 분출 (앞으로 가속) | Update → FixedUpdate |
| **좌클릭** | 왼쪽 와이어 발사 | Update |
| **우클릭** | 오른쪽 와이어 발사 | Update |
| **ESC** | 커서 잠금 해제 | LateUpdate (카메라) |

## 와이어 상태 머신

```
1️⃣  Inactive (대기)
    ↓ [클릭하면 와이어 발사]
    
2️⃣  Launched (발사 중)
    - 와이어가 앞으로 뻗음
    - CheckCollision()에서 고정점 감지
    ↓ [다시 클릭하면 당기기]
    
3️⃣  Pulling (당기기)
    - FixedUpdate에서 AddForce로 플레이어 끌어당김
    ↓ [다시 클릭하면 해제]
    
Inactive (해제)
    - 와이어 숨김
```

## Update 분리 설계 원칙

### ✅ Update (입력 처리)
```csharp
// Player.cs - Update에서
HandleMovement()    // WASD 입력 읽기
HandleRotation()    // 마우스 입력 읽기
```

### ✅ FixedUpdate (물리 적용)
```csharp
// Player.cs - FixedUpdate에서
rb.linearVelocity = new Vector3(currentMovementDirection.x, 
                                rb.linearVelocity.y, 
                                currentMovementDirection.z);
// ↑ 수평 이동만 적용 (와이어/가스 AddForce가 Y축에 영향)

// WireSystem.cs - FixedUpdate에서
rb.AddForce(direction * pullForce, ForceMode.Acceleration);

// GasBoostSystem.cs - FixedUpdate에서
rb.AddForce(boostDirection * boostForce, ForceMode.Acceleration);
```

### ✅ LateUpdate (카메라 추적)
```csharp
// ThirdPersonCamera.cs - LateUpdate에서
// Damping, Dead Zone, Hard Limits 적용
Mathf.Lerp(cameraTargetPosition, desiredPosition, dampingFactor * deltaTime);
```

## 설정 값

**Player.cs**
```csharp
moveSpeed = 5f              // WASD 이동 속도
rotationSpeed = 3f          // 마우스 회전 속도
rb.linearDamping = 0.1f     // 선형 감속 (낮음 = 와이어 힘 충분히 받음)
rb.freezeRotation = true    // 회전 제약 (카메라만 회전 가능)
```

**WireSystem.cs**
```csharp
pullForce = 25f             // 당기는 힘 (AddForce)
maxWireDistance = 100f      // 최대 거리
hookableLayer = Wall        // 고정 가능 레이어
```

**GasBoostSystem.cs**
```csharp
boostForce = 25f            // 가스 분출 힘 (AddForce)
cooldown = 0.2f             // 쿨타임
maxDuration = 2f            // 최대 지속 시간
```

**ThirdPersonCamera.cs**
```csharp
dampingFactor = (5, 5, 5)          // 축별 추적 속도 (클수록 빠름)
hardLimitsMin = (-20, -5, -30)     // 카메라 위치 최소값
hardLimitsMax = (20, 20, 10)       // 카메라 위치 최대값
deadZone = (2, 1.5, 2)             // 불감대 (이 범위 내면 정지)
horizontalSensitivity = 0.25f      // 마우스 감도
```

## 설치

1. Unity 프로젝트 열기
2. Player GameObject 생성
3. Player.cs 컴포넌트 추가
4. 게임 재생 (자동으로 필요한 컴포넌트 생성됨)

## 물리 설정 가이드

**문제: 와이어 당기기가 느림**
- 원인: `linearDamping`이 높음
- 해결: `rb.linearDamping` 값 낮추기 (현재 0.1f)

**문제: 가스 분출이 안 됨**
- 원인: `FixedUpdate`에서 velocity를 덮어씌움
- 해결: `AddForce(ForceMode.Acceleration)` 사용

**문제: 카메라가 뚝뚝 끊김**
- 해결: `LateUpdate` + `Damping` 조합으로 부드럽게 추적

## 주의사항

- 플레이어는 Capsule Collider와 Rigidbody 필요
- 월드에 Wall 레이어 물체 필요 (와이어 고정용)
- Main Camera 필요 (카메라 시스템이 자동으로 찾음)



