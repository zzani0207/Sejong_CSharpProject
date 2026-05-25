# CustomUI / CarDatabase 스크립트 분석 보고서

본 보고서는 `Assets/CustomUI/Script/`와 `Assets/Chanhwi_CarDatabase/Script/` 두 폴더에 포함된 C# 스크립트들의 설계, 객체지향 구조, C# 언어 활용, 디자인 패턴, 그리고 개선 여지를 종합적으로 분석한 문서이다.

---

## 1. 개요

이 시스템은 Unity 환경에서 다음을 목표로 한다.

- **차량 데이터를 `ScriptableObject`로 자산화**하여 디자이너/개발자 협업 친화적으로 관리
- **사이드바 UI에서 차량의 각종 수치(엔진 출력, 배터리, 연료 등)를 슬라이더로 조작**하고 데이터 모델에 즉시 반영
- 패널과 카테고리 구조를 **확장 가능한 형태**로 두어 신규 데이터 카테고리 추가 시 코드 변경 최소화

크게 두 계층으로 나뉜다.

| 계층 | 역할 | 위치 |
|---|---|---|
| 데이터 모델 | 차량 데이터 정의, DB, 런타임 매니저 | `Assets/Chanhwi_CarDatabase/Script/` |
| UI / 제공자 | 사이드바, 패널, 슬라이더 아이템, 데이터 어댑터 | `Assets/CustomUI/Script/` |

---

## 2. 파일 구조

### 2.1 데이터 모델 계층 (`Chanhwi_CarDatabase/Script/`)

| 파일 | 책임 |
|---|---|
| `CarData.cs` | 개별 차량의 모든 속성(성능/연료/배터리/주행/상태)을 가진 `ScriptableObject`. 도메인 로직(`SetChargeLevel`, `SetFuelLevel`, `UpdateRemainingRange` 등) 포함 |
| `CarDataBase.cs` | `CarData` 자산을 모은 데이터베이스 `ScriptableObject`. 인덱스 기반 단일 조회 메서드만 제공 |
| `CarDataManager.cs` | 현재 선택된 차량을 보관하는 런타임 싱글톤. 차량 변경을 `OnCarChanged` 이벤트로 통보 |
| `CarDataCreator.cs` | 에디터 메뉴 헬퍼 - 샘플 7종(테슬라/BMW/포르쉐 등) `CarData` 자산을 일괄 생성 |

### 2.2 UI / 데이터 어댑터 계층 (`CustomUI/Script/`)

| 파일 | 책임 |
|---|---|
| `IDataAdjustable.cs` | 슬라이더로 조절 가능한 단일 필드 인터페이스. 0~1 정규화 값으로 입출력 |
| `UIDataField.cs` | `IDataAdjustable` 구현체. `CarData` 필드를 람다(getter/setter)로 감싸 슬라이더와 연결 |
| `CarDataProviderBase.cs` | 카테고리별 `IDataAdjustable` 목록을 제공하는 추상 클래스 |
| `CarPerformanceDataProvider.cs` | 엔진/속도/가속/토크 |
| `CarEnergyDataProvider.cs` | 배터리/연료/에너지 (한 클래스가 세 카테고리 처리) |
| `CarDrivingDataProvider.cs` | 마일리지/무게 |
| `CustomUI.cs` | 사이드바 가로 너비 토글 + 페이드 (PrimeTween 사용). 키 입력으로 동작 |
| `SidebarUIManager.cs` | 카테고리별 `DataAdjustmentPanel` 생성/제거, 차량 변경 이벤트 구독, 패널 데이터 갱신 |
| `DataAdjustmentPanel.cs` | 한 카테고리의 필드 리스트를 슬라이더 행으로 표시 |
| `DataAdjustmentUIItem.cs` | 라벨 + 슬라이더 + 값 텍스트 1행 단위. 슬라이더 변화를 데이터로, 데이터를 UI로 양방향 동기화 |
| `SidebarUISetup.cs` | 에디터 전용 헬퍼. Odin 인스펙터 버튼으로 사이드바 UI 한 번에 구성 |
| `UIComponentCreator.cs` | 에디터 전용 - DataAdjustmentUIItem / DataAdjustmentPanel 프리팹 기본 형태 자동 생성 메뉴 |

---

## 3. 시스템 아키텍처와 데이터 흐름

### 3.1 의존성 방향

