using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Sidebar UI를 설정하고 자동으로 생성하는 헬퍼
/// 
/// 설정 방법:
/// 1. Canvas > SidebarUI (GameObject 생성)
/// 2. SidebarUI에 CustomUI, SidebarUIManager, SidebarUISetup 추가
/// 3. SidebarUI > PanelContainer (GameObject 생성)
/// 4. SidebarUISetup 인스펙터에서 필요한 것들 할당:
///    - Sidebar Manager (이 GameObject의 SidebarUIManager)
///    - Custom UI (이 GameObject의 CustomUI)
///    - Panel Prefab (프리팹)
///    - Item Prefab (프리팹)
///    - Panel Container (자식 GameObject)
/// 5. 버튼 클릭
/// </summary>
public class SidebarUISetup : MonoBehaviour
{
    [FoldoutGroup("컴포넌트 참조")]
    [SerializeField]
    [Tooltip("SidebarUIManager 컴포넌트를 할당하세요")]
    private SidebarUIManager sidebarManager;

    [FoldoutGroup("컴포넌트 참조")]
    [SerializeField]
    [Tooltip("CustomUI 컴포넌트를 할당하세요")]
    private CustomUI customUI;

    [FoldoutGroup("프리팹")]
    [SerializeField]
    [Tooltip("DataAdjustmentPanel 프리팹을 할당하세요")]
    private GameObject panelPrefab;

    [FoldoutGroup("프리팹")]
    [SerializeField]
    [Tooltip("DataAdjustmentUIItem 프리팹을 할당하세요")]
    private GameObject itemPrefab;

    [FoldoutGroup("레이아웃")]
    [SerializeField]
    [Tooltip("패널들을 배치할 컨테이너 (PanelContainer)")]
    private Transform panelContainer;

    [FoldoutGroup("레이아웃")]
    [SerializeField]
    [Range(0, 50)]
    [Tooltip("패널 간 간격")]
    private float panelSpacing = 10f;

    [FoldoutGroup("표시할 데이터")]
    [SerializeField]
    [Tooltip("성능 정보 표시")]
    private bool showPerformanceData = true;

    [FoldoutGroup("표시할 데이터")]
    [SerializeField]
    [Tooltip("배터리 정보 표시")]
    private bool showBatteryData = true;

    [FoldoutGroup("표시할 데이터")]
    [SerializeField]
    [Tooltip("연료 정보 표시")]
    private bool showFuelData = false;

    [FoldoutGroup("표시할 데이터")]
    [SerializeField]
    [Tooltip("에너지 소비 정보 표시")]
    private bool showEnergyData = false;

#if UNITY_EDITOR
    [PropertySpace(15, 15)]
    [Title("🎯 UI 생성", "모든 것을 할당한 후 버튼을 클릭하세요", TitleAlignment = TitleAlignments.Centered)]

    [Button(ButtonSizes.Large, Icon = SdfIconType.CheckCircle)]
    [GUIColor(0.3f, 0.8f, 0.3f)]
    public void UI자동생성()
    {
        // 필수 요소 확인 (Inspector에서 할당되어야 함)
        if (sidebarManager == null)
        {
            EditorUtility.DisplayDialog("오류", "Sidebar Manager를 할당하세요!", "확인");
            return;
        }

        if (customUI == null)
        {
            EditorUtility.DisplayDialog("오류", "Custom UI를 할당하세요!", "확인");
            return;
        }

        if (panelPrefab == null)
        {
            EditorUtility.DisplayDialog("오류", "Panel Prefab을 할당하세요!", "확인");
            return;
        }

        if (itemPrefab == null)
        {
            EditorUtility.DisplayDialog("오류", "Item Prefab을 할당하세요!", "확인");
            return;
        }

        if (panelContainer == null)
        {
            EditorUtility.DisplayDialog("오류", "Panel Container를 할당하세요!", "확인");
            return;
        }

        // 기존 패널 제거
        sidebarManager.ClearAllPanels();

        // 프리팹 및 컨테이너 할당
        sidebarManager.SetPanelPrefab(panelPrefab);
        sidebarManager.SetItemPrefab(itemPrefab);
        sidebarManager.SetPanelContainer(panelContainer);

        // PanelContainer 레이아웃 자동 설정
        ConfigurePanelContainerLayout();

        // 데이터 제공자 생성 및 등록
        var perfProvider = GetOrAddComponent<CarPerformanceDataProvider>();
        var energyProvider = GetOrAddComponent<CarEnergyDataProvider>();
        sidebarManager.RegisterDataProvider("Performance", perfProvider);
        sidebarManager.RegisterDataProvider("Energy", energyProvider);

        // 패널 생성
        if (showPerformanceData)
        {
            sidebarManager.CreatePanel("Performance", "Performance", perfProvider);
        }

        if (showBatteryData)
        {
            sidebarManager.CreatePanel("Battery", "Battery", energyProvider);
        }

        if (showFuelData)
        {
            sidebarManager.CreatePanel("Fuel", "Fuel", energyProvider);
        }

        if (showEnergyData)
        {
            sidebarManager.CreatePanel("Energy", "Energy", energyProvider);
        }

        EditorUtility.DisplayDialog("완료", "UI가 생성되었습니다!\n\nPlay 모드에서 테스트하세요.", "확인");
    }

