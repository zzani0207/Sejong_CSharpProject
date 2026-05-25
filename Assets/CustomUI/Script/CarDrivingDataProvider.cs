using UnityEngine;
using System.Collections.Generic;

public class CarDrivingDataProvider : CarDataProviderBase
{
    private List<IDataAdjustable> drivingFields = new List<IDataAdjustable>();

    public override List<string> GetAvailableCategories()
    {
        return new List<string> { "Driving" };
    }

    public override List<IDataAdjustable> GetAllDataFields()
    {
        return GetDataFieldsByCategory("Driving");
    }

    public override List<IDataAdjustable> GetDataFieldsByCategory(string category)
    {
        if (category != "Driving") return new List<IDataAdjustable>();

        drivingFields.Clear();

        if (GetCarData() == null)
        {
            Debug.LogWarning("CarDrivingDataProvider: CarData is null!");
            return drivingFields;
        }

        drivingFields.Add(new UIDataField(
            "mileage",
            "Mileage (km)",
            0, 500000,
            () => carData != null ? carData.Mileage : 0f,
            (value) => { if (carData != null) carData.SetMileage(value); }
        ));

        drivingFields.Add(new UIDataField(
            "weight",
            "Weight (kg)",
            500, 3000,
            () => carData != null ? carData.Weight : 0f,
            (value) => { if (carData != null) carData.SetWeight(value); }
        ));

        return drivingFields;
    }
}
