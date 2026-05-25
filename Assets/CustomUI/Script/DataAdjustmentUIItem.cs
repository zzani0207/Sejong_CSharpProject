using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 슬라이더 + 라벨 + 값 텍스트 한 줄로 IDataAdjustable 하나를 표시/편집한다.
/// </summary>
public class DataAdjustmentUIItem : MonoBehaviour
{
    [FoldoutGroup("UI 요소")]
    [SerializeField] private TextMeshProUGUI labelText;

    [FoldoutGroup("UI 요소")]
    [SerializeField] private Slider valueSlider;

    [FoldoutGroup("UI 요소")]
    [SerializeField] private TextMeshProUGUI valueText;

    [FoldoutGroup("설정")]
    [SerializeField]
    [Tooltip("값 표시 형식 (예: F1 = 소수점 1자리)")]
    private string valueFormat = "F1";

    private IDataAdjustable dataField;
    private bool isUpdatingUI = false;
    private float lastSliderValue = -1f;

    private void OnEnable()
    {
        EnsureSliderRef();
    }

    private void EnsureSliderRef()
    {
        // 인스펙터 미할당이면 자식에서, 그래도 못 찾으면 부모 쪽도 시도
        if (valueSlider == null)
            valueSlider = GetComponentInChildren<Slider>(true);
        if (valueSlider == null)
            valueSlider = GetComponentInParent<Slider>(true);
    }

    public void SetDataField(IDataAdjustable field)
    {
        if (field == null) return;

        dataField = field;
        EnsureSliderRef();

        // LayoutElement가 없거나 높이가 0이면 슬라이더가 보이지 않는다 - 안전 기본값 보강
        if (!TryGetComponent<LayoutElement>(out var le))
            le = gameObject.AddComponent<LayoutElement>();
        if (le.preferredHeight < 1f) le.preferredHeight = 36f;
        if (le.minHeight < 1f) le.minHeight = 28f;

        if (labelText != null)
            labelText.text = field.DisplayName;

        if (valueSlider != null)
        {
            valueSlider.minValue = 0f;
            valueSlider.maxValue = 1f;
            valueSlider.value = field.GetNormalizedValue();
            lastSliderValue = valueSlider.value;
        }

        UpdateValueDisplay();
    }

    // Update에서 매 프레임 slider.value를 직접 감시 - onValueChanged listener가 어떤 이유로든
    // 동작 안 해도 setter가 호출되도록 보장하는 백업 메커니즘
    private void Update()
    {
        if (dataField == null || valueSlider == null) return;

        float currentSliderValue = valueSlider.value;

        // 사용자가 슬라이더를 드래그해서 값이 바뀐 경우 (isUpdatingUI=false 상태)
        if (!isUpdatingUI && !Mathf.Approximately(currentSliderValue, lastSliderValue))
        {
            dataField.SetNormalizedValue(currentSliderValue);
            lastSliderValue = currentSliderValue;
        }

        UpdateValueDisplay();
    }

    private void UpdateValueDisplay()
    {
        if (dataField == null) return;

        isUpdatingUI = true;

        // 값 텍스트는 실제 데이터값으로
        if (valueText != null)
        {
            float value = dataField.GetValue();
            valueText.text = value.ToString(valueFormat);
        }

        // 슬라이더는 데이터값에 맞게 동기화 (외부에서 데이터 바꿔도 슬라이더 위치 따라옴)
        if (valueSlider != null)
        {
            float norm = dataField.GetNormalizedValue();
            if (Mathf.Abs(valueSlider.value - norm) > 0.001f)
                valueSlider.value = norm;
            // 데이터값으로 슬라이더 위치를 갱신했으므로 lastSliderValue도 함께 갱신 -
            // 다음 프레임에서 "사용자 변경"으로 잘못 인식되는 것 방지
            lastSliderValue = valueSlider.value;
        }

        isUpdatingUI = false;
    }
}