```
ScriptableObject 자산
        │
   ┌────▼─────┐
   │ CarData  │  (값 보관 + 도메인 로직)
   └────┬─────┘
        │
   ┌────▼──────────┐
   │ CarDataBase   │  (자산 컬렉션)
   └────┬──────────┘
        │
   ┌────▼──────────────┐
   │ CarDataManager    │
   │  (싱글톤, 런타임) ├─── OnCarChanged ───┐
   └────┬──────────────┘                    │
        │                                   ▼
        │                       ┌────────────────────────┐
        └──────────────────────►│ SidebarUIManager        │
                  현재 차량 참조  │  - panelContainer       │
                                │  - activePanels         │
                                │  - dataProviders        │
                                └────┬───────────────────┘
                                     │
                                     ▼
                       ┌──────────────────────────────┐
                       │ CarDataProviderBase 자식들    │
                       │  카테고리 → IDataAdjustable[] │
                       └────┬─────────────────────────┘
                            │
                            ▼
                    UIDataField (getter/setter 람다)
                            │
                            ▼
               DataAdjustmentPanel → DataAdjustmentUIItem
               (TitleText, ItemContainer)   (Label/Slider/Value)
```

### 3.2 핵심 시나리오: 사용자가 슬라이더를 드래그할 때

1. 사용자가 `DataAdjustmentUIItem`의 `Slider` 컴포넌트를 드래그
2. `DataAdjustmentUIItem.Update`가 매 프레임 `slider.value`의 변화를 polling
3. 변화 감지 시 `dataField.SetNormalizedValue(slider.value)` 호출
4. `UIDataField`가 정규화 값을 `[min, max]` 범위로 변환한 뒤 `setter` 람다 호출
5. 람다 안에서 `carData.SetEnginePower(value)` 등 `CarData` 메서드 호출
6. `CarData`가 자기 내부 필드 갱신 (필요 시 `UpdateRemainingRange()`로 파생 값 재계산)
7. 다음 프레임 `Update`에서 `getter`로 갱신된 값을 읽어 `valueText`에 표시

---

## 4. 객체지향적 설계 분석

### 4.1 추상화 (`abstract class`)

`CarDataProviderBase`는 카테고리별 데이터 어댑터의 **공통 계약**을 정의한다.

```csharp
public abstract class CarDataProviderBase : MonoBehaviour
{
    protected CarData carData;
    public virtual void SetCarData(CarData car) { carData = car; }
    public abstract List<IDataAdjustable> GetAllDataFields();
    public abstract List<IDataAdjustable> GetDataFieldsByCategory(string category);
    public abstract List<string> GetAvailableCategories();
    protected CarData GetCarData() { /* fallback to CarDataManager */ }
}
```

세 개의 `abstract` 메서드로 자식 클래스가 반드시 구현해야 할 인터페이스를 강제하고, `SetCarData`는 `virtual`로 두어 필요 시 오버라이드를 허용했다. `GetCarData()`는 `protected`로 두어 자식에게만 노출.

### 4.2 인터페이스 (`interface`)

`IDataAdjustable`은 UI(`DataAdjustmentUIItem`)와 데이터 모델 사이의 **얇은 결합층** 역할이다.

```csharp
public interface IDataAdjustable
{
    string FieldId { get; }
    string DisplayName { get; }
    float GetValue();
    float GetMinValue();
    float GetMaxValue();
    float GetNormalizedValue();
    void SetNormalizedValue(float normalizedValue);
    bool IsActive();
}
```

UI는 `IDataAdjustable`만 알고 있고, 그것이 실제로 `CarData`의 어떤 필드를 가리키는지는 모른다. `UIDataField`가 이 변환을 담당하므로 향후 `CarData`가 아닌 다른 모델(예: `BikeData`)로도 같은 UI 컴포넌트를 재사용할 수 있다.

### 4.3 상속/다형성

`CarDataProviderBase`를 상속하는 3개의 구체 클래스(`CarPerformanceDataProvider`, `CarEnergyDataProvider`, `CarDrivingDataProvider`)가 있고, `SidebarUIManager`는 **부모 타입으로만** 참조한다.

```csharp
private Dictionary<string, CarDataProviderBase> dataProviders;
```

런타임에는 `CarPerformanceDataProvider`이건 `CarEnergyDataProvider`이건 동일한 인터페이스(`GetDataFieldsByCategory`)로 호출한다. 카테고리 추가 시 새 자식 클래스만 만들면 매니저 코드는 한 줄도 바뀌지 않는다. **개방-폐쇄 원칙(OCP)** 의 좋은 사례.

### 4.4 캡슐화

