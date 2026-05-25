# 데이터베이스에서 UI까지 — 데이터 흐름 문서

본 문서는 차량 데이터가 ScriptableObject 자산에서 시작해 사이드바 UI의 슬라이더와 텍스트에 표시되기까지의 전체 경로를 단계별로 정리한 자료다.

---

## 0. 한눈에 보기

```
[.asset 파일]                                                [화면 슬라이더]
     │                                                              ▲
     │ 인스펙터에서                                                  │
     │ 자산 컬렉션 구성                                              │ valueText / slider.value
     ▼                                                              │
┌──────────────┐    GetCar(0)    ┌────────────────┐    OnCarChanged    ┌────────────────────┐
│ CarDataBase  │ ───────────►   │ CarDataManager │ ──── event ─────► │ SidebarUIManager   │
│ (자산 컬렉션)│                 │ (싱글톤)       │                   │ (구독자)           │
└──────────────┘                 └────────┬───────┘                   └────────┬───────────┘
                                          │ CurrentCar                         │ provider.SetCarData
                                          ▼                                    ▼
                                ┌────────────────────────┐         ┌──────────────────────────┐
                                │ CarData (현재 선택된 차)│ ◄──────│ CarDataProviderBase 자식들│
                                │   - enginePower         │ getter │   - GetDataFieldsByCategory│
                                │   - currentChargeLevel  │  람다  │   - SetCarData            │
                                │   - ...                 │ setter │                          │
                                └────────────────────────┘  람다  └────────────┬─────────────┘
                                                                                │ IDataAdjustable 리스트
                                                                                ▼
                                                                  ┌─────────────────────────┐
                                                                  │ DataAdjustmentPanel     │
                                                                  │  Initialize(title, [..])│
                                                                  └────────────┬────────────┘
                                                                                │ Instantiate UIItem
                                                                                ▼
                                                                  ┌─────────────────────────┐
                                                                  │ DataAdjustmentUIItem    │
                                                                  │  - labelText            │
                                                                  │  - valueSlider          │
                                                                  │  - valueText            │
                                                                  └─────────────────────────┘
```

다섯 단계로 묶으면:

1. **자산 단계** — `.asset` 파일에 데이터를 보관
2. **런타임 진입 단계** — `CarDataManager`가 현재 차를 결정하고 이벤트 발화
3. **어댑터 단계** — Provider가 `CarData` 필드들을 `IDataAdjustable`로 감싸기
4. **UI 생성 단계** — Panel이 슬라이더 행(Item)들을 동적 생성
5. **표시/입력 단계** — Item이 매 프레임 데이터와 UI를 양방향 동기화

---

## 1. 자산 단계 — ScriptableObject로서의 데이터

### 1.1 개별 차량 = `CarData.asset`

`CarData`는 `ScriptableObject`를 상속하고 `[CreateAssetMenu]`로 자산 생성 메뉴에 등록되어 있다.

```csharp
[CreateAssetMenu(fileName = "Car_", menuName = "Scriptable Objects/Car Data")]
public class CarData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string carName;
    [SerializeField] private FuelType fuelType;

    [Header("성능 정보")]
    [SerializeField] private float enginePower = 450f;
    ...
}
```

각 차량(테슬라 모델 S, BMW M4 등)이 별도의 `.asset` 파일로 저장된다. 디자이너는 인스펙터에서 값을 편집할 수 있다.

> 7종 샘플 자산은 `Tools > Car Database > Create Sample Cars` 메뉴([CarDataCreator.cs](../Script/CarDataCreator.cs))로 일괄 생성된다.

### 1.2 자산 컬렉션 = `CarDatabase.asset`

`CarDataBase`도 `ScriptableObject`이며, 여러 `CarData` 자산을 배열로 보관한다.

```csharp
[CreateAssetMenu(fileName = "CarDatabase", menuName = "Scriptable Objects/Car Database")]
public class CarDataBase : ScriptableObject
{
    [SerializeField] private CarData[] cars;

    public CarData GetCar(int index) { ... }
}
```

`CarDatabase.asset` 하나에 여러 `CarData` 참조가 들어 있고, 인스펙터에서 추가/제거할 수 있다.

---

## 2. 런타임 진입 단계 — `CarDataManager`

### 2.1 데이터베이스 로드

`CarDataManager`는 씬에 배치된 MonoBehaviour 싱글톤이다. `Awake`에서 `CarDataBase` 자산을 참조한다.

