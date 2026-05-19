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

        if (panelContainer == null)
        {
            EditorUtility.DisplayDialog("오류", "Panel Container를 할당하세요!", "확인");
            return;
        }

        // 기존 패널 제거
        sidebarManager.ClearAllPanels();

        // 프리팹 및 컨테이너 할당
        sidebarManager.SetPanelPrefab(panelPrefab);
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
