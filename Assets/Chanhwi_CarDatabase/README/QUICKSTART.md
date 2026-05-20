# Custom UI System - 빠른 시작 가이드 (Quick Start)

> **이 문서를 읽으면 5분 안에 UI를 구성할 수 있습니다.**

---

## 📋 필수 준비물

- ✅ Unity 프로젝트 (Sirenix Odin Inspector 포함)
- ✅ TextMeshPro (임포트됨)
- ✅ CarDatabase 어셋 (샘플 데이터: `Tools > Car Database > Create Sample Cars`)

---

## 🚀 4단계로 UI 구성하기

### 1단계: 프리팹 생성 (1분)

Unity 에디터에서 메뉴 사용:

```
Tools > Custom UI > Create Item Prefab
Tools > Custom UI > Create Panel Prefab
```

📂 **저장 위치**: `Assets/Chanhwi_CarDatabase/Prefab/`

---

### 2단계: 씬 구조 구성 (2분)

씬에 다음 구조를 만듭니다:

```
Hierarchy:

CarManager  ← 빈 GameObject (Canvas 밖에 생성)
  └─ [CarManager 스크립트 추가 → CarDatabase 어셋 할당]

Canvas
  └─ SidebarUI  ← 빈 GameObject
       ├─ [SidebarUIManager 스크립트 추가]  ← 반드시 추가
       ├─ [CustomUI 스크립트 추가]
       ├─ [SidebarUISetup 스크립트 추가]
       └─ PanelContainer  ← 빈 자식 GameObject
```

> ⚠️ **CarManager 없이 Play하면 슬라이더 값이 모두 0으로 표시됩니다.**

---

### 3단계: SidebarUISetup 필드 할당 (1분)

**SidebarUI** 선택 → Inspector의 **SidebarUISetup**에서 4개 할당:

| 필드 | 드래그할 것 |
|------|-------------|
| Sidebar Manager | 같은 SidebarUI의 `SidebarUIManager` 컴포넌트 |
| Custom UI | 같은 SidebarUI의 `CustomUI` 컴포넌트 |
| Panel Prefab | Prefab 폴더의 `DataAdjustmentPanel` |
| Panel Container | 자식 `PanelContainer` GameObject |

**표시할 데이터 선택:**
```
☑ 성능 정보 표시
☑ 배터리 정보 표시
☐ 연료 정보 표시
☐ 에너지 소비 정보 표시
```

---

### 4단계: UI 생성 (바로)

**✅ UI자동생성** 버튼 클릭 → 완료!

---

## 🎮 실행 테스트

1. **Play 모드 시작** (▶ 버튼)
2. 슬라이더에 자동차 데이터가 표시되는지 확인
3. 배터리 슬라이더 조작 → CarData 값이 실시간 변경됨 확인

---

## 📦 주요 컴포넌트 역할

| 컴포넌트 | 역할 |
|---------|------|
| **CarManager** | 현재 선택된 차량 데이터 관리 (싱글톤) |
| **SidebarUIManager** | 패널 생성·삭제, CarManager 이벤트 처리 |
| **CustomUI** | 전체 UI 표시/숨김 관리 |
| **SidebarUISetup** | Inspector 버튼으로 UI 자동 생성 |
| **DataAdjustmentPanel** | 슬라이더 그룹 패널 (프리팹) |
| **DataAdjustmentUIItem** | 슬라이더 + 값 표시 항목 (프리팹) |

---

## ❌ 문제 해결

| 문제 | 원인 | 해결책 |
|------|------|--------|
| "Sidebar Manager를 할당하세요!" 오류 | SidebarUIManager 컴포넌트 미추가 | SidebarUI에 Add Component → SidebarUIManager |
| 패널이 안 나타남 | Panel Container 미할당 | SidebarUISetup의 Panel Container 필드에 PanelContainer 드래그 |
| 슬라이더 값이 전부 0 | CarManager 없음 또는 CarDatabase 미할당 | Hierarchy에 CarManager 추가 후 CarDatabase 할당 |
| 프리팹 생성 안 됨 | Tools 메뉴 미실행 | `Tools > Custom UI > Create Panel Prefab` 실행 |

---

## 📚 다음 단계

- [세팅 완벽 가이드](SETUP_GUIDE.md) - 각 스크립트 역할 상세 설명
- [UI 시스템 상세](README_UI_SYSTEM.md) - 확장 방법, 커스텀 Provider 추가
- [아키텍처](ARCHITECTURE.md) - 클래스 다이어그램 및 설계 원칙

---

**완료! 이제 리그 오브 레전드 스타일의 자동차 데이터 조정 UI를 사용할 수 있습니다.** 🚗
