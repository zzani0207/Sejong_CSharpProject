# Custom UI System - Architecture Diagram

## 🏗️ 시스템 전체 구조

```
┌────────────────────────────────────────────────────────────────┐
│                    Canvas (UI Root)                            │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  SidebarUI (GameObject)                                  │  │
│  │  ├─ SidebarUIManager (Script) - 패널 관리              │  │
│  │  ├─ CustomUI (Script) - 표시/숨김 제어                 │  │
│  │  ├─ SidebarUISetup (Script) - UI 자동 생성 버튼       │  │
│  │  │                                                       │  │
│  │  │  ┌───────────────────────────────────────────────┐  │  │
│  │  │  │  PanelContainer (VerticalLayoutGroup)         │  │  │
│  │  │  │                                               │  │  │
│  │  │  │  ┌─────────────────────────────────────────┐ │  │  │
│  │  │  │  │  Panel 1: ⚡ 성능 정보                 │ │  │  │
│  │  │  │  │  ┌───────────────────────────────────┐ │ │  │  │
│  │  │  │  │  │ Engine Power    [===●=====] 450 kW │ │ │  │  │
│  │  │  │  │  │ Max Speed       [========●] 250 km/h│ │ │  │  │
│  │  │  │  │  │ Torque          [====●====] 660 Nm │ │ │  │  │
│  │  │  │  │  └───────────────────────────────────┘ │ │  │  │
│  │  │  │  └─────────────────────────────────────────┘ │  │  │
│  │  │  │                                               │  │  │
│  │  │  │  ┌─────────────────────────────────────────┐ │  │  │
│  │  │  │  │  Panel 2: 🔋 배터리                    │ │  │  │
│  │  │  │  │  ┌───────────────────────────────────┐ │ │  │  │
│  │  │  │  │  │ Battery Level   [═════●====] 80%  │ │ │  │  │
│  │  │  │  │  │ Battery Health  [════●═════] 95%  │ │ │  │  │
│  │  │  │  │  │ Capacity        [════════════]    │ │ │  │  │
│  │  │  │  │  └───────────────────────────────────┘ │ │  │  │
│  │  │  │  └─────────────────────────────────────────┘ │  │  │
│  │  │  │                                               │  │  │
│  │  │  │  (더 많은 패널들...)                         │  │  │
│  │  │  │                                               │  │  │
│  │  │  └───────────────────────────────────────────────┘  │  │
│  │                                                       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## 🔄 설정 흐름 (Setup Flow)

```
인스펙터 (Inspector)
    │
    ├─ 프리팹 할당
    │  ├─ Panel Prefab
    │  └─ Item Prefab
    │
    ├─ 컴포넌트 참조 할당
    │  ├─ Sidebar Manager
    │  └─ Custom UI
    │
    ├─ 레이아웃 참조 할당
    │  ├─ Panel Container
    │  └─ Panel Spacing
    │
    ├─ 표시 데이터 선택 (체크박스)
    │  ├─ 성능 정보 표시
    │  ├─ 배터리 정보 표시
    │  ├─ 연료 정보 표시
    │  └─ 에너지 소비 정보 표시
    │
    └─ 버튼 클릭
        └─ ✅ UI자동생성

실행 흐름 (Execution Flow)
    │
    ├─ GetOrAddComponent<CarPerformanceDataProvider>()
    ├─ GetOrAddComponent<CarEnergyDataProvider>()
    │
    └─ sidebarManager.CreatePanel()
        │
        ├─ Panel Prefab Instantiate
        └─ UIDataField들 동적 생성
            └─ UIItem들 (Slider) 생성
```

---

## 📊 클래스 다이어그램
│    IDataAdjustable (Interface)      │
├─────────────────────────────────────┤
│ + GetValue(): float                 │
│ + SetNormalizedValue(float): void   │
│ + GetNormalizedValue(): float       │
│ + GetMinValue(): float              │
│ + GetMaxValue(): float              │
│ + IsActive(): bool                  │
└──────────────────▲──────────────────┘
                   │
                   │ implements
                   │
        ┌──────────┴────────────┐
        │                       │
        ▼                       ▼
┌──────────────────┐  ┌──────────────────────┐
│  UIDataField     │  │ (Other implementations)
│                  │  │
│  - fieldId       │  └──────────────────────┘
│  - displayName   │
│  - minValue      │
│  - maxValue      │
│  - getter        │
│  - setter        │
└──────────────────┘


┌─────────────────────────────────┐
│  CarDataProviderBase (Abstract) │
├─────────────────────────────────┤
│ + GetAvailableCategories()      │
│ + GetAllDataFields()            │
│ + GetDataFieldsByCategory()     │
└──────────────────▲──────────────┘
                   │
        ┌──────────┼──────────┬──────────────────────┐
        │          │          │                      │
        ▼          ▼          ▼                      ▼
  Performance  Energy      BasicInfo             Driving
  DataProvider DataProvider DataProvider         DataProvider
  
  (+ CustomDataProvider)
```

---

## 🎯 인스펙터 설정 흐름