```csharp
private void Awake()
{
    if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
    else { Destroy(gameObject); return; }

    if (carDatabase == null)
        carDatabase = Resources.Load<CarDataBase>("CarDatabase");

    SelectCar(0);
}
```

- 인스펙터에 직접 할당했다면 그대로 사용
- 비어 있으면 `Resources/CarDatabase.asset`을 런타임에 로드

### 2.2 첫 차량 선택과 이벤트 발화

`SelectCar(0)`이 호출되면 데이터베이스의 0번 차량이 `currentCar`로 들어가고, 구독자에게 알린다.

```csharp
public void SelectCar(int index)
{
    CarData newCar = carDatabase.GetCar(index);
    if (newCar != null)
    {
        currentCar = newCar;
        OnCarChanged?.Invoke(currentCar);
    }
}
```

`CarDataManager`가 외부로 노출하는 것은 **세 가지뿐**:

| 노출 항목 | 의미 |
|---|---|
| `Instance` | 싱글톤 접근점 |
| `CurrentCar` | 현재 선택된 `CarData` |
| `OnCarChanged` | 차량 변경 이벤트 (`Action<CarData>`) |

UI 측에서 필요한 것이 이것뿐이라 다른 메서드는 다 정리됐다.

---

## 3. 어댑터 단계 — Provider와 `UIDataField`

`CarData`의 모든 필드는 `private` + read-only property라 UI가 직접 접근할 수 없다. 그래서 **어댑터 계층**이 필요하다.

### 3.1 `CarDataProviderBase` — 카테고리별 어댑터

각 Provider는 카테고리 이름을 받으면 그 카테고리에 속하는 필드들을 `IDataAdjustable` 리스트로 반환한다.

```csharp
public abstract class CarDataProviderBase : MonoBehaviour
{
    protected CarData carData;
    public virtual void SetCarData(CarData car) { carData = car; }
    public abstract List<IDataAdjustable> GetDataFieldsByCategory(string category);
    public abstract List<string> GetAvailableCategories();
}
```

자식 클래스:

- `CarPerformanceDataProvider` → `"Performance"` 카테고리: 엔진/속도/가속/토크
- `CarEnergyDataProvider` → `"Battery" / "Fuel" / "Energy"` 세 카테고리
- `CarDrivingDataProvider` → `"Driving"` 카테고리: 마일리지/무게

### 3.2 `UIDataField` — 필드 한 개를 감싸는 IDataAdjustable

`CarData.EnginePower`처럼 특정 필드를 슬라이더에 노출하려면, getter와 setter를 람다로 작성해서 `UIDataField`에 주입한다.

```csharp
performanceFields.Add(new UIDataField(
    "enginePower",                      // 식별자
    "Engine Power (kW)",                // 라벨에 표시될 이름
    0, 1000,                            // 슬라이더 min, max
    () => carData != null ? carData.EnginePower : 0f,                // getter
    (value) => { if (carData != null) carData.SetEnginePower(value); } // setter
));
```

핵심:

- **getter 람다**는 매번 호출 시 `carData`의 현재 값을 읽는다. UI는 이걸로 화면을 갱신한다.
- **setter 람다**는 슬라이더가 움직이면 호출되어 `carData`에 새 값을 쓴다.
- 람다 안의 `carData`는 **Provider의 인스턴스 필드**를 closure로 캡쳐한다. 즉 Provider의 `SetCarData(newCar)`로 `carData`가 갱신되면 람다도 자동으로 새 차량을 참조하게 된다.

---

## 4. UI 생성 단계 — `SidebarUIManager` + `DataAdjustmentPanel`

### 4.1 패널 생성 (UI자동생성 버튼 시점)

`SidebarUISetup.UI자동생성()`을 누르면 다음 흐름이 일어난다.

```csharp
sidebarManager.SetPanelPrefab(panelPrefab);
sidebarManager.SetItemPrefab(itemPrefab);
sidebarManager.SetPanelContainer(panelContainer);

var perfProvider = GetOrAddComponent<CarPerformanceDataProvider>();
sidebarManager.RegisterDataProvider("Performance", perfProvider);
sidebarManager.CreatePanel("Performance", "Performance", perfProvider);
```

`SidebarUIManager.CreatePanel`은:

