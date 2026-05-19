using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 차 정보를 UI에 표시하는 컨트롤러
/// CarManager의 이벤트를 구독하여 자동으로 UI 업데이트
/// </summary>
public class CarUIController : MonoBehaviour
{
    [Header("차 정보 UI")]
    [SerializeField] private Text carNameText;
    [SerializeField] private Text manufacturerText;
    [SerializeField] private Text modelText;
    [SerializeField] private Text productionYearText;
    [SerializeField] private Text vinText;

    [Header("성능 정보 UI")]
    [SerializeField] private Text enginePowerText;
    [SerializeField] private Text maxSpeedText;
    [SerializeField] private Text accelerationText;
    [SerializeField] private Text torqueText;

    [Header("배터리 정보 UI")]
    [SerializeField] private Text batteryCapacityText;
    [SerializeField] private Text batteryHealthText;
    [SerializeField] private Text chargePercentText;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private Text remainingRangeText;
    [SerializeField] private Text chargingTimeText;

    [Header("연료 정보 UI")]
    [SerializeField] private Text fuelTypeText;
    [SerializeField] private Text fuelLevelText;
    [SerializeField] private Text fuelConsumptionText;
    [SerializeField] private Slider fuelSlider;

    [Header("차량 상태 UI")]
    [SerializeField] private Text mileageText;
    [SerializeField] private Text weightText;
    [SerializeField] private Text colorText;
    [SerializeField] private Text seatingCapacityText;

    [Header("효율성 UI")]
    [SerializeField] private Text energyConsumptionText;

