using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;

/// <summary>
/// 사이드바 UI 전반 제어
/// - 가로 너비 토글 (얇음 ↔ 펼침)
/// - CanvasGroup으로 표시/숨김
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CustomUI : MonoBehaviour
{
    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Tooltip("사이드바 RectTransform (비워두면 이 GameObject의 RectTransform 사용)")]
    private RectTransform sidebarRect;

    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Tooltip("토글 버튼 (선택)")]
    private Button toggleButton;

    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Range(20, 200)]
    [Tooltip("축소 상태 너비 (px)")]
    private float collapsedWidth = 60f;

    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Range(150, 800)]
    [Tooltip("확장 상태 너비 (px)")]
    private float expandedWidth = 340f;

    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("애니메이션 시간 (초). 0이면 즉시 전환")]
    private float animationDuration = 0.25f;

    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Tooltip("시작 시 확장 상태")]
    private bool startExpanded = false;

    [FoldoutGroup("UI 표시")]
    [SerializeField]
    [Tooltip("UI 투명도 제어")]
    private CanvasGroup canvasGroup;

    [FoldoutGroup("UI 표시")]
    [SerializeField]
    [Tooltip("시작 시 UI 표시")]
    private bool startVisible = true;

    private bool isExpanded;
    private Coroutine animCoroutine;

    public bool IsExpanded => isExpanded;

    private void Awake()
    {
        if (sidebarRect == null)
            sidebarRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        SetUIVisible(startVisible);

        isExpanded = startExpanded;
        SetWidthImmediate(isExpanded ? expandedWidth : collapsedWidth);

        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleExpanded);
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(ToggleExpanded);
    }

    /// <summary>
    /// 사이드바 확장/축소 토글
    /// </summary>
    [FoldoutGroup("토글 설정")]
    [Button("토글 테스트", ButtonSizes.Medium)]
    public void ToggleExpanded()
    {
        SetExpanded(!isExpanded);
    }

    public void SetExpanded(bool expanded)
    {
        isExpanded = expanded;
        float target = expanded ? expandedWidth : collapsedWidth;

        if (!Application.isPlaying || animationDuration <= 0f)
        {
            SetWidthImmediate(target);
            return;
        }

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateWidth(target));
    }

    private void SetWidthImmediate(float width)
    {
        if (sidebarRect == null) return;
        var size = sidebarRect.sizeDelta;
        size.x = width;
        sidebarRect.sizeDelta = size;
    }

    private IEnumerator AnimateWidth(float targetWidth)
    {
        float startWidth = sidebarRect.sizeDelta.x;
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);
            SetWidthImmediate(Mathf.Lerp(startWidth, targetWidth, t));
            yield return null;
        }
        SetWidthImmediate(targetWidth);
        animCoroutine = null;
    }

    /// <summary>
    /// UI 표시/숨기기 (alpha)
    /// </summary>
    public void SetUIVisible(bool visible)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }

    public void ToggleUI()
    {
        if (canvasGroup == null) return;
        SetUIVisible(canvasGroup.alpha < 0.5f);
    }
}
