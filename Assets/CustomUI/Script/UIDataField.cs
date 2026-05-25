using UnityEngine;
using System;

[System.Serializable]
public class UIDataField : IDataAdjustable
{
    [SerializeField] private string fieldId;
    [SerializeField] private string displayName;
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;
    [SerializeField] private bool isActive = true;

    private Func<float> getter;
    private Action<float> setter;

    public string FieldId => fieldId;
    public string DisplayName => displayName;

    public UIDataField(string id, string name, float min, float max,
                       Func<float> getterFunc, Action<float> setterFunc)
    {
        fieldId = id;
        displayName = name;
        minValue = min;
        maxValue = max;
        getter = getterFunc;
        setter = setterFunc;
    }

    public float GetValue() => getter?.Invoke() ?? 0f;

    public void SetNormalizedValue(float normalizedValue)
    {
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
}
