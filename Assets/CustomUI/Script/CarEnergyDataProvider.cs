using UnityEngine;
using System.Collections.Generic;

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
                    0, 100,
                    () => carData != null ? carData.CurrentChargeLevel : 0f,
                    (value) => { if (carData != null) carData.SetChargeLevel(value); }
                ));

                energyFields.Add(new UIDataField(
                    "batteryHealth",
                    "Battery Health (%)",
                    0, 100,
                    () => carData != null ? carData.CurrentBatteryHealth : 0f,
                    (value) => { if (carData != null) carData.SetBatteryHealth(value); }
                ));

                energyFields.Add(new UIDataField(
                    "batteryCapacity",
                    "Battery Capacity (kWh)",
                    10, 200,
                    () => carData != null ? carData.BatteryCapacity : 0f,
                    (value) => { if (carData != null) carData.SetBatteryCapacity(value); }
                ));

                break;

            case "Fuel":
                energyFields.Add(new UIDataField(
                    "fuelLevel",
                    "Fuel Level (L)",
                    0, 150,
                    () => carData != null ? carData.CurrentFuelLevel : 0f,
                    (value) => { if (carData != null) carData.SetFuelLevel(value); }
                ));

                energyFields.Add(new UIDataField(
                    "fuelConsumption",
                    "Fuel Consumption (km/L)",
                    0, 25,
                    () => carData != null ? carData.FuelConsumption : 0f,
                    (value) => { if (carData != null) carData.SetFuelConsumption(value); }
                ));

                break;

            case "Energy":
                energyFields.Add(new UIDataField(
                    "energyConsumption",
                    "Energy Consumption (kWh/100km)",
                    5, 50,
                    () => carData != null ? carData.EnergyConsumption : 0f,
                    (value) => { if (carData != null) carData.SetEnergyConsumption(value); }
                ));

                energyFields.Add(new UIDataField(
                    "chargingTime",
                    "Charging Time (hours)",
                    0.5f, 24,
                    () => carData != null ? carData.ChargingTime : 0f,
                    (value) => { if (carData != null) carData.SetChargingTime(value); }
                ));

                break;
        }

        return energyFields;
    }
}