- `CarData`의 모든 데이터 필드는 `private`로 두고 `[SerializeField]`로만 인스펙터 노출. 외부에서는 read-only `=>` property와 의도적으로 노출한 `SetXxx` 메서드로만 접근.
- `CarDataManager.instance`는 `private static`, 접근은 `public static Instance` property로 제한.
- `SidebarUIManager.PanelInfo`는 내부 사용 전용이므로 `private class`로 감춤.

### 4.5 합성 (Composition)

`DataAdjustmentPanel`은 `DataAdjustmentUIItem`을 동적으로 생성/소유한다.
`SidebarUIManager`는 `CarDataProviderBase`들을 dictionary로 보관하고 `DataAdjustmentPanel`을 컨트롤한다.
상속보다 합성을 우선으로 한 구조이며 SRP(단일 책임 원칙)에 가깝다.

---

## 5. C# 언어 문법 활용

### 5.1 Expression-bodied members

`=>` 단축 구문이 read-only property/단일 라인 메서드 전반에 사용된다.

```csharp
public string CarName => carName;
public float RemainingFuelRange => remainingFuelRange;
public void SetEnginePower(float v) => enginePower = Mathf.Max(0, v);
public bool IsActive() => isActive;
```

### 5.2 Switch 문 / 폴스루

`CarData.UpdateRemainingRange()`에서 fuel type별 분기를 case 폴스루로 처리, 동일 로직 중복을 제거했다.

```csharp
switch (fuelType)
{
    case FuelType.Electric:
        remainingBatteryRange = (currentChargeLevel / 100f) * (batteryCapacity / energyConsumption * 100f);
        break;

    case FuelType.Gasoline:
    case FuelType.Diesel:
        remainingFuelRange = currentFuelLevel * fuelConsumption;
        break;

    case FuelType.Hybrid:
    case FuelType.PHEV:
        remainingFuelRange = currentFuelLevel * fuelConsumption;
        remainingBatteryRange = (currentChargeLevel / 100f) * (batteryCapacity / energyConsumption * 100f);
        break;
}
```

### 5.3 Delegate / Lambda (Func, Action)

`UIDataField`는 getter/setter를 `Func<float>` / `Action<float>`으로 받아 차량 필드와 슬라이더를 1행 짜리 람다로 연결한다.

```csharp
new UIDataField(
    "enginePower", "Engine Power (kW)", 0, 1000,
    () => carData != null ? carData.EnginePower : 0f,
    (value) => { if (carData != null) carData.SetEnginePower(value); }
);
```

각 필드마다 별도의 클래스를 만들 필요가 없어 보일러플레이트가 크게 줄었다.

### 5.4 Event / Action 기반 알림

`CarDataManager`는 차량 변경 이벤트 한 종을 노출한다.

```csharp
public event Action<CarData> OnCarChanged;
```

구독자(`SidebarUIManager`)는 차량이 바뀔 때 자동으로 UI를 재초기화한다. 별도의 폴링/수동 호출이 필요 없도록 만든 점이 좋다.

### 5.5 Pattern matching

타입 체크와 변수 선언을 한 번에:

```csharp
if (panelContainer.parent is RectTransform parentRect)
    availableHeight = parentRect.rect.height;
```

### 5.6 Generics

`SidebarUISetup.GetOrAddComponent<T>` 같은 헬퍼가 타입 안전하게 동작.

```csharp
private T GetOrAddComponent<T>() where T : Component
{
    var component = GetComponent<T>();
    if (component == null) component = gameObject.AddComponent<T>();
    return component;
}
```

### 5.7 Reflection

`CarDataCreator`는 `CarData`의 `private` 필드에 일괄 값을 주입하기 위해 `System.Reflection`을 사용. 에디터 자산 일괄 생성 같은 일회성 작업에 적합한 선택이다.

### 5.8 null 조건 연산자와 명시적 null 체크의 혼용

Unity의 fake-null(파괴된 `Object`도 `== null`로 평가되지만 `?.`은 우회) 이슈 때문에 `UnityEngine.Object` 계열은 **명시적 `!= null` 체크**, 일반 C# 객체(`Func`, `Action`)는 `?.`로 일관성 있게 분리되어 있다.

```csharp
setter?.Invoke(actualValue);                   // Action - ?. OK
if (carData != null) carData.SetX(value);      // ScriptableObject - 명시적
```

---

## 7. 디자인 패턴

### 6.1 Singleton — `CarDataManager`

```csharp
private static CarDataManager instance;
public static CarDataManager Instance { get { /* editor + runtime 모두 처리 */ } }
```

Play 모드에서는 `Awake`에서 등록, 에디트 모드에서는 `FindAnyObjectByType`로 폴백하는 점이 특이하다. 전형적인 게임 싱글톤 활용.

