using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// CarData의 필드를 IDataAdjustable로 변환하여 제공하는 추상 클래스
/// 다양한 카테고리별로 데이터 필드를 제공합니다
/// </summary>
public abstract class CarDataProviderBase : MonoBehaviour
{
    [FoldoutGroup("참조")]
    [SerializeField]
    [Tooltip("자동차 데이터 객체")]
    protected CarData carData;

    /// <summary>
    /// 현재 자동차 데이터 설정
    /// </summary>
    public virtual void SetCarData(CarData car)
    {
        carData = car;
    }

    /// <summary>
    /// 모든 사용 가능한 데이터 필드 반환
    /// </summary>
    public abstract List<IDataAdjustable> GetAllDataFields();

    /// <summary>
    /// 특정 카테고리의 데이터 필드 반환
    /// </summary>
    public abstract List<IDataAdjustable> GetDataFieldsByCategory(string category);

    /// <summary>
    /// 사용 가능한 모든 카테고리 반환
    /// </summary>
    public abstract List<string> GetAvailableCategories();

    protected CarData GetCarData()
    {
        if (carData != null) return carData;

        var manager = CarManager.Instance;
        if (manager != null)
            carData = manager.CurrentCar;

        return carData;
    }
}
