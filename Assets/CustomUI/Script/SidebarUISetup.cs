using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 에디터 전용 헬퍼 - 인스펙터 버튼으로 사이드바 UI를 한 번에 구성해준다.
/// 필수 할당: SidebarManager, CustomUI, Panel/Item Prefab, PanelContainer.
/// </summary>
public class SidebarUISetup : MonoBehaviour
{
    [FoldoutGroup("컴포넌트 참조")]
    [SerializeField] private SidebarUIManager sidebarManager;

    [FoldoutGroup("컴포넌트 참조")]
    [SerializeField] private CustomUI customUI;

    [FoldoutGroup("프리팹")]
    [SerializeField] private GameObject panelPrefab;

    [FoldoutGroup("프리팹")]
    [SerializeField] private GameObject itemPrefab;

    [FoldoutGroup("레이아웃")]
    [SerializeField] private Transform panelContainer;

    [FoldoutGroup("레이아웃")]
    [SerializeField]
    [Range(0, 50)]
    private float panelSpacing = 10f;

    [FoldoutGroup("표시할 데이터")]
    [SerializeField] private bool showPerformanceData = true;

    [FoldoutGroup("표시할 데이터")]
    [SerializeField] private bool showBatteryData = true;

    [FoldoutGroup("표시할 데이터")]
    [SerializeField] private bool showFuelData = false;

    [FoldoutGroup("표시할 데이터")]
    [SerializeField] private bool showEnergyData = false;

    [FoldoutGroup("표시할 데이터")]
    [SerializeField] private bool showDrivingData = false;

#if UNITY_EDITOR
    [PropertySpace(15, 15)]
    [Title("UI 생성", "모든 것을 할당한 후 버튼을 클릭하세요", TitleAlignment = TitleAlignments.Centered)]

    [Button(ButtonSizes.Medium)]
    [GUIColor(0.3f, 0.8f, 0.3f)]
    public void UI자동생성()
    {
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

        sidebarManager.ClearAllPanels();

        sidebarManager.SetPanelPrefab(panelPrefab);
        sidebarManager.SetItemPrefab(itemPrefab);
        sidebarManager.SetPanelContainer(panelContainer);

        var perfProvider = GetOrAddComponent<CarPerformanceDataProvider>();
        var energyProvider = GetOrAddComponent<CarEnergyDataProvider>();
        var drivingProvider = GetOrAddComponent<CarDrivingDataProvider>();
        sidebarManager.RegisterDataProvider("Performance", perfProvider);
        sidebarManager.RegisterDataProvider("Energy", energyProvider);
        sidebarManager.RegisterDataProvider("Driving", drivingProvider);

        if (showPerformanceData)
            sidebarManager.CreatePanel("Performance", "Performance", perfProvider);
        if (showBatteryData)
            sidebarManager.CreatePanel("Battery", "Battery", energyProvider);
        if (showFuelData)
            sidebarManager.CreatePanel("Fuel", "Fuel", energyProvider);
        if (showEnergyData)
            sidebarManager.CreatePanel("Energy", "Energy", energyProvider);
        if (showDrivingData)
            sidebarManager.CreatePanel("Driving", "Driving", drivingProvider);

        EditorUtility.DisplayDialog("완료", "UI가 생성되었습니다!", "확인");
    }


    [Button(ButtonSizes.Medium)]
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
            component = gameObject.AddComponent<T>();
        return component;
    }
#endif
}