### 6.2 Observer — Event 기반 알림

`CarDataManager`가 publisher, `SidebarUIManager.OnCarChanged`가 subscriber. 구독자는 자기 생명주기에 맞춰 `+=`/`-=`로 자유롭게 들어왔다 나간다.

### 6.3 Strategy — `CarDataProviderBase`

같은 인터페이스(`GetDataFieldsByCategory`)에 대해 카테고리 종류별로 다른 구현을 제공. `SidebarUIManager`는 어떤 Provider가 어떤 데이터를 만드는지 알 필요가 없다.

### 6.4 Template Method (약한 형태)

`CarDataProviderBase.GetCarData()`는 자식이 매번 차량 참조를 얻을 때 공통적인 폴백 로직(`carData == null이면 CarDataManager에서 가져오기`)을 제공한다. 자식 클래스는 그 결과만 쓰면 된다.

### 6.5 Adapter — `UIDataField`

`CarData`의 다양한 타입(float, int)을 모두 `IDataAdjustable`의 단일한 `float` 인터페이스로 적응시킨다.

```csharp
drivingFields.Add(new UIDataField(
    "mileage", "Mileage (km)", 0, 500000,
    () => carData != null ? carData.Mileage : 0f,
    (value) => { if (carData != null) carData.SetMileage(value); }
));
```

`int` 필드(예: `ProductionYear`)를 노출할 때도 `(float)`/`Mathf.RoundToInt` 캐스팅을 람다 안에 캡슐화해 단일 인터페이스로 노출한다.

### 6.6 ScriptableObject 자산화

`CarData`, `CarDataBase` 모두 `[CreateAssetMenu]`로 자산 생성 가능. 디자이너가 코드 없이 인스펙터에서 데이터 추가, 차량 종류 확장 가능.

### 6.7 Editor Tooling 패턴

`UIComponentCreator`, `CarDataCreator`, `SidebarUISetup` 모두 `#if UNITY_EDITOR` 가드 + `[MenuItem]` 또는 Odin `[Button]`으로 에디터 자동화. 런타임 빌드에는 포함되지 않는다.

---

## 8. 잘 설계된 부분

1. **계층 분리**: 데이터 모델(`CarData`/`CarDataBase`/`CarDataManager`) ↔ UI(`SidebarUIManager` 외) ↔ 어댑터(`Provider`/`UIDataField`)의 책임이 명확하다.
2. **확장성**: 카테고리 추가 = `CarDataProviderBase` 자식 1개 + `SidebarUISetup`에 `bool show...` + `CreatePanel` 한 줄. 다른 코드 수정 불필요.
3. **인스펙터 친화성**: Odin의 `FoldoutGroup`/`Button`/`GUIColor`로 비개발자도 한 번에 UI 구성 가능. `Tooltip`까지 적절히 사용.
4. **자산 시드 자동화**: `CarDataCreator`의 메뉴 한 번이면 7종 샘플 차량이 자산으로 생성되어 초기 셋업 부담이 적다.
5. **이벤트 기반 차량 변경 알림**: 차량을 갈아치울 때 UI 측이 자동으로 갱신되어 폴링/수동 호출이 필요 없다.
6. **사용 흐름만 남기는 슬림한 API**: 미사용 메서드/이벤트/필드를 적극적으로 정리하여 `CarDataManager`는 `OnCarChanged` + `CurrentCar` + `SelectCar`만, `CarDataBase`는 `GetCar(int)`만 노출. 외부에서 잘못 호출할 수 있는 표면적이 작다.

---

## 9. 아쉬운 점

### 8.1 도메인 침범 - `VehicleOverviewPanel` 관련 enum이 `CarData`에 혼재

```csharp
public enum VehicleConnectionStatus { Connected, Disconnected, Syncing, Error }
public enum VehicleSystemStatus { Normal, Warning, Critical, Offline }
public enum VehicleWarningType { ... }
```

데이터 모델 파일에 UI 위젯 전용 enum이 같이 정의되어 있어 응집도가 떨어진다. 별도 파일(`VehicleStatus.cs` 등)로 분리되는 게 맞다.

### 8.2 슬라이더 ↔ 데이터 동기화가 매 프레임 폴링 방식

`DataAdjustmentUIItem.Update`에서 매 프레임 `slider.value` 변화를 감시한다.

```csharp
private void Update()
{
    if (dataField == null || valueSlider == null) return;
    float currentSliderValue = valueSlider.value;
    if (!isUpdatingUI && !Mathf.Approximately(currentSliderValue, lastSliderValue))
    { ... }
    UpdateValueDisplay();
}
```

