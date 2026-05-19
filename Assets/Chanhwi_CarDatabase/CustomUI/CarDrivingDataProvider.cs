using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 자동차의 주행 정보를 제공합니다
/// 마일리지, 무게, 무게 분배 등
/// </summary>
public class CarDrivingDataProvider : CarDataProviderBase
{
    [FoldoutGroup("주행 범위 설정")]
    [SerializeField]
    [Range(100000, 1000000)]
    [Tooltip("최대 마일리지 (km)")]
    private float maxMileage = 500000f;

    [FoldoutGroup("주행 범위 설정")]
    [SerializeField]
    [Range(1000, 5000)]
    [Tooltip("최대 무게 (kg)")]
    private float maxWeight = 3000f;

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
            0,
            maxMileage,
            null,
            () => carData != null ? carData.Mileage : 0f,
            (value) => { },
            "Total Mileage"
        ));

        drivingFields.Add(new UIDataField(
            "weight",
            "Weight (kg)",
            500,
            maxWeight,
            null,
            () => carData != null ? carData.Weight : 0f,
            (value) => { },
            "Vehicle Weight"
        ));

        return drivingFields;
    }
}
