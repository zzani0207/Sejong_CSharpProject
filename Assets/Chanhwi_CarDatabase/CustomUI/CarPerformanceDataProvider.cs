using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 자동차의 성능 데이터를 제공합니다
/// 엔진 파워, 최대 속도, 가속도, 토크 등
/// </summary>
public class CarPerformanceDataProvider : CarDataProviderBase
{
    [FoldoutGroup("성능 범위 설정")]
    [SerializeField]
    [Range(100, 1000)]
    [Tooltip("엔진 파워 최대값 (kW)")]
    private float enginePowerMax = 600f;

    [FoldoutGroup("성능 범위 설정")]
    [SerializeField]
    [Range(200, 500)]
    [Tooltip("최대 속도 최대값 (km/h)")]
    private float maxSpeedMax = 350f;

    [FoldoutGroup("성능 범위 설정")]
    [SerializeField]
    [Range(2, 10)]
    [Tooltip("가속도 최소값 (0-100 초)")]
    private float accelerationMin = 2f;

    [FoldoutGroup("성능 범위 설정")]
    [SerializeField]
    [Range(2, 10)]
    [Tooltip("가속도 최대값 (0-100 초)")]
    private float accelerationMax = 8f;

    [FoldoutGroup("성능 범위 설정")]
    [SerializeField]
    [Range(200, 1200)]
    [Tooltip("토크 최대값 (N·m)")]
    private float torqueMax = 800f;

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
            0,
            enginePowerMax,
            null,
            () => carData != null ? carData.EnginePower : 0f,
            (value) => { },
            "Engine Power"
        ));

        performanceFields.Add(new UIDataField(
            "maxSpeed",
            "Max Speed (km/h)",
            0,
            maxSpeedMax,
            null,
            () => carData != null ? carData.MaxSpeed : 0f,
            (value) => { },
            "Max Speed"
        ));

        performanceFields.Add(new UIDataField(
            "acceleration",
            "Acceleration (0-100)",
            accelerationMin,
            accelerationMax,
            null,
            () => carData != null ? carData.Acceleration : 0f,
            (value) => { },
            "0-100km/h Time"
        ));

        performanceFields.Add(new UIDataField(
            "torque",
            "Torque (N.m)",
            0,
            torqueMax,
            null,
            () => carData != null ? carData.Torque : 0f,
            (value) => { },
            "Engine Torque"
        ));

        return performanceFields;
    }
}
