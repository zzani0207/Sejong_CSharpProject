# Custom UI System - 사용 설명서

## 개요

이 UI 시스템은 **리그 오브 레전드의 사이드바 UI처럼** 자동차의 다양한 데이터를 조정할 수 있도록 설계되었습니다.
**OOP 원칙**을 따르고 **확장 가능한 구조**로 되어 있습니다.

---

## 시스템 아키텍처

### 1. 핵심 클래스

#### `IDataAdjustable` (인터페이스)
- 조정 가능한 데이터 필드의 계약을 정의합니다
- 모든 데이터 필드는 이 인터페이스를 구현합니다

#### `UIDataField` (클래스)
- `IDataAdjustable`의 구현체
- 개별 데이터 필드를 나타냅니다
- Getter/Setter를 통해 값을 관리합니다

#### `DataAdjustmentUIItem` (MonoBehaviour)
- 개별 데이터 필드의 UI 표현
- 슬라이더와 텍스트로 데이터를 표시하고 조정합니다

#### `DataAdjustmentPanel` (MonoBehaviour)
- 여러 `DataAdjustmentUIItem`을 포함하는 패널
- 리그 오브 레전드 사이드바의 한 섹션 같은 역할

#### `SidebarUIManager` (MonoBehaviour)
- 전체 UI 시스템을 관리
- 여러 패널과 데이터 제공자를 조율합니다

#### `SidebarUISetup` (MonoBehaviour)
- **인스펙터에서 UI를 자동으로 생성하는 헬퍼 클래스**
- Context Menu를 통해 버튼 하나로 UI 생성 가능

### 2. 데이터 제공자 (Provider Pattern)

#### `CarDataProviderBase` (추상 클래스)
- 자동차 데이터를 `IDataAdjustable` 형태로 제공하는 기본 클래스

#### 구현체들:
- `CarPerformanceDataProvider`: 성능 정보 (엔진 파워, 최대 속도 등)
- `CarEnergyDataProvider`: 에너지 정보 (배터리, 연료 등)
- `CarBasicInfoDataProvider`: 기본 정보 (연식, 좌석 수 등)
- `CarDrivingDataProvider`: 주행 정보 (마일리지, 무게 등)

---

## 사용 방법

### 1️⃣ 프리팹 생성

Menu에서:
```
Tools > Custom UI > Create Item Prefab
Tools > Custom UI > Create Panel Prefab
```

✅ **자동 설정:**
- `DataAdjustmentUIItem` 프리팹 생성 (Prefabs 폴더)
- `DataAdjustmentPanel` 프리팹 생성 (Panel에 Item 자동 할당)

### 2️⃣ 씬 구조 구성

```
Canvas
├── SidebarUI (GameObject)
│   ├── SidebarUIManager (Script 추가)
│   ├── CustomUI (Script 추가)
│   ├── SidebarUISetup (Script 추가)
│   └── PanelContainer (빈 GameObject)
│       └── VerticalLayoutGroup (컴포넌트 추가)
└── 기타 UI...
```

### 3️⃣ Inspector에서 할당

**SidebarUI**의 **SidebarUISetup** Inspector에서 **4가지를 모두 할당하세요:**

> ⚠️ 할당 전에 SidebarUI에 **SidebarUIManager, CustomUI, SidebarUISetup** 컴포넌트가 모두 추가되어 있어야 합니다.

#### ✅ 필수 할당
- **Sidebar Manager**: 같은 SidebarUI의 `SidebarUIManager` 컴포넌트 할당
- **Custom UI**: 같은 SidebarUI의 `CustomUI` 컴포넌트 할당
- **Panel Prefab**: Prefabs 폴더의 `DataAdjustmentPanel` 프리팹 할당
- **Panel Container**: 자식 `PanelContainer` GameObject 할당

#### 📋 표시할 데이터 선택
```
☑ 성능 정보 표시
☑ 배터리 정보 표시
☐ 연료 정보 표시
☐ 에너지 소비 정보 표시
```

### 4️⃣ UI 자동 생성

