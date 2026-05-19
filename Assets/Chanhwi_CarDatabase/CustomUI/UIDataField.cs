using UnityEngine;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// UI에서 조정할 수 있는 개별 데이터 필드 정의
/// CarData의 특정 필드를 래핑하여 조정 가능하게 만듭니다
/// </summary>
[System.Serializable]
public class UIDataField : IDataAdjustable
{
    [SerializeField]
    [Tooltip("필드의 고유 ID")]
    private string fieldId;

    [SerializeField]
    [Tooltip("UI에 표시될 이름")]
    private string displayName;

    [SerializeField]
    [Range(0, 1000)]
    [Tooltip("최소값")]
    private float minValue = 0f;

    [SerializeField]
    [Range(0, 10000)]
    [Tooltip("최대값")]
    private float maxValue = 100f;

    [SerializeField]
    [Tooltip("필드 설명")]
    private string description;

    [SerializeField]
    [Tooltip("필드가 활성화되어 있는지 여부")]
    private bool isActive = true;

    private CarData carData;
    private Func<float> getter;
    private Action<float> setter;

    public string FieldId => fieldId;
    public string DisplayName => displayName;
    public string Description => description;

    public UIDataField() { }

    public UIDataField(string id, string name, float min, float max, CarData car, 
                       Func<float> getterFunc, Action<float> setterFunc, string desc = "")
    {
        fieldId = id;
        displayName = name;
        minValue = min;
        maxValue = max;
        carData = car;
        getter = getterFunc;
        setter = setterFunc;
        description = desc;
        isActive = true;
    }

    public float GetValue()
    {
        return getter?.Invoke() ?? 0f;
    }

    public void SetNormalizedValue(float normalizedValue)
    {
        // 정규화된 값(0~1)을 실제 범위로 변환
        float clampedValue = Mathf.Clamp01(normalizedValue);
        float actualValue = Mathf.Lerp(minValue, maxValue, clampedValue);
        setter?.Invoke(actualValue);
    }

    public float GetMinValue() => minValue;

    public float GetMaxValue() => maxValue;

    public float GetNormalizedValue()
    {
        float currentValue = GetValue();
        if (maxValue - minValue < 0.001f) return 0f;
        return Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));
    }

    public bool IsActive() => isActive;

    public void SetActive(bool active) => isActive = active;
}