1. `panelPrefab`을 `Instantiate`하여 `DataAdjustmentPanel` GameObject 생성
2. `provider.GetDataFieldsByCategory("Performance")`로 `IDataAdjustable` 리스트 획득
3. `newPanel.Initialize("Performance", fields)` 호출하여 패널에 데이터 전달
4. 생성된 패널을 `activePanels` dictionary에 등록 (차량 변경 시 재초기화하기 위해)

### 4.2 슬라이더 행 생성 — `DataAdjustmentPanel.CreateUIItems`

`Initialize`가 받은 `IDataAdjustable` 리스트를 순회하면서 `DataAdjustmentUIItem` GameObject를 하나씩 인스턴스화한다.

```csharp
foreach (var field in dataFields)
{
    GameObject itemObj = Instantiate(itemPrefab, itemContainer);
    DataAdjustmentUIItem item = itemObj.GetComponent<DataAdjustmentUIItem>();
    item.SetDataField(field);     // ← 핵심: 어떤 IDataAdjustable을 표시할지 주입
    uiItems.Add(item);
}
```

이 시점에 화면에는 라벨 + 슬라이더 + 값 텍스트로 구성된 행들이 한 카테고리 패널 안에 차례로 나타난다.

---

## 5. 표시/입력 단계 — `DataAdjustmentUIItem`

행 단위 UI가 실제로 데이터와 양방향 동기화하는 곳이다.

### 5.1 초기 동기화 — `SetDataField`

```csharp
public void SetDataField(IDataAdjustable field)
{
    dataField = field;

    if (labelText != null) labelText.text = field.DisplayName;

    if (valueSlider != null)
    {
        valueSlider.minValue = 0f;
        valueSlider.maxValue = 1f;
        valueSlider.value = field.GetNormalizedValue();  // 현재 데이터값을 정규화해서 위치 설정
    }

    UpdateValueDisplay();   // valueText에 GetValue 결과 표시
}
```

라벨 텍스트, 슬라이더 위치, 값 텍스트가 모두 데이터의 현재 상태로 초기화된다.

### 5.2 매 프레임 양방향 동기화 — `Update`

```csharp
private void Update()
{
    if (dataField == null || valueSlider == null) return;

    float currentSliderValue = valueSlider.value;

    // ① 사용자가 슬라이더를 끌어서 변화 → setter 호출
    if (!isUpdatingUI && !Mathf.Approximately(currentSliderValue, lastSliderValue))
    {
        dataField.SetNormalizedValue(currentSliderValue);   // → CarData에 새 값 쓰기
        lastSliderValue = currentSliderValue;
    }

    // ② 데이터값을 다시 UI로 — valueText 갱신 + 슬라이더 위치 동기화
    UpdateValueDisplay();
}
```

두 방향:

- **UI → 데이터**: 슬라이더 변화 감지 → `dataField.SetNormalizedValue` → `UIDataField`의 setter 람다 → `CarData.SetX(value)`
- **데이터 → UI**: 매 프레임 `dataField.GetValue` → `valueText.text` 갱신 + `slider.value` 보정

데이터가 외부(예: 다른 시스템)에서 바뀌어도 다음 프레임에 자동으로 UI에 반영된다.

---

## 6. 차량 변경 시의 흐름

차량을 다른 것으로 갈아치우면 모든 패널이 새 데이터로 다시 채워져야 한다. 이를 위해 `SidebarUIManager`는 `OnCarChanged` 이벤트를 구독한다.

```csharp
private void Start()
{
    if (CarDataManager.Instance != null)
    {
        CarDataManager.Instance.OnCarChanged += OnCarChanged;
        OnCarChanged(CarDataManager.Instance.CurrentCar);
    }
}

private void OnCarChanged(CarData newCar)
{
    currentCarData = newCar;
    UpdateCarDisplay();

    foreach (var provider in dataProviders.Values)
        provider.SetCarData(newCar);                         // ① 모든 Provider에 새 차량 주입

    foreach (var info in activePanels.Values)
    {
        var fields = info.provider.GetDataFieldsByCategory(info.categoryName);
        info.panel.Initialize(info.title, fields);           // ② 패널을 새 필드들로 재초기화
    }
}
```

두 단계:

1. **Provider의 `carData` 갱신**: 람다 안의 closure가 새 차량을 가리키게 됨
2. **패널 재초기화**: 새 `UIDataField` 리스트로 슬라이더 행들을 다시 생성. 기존 행들은 제거됨

---