원래는 `Slider.onValueChanged` 이벤트로 처리하는 것이 표준이지만, 프리팹에서 SerializeField 누락 같은 환경 이슈로 listener가 등록 안 되는 케이스를 우회하느라 폴링으로 변경되었다. UI 아이템이 많아질수록 미세하게 비효율이며, 본질적인 원인(프리팹 참조 문제)을 코드로 가린 형태다.

### 8.3 카테고리 키가 문자열 — 컴파일 타임 안전성 약함

```csharp
sidebarManager.CreatePanel("Performance", "Performance", perfProvider);
sidebarManager.CreatePanel("Battery", "Battery", energyProvider);
```

오타가 나도 컴파일러가 잡지 못한다. `enum` 또는 `static readonly` 상수로 두는 편이 안전.

### 8.4 `SidebarUIManager.RegisterDataProvider`의 호출 시점 이슈

```csharp
public void RegisterDataProvider(string providerName, CarDataProviderBase provider)
{
    dataProviders[providerName] = provider;
    provider.SetCarData(currentCarData);   // ← 등록 시점에 currentCarData가 null일 수 있음
}
```

에디트 모드에서 `SidebarUISetup.UI자동생성()`을 누른 시점에는 `currentCarData`가 null이라 provider의 `carData`도 null로 시작. 다행히 Play 모드 진입 시 `SidebarUIManager.Start`에서 `OnCarChanged(CurrentCar)`를 한 번 호출해 보완하지만, 두 경로에 의존하는 부분이 명시적이지 않다.

### 8.5 `CarEnergyDataProvider` 하나가 세 카테고리(Energy/Battery/Fuel) 담당

```csharp
public override List<string> GetAvailableCategories()
    => new List<string> { "Energy", "Battery", "Fuel" };
```

`switch (category)`로 분기. 한 클래스가 단일 책임에서 벗어나 있고, 카테고리 추가 시 같은 클래스를 계속 수정해야 한다. **Open-Closed Principle 위반**.

### 8.6 에디트 모드와 런타임 모드 처리가 분산

`CarDataManager.Instance`에 에디트 모드/Play 모드 분기가 있고, `SidebarUIManager.Start`도 Play 진입 시 OnCarChanged를 자기 자신이 직접 호출해 첫 차량 동기화를 보완하는 등 모드별 로직이 여러 곳에 분산되어 있다.

### 8.7 GameObject 부착 위치에 강하게 결합된 자동 컴포넌트 부착

```csharp
var perfProvider = GetOrAddComponent<CarPerformanceDataProvider>();
```

`SidebarUISetup`이 부착된 GameObject에 데이터 제공자들이 같이 부착된다. UI/데이터 어댑터의 책임이 GameObject 한 곳에 묶이는 구조. 사이드바와 데이터 어댑터가 위치적으로 분리 어렵다.

### 8.8 슬라이더 참조 자동 보강이 의도를 가린다

`DataAdjustmentUIItem.EnsureSliderRef`는 `GetComponentInChildren` + `GetComponentInParent`까지 시도한다. 견고하긴 하지만 프리팹 구조 변경 시 어디가 잡힐지 추적이 어렵고, 프리팹 작성 가이드를 코드 자동화로 대체해버린 형태가 되었다.

---

## 6. CardDatabase / CustomUI 시스템의 객체지향 설계

본 절에서는 다섯 가지 객체지향 설계 원칙(추상화, 캡슐화, 상속, 다형성, 책임 분리)이 CardDatabase와 CustomUI 시스템에서 어떻게 적용되었는지 살펴본다.

### 6.1 추상화

본 시스템에서는 데이터 조정 기능을 추상화하여 UI가 구체적인 데이터 모델을 알 필요 없이 동작하도록 설계하였다.

**`IDataAdjustable` 인터페이스를 통한 필드 추상화**

모든 조정 가능한 데이터 필드는 `IDataAdjustable` 인터페이스를 구현한다. UI(특히 `DataAdjustmentUIItem`)는 이 인터페이스만 알고, 실제로는 `CarData`의 어떤 필드인지는 알 필요가 없다.

```csharp
public interface IDataAdjustable
{
    string FieldId { get; }
    string DisplayName { get; }
    float GetValue();
    float SetNormalizedValue(float value);  // 0~1 정규화 범위
    float GetNormalizedValue();
    // ...
}
```

이를 통해 `CarData`의 필드(엔진 파워, 배터리 용량 등)가 변경되어도 UI 컴포넌트는 수정할 필요가 없다. 또한 향후 `BikeData`, `TruckData` 같은 다른 데이터 모델을 추가할 때도 같은 UI를 재사용할 수 있다.

