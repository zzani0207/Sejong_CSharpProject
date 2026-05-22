using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 자동차의 여러 데이터 필드를 조정할 수 있는 패널
/// 리그 오브 레전드 사이드바 UI처럼 수직 레이아웃으로 표시됩니다
/// </summary>
public class DataAdjustmentPanel : MonoBehaviour
{
    [FoldoutGroup("UI 요소")]
    [SerializeField]
    [Tooltip("패널 제목을 표시할 TextMeshPro")]
    private TextMeshProUGUI panelTitleText;

    [FoldoutGroup("UI 요소")]
    [SerializeField]
    [Tooltip("UI 항목들을 배치할 부모 객체")]
    private Transform itemContainer;

    [FoldoutGroup("UI 요소")]
    [SerializeField]
    [Tooltip("아이템 프리팹")]
    private GameObject itemPrefab;

    [FoldoutGroup("UI 요소")]
    [SerializeField]
    [Tooltip("아이템 컨테이너의 레이아웃 그룹")]
    private LayoutGroup containerLayout;

    private List<DataAdjustmentUIItem> uiItems = new List<DataAdjustmentUIItem>();
    private List<IDataAdjustable> dataFields = new List<IDataAdjustable>();

    // 동적 간격 조정을 위한 변수
    private int totalPanelCount = 1;
    private RectTransform panelRect;
    private VerticalLayoutGroup panelVlg;

    private void OnEnable()
    {
        // itemPrefab 자동 할당 시도
        if (itemPrefab == null)
        {
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("DataAdjustmentUIItem t:GameObject");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                itemPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            #endif
        }

        EnsureLayoutComponents();
    }

    /// <summary>
    /// Panel 본체와 ItemContainer에 필요한 레이아웃 컴포넌트 자동 보강
    /// (슬라이더가 안 보이는 가장 흔한 원인 - LayoutGroup/ContentSizeFitter 누락)
    /// </summary>
    private void EnsureLayoutComponents()
    {
        // Panel 본체: VerticalLayoutGroup + ContentSizeFitter (Title + ItemContainer 세로 정렬)
        panelVlg = GetComponent<VerticalLayoutGroup>();
        if (panelVlg == null) panelVlg = gameObject.AddComponent<VerticalLayoutGroup>();
        panelVlg.childForceExpandWidth = true;
        panelVlg.childForceExpandHeight = false;
        panelVlg.childControlWidth = true;
        panelVlg.childControlHeight = true;
        panelVlg.spacing = 4;
        if (panelVlg.padding.left == 0 && panelVlg.padding.right == 0)
            panelVlg.padding = new RectOffset(10, 10, 8, 8);

        var panelCsf = GetComponent<ContentSizeFitter>();
        if (panelCsf == null) panelCsf = gameObject.AddComponent<ContentSizeFitter>();
        panelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        panelCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Panel RectTransform 캐시
        panelRect = GetComponent<RectTransform>();

        // ItemContainer: VerticalLayoutGroup + ContentSizeFitter (아이템들 세로 정렬)
        if (itemContainer == null) return;
        var ig = itemContainer.gameObject;
        var itemVlg = ig.GetComponent<VerticalLayoutGroup>();
        if (itemVlg == null) itemVlg = ig.AddComponent<VerticalLayoutGroup>();
        itemVlg.childForceExpandWidth = true;
        itemVlg.childForceExpandHeight = false;
        itemVlg.childControlWidth = true;
        itemVlg.childControlHeight = false;
        itemVlg.spacing = 4;
        containerLayout = itemVlg;

        var itemCsf = ig.GetComponent<ContentSizeFitter>();
        if (itemCsf == null) itemCsf = ig.AddComponent<ContentSizeFitter>();
        itemCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        itemCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    /// <summary>
    /// 외부(SidebarUIManager)에서 itemPrefab 주입
    /// </summary>
    public void SetItemPrefab(GameObject prefab)
    {
        itemPrefab = prefab;
    }

    /// <summary>
    /// 패널 초기화 및 데이터 필드 설정
    /// </summary>
    public void Initialize(string title, List<IDataAdjustable> fields)
    {
        if (panelTitleText != null)
        {
            panelTitleText.text = title;
        }

        dataFields.Clear();
        dataFields.AddRange(fields);

        CreateUIItems();
    }

    /// <summary>
    /// UI 아이템 생성
    /// </summary>
    private void CreateUIItems()
    {
        // 기존 아이템 제거
        foreach (var item in uiItems)
        {
            SafeDestroy(item.gameObject);
        }
        uiItems.Clear();

        // 새로운 아이템 생성
        if (itemPrefab == null || itemContainer == null)
        {
            Debug.LogError("DataAdjustmentPanel: itemPrefab or itemContainer is not assigned!");
            return;
        }

        foreach (var field in dataFields)
        {
            if (!field.IsActive()) continue;

            GameObject itemObj = Instantiate(itemPrefab, itemContainer);
            DataAdjustmentUIItem item = itemObj.GetComponent<DataAdjustmentUIItem>();
            if (item != null)
            {
                item.SetDataField(field);
                uiItems.Add(item);
            }
            else
            {
                Debug.LogError("DataAdjustmentPanel: itemPrefab has no DataAdjustmentUIItem component!");
                SafeDestroy(itemObj);
            }
        }

        // 레이아웃 강제 갱신
        if (containerLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerLayout.GetComponent<RectTransform>());
        }
    }

