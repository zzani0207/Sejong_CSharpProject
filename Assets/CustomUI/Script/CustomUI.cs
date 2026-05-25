using UnityEngine;
using Sirenix.OdinInspector;
using PrimeTween;

/// <summary>
/// 사이드바 가로 너비를 트윈으로 토글(축소↔확장)하고 컨텐츠를 페이드시킨다.
/// 토글은 키보드 입력으로 동작.
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
    [Tooltip("애니메이션 이징 (PrimeTween 내장 - InOutCubic, OutBack, OutElastic 등)")]
    private Ease ease = Ease.OutCubic;

    [FoldoutGroup("토글 설정")]
    [SerializeField] private bool startExpanded = false;

    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Tooltip("축소 상태에서 숨길 컨텐츠 (보통 PanelContainer)")]
    private GameObject contentToHideOnCollapse;

    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Tooltip("이 키를 누르면 토글된다 (기본: 스페이스바)")]
    private KeyCode toggleKey = KeyCode.Space;

    [FoldoutGroup("UI 표시")]
    [SerializeField] private CanvasGroup canvasGroup;

    [FoldoutGroup("UI 표시")]
    [SerializeField] private bool startVisible = true;

    private bool isExpanded;
    private CanvasGroup contentCanvasGroup;

    private void Awake()
    {
        if (sidebarRect == null)
            sidebarRect = GetComponent<RectTransform>();

        if (contentToHideOnCollapse != null)
        {
            contentCanvasGroup = contentToHideOnCollapse.GetComponent<CanvasGroup>();
            if (contentCanvasGroup == null)
                contentCanvasGroup = contentToHideOnCollapse.AddComponent<CanvasGroup>();
        }
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
        if (contentToHideOnCollapse != null)
        {
            contentToHideOnCollapse.SetActive(isExpanded);
            if (contentCanvasGroup != null) contentCanvasGroup.alpha = isExpanded ? 1f : 0f;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleExpanded();
    }

    [FoldoutGroup("토글 설정")]
    [Button("토글 테스트", ButtonSizes.Medium)]
    public void ToggleExpanded() => SetExpanded(!isExpanded);

    public void SetExpanded(bool expanded)
    {
        isExpanded = expanded;
        float target = expanded ? expandedWidth : collapsedWidth;

        // 에디트 모드 또는 즉시 전환
        if (!Application.isPlaying || animationDuration <= 0f)
        {
            SetWidthImmediate(target);
            if (contentToHideOnCollapse != null)
            {
                contentToHideOnCollapse.SetActive(expanded);
                if (contentCanvasGroup != null) contentCanvasGroup.alpha = expanded ? 1f : 0f;
            }
            return;
        }

        Tween.StopAll(onTarget: sidebarRect);
        if (contentCanvasGroup != null) Tween.StopAll(onTarget: contentCanvasGroup);

        // 페이드 인 준비를 위해 확장 시작 시점에 컨텐츠를 미리 활성화
        if (expanded && contentToHideOnCollapse != null)
            contentToHideOnCollapse.SetActive(true);

        float startWidth = sidebarRect.sizeDelta.x;
        Tween.Custom(startWidth, target, animationDuration, val =>
        {
            var size = sidebarRect.sizeDelta;
            size.x = val;
            sidebarRect.sizeDelta = size;
        }, ease);

        if (contentCanvasGroup != null)
        {
            float endAlpha = expanded ? 1f : 0f;
            Tween.Alpha(contentCanvasGroup, endAlpha, animationDuration, ease)
                .OnComplete(() =>
                {
                    if (!expanded && contentToHideOnCollapse != null)
                        contentToHideOnCollapse.SetActive(false);
                });
        }
    }

    private void SetWidthImmediate(float width)
    {
        if (sidebarRect == null) return;
        var size = sidebarRect.sizeDelta;
        size.x = width;
        sidebarRect.sizeDelta = size;
    }

    private void SetUIVisible(bool visible)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }
}