**`CarDataProviderBase` 추상 클래스를 통한 제공자 추상화**

각 Provider는 `CarDataProviderBase`를 상속하여 공통의 추상 메서드를 구현해야 한다.

```csharp
public abstract class CarDataProviderBase : MonoBehaviour
{
    public abstract List<IDataAdjustable> GetAllDataFields();
    public abstract List<IDataAdjustable> GetDataFieldsByCategory(string category);
    public abstract List<string> GetAvailableCategories();
}
```

`SidebarUIManager`는 구체적인 Provider 클래스(CarPerformanceDataProvider, CarEnergyDataProvider 등)를 직접 참조하지 않고, 부모 타입의 인터페이스만으로 작동한다. 이로 인해 새로운 데이터 카테고리 추가 시 매니저 코드는 전혀 변경되지 않는다.

### 6.2 캡슐화

각 클래스는 자신이 관리해야 하는 데이터와 상태를 내부에 엄격히 제한하고, public 메서드/프로퍼티를 통해서만 제어된다.

**`CarData`의 데이터 보호**

모든 필드는 `private` 또는 `[SerializeField]`로 선언되어 외부 직접 접근을 차단한다.

```csharp
[SerializeField] private float enginePower = 450f;
[SerializeField] private float currentChargeLevel = 80f;

// 외부는 read-only property로만 접근 가능
public float EnginePower => enginePower;
public float CurrentChargeLevel => currentChargeLevel;

// 의도된 변경만 public 메서드로 노출
public void SetEnginePower(float v) => enginePower = Mathf.Max(0, v);
public void SetChargeLevel(float c) => /* 범위 검증 + UpdateRemainingRange */ ...
```

이를 통해 `CarData`의 내부 일관성(예: 음수 값 방지, 배터리 충전도와 주행거리의 자동 동기화)이 보장된다.

**`CarDataManager`의 싱글톤 캡슐화**

```csharp
private static CarDataManager instance;
public static CarDataManager Instance
{
    get
    {
        if (instance == null)
            instance = FindObjectOfType<CarDataManager>();
        return instance;
    }
}

private void Awake()
{
    if (instance == null) { instance = this; }
}
```

싱글톤 인스턴스는 `private static`으로 보호되며, 생성/파괴 로직을 `Awake`에 캡슐화하여 외부에서 직접 생성하거나 삭제할 수 없다.

**`SidebarUIManager`의 내부 상태 숨김**

```csharp
private Dictionary<string, PanelInfo> activePanels = new();
private Dictionary<string, CarDataProviderBase> dataProviders = new();
private CarData currentCarData;

// 외부는 public 메서드(CreatePanel, RegisterDataProvider 등)로만 관리
public void CreatePanel(string categoryName, string title, CarDataProviderBase provider) { ... }
public void RegisterDataProvider(string name, CarDataProviderBase provider) { ... }
```

패널과 Provider의 내부 상태는 외부 수정으로부터 보호되고, 오직 매니저의 public 메서드를 통해서만 변경 가능하다.

### 6.3 상속

본 시스템의 주요 스크립트들은 Unity의 생명주기를 활용하기 위해 `MonoBehaviour`를 상속한다.

- `CarDataManager`: MonoBehaviour 상속 → `Awake`, `Start`에서 싱글톤 초기화 및 이벤트 구독
- `SidebarUIManager`: MonoBehaviour 상속 → `Start`에서 `CarDataManager.OnCarChanged` 구독, 패널 동적 생성 관리
- `CarDataProviderBase`: MonoBehaviour 상속 → `GetComponent<CarPerformanceDataProvider>()` 등으로 GameObject에 부착 가능
- `DataAdjustmentPanel`: MonoBehaviour 상속 → LayoutGroup/ContentSizeFitter와 협력하여 UI 레이아웃 자동 정렬
- `DataAdjustmentUIItem`: MonoBehaviour 상속 → `Update`에서 매 프레임 슬라이더 변화를 감시

또한 `CarDataProviderBase`는 추상 클래스로서 자식 클래스들(`CarPerformanceDataProvider`, `CarEnergyDataProvider`, `CarDrivingDataProvider`)에게 공통의 필드(`protected CarData carData`)와 구현 가능한 메서드(`SetCarData`, `GetCarData`)를 제공한다.

### 6.4 다형성

인터페이스와 추상 클래스를 통한 다형성이 시스템 전역에서 유연성을 제공한다.

**`IDataAdjustable` 다형성**

