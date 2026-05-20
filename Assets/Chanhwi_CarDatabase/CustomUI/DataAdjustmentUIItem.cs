using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 개별 데이터 조정 항목의 UI
/// 슬라이더와 텍스트로 하나의 데이터 필드를 표시하고 조정합니다
/// </summary>
public class DataAdjustmentUIItem : MonoBehaviour
{
    [FoldoutGroup("UI 요소")]
    [SerializeField]
    [Tooltip("필드 이름을 표시할 TextMeshPro")]
    private TextMeshProUGUI labelText;

    [FoldoutGroup("UI 요소")]
    [SerializeField]
    [Tooltip("값을 조정할 슬라이더")]
    private Slider valueSlider;

    [FoldoutGroup("UI 요소")]
    [SerializeField]
    [Tooltip("현재 값을 표시할 TextMeshPro")]
    private TextMeshProUGUI valueText;

    [FoldoutGroup("설정")]
    [SerializeField]
    [Tooltip("값 표시 형식 (예: F1 = 소수점 1자리)")]
    private string valueFormat = "F1";

    private IDataAdjustable dataField;
    private bool isUpdatingUI = false;

    private void OnEnable()
    {
        if (valueSlider != null)
        {
            valueSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    private void OnDisable()
    {
        if (valueSlider != null)
        {
            valueSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }
    }

    /// <summary>
    /// 데이터 필드 설정 및 UI 초기화
    /// </summary>
    public void SetDataField(IDataAdjustable field)
    {
        if (field == null) return;

        dataField = field;

        // LayoutElement 자동 보강 - 아이템 높이가 0이면 슬라이더가 보이지 않음
        if (!TryGetComponent<LayoutElement>(out var le))
            le = gameObject.AddComponent<LayoutElement>();
        if (le.preferredHeight < 1f) le.preferredHeight = 36f;
        if (le.minHeight < 1f) le.minHeight = 28f;
        
        // UI 업데이트
        if (labelText != null)
        {
            labelText.text = field.DisplayName;
        }

        if (valueSlider != null)
        {
            valueSlider.minValue = 0f;
            valueSlider.maxValue = 1f;
            valueSlider.value = field.GetNormalizedValue();
        }

        UpdateValueDisplay();
    }

    private void OnSliderChanged(float normalizedValue)
    {
        if (dataField == null || isUpdatingUI) return;

        dataField.SetNormalizedValue(normalizedValue);
        UpdateValueDisplay();
    }

    /// <summary>
    /// 값 텍스트 업데이트
    /// </summary>
    private void UpdateValueDisplay()
    {
        if (dataField == null || valueText == null) return;

        isUpdatingUI = true;
        
        float value = dataField.GetValue();
        valueText.text = value.ToString(valueFormat);
        
        if (valueSlider != null && Mathf.Abs(valueSlider.value - dataField.GetNormalizedValue()) > 0.001f)
        {
            valueSlider.value = dataField.GetNormalizedValue();
        }

        isUpdatingUI = false;
    }

    /// <summary>
    /// 매 프레임 데이터 동기화
    /// </summary>
    private void Update()
    {
        if (dataField == null) return;
        UpdateValueDisplay();
    }

    /// <summary>
    /// 아이템 활성화/비활성화
    /// </summary>
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
