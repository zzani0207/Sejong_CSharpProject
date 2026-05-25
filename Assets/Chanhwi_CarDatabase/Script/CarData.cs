using UnityEngine;

public enum FuelType
{
    Electric,
    Gasoline,
    Diesel,
    Hybrid,
    PHEV
}

// VehicleOverviewPanel과 통합을 위한 상태 열거형들
public enum VehicleConnectionStatus
{
    Connected,
    Disconnected,
    Syncing,
    Error
}

public enum VehicleSystemStatus
{
    Normal,
    Warning,
    Critical,
    Offline
}

public enum VehicleWarningType
{
    None,
    TirePressureLow,
    BatteryLow,
    DoorOpen,
    LightLeftOn,
    SensorOffline
}

[CreateAssetMenu(fileName = "Car_", menuName = "Scriptable Objects/Car Data")]
public class CarData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string carName;
    [SerializeField] private string manufacturer;
    [SerializeField] private string model;
    [SerializeField] private int productionYear;
    [SerializeField] private string vinNumber;
    [SerializeField] private FuelType fuelType;

    [Header("성능 정보")]
    [SerializeField] private float enginePower = 450f; // kW
    [SerializeField] private float maxSpeed = 250f; // km/h
    [SerializeField] private float acceleration = 2.5f; // 0-100 km/h 초
    [SerializeField] private float torque = 660f; // N·m

    [Header("연료 정보 (가솔린/디젤)")]
    [SerializeField] private float fuelTankCapacity = 60f; // L
    [SerializeField] private float currentFuelLevel = 45f; // L
    [SerializeField] private float fuelConsumption = 6.5f; // km/L
    [SerializeField] private float remainingFuelRange = 300f; // km

    [Header("배터리 정보 (전기/하이브리드/PHEV)")]
    [SerializeField] private float batteryCapacity = 100f; // kWh
    [SerializeField] private float currentBatteryHealth = 95f; // %
    [SerializeField] private float currentChargeLevel = 80f; // %
    [SerializeField] private float remainingBatteryRange = 450f; // km
    [SerializeField] private float chargingTime = 8.5f; // h (80% 기준)
    [SerializeField] private float energyConsumption = 15f; // kWh/100km

    [Header("차량 상태")]
    [SerializeField] private float mileage = 15000f; // km
    [SerializeField] private float weight = 2100f; // kg
    [SerializeField] private string color = "Pearl White Multi-Coat";
    [SerializeField] private int seatingCapacity = 5;

    [Header("Vehicle Overview Panel (UI 연동)")]
    [SerializeField] private string vehicleLocation = "Garage A";
    [SerializeField] private VehicleConnectionStatus connectionStatus = VehicleConnectionStatus.Connected;
    [SerializeField] private VehicleSystemStatus systemStatus = VehicleSystemStatus.Normal;
    [SerializeField] private VehicleWarningType warningType = VehicleWarningType.None;
    [SerializeField] private System.DateTime lastUpdateTime = System.DateTime.Now;

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

    public float FuelTankCapacity => fuelTankCapacity;
    public float CurrentFuelLevel => currentFuelLevel;
    public float FuelConsumption => fuelConsumption;
    public float RemainingFuelRange => remainingFuelRange;

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

    public string VehicleLocation => vehicleLocation;
    public VehicleConnectionStatus ConnectionStatus => connectionStatus;
    public VehicleSystemStatus SystemStatus => systemStatus;
    public VehicleWarningType WarningType => warningType;
    public System.DateTime LastUpdateTime => lastUpdateTime;

    public void SetChargeLevel(float newChargeLevel)
    {
        if (fuelType == FuelType.Electric || fuelType == FuelType.Hybrid || fuelType == FuelType.PHEV)
        {
            currentChargeLevel = Mathf.Clamp01(newChargeLevel / 100f) * 100f;
            UpdateRemainingRange();
        }
    }

    public void SetFuelLevel(float newFuelLiters)
    {
        if (fuelType == FuelType.Gasoline || fuelType == FuelType.Diesel ||
            fuelType == FuelType.Hybrid || fuelType == FuelType.PHEV)
        {
            currentFuelLevel = Mathf.Clamp(newFuelLiters, 0, fuelTankCapacity);
            UpdateRemainingRange();
        }
    }

    public void SetEnginePower(float v) => enginePower = Mathf.Max(0, v);
    public void SetMaxSpeed(float v) => maxSpeed = Mathf.Max(0, v);
    public void SetAcceleration(float v) => acceleration = Mathf.Max(0, v);
    public void SetTorque(float v) => torque = Mathf.Max(0, v);

    public void SetFuelTankCapacity(float v)
    {
        fuelTankCapacity = Mathf.Max(0, v);
        currentFuelLevel = Mathf.Min(currentFuelLevel, fuelTankCapacity);
        UpdateRemainingRange();
    }

    public void SetFuelConsumption(float v)
    {
        fuelConsumption = Mathf.Max(0, v);
        UpdateRemainingRange();
    }

    public void SetBatteryCapacity(float v)
    {
        batteryCapacity = Mathf.Max(0, v);
        UpdateRemainingRange();
    }

    public void SetBatteryHealth(float v) => currentBatteryHealth = Mathf.Clamp(v, 0, 100);

    public void SetEnergyConsumption(float v)
    {
        // 0이면 RemainingBatteryRange 계산에서 나눗셈 폭주 - 최소값 보호
        energyConsumption = Mathf.Max(0.01f, v);
        UpdateRemainingRange();
    }

    public void SetChargingTime(float v) => chargingTime = Mathf.Max(0, v);
    public void SetMileage(float v) => mileage = Mathf.Max(0, v);
    public void SetWeight(float v) => weight = Mathf.Max(0, v);
    public void SetProductionYear(int v) => productionYear = v;
    public void SetSeatingCapacity(int v) => seatingCapacity = Mathf.Max(1, v);

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
            case FuelType.PHEV:
                remainingFuelRange = currentFuelLevel * fuelConsumption;
                remainingBatteryRange = (currentChargeLevel / 100f) * (batteryCapacity / energyConsumption * 100f);
                break;
        }
    }
}