    [Header("네비게이션 버튼")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Text carIndexText;

    private void Start()
    {
        // CarManager 이벤트 구독
        if (CarManager.Instance != null)
        {
            CarManager.Instance.OnCarChanged += UpdateAllUI;
            CarManager.Instance.OnBatteryChanged += UpdateBatteryUI;
            CarManager.Instance.OnFuelChanged += UpdateFuelUI;
            CarManager.Instance.OnMileageChanged += UpdateMileageUI;

            // 버튼 리스너 등록
            if (previousButton != null)
                previousButton.onClick.AddListener(CarManager.Instance.SelectPreviousCar);
            if (nextButton != null)
                nextButton.onClick.AddListener(CarManager.Instance.SelectNextCar);

            // 초기 UI 업데이트
            UpdateAllUI(CarManager.Instance.CurrentCar);
        }
        else
        {
            Debug.LogError("CarManager not found!");
        }
    }

    /// <summary>
    /// 모든 UI 업데이트
    /// </summary>
    private void UpdateAllUI(CarData car)
    {
        if (car == null) return;

        UpdateBasicInfo(car);
        UpdatePerformanceInfo(car);
        UpdateFuelInfo(car);
        UpdateBatteryInfo(car);
        UpdateVehicleStatus(car);
        UpdateEfficiencyInfo(car);
        UpdateNavigationInfo();
    }

    /// <summary>
    /// 기본 정보 UI 업데이트
    /// </summary>
    private void UpdateBasicInfo(CarData car)
    {
        if (carNameText != null) carNameText.text = car.CarName;
        if (manufacturerText != null) manufacturerText.text = $"제조사: {car.Manufacturer}";
        if (modelText != null) modelText.text = $"모델: {car.Model}";
        if (productionYearText != null) productionYearText.text = $"출시년도: {car.ProductionYear}";
        if (vinText != null) vinText.text = $"VIN: {car.VinNumber}";
    }

    /// <summary>
    /// 성능 정보 UI 업데이트
    /// </summary>
    private void UpdatePerformanceInfo(CarData car)
    {
        if (enginePowerText != null) enginePowerText.text = $"엔진: {car.EnginePower}kW";
        if (maxSpeedText != null) maxSpeedText.text = $"최대속도: {car.MaxSpeed}km/h";
        if (accelerationText != null) accelerationText.text = $"가속: 0-100km/h {car.Acceleration}초";
        if (torqueText != null) torqueText.text = $"토크: {car.Torque}N·m";
    }

    /// <summary>
    /// 배터리 정보 UI 업데이트
    /// </summary>
    private void UpdateBatteryInfo(CarData car)
    {
        // 배터리 정보가 있는 차만 표시
        if (car.FuelType == FuelType.Electric || car.FuelType == FuelType.Hybrid || car.FuelType == FuelType.PHEV)
        {
            if (batteryCapacityText != null) batteryCapacityText.text = $"용량: {car.BatteryCapacity}kWh";
            if (batteryHealthText != null) batteryHealthText.text = $"건강도: {car.CurrentBatteryHealth:F1}%";
            if (chargePercentText != null) chargePercentText.text = $"{car.CurrentChargeLevel:F1}%";
            if (batterySlider != null) batterySlider.value = car.CurrentChargeLevel / 100f;
            if (remainingRangeText != null) remainingRangeText.text = $"배터리 남은거리: {car.RemainingBatteryRange:F0}km";
            if (chargingTimeText != null) chargingTimeText.text = $"충전시간: {car.ChargingTime:F1}시간";
        }
        else
        {
            // 배터리가 없는 차는 UI 비활성화
            if (batteryCapacityText != null) batteryCapacityText.text = "N/A";
            if (batteryHealthText != null) batteryHealthText.text = "N/A";
            if (chargePercentText != null) chargePercentText.text = "N/A";
            if (remainingRangeText != null) remainingRangeText.text = "N/A";
            if (chargingTimeText != null) chargingTimeText.text = "N/A";
        }
    }

    /// <summary>
    /// 연료 정보 UI 업데이트
    /// </summary>
    private void UpdateFuelInfo(CarData car)
    {
        if (fuelTypeText != null) fuelTypeText.text = $"연료: {car.FuelTypeString}";

        // 연료 정보가 있는 차만 표시
        if (car.FuelType == FuelType.Gasoline || car.FuelType == FuelType.Diesel || 
            car.FuelType == FuelType.Hybrid || car.FuelType == FuelType.PHEV)
        {
            if (fuelLevelText != null) fuelLevelText.text = $"{car.CurrentFuelLevel:F1}L / {car.FuelTankCapacity}L";
            if (fuelConsumptionText != null) fuelConsumptionText.text = $"연비: {car.FuelConsumption}km/L";
            if (fuelSlider != null) fuelSlider.value = car.CurrentFuelLevel / car.FuelTankCapacity;
        }
        else
        {
            if (fuelLevelText != null) fuelLevelText.text = "N/A";
            if (fuelConsumptionText != null) fuelConsumptionText.text = "N/A";
            if (fuelSlider != null) fuelSlider.value = 0;
        }
    }

    /// <summary>
    /// 배터리 UI만 업데이트 (배터리 변경 시)
    /// </summary>
    private void UpdateBatteryUI(float batteryLevel)
    {
        if (CarManager.Instance != null && CarManager.Instance.CurrentCar != null)
        {
            UpdateBatteryInfo(CarManager.Instance.CurrentCar);
        }
    }

    /// <summary>
    /// 연료 UI만 업데이트 (연료 변경 시)
    /// </summary>
    private void UpdateFuelUI(float fuelLevel)
    {
        if (CarManager.Instance != null && CarManager.Instance.CurrentCar != null)
        {
            UpdateFuelInfo(CarManager.Instance.CurrentCar);
        }
    }

    /// <summary>
    /// 차량 상태 UI 업데이트
    /// </summary>
    private void UpdateVehicleStatus(CarData car)
    {
        if (mileageText != null) mileageText.text = $"주행거리: {car.Mileage:F0}km";
        if (weightText != null) weightText.text = $"무게: {car.Weight}kg";
        if (colorText != null) colorText.text = $"색상: {car.Color}";
        if (seatingCapacityText != null) seatingCapacityText.text = $"탑승인원: {car.SeatingCapacity}명";
    }

    /// <summary>
    /// 주행거리 UI 업데이트
    /// </summary>
    private void UpdateMileageUI(float mileage)
    {
        if (mileageText != null) mileageText.text = $"주행거리: {mileage:F0}km";
    }

    /// <summary>
    /// 효율성 정보 UI 업데이트
    /// </summary>
    private void UpdateEfficiencyInfo(CarData car)
    {
        if (energyConsumptionText != null) energyConsumptionText.text = $"에너지 소비: {car.EnergyConsumption}kWh/100km";
    }

    /// <summary>
    /// 네비게이션 정보 UI 업데이트
    /// </summary>
    private void UpdateNavigationInfo()
    {
        if (CarManager.Instance != null && carIndexText != null)
        {
            carIndexText.text = $"{CarManager.Instance.CurrentCarIndex + 1}/{CarManager.Instance.GetTotalCarCount()}";
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (CarManager.Instance != null)
        {
            CarManager.Instance.OnCarChanged -= UpdateAllUI;
            CarManager.Instance.OnBatteryChanged -= UpdateBatteryUI;
            CarManager.Instance.OnFuelChanged -= UpdateFuelUI;
            CarManager.Instance.OnMileageChanged -= UpdateMileageUI;
        }
    }
}
