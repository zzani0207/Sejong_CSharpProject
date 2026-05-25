using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// CarData의 필드를 UI에서 조정 가능한 IDataAdjustable 목록으로 노출하는 어댑터.
/// 카테고리별로 필드를 묶어 SidebarUIManager가 패널 단위로 가져갈 수 있게 한다.
/// </summary>
public abstract class CarDataProviderBase : MonoBehaviour
{
    [FoldoutGroup("참조")]
    [SerializeField]
    [Tooltip("자동차 데이터 객체")]
    protected CarData carData;

    public virtual void SetCarData(CarData car)
    {
        carData = car;
    }

    public abstract List<IDataAdjustable> GetAllDataFields();
    public abstract List<IDataAdjustable> GetDataFieldsByCategory(string category);
    public abstract List<string> GetAvailableCategories();

    protected CarData GetCarData()
    {
        if (carData != null) return carData;

        var manager = CarDataManager.Instance;
        if (manager != null)
            carData = manager.CurrentCar;

        return carData;
    }
}
