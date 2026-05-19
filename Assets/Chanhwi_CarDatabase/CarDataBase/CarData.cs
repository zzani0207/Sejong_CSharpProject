using UnityEngine;

/// <summary>
/// 연료 타입 구분
/// </summary>
public enum FuelType
{
    Electric,  // 전기
    Gasoline,  // 휘발유
    Diesel,    // 디젤
    Hybrid,    // 하이브리드
    PHEV       // 플러그인 하이브리드
}

/// <summary>
/// 개별 차의 데이터를 관리하는 ScriptableObject
/// 각 차의 상세 정보를 저장합니다 - 모든 차량 종류 지원
/// </summary>
[CreateAssetMenu(fileName = "Car_", menuName = "Scriptable Objects/Car Data")]
public class CarData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string carName = "Tesla Model S";
    [SerializeField] private string manufacturer = "Tesla";
    [SerializeField] private string model = "Model S";
    [SerializeField] private int productionYear = 2024;
    [SerializeField] private string vinNumber = "5YJ3E1EA2MF123456";
    [SerializeField] private FuelType fuelType = FuelType.Electric;

    [Header("성능 정보")]
    [SerializeField] private float enginePower = 450f; // kW
    [SerializeField] private float maxSpeed = 250f; // km/h
    [SerializeField] private float acceleration = 2.5f; // 0-100 km/h 시간(초)
    [SerializeField] private float torque = 660f; // N·m

    [Header("연료 정보 (가솔린/디젤)")]
    [SerializeField] private float fuelTankCapacity = 60f; // 리터
    [SerializeField] private float currentFuelLevel = 45f; // 리터
    [SerializeField] private float fuelConsumption = 6.5f; // km/L
    [SerializeField] private float remainingFuelRange = 300f; // km

    [Header("배터리 정보 (전기/하이브리드/PHEV)")]
    [SerializeField] private float batteryCapacity = 100f; // kWh
    [SerializeField] private float currentBatteryHealth = 95f; // %
    [SerializeField] private float currentChargeLevel = 80f; // %
    [SerializeField] private float remainingBatteryRange = 450f; // km
    [SerializeField] private float chargingTime = 8.5f; // 시간 (80% 기준)
    [SerializeField] private float energyConsumption = 15f; // kWh per 100km

    [Header("차량 상태")]
    [SerializeField] private float mileage = 15000f; // km
    [SerializeField] private float weight = 2100f; // kg
    [SerializeField] private string color = "Pearl White Multi-Coat";
    [SerializeField] private int seatingCapacity = 5;

    // Properties - Read Only
    public string CarName => carName;
    public string Manufacturer => manufacturer;
    public string Model => model;
    public int ProductionYear => productionYear;
    public string VinNumber => vinNumber;
    public FuelType FuelType => fuelType;
    public string FuelTypeString => fuelType.ToString();

    public float EnginePower => enginePower;
    public float MaxSpeed => maxSpeed;
    public float Acceleration => acceleration;
    public float Torque => torque;

    // 연료 (가솔린/디젤)
    public float FuelTankCapacity => fuelTankCapacity;
    public float CurrentFuelLevel => currentFuelLevel;
    public float FuelConsumption => fuelConsumption;
    public float RemainingFuelRange => remainingFuelRange;

    // 배터리 (전기/하이브리드/PHEV)
    public float BatteryCapacity => batteryCapacity;
    public float CurrentBatteryHealth => currentBatteryHealth;
    public float CurrentChargeLevel => currentChargeLevel;
    public float RemainingBatteryRange => remainingBatteryRange;
    public float ChargingTime => chargingTime;
    public float EnergyConsumption => energyConsumption;

    public float Mileage => mileage;
    public float Weight => weight;
    public string Color => color;
    public int SeatingCapacity => seatingCapacity;

    /// <summary>
    /// 차량 타입에 관계없이 남은 거리를 반환 (연료 또는 배터리)
    /// </summary>
    public float RemainingRange
    {
        get
        {
            return fuelType switch
            {
                FuelType.Electric => remainingBatteryRange,
                FuelType.Gasoline => remainingFuelRange,
                FuelType.Diesel => remainingFuelRange,
                FuelType.Hybrid => remainingFuelRange + remainingBatteryRange,
                FuelType.PHEV => remainingFuelRange + remainingBatteryRange,
                _ => 0f
            };
        }
    }

    /// <summary>
    /// 현재 연료/배터리 레벨 (0-100%)
    /// </summary>
    public float CurrentFuelLevel_Percent
    {
        get
        {
            return fuelType switch
            {
                FuelType.Electric => currentChargeLevel,
                FuelType.Gasoline => (currentFuelLevel / fuelTankCapacity) * 100f,
                FuelType.Diesel => (currentFuelLevel / fuelTankCapacity) * 100f,
                FuelType.Hybrid => ((currentFuelLevel / fuelTankCapacity) * 100f + currentChargeLevel) / 2f,
                FuelType.PHEV => ((currentFuelLevel / fuelTankCapacity) * 100f + currentChargeLevel) / 2f,
                _ => 0f
            };
        }
    }

    /// <summary>
    /// 배터리 충전 레벨 업데이트 (전기/하이브리드/PHEV)
    /// </summary>
    public void SetChargeLevel(float newChargeLevel)
    {
        if (fuelType == FuelType.Electric || fuelType == FuelType.Hybrid || fuelType == FuelType.PHEV)
        {
            currentChargeLevel = Mathf.Clamp01(newChargeLevel / 100f) * 100f;
            UpdateRemainingRange();
        }
    }

    /// <summary>
    /// 연료 충전 레벨 업데이트 (가솔린/디젤/하이브리드/PHEV)
    /// </summary>
    public void SetFuelLevel(float newFuelLiters)
    {
        if (fuelType == FuelType.Gasoline || fuelType == FuelType.Diesel || 
            fuelType == FuelType.Hybrid || fuelType == FuelType.PHEV)
        {
            currentFuelLevel = Mathf.Clamp(newFuelLiters, 0, fuelTankCapacity);
            UpdateRemainingRange();
        }
    }

    /// <summary>
    /// 주행거리 업데이트
    /// </summary>
    public void AddMileage(float distance)
    {
        if (distance > 0)
        {
            mileage += distance;
            
            // 주행거리에 따라 배터리 건강도 저하 (시뮬레이션)
            if (fuelType == FuelType.Electric || fuelType == FuelType.Hybrid || fuelType == FuelType.PHEV)
            {
                currentBatteryHealth -= (distance / 50000f);
                currentBatteryHealth = Mathf.Max(70f, currentBatteryHealth);
            }
        }
    }

    /// <summary>
    /// 남은 주행 거리 계산
    /// </summary>
    private void UpdateRemainingRange()
    {
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
                remainingFuelRange = currentFuelLevel * fuelConsumption;
                remainingBatteryRange = (currentChargeLevel / 100f) * (batteryCapacity / energyConsumption * 100f);
                break;

            case FuelType.PHEV:
                remainingFuelRange = currentFuelLevel * fuelConsumption;
                remainingBatteryRange = (currentChargeLevel / 100f) * (batteryCapacity / energyConsumption * 100f);
                break;
        }
    }

    /// <summary>
    /// 차의 모든 정보를 문자열로 반환
    /// </summary>
    public override string ToString()
    {
        string baseInfo = $"[{CarName}]\n" +
                   $"제조사: {Manufacturer} ({ProductionYear})\n" +
                   $"연료타입: {FuelTypeString}\n" +
                   $"성능: {EnginePower}kW, {MaxSpeed}km/h\n" +
                   $"주행거리: {Mileage}km";

        string fuelInfo = fuelType switch
        {
            FuelType.Gasoline or FuelType.Diesel =>
                $"\n연료: {CurrentFuelLevel:F1}/{FuelTankCapacity}L (남은거리: {RemainingFuelRange:F0}km)",
            
            FuelType.Electric =>
                $"\n배터리: {CurrentChargeLevel}% (남은거리: {RemainingBatteryRange:F0}km)",
            
            FuelType.Hybrid or FuelType.PHEV =>
                $"\n연료: {CurrentFuelLevel:F1}L / 배터리: {CurrentChargeLevel}%\n" +
                $"총 남은거리: {RemainingRange:F0}km",
            
            _ => ""
        };

        return baseInfo + fuelInfo;
    }
}