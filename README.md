# Attack on Titan - 3D Movement System

진격의 거인 스타일의 3D 입체기동장치 게임 시스템입니다. 플레이어는 와이어를 발사하여 환경에 고정하고 가스 분출을 통해 이동합니다.

## 📋 프로젝트 소개

- **플랫폼**: Unity (C#)
- **장르**: 3D 액션 게임
- **핵심 시스템**: 입체기동장치, 와이어 시스템, 가스 분출

### 주요 기능

✨ **와이어 시스템**
- 양쪽 발사부에서 독립적인 와이어 발사
- 카메라 중앙 광선 기반 타겟팅
- Hookable Layer에만 고정되는 스마트 앵커 시스템
- 실시간 와이어 장력 시각화

⛽ **가스 분출 시스템**
- 스페이스바로 조작 가능한 가스 부스트
- 쿨타임 및 최대 지속 시간 관리
- 카메라 방향 기반 추진

📷 **3인칭 카메라**
- 마우스 기반 자유로운 시점 제어
- 부드러운 카메라 추적
- 수직/수평 각도 제한

---

## 🏗️ 아키텍처 (SOLID 원칙 준수)

### 단일책임의 원칙(SRP)에 따른 클래스 분리

```
Player (시스템 조율)
  ├── WireSystem (와이어 로직) × 2
  │   └── WireVisualizer (시각화) × 2
  │       └── WireState (상태 데이터)
  ├── GasBoostSystem (가스 시스템)
  ├── PlayerInputHandler (입력 처리)
  └── ThirdPersonCamera (카메라)
```

### 각 클래스의 책임

| 클래스 | 책임 | 파일 |
|--------|------|------|
| **Player** | 시스템 생명주기 관리 및 조율 | `Player.cs` |
| **WireSystem** | 와이어 상태 전환, 충돌 감지, 물리 적용 | `WireSystem.cs` |
| **WireState** | 와이어 상태 정보 저장 | `WireState.cs` |
| **WireVisualizer** | LineRenderer를 이용한 와이어 렌더링 | `WireVisualizer.cs` |
| **GasBoostSystem** | 가스 분출 입력 및 물리 적용 | `GasBoostSystem.cs` |
| **PlayerInputHandler** | 마우스/키보드 입력 감지 | `PlayerInputHandler.cs` |
| **ThirdPersonCamera** | 3인칭 카메라 제어 | `ThirdPersonCamera.cs` |

---

## 🎮 조작 방법

| 입력 | 기능 |
|------|------|
| **마좌클릭** | 왼쪽 와이어 발사 |
| **마우스우클릭** | 오른쪽 와이어 발사 |
| **스페이스바** | 가스 분출 |
| **마우스 이동** | 카메라 회전 |
| **ESC** | 커서 잠금 해제 |

---

## 🔧 설정 값

### 와이어 시스템

```csharp
pullForce = 5f              // 당기는 힘의 크기
maxWireDistance = 100f      // 와이어 최대 거리
wireSpeed = 0.05f           // 와이어 표시 속도 (초, 작을수록 빠름)
```

### 가스 분출

```csharp
boostForce = 5f             // 가스 분출 힘
cooldown = 0.2f             // 쿨타임 (초)
maxDuration = 2f            // 최대 지속 시간 (초)
```

### 카메라

```csharp
distance = 7f               // 플레이어와의 거리
height = 1.5f               // 높이 오프셋
smoothSpeed = 5f            // 추적 부드러움 (작을수록 빠름)
mouseSensitivity = 0.5f     // 마우스 감도
```

---

## 💾 설치 및 실행

### 필수 요구사항
- Unity 6.0 이상
- C# 11.0 이상
- .NET 프레임워크

### 설치 단계

1. 리포지토리 클론
```bash
git clone <repository-url>
cd AttackOnTitan
```

2. Unity에서 프로젝트 열기
3. Assets/@Script 폴더에 모든 스크립트 확인
4. 플레이어 오브젝트에 필요한 컴포넌트 연결
5. 플레이

---

## 🎯 핵심 기능 상세

### 1. 와이어 시스템

**라이프사이클**
```
Inactive (대기)
   ↓ [클릭]
Launched (발사 - 이동 중)
   ↓ [클릭]
Pulling (당기기 - 플레이어 이동)
   ↓ [클릭]
Inactive (해제)
```

**타겟팅 메커니즘**
- 카메라 중앙에서 광선 발사 (Raycast)
- Hookable Layer 물체에만 고정
- 다른 물체는 무시하고 기본 거리 설정

**충돌 감지**
- 발사 경로상의 장애물 감지
- 자동 앵커 고정 또는 해제

### 2. 가스 분출

**특징**
- 스페이스바 누르는 동안 작동
- 카메라 방향으로 추진 (Y축 30% 감소)
- 쿨타임 및 최대 지속 시간 제한

### 3. 카메라 시스템

**기능**
- 마우스로 시점 자유 제어
- 플레이어 추적 (Lerp 기반 부드러움)
- 수직(-30°~60°) / 수평 각도 제한

## 📊 코드 품질

✅ **SOLID 원칙 준수**
- Single Responsibility: 각 클래스는 하나의 책임만
- Open/Closed: 확장에는 열려있고 수정에는 닫혀있음
- Liskov Substitution: 인터페이스 기반 설계
- Interface Segregation: 명확한 책임 분리
- Dependency Inversion: 고수준 모듈이 저수준에 의존하지 않음

✅ **성능 최적화**
- FixedUpdate에서 물리 계산
- Raycast 캐싱 및 레이어 마스크 활용
- LineRenderer 재사용

✅ **확장성**
- 새 와이어 시스템 추가 용이 (WireSystem 복사)
- 새 부스트 시스템 추가 간편 (GasBoostSystem 상속)

