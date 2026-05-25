using UnityEngine;

// 에디터 메뉴 - 샘플 차 데이터 7개를 한 번에 생성한다. 빈 프로젝트 초기화용.
#if UNITY_EDITOR
using UnityEditor;

public class CarDataCreator : MonoBehaviour
{
    [MenuItem("Tools/Car Database/Create Sample Cars")]
    public static void CreateSampleCars()
    {
        string folderPath = "Assets/Chanhwi_CarDatabase";

        // 테슬라 모델 S (전기)
        CreateCarData(folderPath, "Car_TeslaModelS",
            "Tesla Model S", "Tesla", "Model S", 2024, "5YJ3E1EA2MF123456",
            450f, 250f, 2.5f, 660f,
            FuelType.Electric,
            60f, 45f, 6.5f, 300f,           // 연료 (전기차라 사용 안 됨)
            100f, 95f, 80f, 450f, 8.5f, 15f,
            "Garage A", VehicleConnectionStatus.Connected, VehicleSystemStatus.Normal, VehicleWarningType.None);

        // BMW M4 (가솔린)
        CreateCarData(folderPath, "Car_BMWM4",
            "BMW M4", "BMW", "M4", 2024, "WBSF93010WEJ12345",
            503f, 290f, 3.9f, 650f,
            FuelType.Gasoline,
            70f, 60f, 9.5f, 570f,
            0f, 100f, 0f, 0f, 0f, 0f,       // 배터리 (가솔린이라 사용 안 됨)
            "Garage B", VehicleConnectionStatus.Connected, VehicleSystemStatus.Normal, VehicleWarningType.None);

        // 포르쉐 911 터보 (가솔린)
        CreateCarData(folderPath, "Car_Porsche911Turbo",
            "Porsche 911 Turbo", "Porsche", "911 Turbo", 2024, "WP0CA2993NS123456",
            580f, 330f, 2.7f, 750f,
            FuelType.Gasoline,
            90f, 70f, 6.8f, 476f,
            0f, 100f, 0f, 0f, 0f, 0f,
            "Outside", VehicleConnectionStatus.Disconnected, VehicleSystemStatus.Normal, VehicleWarningType.None);

        // 현대 아이오닉 5 (전기)
        CreateCarData(folderPath, "Car_HyundaiIoniq5",
            "Hyundai Ioniq 5", "Hyundai", "Ioniq 5", 2024, "KMHEC4A46EU123456",
            225f, 230f, 4.7f, 350f,
            FuelType.Electric,
            60f, 45f, 6.5f, 300f,
            84f, 98f, 75f, 380f, 6.8f, 13.5f,
            "Charging Station", VehicleConnectionStatus.Syncing, VehicleSystemStatus.Warning, VehicleWarningType.BatteryLow);

        // 기아 EV9 (전기)
        CreateCarData(folderPath, "Car_KiaEV9",
            "Kia EV9", "Kia", "EV9", 2024, "KNDC4CB46LU123456",
            282f, 260f, 3.8f, 550f,
            FuelType.Electric,
            60f, 45f, 6.5f, 300f,
            99.8f, 92f, 85f, 420f, 7.2f, 14.2f,
            "Garage C", VehicleConnectionStatus.Connected, VehicleSystemStatus.Normal, VehicleWarningType.None);

        // 토요타 프리우스 (하이브리드)
        CreateCarData(folderPath, "Car_ToyotaPrius",
            "Toyota Prius", "Toyota", "Prius", 2024, "JTDKN3FU4M9123456",
            180f, 200f, 9.2f, 300f,
            FuelType.Hybrid,
            43f, 40f, 21.5f, 860f,
            13.6f, 88f, 70f, 100f, 4.5f, 5.5f,
            "Garage D", VehicleConnectionStatus.Connected, VehicleSystemStatus.Normal, VehicleWarningType.TirePressureLow);

        // BMW X5 (디젤)
        CreateCarData(folderPath, "Car_BMWX5",
            "BMW X5", "BMW", "X5", 2024, "WBAFD41051G123456",
            265f, 250f, 5.5f, 620f,
            FuelType.Diesel,
            85f, 75f, 12.3f, 921f,
            0f, 100f, 0f, 0f, 0f, 0f,
            "Parking Lot", VehicleConnectionStatus.Error, VehicleSystemStatus.Critical, VehicleWarningType.SensorOffline);

        Debug.Log("샘플 차 데이터 7개 생성 완료");
        AssetDatabase.Refresh();
    }

    private static void CreateCarData(string folderPath, string fileName,
        string carName, string manufacturer, string model, int productionYear, string vinNumber,
        float enginePower, float maxSpeed, float acceleration, float torque,
        FuelType fuelType,
        float fuelTankCapacity, float currentFuelLevel, float fuelConsumption, float remainingFuelRange,
        float batteryCapacity, float batteryHealth, float chargeLevel, float remainingBatteryRange,
        float chargingTime, float energyConsumption,
        string vehicleLocation = "Garage A", 
        VehicleConnectionStatus connectionStatus = VehicleConnectionStatus.Connected,
        VehicleSystemStatus systemStatus = VehicleSystemStatus.Normal,
        VehicleWarningType warningType = VehicleWarningType.None)
    {
        CarData carData = ScriptableObject.CreateInstance<CarData>();

        SetFieldValue(carData, "carName", carName);
        SetFieldValue(carData, "manufacturer", manufacturer);
        SetFieldValue(carData, "model", model);
        SetFieldValue(carData, "productionYear", productionYear);
        SetFieldValue(carData, "vinNumber", vinNumber);
        SetFieldValue(carData, "fuelType", fuelType);

        SetFieldValue(carData, "enginePower", enginePower);
        SetFieldValue(carData, "maxSpeed", maxSpeed);
        SetFieldValue(carData, "acceleration", acceleration);
        SetFieldValue(carData, "torque", torque);

        SetFieldValue(carData, "fuelTankCapacity", fuelTankCapacity);
        SetFieldValue(carData, "currentFuelLevel", currentFuelLevel);
        SetFieldValue(carData, "fuelConsumption", fuelConsumption);
        SetFieldValue(carData, "remainingFuelRange", remainingFuelRange);

        SetFieldValue(carData, "batteryCapacity", batteryCapacity);
        SetFieldValue(carData, "currentBatteryHealth", batteryHealth);
        SetFieldValue(carData, "currentChargeLevel", chargeLevel);
        SetFieldValue(carData, "remainingBatteryRange", remainingBatteryRange);
        SetFieldValue(carData, "chargingTime", chargingTime);
        SetFieldValue(carData, "energyConsumption", energyConsumption);

        SetFieldValue(carData, "mileage", 0f);
        SetFieldValue(carData, "weight", 2000f);
        SetFieldValue(carData, "color", "Silver");
        SetFieldValue(carData, "seatingCapacity", 5);

        // VehicleOverviewPanel 연동 필드
        SetFieldValue(carData, "vehicleLocation", vehicleLocation);
        SetFieldValue(carData, "connectionStatus", connectionStatus);
        SetFieldValue(carData, "systemStatus", systemStatus);
        SetFieldValue(carData, "warningType", warningType);
        SetFieldValue(carData, "lastUpdateTime", System.DateTime.Now);

        string path = $"{folderPath}/{fileName}.asset";
        AssetDatabase.CreateAsset(carData, path);
        Debug.Log($"생성됨: {carName} ({fuelType}) -> {path}");
    }

    // CarData의 모든 필드가 private이라 [SerializeField] 우회 - Inspector를 통하지 않는 일괄 생성용
    private static void SetFieldValue(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            field.SetValue(obj, value);
    }
}
#endif
