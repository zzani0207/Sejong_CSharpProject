using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 프리팹 생성을 위한 유틸리티 클래스
/// 에디터에서만 실행되며, 필요한 프리팹들을 자동으로 생성합니다
/// </summary>
#if UNITY_EDITOR
public class UIComponentCreator
{
    /// <summary>
    /// DataAdjustmentUIItem 프리팹 생성
    /// </summary>
    [UnityEditor.MenuItem("Tools/Custom UI/Create Item Prefab")]
    public static void CreateItemPrefab()
    {
        // 루트 패널
        GameObject itemRoot = new GameObject("DataAdjustmentUIItem");
        RectTransform itemRect = itemRoot.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(300, 60);

        // 레이아웃 그룹 설정
        HorizontalLayoutGroup itemLayout = itemRoot.AddComponent<HorizontalLayoutGroup>();
        itemLayout.childForceExpandHeight = true;
        itemLayout.childForceExpandWidth = false;
        itemLayout.spacing = 10;
        itemLayout.padding = new RectOffset(5, 5, 5, 5);

        // 라벨 텍스트
        GameObject labelObj = new GameObject("LabelText");
        labelObj.transform.SetParent(itemRoot.transform);
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = "Label";
        labelText.fontSize = 3;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(100, 60);
        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 100;

        // 슬라이더
        GameObject sliderObj = new GameObject("ValueSlider");
        sliderObj.transform.SetParent(itemRoot.transform);
        Image sliderBg = sliderObj.AddComponent<Image>();
        sliderBg.color = new Color(0.2f, 0.2f, 0.2f, 1);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(150, 20);

        // 슬라이더 Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0, 0.5f, 1, 1);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // 슬라이더 컴포넌트 설정
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.fillRect = fillRect;

        // 값 텍스트
        GameObject valueObj = new GameObject("ValueText");
        valueObj.transform.SetParent(itemRoot.transform);
        TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
        valueText.text = "100.0";
        valueText.fontSize = 3;
        RectTransform valueRect = valueObj.GetComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(60, 60);
        LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 60;

        // DataAdjustmentUIItem 스크립트 추가
        DataAdjustmentUIItem itemScript = itemRoot.AddComponent<DataAdjustmentUIItem>();
        itemScript.GetType().GetField("labelText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(itemScript, labelText);
        itemScript.GetType().GetField("valueSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(itemScript, slider);
        itemScript.GetType().GetField("valueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(itemScript, valueText);

        Debug.Log("DataAdjustmentUIItem 프리팹이 생성되었습니다. 저장해주세요.");
    }

    /// <summary>
    /// DataAdjustmentPanel 프리팹 생성
    /// </summary>
    [UnityEditor.MenuItem("Tools/Custom UI/Create Panel Prefab")]
    public static void CreatePanelPrefab()
    {
        // 루트 패널
        GameObject panelRoot = new GameObject("DataAdjustmentPanel");
        Image panelImage = panelRoot.AddComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(350, 200);

        // 레이아웃 그룹
        VerticalLayoutGroup panelLayout = panelRoot.AddComponent<VerticalLayoutGroup>();
        panelLayout.childForceExpandHeight = false;
        panelLayout.childForceExpandWidth = true;
        panelLayout.spacing = 5;
        panelLayout.padding = new RectOffset(10, 10, 10, 10);

        // 제목 텍스트
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelRoot.transform);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Panel Title";
        titleText.fontSize = 4;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(300, 40);
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 40;

        // 아이템 컨테이너
        GameObject containerObj = new GameObject("ItemContainer");
        containerObj.transform.SetParent(panelRoot.transform);
        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(300, 150);
        VerticalLayoutGroup containerLayout = containerObj.AddComponent<VerticalLayoutGroup>();
        containerLayout.childForceExpandHeight = false;
        containerLayout.childForceExpandWidth = true;
        containerLayout.spacing = 5;
        LayoutElement containerLayoutElement = containerObj.AddComponent<LayoutElement>();
        containerLayoutElement.preferredHeight = 150;
        containerLayoutElement.flexibleHeight = 1;

        // DataAdjustmentPanel 스크립트 추가
        DataAdjustmentPanel panelScript = panelRoot.AddComponent<DataAdjustmentPanel>();
        panelScript.GetType().GetField("panelTitleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(panelScript, titleText);
        panelScript.GetType().GetField("itemContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(panelScript, containerRect);
        panelScript.GetType().GetField("containerLayout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(panelScript, containerLayout);
        
        // itemPrefab 자동 찾기 및 할당
        string[] itemPrefabGuids = UnityEditor.AssetDatabase.FindAssets("DataAdjustmentUIItem t:GameObject");
        if (itemPrefabGuids.Length > 0)
        {
            string itemPath = UnityEditor.AssetDatabase.GUIDToAssetPath(itemPrefabGuids[0]);
            GameObject itemPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(itemPath);
            if (itemPrefab != null)
            {
                panelScript.GetType().GetField("itemPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(panelScript, itemPrefab);
                Debug.Log($"ItemPrefab이 자동으로 할당되었습니다: {itemPath}");
            }
        }

        Debug.Log("DataAdjustmentPanel 프리팹이 생성되었습니다. 저장해주세요.");
    }
}
#endif
