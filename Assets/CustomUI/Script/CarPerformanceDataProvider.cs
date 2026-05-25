using UnityEngine;
using System.Collections.Generic;

public class CarPerformanceDataProvider : CarDataProviderBase
{
    private List<IDataAdjustable> performanceFields = new List<IDataAdjustable>();

    public override List<string> GetAvailableCategories()
    {
        return new List<string> { "Performance" };
    }

    public override List<IDataAdjustable> GetAllDataFields()
    {
        return GetDataFieldsByCategory("Performance");
    }

    public override List<IDataAdjustable> GetDataFieldsByCategory(string category)
    {
        if (category != "Performance") return new List<IDataAdjustable>();

        performanceFields.Clear();

        if (GetCarData() == null)
        {
            Debug.LogWarning("CarPerformanceDataProvider: CarData is null!");
            return performanceFields;
        }

        performanceFields.Add(new UIDataField(
            "enginePower",
            "Engine Power (kW)",
            0, 1000,
            () => carData != null ? carData.EnginePower : 0f,
            (value) => { if (carData != null) carData.SetEnginePower(value); }
        ));

        performanceFields.Add(new UIDataField(
            "maxSpeed",
            "Max Speed (km/h)",
            0, 500,
            () => carData != null ? carData.MaxSpeed : 0f,
            (value) => { if (carData != null) carData.SetMaxSpeed(value); }
        ));

        performanceFields.Add(new UIDataField(
            "acceleration",
            "Acceleration (0-100 sec)",
            1, 15,
            () => carData != null ? carData.Acceleration : 0f,
            (value) => { if (carData != null) carData.SetAcceleration(value); }
        ));

        performanceFields.Add(new UIDataField(
            "torque",
            "Torque (N·m)",
            0, 1200,
            () => carData != null ? carData.Torque : 0f,
            (value) => { if (carData != null) carData.SetTorque(value); }
        ));

        return performanceFields;
    }
}
