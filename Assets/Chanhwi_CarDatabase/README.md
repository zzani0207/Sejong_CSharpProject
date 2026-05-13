# 연결된 자동차(Connected Car) 데이터베이스 시스템

이 시스템은 여러 대의 자동차 데이터를 효율적으로 관리하고 UI에 실시간으로 동기화하는 객체지향 설계입니다.

## 📁 파일 구조

```
Chanhwi_CarDatabase/
├── CarData.cs              # 개별 차 데이터 (ScriptableObject)
├── CarDataBase.cs          # 차 데이터베이스 관리
├── CarManager.cs           # 런타임 차량 상태 관리 (Singleton)
├── CarUIController.cs      # UI 연동 컨트롤러
├── CarDataCreator.cs       # 샘플 데이터 생성 에디터 스크립트
└── README.md              # 이 파일
```

## 🏗️ 아키텍처

### 1. **CarData.cs** - 개별 차의 데이터
- **역할**: 하나의 차에 대한 모든 정보 저장
- **ScriptableObject**: Inspector에서 쉽게 편집 가능
- **포함 정보**:
  - 기본: 이름, 제조사, 모델, 년도, VIN
  - 성능: 엔진파워, 최대속도, 가속도, 토크
  - 배터리: 용량, 건강도, 충전률, 남은거리
  - 상태: 주행거리, 무게, 색상, 탑승인원
  - 효율: 에너지소비량, 충전시간

### 2. **CarDataBase.cs** - 데이터베이스
- **역할**: 모든 차 데이터를 한 곳에서 관리
- **기능**:
  - 인덱스/이름으로 차 검색
  - 제조사별 필터링
  - 성능 정렬 (최대속도 등)
  - 딕셔너리 캐싱으로 빠른 검색

### 3. **CarManager.cs** - 런타임 매니저 (Singleton)
- **역할**: 게임 실행 중 차 상태 관리
- **특징**:
  - 현재 선택된 차 추적
  - 이벤트 시스템 (배터리, 주행거리 변경 시 알림)
  - 차 선택, 전환 기능
  - `DontDestroyOnLoad` - 씬 전환 시에도 유지

### 4. **CarUIController.cs** - UI 연동
- **역할**: CarManager의 이벤트를 구독하여 UI 자동 업데이트
- **특징**:
  - 모든 Text/Slider UI 자동 갱신
  - 배터리 슬라이더 실시간 연동
  - 이전/다음 차 네비게이션 버튼

## 🚀 사용 방법

### 1️⃣ 초기 설정

#### A. 샘플 데이터 자동 생성 (권장)
```
Unity Editor > Tools > Car Database > Create Sample Cars
```
자동으로 6개의 샘플 차 데이터가 생성됩니다.

#### B. 수동으로 차 생성
```
우클릭 > Create > Scriptable Objects > Car Data
```

### 2️⃣ CarDatabase 생성

```
우클릭 > Create > Scriptable Objects > Car Database
```

Inspector에서 생성한 CarData들을 배열에 할당합니다.

### 3️⃣ 씬 설정

1. 빈 GameObject 생성 → `CarManager` 스크립트 추가
2. CarManager의 Inspector에서 CarDatabase 할당
3. Canvas에 UI 구성 (Text, Slider 등)
4. Canvas 또는 Panel에 `CarUIController` 스크립트 추가
5. Inspector에서 각 UI 요소 할당

## 💻 코드 예제

### CarManager 사용

```csharp
// 차 선택
CarManager.Instance.SelectCar(0);           // 인덱스로
CarManager.Instance.SelectCarByName("Tesla Model S");  // 이름으로

// 차 전환
CarManager.Instance.SelectNextCar();
CarManager.Instance.SelectPreviousCar();

// 배터리 조작
CarManager.Instance.ChargeBattery(20);      // 20% 충전
CarManager.Instance.DischargeBattery(10);   // 10% 방전

// 주행거리 추가
CarManager.Instance.AddMileage(100);        // 100km 주행

// 현재 차 정보
Debug.Log(CarManager.Instance.CurrentCar.CarName);
Debug.Log(CarManager.Instance.CurrentCar.CurrentChargeLevel);

// 정보 출력
CarManager.Instance.PrintCurrentCarInfo();
CarManager.Instance.PrintAllCarsInfo();
```

### 이벤트 구독 (커스텀 스크립트에서)

```csharp
public class MyScript : MonoBehaviour
{
    private void Start()
    {
        CarManager.Instance.OnCarChanged += HandleCarChanged;
        CarManager.Instance.OnBatteryChanged += HandleBatteryChanged;
        CarManager.Instance.OnMileageChanged += HandleMileageChanged;
    }

    private void HandleCarChanged(CarData car)
    {
        Debug.Log($"차가 {car.CarName}으로 변경되었습니다!");
    }

    private void HandleBatteryChanged(float batteryLevel)
    {
        Debug.Log($"배터리: {batteryLevel}%");
    }

    private void HandleMileageChanged(float mileage)
    {
        Debug.Log($"주행거리: {mileage}km");
    }

    private void OnDestroy()
    {
        CarManager.Instance.OnCarChanged -= HandleCarChanged;
        CarManager.Instance.OnBatteryChanged -= HandleBatteryChanged;
        CarManager.Instance.OnMileageChanged -= HandleMileageChanged;
    }
}
```

## 🎯 객체지향 설계 원칙

1. **단일 책임 원칙 (SRP)**
   - 각 클래스는 하나의 책임만 가짐
   - CarData: 데이터 저장
   - CarDatabase: 데이터 관리
   - CarManager: 상태 관리
   - CarUIController: UI 표시

2. **개방-폐쇄 원칙 (OCP)**
   - 새로운 자동차 종류 추가 시 기존 코드 수정 불필요
   - ScriptableObject만 생성하면 됨

3. **의존성 역전 원칙 (DIP)**
   - CarUIController는 구체적인 CarManager에 의존하지 않음
   - 이벤트를 통한 느슨한 결합

4. **싱글톤 패턴**
   - CarManager는 게임 전체에서 하나만 존재
   - 어디서든 쉽게 접근 가능

## 📊 데이터 정보

### 포함된 차 정보 카테고리
- ✅ 기본 정보 (이름, 제조사, 모델, 년도, VIN)
- ✅ 성능 정보 (엔진, 속도, 가속, 토크)
- ✅ 배터리/에너지 (용량, 건강도, 충전률)
- ✅ 차량 상태 (주행거리, 무게, 색상, 탑승인원)
- ✅ 효율성 (에너지소비, 충전시간)
- ✅ 동적 메서드 (배터리 변화에 따른 남은거리 자동 계산)

## 🔧 확장 가능 부분

```csharp
// 예시: 차량 유지보수 정보 추가
[Header("유지보수")]
[SerializeField] private float lastMaintenanceKm;
[SerializeField] private int maintenanceIntervalKm;

// 예시: 차량 위치 추적
[Header("GPS 위치")]
[SerializeField] private float latitude;
[SerializeField] private float longitude;

// 예시: 차량 진단 정보
[Header("진단")]
[SerializeField] private string[] faultCodes;
```

## ⚠️ 주의사항

- `CarManager`는 Singleton이므로 여러 개 생성하면 안 됨
- UI는 반드시 `CarUIController`가 있는 Canvas에 자식으로 배치
- CarDatabase 할당 없이 게임 시작 시 에러 발생

## 📝 라이선스

팀 프로젝트용 - 자유롭게 수정 및 확장 가능

---

**마지막 수정**: 2024년 (작성일)
