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