**SidebarUISetup** Inspector에서:
```
✅ UI자동생성 ← 클릭!
```

완료! 이제 Play 모드에서 슬라이더로 데이터를 조정할 수 있습니다.

---

## 확장 방법

### 새로운 데이터 제공자 만들기

```csharp
public class CarMyCustomDataProvider : CarDataProviderBase
{
    public override List<string> GetAvailableCategories()
    {
        return new List<string> { "MyCustom" };
    }

    public override List<IDataAdjustable> GetDataFieldsByCategory(string category)
    {
        var fields = new List<IDataAdjustable>();
        CarData car = GetCarData();

        fields.Add(new UIDataField(
            "myField",
            "My Custom Field",
            0,
            100,
            car,
            () => car.MaxSpeed,
            (value) => { /* 값 설정 로직 */ },
            "설명"
        ));

        return fields;
    }

    public override List<IDataAdjustable> GetAllDataFields()
    {
        return GetDataFieldsByCategory("MyCustom");
    }
}
```

### SidebarUISetup에 새 제공자 추가

```csharp
// 1. 필드 추가 (Inspector에서 체크박스로 표시/숨김)
[FoldoutGroup("표시할 데이터")]
[SerializeField]
private bool showMyCustomData = true;

// 2. UI자동생성() 메서드 안에 코드 추가
[Button(ButtonSizes.Large, Icon = SdfIconType.CheckCircle)]
[GUIColor(0.3f, 0.8f, 0.3f)]
public void UI자동생성()
{
    // ... 기존 코드 ...
    
    if (showMyCustomData)
    {
        var provider = GetOrAddComponent<CarMyCustomDataProvider>();
        sidebarManager.CreatePanel("MyCustom", "🎨 내 커스텀 정보", provider);
    }
    
    // ...
}
```

그러면 새로운 패널이 자동으로 생성됩니다!

---

## 주요 기능

✅ **OOP 설계**: 인터페이스와 추상 클래스 기반  
✅ **확장 가능**: 새로운 데이터 제공자 추가 용이  
✅ **인스펙터 친화적**: 버튼 하나로 UI 자동 생성  
✅ **자동 정렬**: 레이아웃 자동 계산 및 정렬  
✅ **실시간 동기화**: CarDataManager 이벤트와 통합  
✅ **모듈화**: 각 클래스가 단일 책임 원칙 준수

---

## 클래스 다이어그램

```
IDataAdjustable (interface)
    ↑
    └─ UIDataField

CarDataProviderBase (abstract)
    ↑
    ├─ CarPerformanceDataProvider
    ├─ CarEnergyDataProvider
    ├─ CarBasicInfoDataProvider
    ├─ CarDrivingDataProvider
    └─ (사용자 정의 제공자들)

DataAdjustmentUIItem (MonoBehaviour)
DataAdjustmentPanel (MonoBehaviour)
SidebarUIManager (MonoBehaviour)
SidebarUISetup (MonoBehaviour)
CustomUI (MonoBehaviour)
```

---

## 팁

- **Getter/Setter**: UIDataField의 getter/setter를 정의할 때 주의해서 값의 범위를 확인하세요
- **카테고리**: 같은 제공자 내에서 여러 카테고리를 제공할 수 있습니다 (예: CarEnergyDataProvider는 "Battery", "Fuel" 제공)
- **프리팹 재사용**: DataAdjustmentPanel와 DataAdjustmentUIItem 프리팹은 여러 인스턴스에서 재사용됩니다

---

## 문제 해결

| 문제 | 원인 | 해결책 |
|------|------|--------|
| UI가 표시되지 않음 | PanelContainer를 할당하지 않음 | Inspector에서 Panel Container 할당 |
| 스크롤이 안 됨 | ScrollView를 추가하지 않음 | Panel Container의 부모에 ScrollView 추가 |
| 데이터가 업데이트되지 않음 | Getter가 제대로 구현되지 않음 | UIDataField의 getter 함수 확인 |
| 레이아웃이 이상함 | LayoutGroup 설정이 잘못됨 | "자동으로 UI 생성 및 정렬"을 다시 실행 |
