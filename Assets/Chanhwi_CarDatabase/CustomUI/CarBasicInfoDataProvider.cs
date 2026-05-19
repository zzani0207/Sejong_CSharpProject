using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 자동차의 기본 정보를 제공합니다
/// 자동차 이름, 제조사, 모델, 색상, 좌석 수 등
/// </summary>
public class CarBasicInfoDataProvider : CarDataProviderBase
{
    private List<IDataAdjustable> basicInfoFields = new List<IDataAdjustable>();

    public override List<string> GetAvailableCategories()
    {
        return new List<string> { "BasicInfo" };
    }

    public override List<IDataAdjustable> GetAllDataFields()
    {
        return GetDataFieldsByCategory("BasicInfo");
    }

    public override List<IDataAdjustable> GetDataFieldsByCategory(string category)
    {
        if (category != "BasicInfo") return new List<IDataAdjustable>();

        basicInfoFields.Clear();

        if (GetCarData() == null)
        {
            Debug.LogWarning("CarBasicInfoDataProvider: CarData is null!");
            return basicInfoFields;
        }

        basicInfoFields.Add(new UIDataField(
            "productionYear",
            "Production Year",
            1990,
            2030,
            null,
            () => carData != null ? (float)carData.ProductionYear : 0f,
            (value) => { },
            "Production Year"
        ));

        basicInfoFields.Add(new UIDataField(
            "seatingCapacity",
            "Seating Capacity",
            1,
            10,
            null,
            () => carData != null ? (float)carData.SeatingCapacity : 0f,
            (value) => { },
            "Seating Capacity"
        ));

        return basicInfoFields;
    }
}
