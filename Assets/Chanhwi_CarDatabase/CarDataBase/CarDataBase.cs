using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 차 데이터를 관리하는 데이터베이스
/// 여러 CarData를 한 곳에서 관리합니다
/// </summary>
[CreateAssetMenu(fileName = "CarDatabase", menuName = "Scriptable Objects/Car Database")]
public class CarDataBase : ScriptableObject
{
    [SerializeField] private CarData[] cars;

    private Dictionary<string, CarData> carDictionary;

    private void OnEnable()
    {
        InitializeDictionary();
    }

    /// <summary>
    /// 빠른 검색을 위한 Dictionary 초기화
    /// </summary>
    private void InitializeDictionary()
    {
        if (carDictionary == null)
        {
            carDictionary = new Dictionary<string, CarData>();
        }
        else
        {
            carDictionary.Clear();
        }

        if (cars != null)
        {
            foreach (var car in cars)
            {
                if (car != null)
                {
                    carDictionary[car.CarName] = car;
                }
            }
        }
    }

    /// <summary>
    /// 인덱스로 차 데이터 조회
    /// </summary>
    public CarData GetCar(int index)
    {
        if (cars != null && index >= 0 && index < cars.Length)
        {
            return cars[index];
        }
        Debug.LogWarning($"Invalid car index: {index}");
        return null;
    }

    /// <summary>
    /// 차 이름으로 데이터 조회
    /// </summary>
    public CarData GetCar(string carName)
    {
        if (carDictionary == null)
        {
            InitializeDictionary();
        }

        if (carDictionary.TryGetValue(carName, out var car))
        {
            return car;
        }
        Debug.LogWarning($"Car not found: {carName}");
        return null;
    }

    /// <summary>
    /// 모든 차 반환
    /// </summary>
    public CarData[] GetAllCars()
    {
        return cars ?? System.Array.Empty<CarData>();
    }

    /// <summary>
    /// 차의 총 개수
    /// </summary>
    public int CarCount => cars != null ? cars.Length : 0;

    /// <summary>
    /// 특정 제조사의 차량 목록 반환
    /// </summary>
    public CarData[] GetCarsByManufacturer(string manufacturer)
    {
        List<string> result = new List<string>();
        if (cars != null)
        {
            foreach (var car in cars)
            {
                if (car != null && car.Manufacturer == manufacturer)
                {
                    result.Add(car.CarName);
                }
            }
        }

        CarData[] resultCars = new CarData[result.Count];
        for (int i = 0; i < result.Count; i++)
        {
            resultCars[i] = GetCar(result[i]);
        }
        return resultCars;
    }

    /// <summary>
    /// 최고 속도로 차량 정렬
    /// </summary>
    public CarData[] GetCarsSortedByMaxSpeed()
    {
        if (cars == null || cars.Length == 0)
            return System.Array.Empty<CarData>();

        CarData[] sorted = (CarData[])cars.Clone();
        System.Array.Sort(sorted, (a, b) => b.MaxSpeed.CompareTo(a.MaxSpeed));
        return sorted;
    }

    /// <summary>
    /// 가장 빠른 차 반환
    /// </summary>
    public CarData GetFastestCar()
    {
        if (cars == null || cars.Length == 0)
            return null;

        CarData fastest = cars[0];
        foreach (var car in cars)
        {
            if (car != null && car.MaxSpeed > fastest.MaxSpeed)
            {
                fastest = car;
            }
        }
        return fastest;
    }

    /// <summary>
    /// 배터리가 가장 건강한 차 반환
    /// </summary>
    public CarData GetBestBatteryConditionCar()
    {
        if (cars == null || cars.Length == 0)
            return null;

        CarData best = cars[0];
        foreach (var car in cars)
        {
            if (car != null && car.CurrentBatteryHealth > best.CurrentBatteryHealth)
            {
                best = car;
            }
        }
        return best;
    }

    /// <summary>
    /// 데이터베이스의 모든 차를 콘솔에 출력
    /// </summary>
    public void PrintAllCars()
    {
        if (cars == null || cars.Length == 0)
        {
            Debug.Log("데이터베이스에 차가 없습니다.");
            return;
        }

        Debug.Log($"=== 차 데이터베이스 (총 {cars.Length}대) ===");
        for (int i = 0; i < cars.Length; i++)
        {
            Debug.Log($"[{i}] {cars[i].CarName} - {cars[i].MaxSpeed}km/h, 배터리: {cars[i].CurrentBatteryHealth}%");
        }
    }
}