    /// <summary>
    /// PanelContainer에 VerticalLayoutGroup + ContentSizeFitter 자동 세팅
    /// </summary>
    private void ConfigurePanelContainerLayout()
    {
        var vlg = panelContainer.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = panelContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = panelSpacing;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var csf = panelContainer.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = panelContainer.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    /// <summary>
    /// ScrollView 자동 생성 - PanelContainer를 Scroll 가능하게 만들기
    /// </summary>
    [Button(ButtonSizes.Medium, Icon = SdfIconType.Arrows)]
    [GUIColor(0.3f, 0.6f, 0.8f)]
    public void ScrollView생성()
    {
        if (panelContainer == null)
        {
            EditorUtility.DisplayDialog("오류", "Panel Container를 할당하세요!", "확인");
            return;
        }

        // 이미 ScrollView가 있으면 스킵
        ScrollRect existingScroll = panelContainer.GetComponentInParent<ScrollRect>();
        if (existingScroll != null)
        {
            EditorUtility.DisplayDialog("알림", "이미 ScrollView가 설정되어 있습니다!", "확인");
            return;
        }

        // PanelContainer의 부모를 ScrollView로 변경
        Transform panelContainerParent = panelContainer.parent;
        
        // ScrollView 루트 생성
        GameObject scrollViewRoot = new GameObject("ScrollView");
        scrollViewRoot.transform.SetParent(panelContainerParent, false);
        scrollViewRoot.transform.SetAsFirstSibling(); // 제일 앞에 배치

        RectTransform scrollViewRect = scrollViewRoot.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = Vector2.zero;
        scrollViewRect.anchorMax = Vector2.one;
        scrollViewRect.offsetMin = Vector2.zero;
        scrollViewRect.offsetMax = Vector2.zero;

        // ScrollView 컴포넌트
        ScrollRect scrollRect = scrollViewRoot.AddComponent<ScrollRect>();
        Image scrollBg = scrollViewRoot.AddComponent<Image>();
        scrollBg.color = new Color(1, 1, 1, 0); // 투명
        LayoutElement scrollLayout = scrollViewRoot.AddComponent<LayoutElement>();

        // Viewport 생성
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewRoot.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0);
        
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // PanelContainer를 Viewport 아래로 이동 (Content로 변경)
        panelContainer.SetParent(viewportObj.transform, false);
        RectTransform panelRect = panelContainer.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // ScrollRect 설정
        scrollRect.content = panelRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;

        EditorUtility.DisplayDialog("완료", "ScrollView가 생성되었습니다!\n\n" +
            "- PanelContainer가 자동으로 Content로 변환되었습니다\n" +
            "- 스크롤이 필요하면 세로로 스크롤됩니다", "확인");
    }

    [Button(ButtonSizes.Medium, Icon = SdfIconType.Trash)]
    [GUIColor(0.8f, 0.3f, 0.3f)]
    public void UI제거()
    {
        if (sidebarManager != null)
        {
            sidebarManager.ClearAllPanels();
            EditorUtility.DisplayDialog("완료", "UI가 제거되었습니다!", "확인");
        }
    }

    /// <summary>
    /// 토글 버튼을 Canvas 루트 레벨로 이동 (독립적인 위치 제어를 위해)
    /// </summary>
    [Button(ButtonSizes.Medium, Icon = SdfIconType.ArrowsMove)]
    [GUIColor(0.6f, 0.8f, 0.3f)]
    public void 토글버튼Canvas레벨조정()
    {
        if (customUI == null)
        {
            EditorUtility.DisplayDialog("오류", "Custom UI를 할당하세요!", "확인");
            return;
        }

        // CustomUI에서 ToggleButton 찾기
        Button toggleBtn = customUI.GetComponentInChildren<Button>();
        if (toggleBtn == null)
        {
            EditorUtility.DisplayDialog("오류", "CustomUI 하위에서 Button을 찾을 수 없습니다!", "확인");
            return;
        }

        // Canvas 루트 찾기
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("오류", "Canvas를 찾을 수 없습니다!", "확인");
            return;
        }

        // 토글 버튼의 world position 저장 (이동 전)
        RectTransform toggleRect = toggleBtn.GetComponent<RectTransform>();
        Vector3 worldPos = toggleRect.position;

        // Canvas 루트로 이동
        toggleRect.SetParent(canvas.transform, false);

        // world position 복원
        toggleRect.position = worldPos;

        // Anchor 설정 (Canvas 기준으로 독립적)
        toggleRect.anchorMin = new Vector2(0, 1);
        toggleRect.anchorMax = new Vector2(0, 1);
        toggleRect.pivot = new Vector2(0, 1);

        EditorUtility.DisplayDialog("완료", "토글 버튼이 Canvas 루트로 이동했습니다!\n" +
            "이제 사이드바와 독립적으로 움직입니다.", "확인");
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        var component = GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }
#endif
}
