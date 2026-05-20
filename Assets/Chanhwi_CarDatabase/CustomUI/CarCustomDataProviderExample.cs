using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 커스텀 데이터 제공자 예제
/// 사용자가 이 예제를 기반으로 자신만의 데이터 제공자를 만들 수 있습니다
/// 
/// 사용법:
/// 1. 이 클래스를 상속받아 새로운 제공자 클래스를 만듭니다
/// 2. GetAvailableCategories()에서 제공할 카테고리 목록을 반환합니다
/// 3. GetDataFieldsByCategory()에서 해당 카테고리의 데이터 필드를 생성합니다
/// 4. SidebarUISetup에서 새 제공자를 등록합니다
/// </summary>
public class CarCustomDataProviderExample : CarDataProviderBase
{
    [FoldoutGroup("Custom Range")]
    [SerializeField]
    [Range(0, 1000)]
    [Tooltip("커스텀 파라미터 1 최대값")]
    private float customParam1Max = 100f;

    [FoldoutGroup("Custom Range")]
    [SerializeField]
    [Range(0, 1000)]
    [Tooltip("커스텀 파라미터 2 최대값")]
    private float customParam2Max = 100f;

    private List<IDataAdjustable> customFields = new List<IDataAdjustable>();

    public override List<string> GetAvailableCategories()
    {
        return new List<string> { "Custom" };
    }

    public override List<IDataAdjustable> GetAllDataFields()
    {
        return GetDataFieldsByCategory("Custom");
    }

    public override List<IDataAdjustable> GetDataFieldsByCategory(string category)
    {
        if (category != "Custom") return new List<IDataAdjustable>();

        customFields.Clear();
        CarData car = GetCarData();

        if (car == null)
        {
            Debug.LogWarning("CarCustomDataProviderExample: CarData is null!");
            return customFields;
        }

        // 예제: 커스텀 필드 1
        // UIDataField의 매개변수:
        // 1. fieldId: 고유 식별자 (필수)
        // 2. displayName: UI에 표시할 이름 (필수)
        // 3. minValue: 최소값 (필수)
        // 4. maxValue: 최대값 (필수)
        // 5. carData: 차량 데이터 객체 (필수)
        // 6. getter: 값을 가져오는 함수 (필수)
        // 7. setter: 값을 설정하는 함수 (필수)
        // 8. description: 필드 설명 (선택)

        customFields.Add(new UIDataField(
            "customParam1",
            "Custom Parameter 1",
            0,
            customParam1Max,
            car,
            () => car.EnginePower * 0.5f, // 예: 엔진 파워를 기반으로 계산
            (value) =>
            {
                // 값 설정 로직 구현
                // 예: 데이터베이스에 저장, 게임 상태 업데이트 등
                Debug.Log($"Custom Parameter 1 set to: {value}");
            },
            "커스텀 파라미터 1"
        ));

        // 예제: 커스텀 필드 2
        customFields.Add(new UIDataField(
            "customParam2",
            "Custom Parameter 2",
            0,
            customParam2Max,
            car,
            () => car.CurrentBatteryHealth,
            (value) =>
            {
                Debug.Log($"Custom Parameter 2 set to: {value}");
            },
            "커스텀 파라미터 2"
        ));

        return customFields;
    }

    /// <summary>
    /// 커스텀 메서드 예제
    /// 필요시 특정 동작을 수행하는 메서드를 추가할 수 있습니다
    /// </summary>
    public void DoCustomAction()
    {
        Debug.Log("Custom action executed!");
    }
}
