using UnityEngine;

/// <summary>
/// 조정 가능한 데이터 필드의 인터페이스
/// 다양한 데이터 타입의 조정을 지원합니다
/// Odin Inspector와 통합되어 더 나은 인스펙터 경험을 제공합니다
/// </summary>
public interface IDataAdjustable
{
    /// <summary>
    /// 필드의 고유 ID
    /// </summary>
    string FieldId { get; }

    /// <summary>
    /// 필드의 표시 이름
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 현재 값을 반환
    /// </summary>
    float GetValue();

    /// <summary>
    /// 값을 설정 (0~1 범위의 정규화된 값)
    /// </summary>
    void SetNormalizedValue(float normalizedValue);

    /// <summary>
    /// 최소값 반환
    /// </summary>
    float GetMinValue();

    /// <summary>
    /// 최대값 반환
    /// </summary>
    float GetMaxValue();

    /// <summary>
    /// 현재 값의 정규화된 형태 (0~1)
    /// </summary>
    float GetNormalizedValue();

    /// <summary>
    /// 필드가 활성화되어 있는지 확인
    /// </summary>
    bool IsActive();
}
