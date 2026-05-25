using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 한 카테고리의 데이터 필드들을 슬라이더 리스트로 보여주는 패널.
/// 화면 높이에 따라 패널 간격을 동적으로 줄여 모든 패널이 보이도록 한다.
/// </summary>
public class DataAdjustmentPanel : MonoBehaviour
{
    [FoldoutGroup("UI 요소")]
    [SerializeField] private TextMeshProUGUI panelTitleText;

    [FoldoutGroup("UI 요소")]
    [SerializeField] private Transform itemContainer;

    [FoldoutGroup("UI 요소")]
    [SerializeField] private GameObject itemPrefab;

    [FoldoutGroup("UI 요소")]
    [SerializeField] private LayoutGroup containerLayout;

    private List<DataAdjustmentUIItem> uiItems = new List<DataAdjustmentUIItem>();
    private List<IDataAdjustable> dataFields = new List<IDataAdjustable>();

    private void OnEnable()
    {
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

    // 슬라이더가 안 보이는 가장 흔한 원인이 LayoutGroup/ContentSizeFitter 누락이라 자동 보강한다
    private void EnsureLayoutComponents()
    {
        var panelVlg = GetComponent<VerticalLayoutGroup>();
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

    public void SetItemPrefab(GameObject prefab)
    {
        itemPrefab = prefab;
    }

    public void Initialize(string title, List<IDataAdjustable> fields)
    {
        if (panelTitleText != null)
            panelTitleText.text = title;

        dataFields.Clear();
        dataFields.AddRange(fields);

        CreateUIItems();
    }

    private void CreateUIItems()
    {
        foreach (var item in uiItems)
            SafeDestroy(item.gameObject);
        uiItems.Clear();

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

        if (containerLayout != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerLayout.GetComponent<RectTransform>());
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
