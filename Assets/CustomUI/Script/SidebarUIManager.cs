using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 사이드바 안에 카테고리별 DataAdjustmentPanel을 생성/제거하고,
/// CarDataManager의 차량 변경을 받아 모든 패널의 데이터를 갱신한다.
/// </summary>
public class SidebarUIManager : MonoBehaviour
{
    [FoldoutGroup("UI 요소")]
    [SerializeField] private TextMeshProUGUI carNameText;

    [FoldoutGroup("UI 요소")]
    [SerializeField] private Transform panelContainer;

    [FoldoutGroup("프리팹")]
    [SerializeField] private GameObject panelPrefab;

    [FoldoutGroup("프리팹")]
    [SerializeField] private GameObject itemPrefab;

    private class PanelInfo
    {
        public DataAdjustmentPanel panel;
        public string title;
        public string categoryName;
        public CarDataProviderBase provider;
    }

    private Dictionary<string, PanelInfo> activePanels = new Dictionary<string, PanelInfo>();
    private Dictionary<string, CarDataProviderBase> dataProviders = new Dictionary<string, CarDataProviderBase>();

    private CarData currentCarData;

    private void Start()
    {
        if (CarDataManager.Instance != null)
        {
            CarDataManager.Instance.OnCarChanged += OnCarChanged;
            OnCarChanged(CarDataManager.Instance.CurrentCar);
        }
    }

    private void OnDestroy()
    {
        if (CarDataManager.Instance != null)
            CarDataManager.Instance.OnCarChanged -= OnCarChanged;
    }

    private void OnCarChanged(CarData newCar)
    {
        currentCarData = newCar;
        UpdateCarDisplay();

        foreach (var provider in dataProviders.Values)
            provider.SetCarData(newCar);

        foreach (var info in activePanels.Values)
        {
            var fields = info.provider.GetDataFieldsByCategory(info.categoryName);
            info.panel.Initialize(info.title, fields);
        }
    }

    private void UpdateCarDisplay()
    {
        if (currentCarData != null && carNameText != null)
            carNameText.text = currentCarData.CarName;
    }

    public void SetPanelPrefab(GameObject panel) => panelPrefab = panel;
    public void SetItemPrefab(GameObject item) => itemPrefab = item;
    public void SetPanelContainer(Transform container) => panelContainer = container;

    public void RegisterDataProvider(string providerName, CarDataProviderBase provider)
    {
        if (!dataProviders.ContainsKey(providerName))
        {
            dataProviders[providerName] = provider;
            provider.SetCarData(currentCarData);
        }
    }

    public void CreatePanel(string categoryName, string panelTitle, CarDataProviderBase provider)
    {
        if (provider == null || panelContainer == null || panelPrefab == null)
        {
            Debug.LogError("SidebarUIManager: Missing required components!");
            return;
        }

        if (activePanels.ContainsKey(categoryName))
            return;

        GameObject panelObj = Instantiate(panelPrefab, panelContainer);
        DataAdjustmentPanel newPanel = panelObj.GetComponent<DataAdjustmentPanel>();
        if (newPanel == null)
        {
            Debug.LogError("Panel prefab에 DataAdjustmentPanel 컴포넌트가 없습니다!");
            SafeDestroy(panelObj);
            return;
        }
        panelObj.name = $"Panel_{categoryName}";

        // 런타임에는 패널의 #if UNITY_EDITOR 자동검색이 작동 안 하므로 itemPrefab을 명시적으로 주입
        if (itemPrefab != null)
            newPanel.SetItemPrefab(itemPrefab);

        var dataFields = provider.GetDataFieldsByCategory(categoryName);
        newPanel.Initialize(panelTitle, dataFields);

        activePanels[categoryName] = new PanelInfo
        {
            panel = newPanel,
            title = panelTitle,
            categoryName = categoryName,
            provider = provider
        };
    }

    // activePanels 외에 panelContainer의 잔여 패널까지 청소 (씬 재로드 후 남는 경우 방어)
    public void ClearAllPanels()
    {
        foreach (var info in activePanels.Values)
            SafeDestroy(info.panel.gameObject);
        activePanels.Clear();

        if (panelContainer != null)
        {
            for (int i = panelContainer.childCount - 1; i >= 0; i--)
            {
                var child = panelContainer.GetChild(i);
                if (child.name.StartsWith("Panel_"))
                    SafeDestroy(child.gameObject);
            }
        }
    }

    private static void SafeDestroy(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }
}
