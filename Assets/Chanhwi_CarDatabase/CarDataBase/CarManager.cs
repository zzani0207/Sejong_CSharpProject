using UnityEngine;
using System;

/// <summary>
/// 런타임에서 차량 데이터를 관리하고 UI와 연동하는 매니저
/// 싱글톤 패턴으로 구현
/// </summary>
public class CarManager : MonoBehaviour
{
    [SerializeField] private CarDataBase carDatabase;

    private CarData currentCar;
    private int currentCarIndex = 0;

    /// <summary>
    /// 현재 선택된 차
    /// </summary>
    public CarData CurrentCar => currentCar;

    /// <summary>
    /// 현재 선택된 차의 인덱스
    /// </summary>
    public int CurrentCarIndex => currentCarIndex;

    // 🔔 이벤트들 - UI에서 구독하여 자동으로 업데이트
    public event Action<CarData> OnCarChanged;
    public event Action<float> OnBatteryChanged;
    public event Action<float> OnFuelChanged;
    public event Action<float> OnMileageChanged;

    private static CarManager instance;

    public static CarManager Instance
    {
        get
        {
#if UNITY_EDITOR
            // 에디터(에디트 모드) - 씬에서 직접 찾기
            if (instance == null && !Application.isPlaying)
            {
                instance = FindAnyObjectByType<CarManager>();
            }
#endif
            return instance;
        }
    }

    private void Awake()
    {
        // 싱글톤 패턴
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 데이터베이스 로드 체크
        if (carDatabase == null)
        {
            carDatabase = Resources.Load<CarDataBase>("CarDatabase");
            if (carDatabase == null)
            {
                Debug.LogError("CarDatabase not found! Please assign it in Inspector or place in Resources folder.");
                return;
            }
        }

        // 첫 번째 차로 초기화
        SelectCar(0);
    }

    /// <summary>
    /// 인덱스로 차 선택
    /// </summary>
    public void SelectCar(int index)
    {
        CarData newCar = carDatabase.GetCar(index);
        if (newCar != null)
        {
            currentCar = newCar;
            currentCarIndex = index;
            OnCarChanged?.Invoke(currentCar);
            Debug.Log($"차 선택됨: {currentCar.CarName}");
        }
        else
        {
            Debug.LogWarning($"Invalid car index: {index}");
        }
    }

    /// <summary>
    /// 차 이름으로 차 선택
    /// </summary>
    public void SelectCarByName(string carName)
    {
        CarData newCar = carDatabase.GetCar(carName);
        if (newCar != null)
        {
            currentCar = newCar;
            
            // 현재 인덱스 찾기
            for (int i = 0; i < carDatabase.CarCount; i++)
            {
                if (carDatabase.GetCar(i) == newCar)
                {
                    currentCarIndex = i;
                    break;
                }
            }

            OnCarChanged?.Invoke(currentCar);
            Debug.Log($"차 선택됨: {currentCar.CarName}");
        }
        else
        {
            Debug.LogWarning($"Car not found: {carName}");
        }
    }

    /// <summary>
    /// 다음 차로 변경
    /// </summary>
    public void SelectNextCar()
    {
        int nextIndex = (currentCarIndex + 1) % carDatabase.CarCount;
        SelectCar(nextIndex);
    }

    /// <summary>
    /// 이전 차로 변경
    /// </summary>
    public void SelectPreviousCar()
    {
        int prevIndex = currentCarIndex - 1;
        if (prevIndex < 0)
        {
            prevIndex = carDatabase.CarCount - 1;
        }
        SelectCar(prevIndex);
    }

    /// <summary>
    /// 현재 차의 배터리 충전 (전기/하이브리드/PHEV)
    /// </summary>
    public void ChargeBattery(float amount)
    {
        if (currentCar != null && (currentCar.FuelType == FuelType.Electric || 
            currentCar.FuelType == FuelType.Hybrid || currentCar.FuelType == FuelType.PHEV))
        {
            float newCharge = currentCar.CurrentChargeLevel + amount;
            currentCar.SetChargeLevel(newCharge);
            OnBatteryChanged?.Invoke(currentCar.CurrentChargeLevel);
            Debug.Log($"배터리 충전: {currentCar.CurrentChargeLevel}%");
        }
        else if (currentCar != null)
        {
            Debug.LogWarning($"{currentCar.CarName}은(는) 배터리가 없는 {currentCar.FuelTypeString} 차량입니다.");
        }
    }

    /// <summary>
    /// 현재 차의 배터리 방전
    /// </summary>
    public void DischargeBattery(float amount)
    {
        ChargeBattery(-amount);
    }

    /// <summary>
    /// 현재 차의 연료 충전 (가솔린/디젤/하이브리드/PHEV)
    /// </summary>
    public void RefuelCar(float liters)
    {
        if (currentCar != null && (currentCar.FuelType == FuelType.Gasoline || 
            currentCar.FuelType == FuelType.Diesel || currentCar.FuelType == FuelType.Hybrid || 
            currentCar.FuelType == FuelType.PHEV))
        {
            float newFuel = currentCar.CurrentFuelLevel + liters;
            currentCar.SetFuelLevel(newFuel);
            OnFuelChanged?.Invoke(currentCar.CurrentFuelLevel);
            Debug.Log($"연료 충전: {currentCar.CurrentFuelLevel:F1}L");
        }
        else if (currentCar != null)
        {
            Debug.LogWarning($"{currentCar.CarName}은(는) 연료탱크가 없는 {currentCar.FuelTypeString} 차량입니다.");
        }
    }

    /// <summary>
    /// 현재 차의 연료 사용
    /// </summary>
    public void UseFuel(float liters)
    {
        RefuelCar(-liters);
    }

    /// <summary>
    /// 현재 차의 주행거리 추가
    /// </summary>
    public void AddMileage(float distance)
    {
        if (currentCar != null && distance > 0)
        {
            currentCar.AddMileage(distance);
            OnMileageChanged?.Invoke(currentCar.Mileage);
            Debug.Log($"주행거리 추가: {distance}km (총 {currentCar.Mileage}km)");
        }
    }

    /// <summary>
    /// 데이터베이스의 모든 차 반환
    /// </summary>
    public CarData[] GetAllCars()
    {
        return carDatabase.GetAllCars();
    }

    /// <summary>
    /// 데이터베이스의 차 개수
    /// </summary>
    public int GetTotalCarCount()
    {
        return carDatabase.CarCount;
    }

    /// <summary>
    /// 모든 차 정보 출력
    /// </summary>
    public void PrintAllCarsInfo()
    {
        carDatabase.PrintAllCars();
    }

    /// <summary>
    /// 현재 차 정보 출력
    /// </summary>
    public void PrintCurrentCarInfo()
    {
        if (currentCar != null)
        {
            Debug.Log(currentCar.ToString());
        }
    }
}
