using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 자동차의 에너지 데이터를 제공합니다
/// 배터리 충전량, 연료량, 남은 주행거리 등
/// </summary>
public class CarEnergyDataProvider : CarDataProviderBase
{
    private List<IDataAdjustable> energyFields = new List<IDataAdjustable>();

    public override List<string> GetAvailableCategories()
    {
        return new List<string> { "Energy", "Battery", "Fuel" };
    }

    public override List<IDataAdjustable> GetAllDataFields()
    {
        var allFields = new List<IDataAdjustable>();
        allFields.AddRange(GetDataFieldsByCategory("Energy"));
        allFields.AddRange(GetDataFieldsByCategory("Battery"));
        allFields.AddRange(GetDataFieldsByCategory("Fuel"));
        return allFields;
    }

    public override List<IDataAdjustable> GetDataFieldsByCategory(string category)
    {
        energyFields.Clear();

        if (GetCarData() == null)
        {
            Debug.LogWarning("CarEnergyDataProvider: CarData is null!");
            return energyFields;
        }

        switch (category)
        {
            case "Battery":
                energyFields.Add(new UIDataField(
                    "chargeLevel",
                    "Battery Level (%)",
                    0,
                    100,
                    null,
                    () => carData != null ? carData.CurrentChargeLevel : 0f,
                    (value) => carData?.SetChargeLevel(value),
                    "Battery Charge Level"
                ));

                energyFields.Add(new UIDataField(
                    "batteryHealth",
                    "Battery Health (%)",
                    0,
                    100,
                    null,
                    () => carData != null ? carData.CurrentBatteryHealth : 0f,
                    (value) => { },
                    "Battery Health (Read Only)"
                ));

                energyFields.Add(new UIDataField(
                    "batteryCapacity",
                    "Battery Capacity (kWh)",
                    0,
                    200,
                    null,
                    () => carData != null ? carData.BatteryCapacity : 0f,
                    (value) => { },
                    "Battery Capacity (Read Only)"
                ));

                energyFields.Add(new UIDataField(
                    "batteryRange",
                    "Battery Range (km)",
                    0,
                    1000,
                    null,
                    () => carData != null ? carData.RemainingBatteryRange : 0f,
                    (value) => { },
                    "Battery Range (Read Only)"
                ));

                break;

            case "Fuel":
                energyFields.Add(new UIDataField(
                    "fuelLevel",
                    "Fuel Level (L)",
                    0,
                    carData != null ? carData.FuelTankCapacity : 100f,
                    null,
                    () => carData != null ? carData.CurrentFuelLevel : 0f,
                    (value) => carData?.SetFuelLevel(value),
                    "Current Fuel Level"
                ));

                energyFields.Add(new UIDataField(
                    "fuelConsumption",
                    "Fuel Consumption (km/L)",
                    0,
                    20,
                    null,
                    () => carData != null ? carData.FuelConsumption : 0f,
                    (value) => { },
                    "Fuel Consumption (Read Only)"
                ));

                energyFields.Add(new UIDataField(
                    "fuelRange",
                    "Fuel Range (km)",
                    0,
                    1000,
                    null,
                    () => carData != null ? carData.RemainingFuelRange : 0f,
                    (value) => { },
                    "Fuel Range (Read Only)"
                ));

                break;

            case "Energy":
                energyFields.Add(new UIDataField(
                    "energyConsumption",
                    "Energy Consumption (kWh/100km)",
                    0,
                    30,
                    null,
                    () => carData != null ? carData.EnergyConsumption : 0f,
                    (value) => { },
                    "Energy Consumption (Read Only)"
                ));

                energyFields.Add(new UIDataField(
                    "chargingTime",
                    "Charging Time (hours)",
                    0,
                    24,
                    null,
                    () => carData != null ? carData.ChargingTime : 0f,
                    (value) => { },
                    "Charging Time (Read Only)"
                ));

                break;
        }

        return energyFields;
    }
}