`UIDataField`는 `IDataAdjustable`의 구현체이며, 같은 인터페이스로 여러 데이터 타입을 처리한다.

```csharp
// 엔진 파워 슬라이더
new UIDataField("enginePower", "Engine Power", 0, 1000,
    () => carData.EnginePower,
    (v) => carData.SetEnginePower(v)
);

// 배터리 충전도 슬라이더  
new UIDataField("chargeLevel", "Battery Level", 0, 100,
    () => carData.CurrentChargeLevel,
    (v) => carData.SetChargeLevel(v)
);

// 마일리지 슬라이더
new UIDataField("mileage", "Mileage", 0, 500000,
    () => carData.Mileage,
    (v) => carData.SetMileage(v)
);
```

모두 같은 `IDataAdjustable` 인터페이스로 취급되지만, 각자 다른 게터/세터 람다를 가지고 있어 서로 다르게 동작한다.

**`CarDataProviderBase` 다형성**

`SidebarUIManager`는 여러 Provider를 dictionary로 관리하되, 모두 `CarDataProviderBase` 부모 타입으로 참조한다.

```csharp
private Dictionary<string, CarDataProviderBase> dataProviders;

foreach (var provider in dataProviders.Values)
    provider.SetCarData(newCar);  // 실제 타입이 무엇이든 같은 메서드로 호출
```

런타임에 `CarPerformanceDataProvider.GetDataFieldsByCategory("Performance")`를 호출하든 `CarEnergyDataProvider.GetDataFieldsByCategory("Battery")`를 호출하든, 매니저는 `CarDataProviderBase` 메서드만 본다. 따라서 새로운 Provider 자식 클래스가 추가되어도 매니저 코드는 변하지 않는다.

### 6.5 책임 분리

본 시스템의 가장 중점적인 설계 원칙은 단일 책임 원칙(Single Responsibility Principle, SRP)이다. 각 클래스는 하나의 명확한 책임만을 진다.

| 클래스 | 책임 |
|---|---|
| **CarData** | 개별 차량의 속성(성능, 에너지, 주행)을 저장하고, 도메인 로직(SetChargeLevel, UpdateRemainingRange 등)을 실행 |
| **CarDataBase** | CarData 자산들의 컬렉션을 관리하고, 인덱스로 차량을 조회 제공 |
| **CarDataManager** | 현재 선택된 차량을 메모리에 유지하고, 차량 변경을 이벤트로 알림 |
| **IDataAdjustable** | 슬라이더로 조작 가능한 단일 필드의 계약을 정의 |
| **UIDataField** | 특정 차량 필드(예: enginePower)를 0~1 정규화 범위로 변환하여 슬라이더와 연결 |
| **CarDataProviderBase (및 자식들)** | 특정 카테고리(Performance, Battery, Fuel 등)의 필드 목록을 IDataAdjustable 배열로 제공 |
| **SidebarUIManager** | 다양한 Provider로부터 받은 필드들을 Panel 단위로 동적 생성/삭제하고, 차량 변경 시 모든 Panel 갱신 |
| **DataAdjustmentPanel** | 한 카테고리의 필드 리스트(IDataAdjustable[])를 받아 슬라이더 UI 행들을 동적 생성 |
| **DataAdjustmentUIItem** | 단일 필드(IDataAdjustable)를 라벨 + 슬라이더 + 값 텍스트 한 줄로 표시하고, 슬라이더 조작을 데이터 변경으로 변환 |

예를 들어, `CarPerformanceDataProvider`는 엔진 파워, 최대 속도, 토크, 가속도 데이터만을 담당한다. 배터리 정보는 `CarEnergyDataProvider`가, 마일리지는 `CarDrivingDataProvider`가 각각 담당한다. 이로 인해:

- 새로운 카테고리 추가 시 기존 Provider는 수정되지 않음 (OCP)
- 각 Provider가 작고 이해하기 쉬움 (SRP)
- 테스트할 때 필요한 기능만 격리 가능 (테스트 용이성)

---

## 9. 개선 제안

### 9.1 카테고리 키를 상수/enum으로

```csharp
public static class PanelCategories
{
    public const string Performance = "Performance";
    public const string Battery = "Battery";
    public const string Fuel = "Fuel";
}
```

또는 `enum` + 확장 메서드로 디스플레이 문자열 변환.

### 9.2 Provider를 카테고리당 한 클래스로 쪼개기

`CarEnergyDataProvider` → `CarBatteryDataProvider`, `CarFuelDataProvider`, `CarEnergyConsumptionDataProvider`로 분리. 각 클래스는 단일 카테고리만 담당.

