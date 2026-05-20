using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using PrimeTween;

/// <summary>
/// 사이드바 UI 전반 제어
/// - 가로 너비 토글 (얇음 ↔ 펼침) + 컨텐츠 fade in/out
/// - PrimeTween + AnimationCurve(베지어 편집 가능)
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
    [Tooltip("애니메이션 이징 (PrimeTween 내장 - InOutCubic, OutBack, OutElastic 등)")]
    private Ease ease = Ease.OutCubic;

    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Tooltip("시작 시 확장 상태")]
    private bool startExpanded = false;

    [FoldoutGroup("토글 설정")]
    [SerializeField]
    [Tooltip("축소 상태에서 숨길 컨텐츠 (보통 PanelContainer)")]
    private GameObject contentToHideOnCollapse;

    [FoldoutGroup("UI 표시")]
    [SerializeField]
    [Tooltip("UI 투명도 제어")]
    private CanvasGroup canvasGroup;

    [FoldoutGroup("UI 표시")]
    [SerializeField]
    [Tooltip("시작 시 UI 표시")]
    private bool startVisible = true;

    private bool isExpanded;
    private CanvasGroup contentCanvasGroup;

    public bool IsExpanded => isExpanded;

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

        // 토글 버튼이 사이드바 폭에 stretched되지 않도록 anchor 고정
        NormalizeToggleButtonAnchor();
    }

    /// <summary>
    /// 토글 버튼이 사이드바와 함께 늘어나지 않도록 anchor를 고정으로 정규화
    /// </summary>
    private void NormalizeToggleButtonAnchor()
    {
        if (toggleButton == null) return;
        var rt = toggleButton.transform as RectTransform;
        if (rt == null) return;

        // anchor가 가로로 stretched 상태면 현재 화면상 크기를 보존하면서 좌상단 고정으로 변경
        bool stretchedX = Mathf.Abs(rt.anchorMax.x - rt.anchorMin.x) > 0.01f;
        if (stretchedX)
        {
            Vector2 size = rt.rect.size;
            Vector3 worldPos = rt.position;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = size;
            rt.position = worldPos;
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

        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleExpanded);
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(ToggleExpanded);
    }

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

        // 에디터/즉시 전환
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

        // 이전 트윈 중단
        Tween.StopAll(onTarget: sidebarRect);
        if (contentCanvasGroup != null) Tween.StopAll(onTarget: contentCanvasGroup);

        // 확장 시작: 컨텐츠 즉시 활성화 (페이드 인 준비)
        if (expanded && contentToHideOnCollapse != null)
        {
            contentToHideOnCollapse.SetActive(true);
        }

        // 폭 트윈 - PrimeTween Ease 사용
        float startWidth = sidebarRect.sizeDelta.x;
        Tween.Custom(startWidth, target, animationDuration, val =>
        {
            var size = sidebarRect.sizeDelta;
            size.x = val;
            sidebarRect.sizeDelta = size;
        }, ease);

        // 컨텐츠 알파 페이드
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