```
┌─────────────────────────────────────────┐
│     SidebarUISetup (Inspector)          │
├─────────────────────────────────────────┤
│                                         │
│  Sidebar Manager: [자동]               │
│  Panel Prefab: [DataAdjustmentPanel]    │
│  Panel Container: [PanelContainer]      │
│  Item Prefab: [DataAdjustmentUIItem]    │
│                                         │
│  Show Performance Data: ✓               │
│  Show Battery Data: ✓                   │
│  Show Fuel Data: ☐                      │
│  Show Energy Data: ☐                    │
│                                         │
│  [우클릭 > 자동으로 UI 생성 및 정렬]   │
│                                         │
└─────────────────┬───────────────────────┘
                  │ (Context Menu Click)
                  ▼
      ┌───────────────────────┐
      │  AutoSetupUI()        │
      │  • Provider 생성      │
      │  • Panel 생성         │
      │  • Layout 정렬        │
      └───────┬───────────────┘
              │
              ▼
      ┌───────────────────────┐
      │  SidebarUIManager     │
      │  .CreatePanel()       │
      │  • 동적 생성          │
      │  • 자동 정렬          │
      └───────┬───────────────┘
              │
              ▼
       ┌──────────────┐
       │ UI 완성! 🎉  │
       └──────────────┘
```

---

## 🔌 이벤트 연동

```
CarManager
    │
    └─→ OnCarChanged (Event)
        │
        ├─→ SidebarUIManager.OnCarChanged()
        │   └─→ UpdateCarDisplay()
        │   └─→ RefreshAllPanels()
        │
        └─→ All DataProviders
            └─→ SetCarData(newCar)
                └─→ All UIDataFields updated
                    └─→ All Sliders refreshed
```

---

## 📁 파일 구조

```
Assets/Chanhwi_CarDatabase/CustomUI/
├── Core Scripts/
│   ├── IDataAdjustable.cs ..................... 인터페이스
│   ├── UIDataField.cs ......................... 데이터 필드
│   └── CustomUI.cs ............................ 메인 관리자
│
├── UI Components/
│   ├── DataAdjustmentUIItem.cs ................ 슬라이더 항목
│   ├── DataAdjustmentPanel.cs ................. 패널
│   └── SidebarUIManager.cs .................... UI 관리자
│
├── Data Providers/
│   ├── CarDataProviderBase.cs ................. 추상 기본 클래스
│   ├── CarPerformanceDataProvider.cs .......... 성능 정보
│   ├── CarEnergyDataProvider.cs ............... 에너지 정보
│   ├── CarBasicInfoDataProvider.cs ............ 기본 정보
│   ├── CarDrivingDataProvider.cs .............. 주행 정보
│   └── CarCustomDataProviderExample.cs ........ 확장 예제
│
├── Setup & Tools/
│   ├── SidebarUISetup.cs ...................... UI 자동 생성
│   └── UIComponentCreator.cs .................. 프리팹 생성 (Editor)
│
├── Documentation/
│   ├── QUICKSTART.md .......................... 5분 설정
│   ├── README_UI_SYSTEM.md .................... 상세 가이드
│   └── ARCHITECTURE.md ........................ 이 파일
│
└── Prefabs/
    ├── DataAdjustmentPanel.prefab ............ 패널 프리팹
    └── DataAdjustmentUIItem.prefab .......... 항목 프리팹
```

---

## 🎮 런타임 흐름

```
1. Start()
   └─→ CustomUI.Initialize()
       └─→ SidebarUIManager 할당
       └─→ CarManager.OnCarChanged 구독

2. CarManager에서 자동차 변경
   └─→ OnCarChanged 발생
       └─→ SidebarUIManager 갱신
           └─→ All DataProviders SetCarData()
               └─→ All UIDataFields 업데이트

3. 사용자가 슬라이더 조작
   └─→ DataAdjustmentUIItem.OnSliderChanged()
       └─→ UIDataField.SetNormalizedValue()
           └─→ Setter 실행
               └─→ 데이터 변경

4. Update()
   └─→ DataAdjustmentUIItem 수동으로 값 동기화
       └─→ Slider와 Text 업데이트
```

---

## ✨ 주요 설계 원칙

1. **Single Responsibility**: 각 클래스가 하나의 책임만 담당
2. **Open/Closed**: 기존 코드 수정 없이 새 DataProvider 추가 가능
3. **Dependency Inversion**: 인터페이스에 의존, 구체적 구현에 의존 X
4. **DRY (Don't Repeat Yourself)**: 공통 로직을 기본 클래스에 집중
5. **Interface Segregation**: 작고 명확한 인터페이스 (IDataAdjustable)

---

## 🚀 확장 시나리오

```
새로운 DataProvider 추가

Step 1: CarDataProviderBase 상속
    └─ class MyCustomDataProvider : CarDataProviderBase

Step 2: 추상 메서드 구현
    └─ GetAvailableCategories()
    └─ GetDataFieldsByCategory()
    └─ GetAllDataFields()

Step 3: UIDataField 생성 (getter/setter 정의)
    └─ performanceFields.Add(new UIDataField(...))

Step 4: SidebarUISetup.AutoSetupUI()에 추가
    └─ CreateDataProvider<MyCustomDataProvider>()
    └─ sidebarManager.CreatePanel(...)

Step 5: Inspector 체크박스 추가
    └─ [SerializeField] private bool showMyCustomData = true;

Step 6: 완료! 인스펙터에서 "자동으로 UI 생성 및 정렬" 실행
```

