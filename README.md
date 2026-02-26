# Attack on Titan - Wire System

Unity에서 구현한 진격의 거인 스타일의 입체기동장치 시스템.

## 주요 기능

- **양쪽 와이어 시스템**: 좌클릭(왼쪽), 우클릭(오른쪽) 독립적 발사
- **3인칭 카메라**: 마우스 제어 (좌우 -45~45도, 상하 위만 20도)
- **물리 기반 이동**: 와이어로 당겨서 플레이어 이동
- **완전 자동화**: Player.cs 컴포넌트만 추가해도 모든 기능 작동

## 아키텍처 (SOLID 원칙)

```
Player (조율)
  ├── WireSystem × 2 (좌/우 와이어)
  │   ├── WireVisualizer (시각화)
  │   └── WireState (상태)
  ├── PlayerInputHandler (입력)
  ├── GasBoostSystem (가스)
  └── ThirdPersonCamera (카메라)
```

| 클래스 | 책임 |
|--------|------|
| Player | 시스템 조율 |
| WireSystem | 와이어 발사, 상태 전환, 물리 적용 |
| WireVisualizer | 와이어 렌더링 |
| PlayerInputHandler | 마우스/키 입력 |
| GasBoostSystem | 가스 분출 |
| ThirdPersonCamera | 카메라 제어 |

## 조작

| 입력 | 기능 |
|------|------|
| 좌클릭 | 왼쪽 와이어 |
| 우클릭 | 오른쪽 와이어 |
| 마우스 이동 | 카메라 회전 |
| ESC | 커서 잠금 해제 |

## 와이어 상태

```
1. Inactive (대기)
   ↓ [클릭]
2. Launched (발사 중)
   ↓ [클릭]
3. Pulling (당기기 - 플레이어 이동)
   ↓ [클릭]
Inactive (해제)
```

## 설정 값

**WireSystem.cs**
```csharp
pullForce = 25f             // 당기는 힘
maxWireDistance = 100f      // 최대 거리
hookableLayer = Wall        // 고정 가능 레이어
```

**Player.cs (Rigidbody)**
```csharp
linearDamping = 1.0f        // 선형 감속
angularDamping = 1.0f       // 회전 감속
freezeRotation = true       // 회전 제약
```

**ThirdPersonCamera.cs**
```csharp
cameraLocalPosition = (0, 5, -9)    // 카메라 위치 (플레이어 기준)
cameraLocalRotation = (10, 0, 0)    // 기본 회전
horizontalSensitivity = 1f          // 마우스 감도
```

## 설치

1. Unity 프로젝트 열기
2. Player GameObject 생성
3. Player.cs 컴포넌트 추가
4. 게임 재생 (자동으로 필요한 컴포넌트 생성됨)

## 주의사항

- 플레이어 기반 도형으로 구성 (일러/모델 없음)
- 월드에 Wall 레이어 물체 필요 (와이어 고정용)
- Main Camera 필요