## 7. 전체 시퀀스 다이어그램

### 7.1 Play 모드 진입 (앱 시작)

```
[Unity]                [CarDataManager]      [SidebarUIManager]    [Provider×N]   [Panel×N]
   │                          │                       │                  │              │
   │── Awake ────────────────►│                       │                  │              │
   │                          │── carDatabase 로드 ────│                  │              │
   │                          │── SelectCar(0) ───────│                  │              │
   │                          │   OnCarChanged 발화 (구독자 X)            │              │
   │                          │                       │                  │              │
   │── Start ─────────────────────────────────────────►│                  │              │
   │                          │                       │── OnCarChanged 구독              │
   │                          │◄── CurrentCar 조회 ───│                  │              │
   │                          │                       │── 자기 호출 OnCarChanged(currentCar)
   │                          │                       │── SetCarData(newCar) ───────────►│
   │                          │                       │── GetDataFieldsByCategory ──────►│
   │                          │                       │◄── IDataAdjustable[] ────────────│
   │                          │                       │── Initialize ──────────────────────────►│
   │                          │                       │                  │              │── CreateUIItems
   │                          │                       │                  │              │── SetDataField (각 행)
```

### 7.2 사용자가 슬라이더를 끌 때

```
[사용자]      [Slider]      [DataAdjustmentUIItem]    [UIDataField]    [CarData]
   │            │                    │                      │              │
   │── drag ───►│                    │                      │              │
   │            │ value 변화          │                      │              │
   │            │                    │                      │              │
   │            │   ┌───── Update ─── (매 프레임 polling)                   │
   │            │   │ slider.value ≠ lastSliderValue 감지                  │
   │            │   ├── SetNormalizedValue(v) ────────────►│              │
   │            │   │                                       │── setter 람다 │
   │            │   │                                       │── carData.SetX(value) ───►│
   │            │   │                                       │              │── 필드 갱신
   │            │   │                                       │              │   + UpdateRemainingRange()
   │            │   │ UpdateValueDisplay                                   │
   │            │   ├── GetValue ─────────────────────────►│              │
   │            │   │                                       │── getter 람다 │
   │            │   │                                       │◄── carData.X ─│
   │            │   │◄── 현재 값 ─────────────────────────────│              │
   │            │   │ valueText.text = 값.ToString()                       │
   │            │   └─────────────────────────────────────────────────────►│
```

### 7.3 차량을 다른 것으로 변경

```
[누군가]    [CarDataManager]    [SidebarUIManager]    [Provider×N]    [Panel×N]
   │              │                       │                  │              │
   │── SelectCar(i) ─►│                   │                  │              │
   │              │── currentCar = newCar │                  │              │
   │              │── OnCarChanged ───────►│                  │              │
   │              │                       │── SetCarData(newCar) ───────────►│
   │              │                       │                  │── provider.carData = newCar
   │              │                       │── GetDataFieldsByCategory ───────►│
   │              │                       │◄── 새 UIDataField[] ──────────────│
   │              │                       │── Initialize ────────────────────────────►│
   │              │                       │                  │              │── CreateUIItems
   │              │                       │                  │              │   (기존 행 제거 + 새로 생성)
```

---

## 8. 정리 — 데이터가 UI에 흐르는 핵심 원리

- **자산은 `ScriptableObject`로 디스크에 영구 저장**되고, 런타임에 `CarDataManager`가 메모리로 가져온다.
- `CarDataManager`는 **차량 변경을 이벤트로 통보**할 뿐, UI를 직접 모르고 UI도 매니저를 강하게 알지 않는다 — 이벤트 한 줄로 결합도가 끊긴다.
- Provider는 `CarData`의 private 필드를 슬라이더가 이해할 수 있는 단일 인터페이스(`IDataAdjustable`)로 **번역**한다.
- UI 측은 자기가 표시하는 데이터가 `CarData`인지, `BikeData`인지 알 필요 없이 `IDataAdjustable`만 다룬다.
- `DataAdjustmentUIItem`이 매 프레임 `GetValue`/`SetNormalizedValue`만 호출해 **양방향 동기화**가 자연스럽게 일어난다.

이렇게 다섯 계층(`ScriptableObject 자산 → Manager → Provider → Panel → Item`)을 거치며, 각 계층은 바로 옆 계층과만 대화한다. 한 계층의 구현이 바뀌어도 나머지에 영향이 거의 없는 구조다.