### 9.3 데이터 변경을 이벤트로 통보 — Polling 제거

`CarData`에 `event Action OnAnyValueChanged`를 두고 `SetXxx` 메서드들이 발화. `DataAdjustmentUIItem`은 그 이벤트를 구독하여 `valueText` 갱신, 슬라이더 변경 시에만 `Slider.onValueChanged`로 setter 호출. 매 프레임 `Update` 폴링 제거 가능.

### 9.4 의존성 주입 (DI)

현재 `CarDataManager.Instance` 정적 참조를 직접 사용. 대신 생성자/세터 주입 또는 Zenject 같은 컨테이너로 받으면 테스트 가능성이 크게 올라간다.

### 9.5 `ScriptableObject` 변경 시 자동 dirty 처리

```csharp
public void SetEnginePower(float v)
{
    enginePower = Mathf.Max(0, v);
#if UNITY_EDITOR
    UnityEditor.EditorUtility.SetDirty(this);
#endif
}
```

에디트 모드에서 슬라이더로 값 변경 시 `.asset` 파일에 즉시 저장되도록.

### 9.6 Editor 코드와 런타임 코드 분리

`UIComponentCreator`, `CarDataCreator` 같은 에디터 헬퍼는 `Editor/` 하위 폴더로 옮기고 별도 `.asmdef`로 묶으면 빌드 시 자동 제외 + 컴파일 시간 단축.

### 9.7 프리팹 작성 가이드를 통한 자동 보강 약화

`DataAdjustmentUIItem.EnsureSliderRef`의 자동 검색은 안전망일 뿐, 프리팹에서 `valueSlider` SerializeField를 명시적으로 지정하는 것이 정석. 자동 검색은 `LogWarning` 한 줄로 줄이고 프리팹 검수 체크리스트를 README에 명시.

### 9.8 도메인 enum 분리

`VehicleConnectionStatus`, `VehicleSystemStatus`, `VehicleWarningType`을 `VehicleStatus.cs` 같은 별도 파일로.

### 9.9 단위 테스트 도입

`CarData`의 도메인 로직(`UpdateRemainingRange`, `SetChargeLevel`의 fuel type 제약 등)은 순수 함수에 가까워 EditMode 테스트로 검증하기 좋다. 현재 자동화된 검증이 없다.

---

## 10. 종합 평가

| 항목 | 평가 |
|---|---|
| 계층 분리 | 양호 - 데이터/어댑터/UI가 명확히 나뉨 |
| 확장성 | 양호 - 새 Provider 추가 시 매니저 수정 불필요 |
| C# 문법 활용도 | 우수 - 폴스루 switch / pattern matching / 람다 / 제네릭 적극 활용 |
| 디자인 패턴 인지 | 우수 - Singleton, Observer, Strategy, Adapter가 의도적으로 적용됨 |
| API 표면적 | 양호 - 미사용 메서드/이벤트를 적극적으로 정리해 표면적이 작음 |
| 견고성 | 보통 - 자동 보강 / 폴백 로직이 많아 동작은 하지만 의도 추적이 어려운 부분 존재 |
| 유지보수성 | 보통 - 문자열 카테고리 키, 한 Provider가 다중 카테고리 담당 등 OCP 약화 |
| 테스트 가능성 | 낮음 - 정적 의존성(`CarDataManager.Instance`)과 Unity 결합도가 강함 |

전반적으로 객체지향과 디자인 패턴의 핵심 개념(추상화/다형성/Observer/Strategy/Adapter)을 잘 의식하고 작성된 코드이다. 다만 실용성에 무게가 실리면서 도메인 분리, 단일 책임, 테스트 용이성에 절충이 있었다. 개선 제안 항목들을 단계적으로 적용하면 학습용 프로젝트를 넘어 실무 코드베이스로 발전시킬 수 있는 토대가 충분하다.

---

*분석 대상 경로:*

- `Assets/Chanhwi_CarDatabase/Script/` — `CarData.cs`, `CarDataBase.cs`, `CarDataManager.cs`, `CarDataCreator.cs`
- `Assets/CustomUI/Script/` — `CustomUI.cs`, `SidebarUIManager.cs`, `SidebarUISetup.cs`, `DataAdjustmentPanel.cs`, `DataAdjustmentUIItem.cs`, `IDataAdjustable.cs`, `UIDataField.cs`, `CarDataProviderBase.cs`, `CarPerformanceDataProvider.cs`, `CarEnergyDataProvider.cs`, `CarDrivingDataProvider.cs`, `UIComponentCreator.cs`