    /// <summary>
    /// 패널 활성화/비활성화
    /// </summary>
    public void SetPanelActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// 현재 보유한 데이터 필드 개수
    /// </summary>
    public int GetFieldCount() => dataFields.Count;

    /// <summary>
    /// 데이터 필드 추가
    /// </summary>
    public void AddDataField(IDataAdjustable field)
    {
        if (!dataFields.Contains(field))
        {
            dataFields.Add(field);
        }
    }

    /// <summary>
    /// 모든 아이템 새로고침
    /// </summary>
    public void RefreshAllItems()
    {
        CreateUIItems();
    }

    /// <summary>
    /// 전체 패널 개수 설정 및 간격 동적 조정
    /// (SidebarUIManager에서 호출)
    /// </summary>
    public void SetTotalPanelCount(int count)
    {
        totalPanelCount = Mathf.Max(1, count);
        UpdateDynamicSpacing();
    }

    /// <summary>
    /// 화면 높이에 따라 패널 간격 동적 조정
    /// 모든 패널이 화면에 보이도록 spacing을 계산
    /// </summary>
    private void UpdateDynamicSpacing()
    {
        if (panelVlg == null || panelRect == null)
            return;

        // Canvas 높이 (또는 부모 RectTransform 높이)
        Canvas canvas = GetComponentInParent<Canvas>();
        float availableHeight = canvas != null ? canvas.GetComponent<RectTransform>().rect.height : 1080f;

        // 패널 하나당 평균 사용 가능한 높이
        float heightPerPanel = availableHeight / totalPanelCount;

        // 패널의 타이틀, 아이템들의 높이 예상값
        // 타이틀: ~40px, 패딩: ~16px, 아이템 개수에 따라 가변
        float reservedHeight = 40f + 16f;  // 타이틀 + 패딩
        float estimatedItemHeight = uiItems.Count * 36f; // 아이템당 약 36px
        float totalEstimatedHeight = reservedHeight + estimatedItemHeight;

        // 이 패널이 차지하면 안 될 높이 여유 (다른 패널들을 위해)
        float maxHeightForThisPanel = heightPerPanel * 0.9f; // 90% 사용

        // 간격 계산: 아이템들 사이의 공간을 줄여서 패널을 압축
        int itemCount = uiItems.Count;
        float newSpacing = 4f; // 기본값

        if (itemCount > 1 && totalEstimatedHeight > maxHeightForThisPanel)
        {
            // 과도하게 커지면 간격을 줄임
            float reduction = (totalEstimatedHeight - maxHeightForThisPanel) / itemCount;
            newSpacing = Mathf.Max(0f, 4f - reduction);
        }

        panelVlg.spacing = newSpacing;

        // 레이아웃 갱신
        if (panelRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
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
}

