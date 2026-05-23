using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 전체 사이드바 UI를 관리합니다
/// 리그 오브 레전드 스타일의 사이드바로 여러 패널을 표시합니다
/// </summary>
public class SidebarUIManager : MonoBehaviour
{
    [FoldoutGroup("UI 요소")]
    [SerializeField]
    [Tooltip("자동차 이름을 표시할 TextMeshPro")]
    private TextMeshProUGUI carNameText;

    [FoldoutGroup("UI 요소")]
    [SerializeField]
    [Tooltip("모든 패널을 배치할 컨테이너")]
    private Transform panelContainer;

    [FoldoutGroup("프리팹")]
    [SerializeField]
    [Tooltip("DataAdjustmentPanel 프리팹")]
    private GameObject panelPrefab;

    [FoldoutGroup("프리팹")]
    [SerializeField]
    [Tooltip("DataAdjustmentUIItem 프리팹 (각 패널에 주입)")]
    private GameObject itemPrefab;

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
        RegisterCarManagerEvents();
    }

    /// <summary>
    /// CarManager 이벤트 등록
    /// </summary>
    private void RegisterCarManagerEvents()
    {
        // CarManager의 이벤트 구독
        if (CarManager.Instance != null)
        {
            CarManager.Instance.OnCarChanged += OnCarChanged;
            OnCarChanged(CarManager.Instance.CurrentCar);
        }
    }

    private void OnDestroy()
    {
        if (CarManager.Instance != null)
        {
            CarManager.Instance.OnCarChanged -= OnCarChanged;
        }
    }

    /// <summary>
    /// 자동차 변경 시 호출되는 콜백
    /// </summary>
    private void OnCarChanged(CarData newCar)
    {
        currentCarData = newCar;
        UpdateCarDisplay();

        foreach (var provider in dataProviders.Values)
            provider.SetCarData(newCar);

        // 각 패널을 최신 데이터로 재초기화
        foreach (var info in activePanels.Values)
        {
            var fields = info.provider.GetDataFieldsByCategory(info.categoryName);
            info.panel.Initialize(info.title, fields);
        }
    }

    /// <summary>
    /// 자동차 이름 업데이트
    /// </summary>
    private void UpdateCarDisplay()
    {
        if (currentCarData != null && carNameText != null)
        {
            carNameText.text = currentCarData.CarName;
        }
    }

    /// <summary>
    /// Panel 프리팹 설정
    /// </summary>
    public void SetPanelPrefab(GameObject panel)
    {
        panelPrefab = panel;
    }

    /// <summary>
    /// Item 프리팹 설정 (런타임에 각 패널에 주입됨)
    /// </summary>
    public void SetItemPrefab(GameObject item)
    {
        itemPrefab = item;
    }

    /// <summary>
    /// 패널 컨테이너 설정 (SidebarUISetup에서 호출)
    /// </summary>
    public void SetPanelContainer(Transform container)
    {
        panelContainer = container;
    }

    /// <summary>
    /// 데이터 제공자 등록
    /// </summary>
    public void RegisterDataProvider(string providerName, CarDataProviderBase provider)
    {
        if (!dataProviders.ContainsKey(providerName))
        {
            dataProviders[providerName] = provider;
            provider.SetCarData(currentCarData);
        }
    }

    /// <summary>
    /// 특정 카테고리의 패널 생성
    /// </summary>
    public void CreatePanel(string categoryName, string panelTitle, CarDataProviderBase provider)
    {
        if (provider == null || panelContainer == null || panelPrefab == null)
        {
            Debug.LogError("SidebarUIManager: Missing required components!");
            return;
        }

        // 이미 존재하는 패널은 건너뛰기
        if (activePanels.ContainsKey(categoryName))
            return;

        // 새 패널 생성
        GameObject panelObj = Instantiate(panelPrefab, panelContainer);
        DataAdjustmentPanel newPanel = panelObj.GetComponent<DataAdjustmentPanel>();
        if (newPanel == null)
        {
            Debug.LogError("Panel prefab에 DataAdjustmentPanel 컴포넌트가 없습니다!");
            SafeDestroy(panelObj);
            return;
        }
        panelObj.name = $"Panel_{categoryName}";

        // itemPrefab 주입 (런타임에 패널의 #if UNITY_EDITOR 자동검색이 작동 안 하므로 필수)
        if (itemPrefab != null)
            newPanel.SetItemPrefab(itemPrefab);

        // 데이터 필드 가져오기
        var dataFields = provider.GetDataFieldsByCategory(categoryName);
        newPanel.Initialize(panelTitle, dataFields);

        activePanels[categoryName] = new PanelInfo
        {
            panel = newPanel,
            title = panelTitle,
            categoryName = categoryName,
            provider = provider
        };

        // 🔴 패널 개수 업데이트 및 동적 간격 조정
        UpdateAllPanelDynamicSpacing();
    }

    /// <summary>
    /// 패널 표시/숨기기
    /// </summary>
    public void SetPanelActive(string categoryName, bool active)
    {
        if (activePanels.TryGetValue(categoryName, out var info))
        {
            info.panel.SetPanelActive(active);
        }
    }

    /// <summary>
    /// 모든 패널 제거
    /// (activePanels 추적 외에 panelContainer의 모든 자식도 정리 - 씬 재로드 후 잔여 패널 대응)
    /// </summary>
    public void ClearAllPanels()
    {
        foreach (var info in activePanels.Values)
        {
            SafeDestroy(info.panel.gameObject);
        }
        activePanels.Clear();

        // panelContainer에 남아있는 잔여 패널 자식 모두 제거
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

    /// <summary>
    /// 모든 활성 패널의 동적 간격 업데이트
    /// 화면에 모든 패널이 맞도록 자동으로 조정
    /// </summary>
    private void UpdateAllPanelDynamicSpacing()
    {
        int panelCount = activePanels.Count;
        foreach (var info in activePanels.Values)
        {
            info.panel.SetTotalPanelCount(panelCount);
        }
    }

    /// <summary>
    /// 에디터/플레이 모드 모두에서 안전하게 GameObject 제거
    /// </summary>
    private static void SafeDestroy(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }

    /// <summary>
    /// 현재 활성 패널 목록 반환
    /// </summary>
    public List<string> GetActivePanelNames()
    {
        return new List<string>(activePanels.Keys);
    }
}
