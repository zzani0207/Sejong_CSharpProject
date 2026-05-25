using UnityEngine;

/// <summary>
/// 슬라이더 등 UI에서 조작 가능한 단일 필드. 값은 항상 0~1 정규화로 주고받는다.
/// </summary>
public interface IDataAdjustable
{
    string FieldId { get; }
    string DisplayName { get; }

    float GetValue();
    float GetMinValue();
    float GetMaxValue();
    float GetNormalizedValue();

    /// <summary>0~1 정규화 값을 [Min, Max] 범위로 매핑하여 적용.</summary>
    void SetNormalizedValue(float normalizedValue);

    bool IsActive();
}
