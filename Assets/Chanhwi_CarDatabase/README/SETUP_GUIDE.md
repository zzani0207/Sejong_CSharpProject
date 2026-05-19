# 🎯 SidebarUI 세팅 완벽 가이드

## 📋 각 스크립트의 역할 및 할당 내용

### 1️⃣ **CarManager** (데이터 소스 - 반드시 씬에 존재해야 함)

**위치**: Hierarchy 최상위 (Canvas 밖)

> ⚠️ **이것이 없으면 슬라이더에 데이터가 표시되지 않습니다.**

**설정 방법**:
```
1. Hierarchy → Create Empty → 이름을 "CarManager"로 변경
2. Add Component → CarManager 스크립트 추가
3. Inspector에서 Car Database 어셋 할당
```

---

### 2️⃣ **SidebarUI GameObject 컴포넌트 추가**

**위치**: Canvas > SidebarUI

SidebarUI 오브젝트에 아래 **3개 스크립트를 모두 Add Component** 해야 합니다:

```
SidebarUI (GameObject)
  ├─ SidebarUIManager  ← Add Component
  ├─ CustomUI          ← Add Component
  └─ SidebarUISetup    ← Add Component
```

> ⚠️ 셋 중 하나라도 없으면 "Sidebar Manager를 할당하세요!" 오류가 납니다.

---

### 3️⃣ **SidebarUISetup 필드 할당**

SidebarUI 선택 → Inspector의 **SidebarUISetup** 섹션에서 할당:

| 필드 | 드래그할 것 | 찾는 위치 |
|------|-------------|-----------|
| Sidebar Manager | `SidebarUIManager` 컴포넌트 | 같은 SidebarUI 오브젝트 |
| Custom UI | `CustomUI` 컴포넌트 | 같은 SidebarUI 오브젝트 |
| Panel Prefab | `DataAdjustmentPanel` | Prefab 폴더 |
| Panel Container | `PanelContainer` | SidebarUI 자식 오브젝트 |

**같은 GameObject의 컴포넌트를 드래그하는 방법**:
```
Hierarchy에서 SidebarUI 오브젝트를 Inspector의 필드로 드래그
→ 컴포넌트 선택 팝업이 뜸 → SidebarUIManager / CustomUI 선택
```

**표시할 데이터 선택** (체크박스):
```
☑ 성능 정보 표시
☑ 배터리 정보 표시
☐ 연료 정보 표시
☐ 에너지 소비 정보 표시
```

---

### 4️⃣ **UI 생성 버튼 클릭**

모든 할당 완료 후 → **✅ UI자동생성** 버튼 클릭

---

## 🎬 전체 프로세스 요약

```
0️⃣ 프리팹 생성 (처음 한 번만)
   Tools > Custom UI > Create Panel Prefab

1️⃣ CarManager 씬에 추가
   Hierarchy → Create Empty "CarManager"
   → CarManager 스크립트 추가
   → CarDatabase 어셋 할당

2️⃣ Canvas 구조 구성
   Canvas
   ├─ SidebarUI
   │   ├─ [SidebarUIManager 컴포넌트]
   │   ├─ [CustomUI 컴포넌트]
   │   ├─ [SidebarUISetup 컴포넌트]
   │   └─ PanelContainer (빈 GameObject)
   └─ ...

3️⃣ SidebarUISetup 필드 4개 할당
   ✓ Sidebar Manager  → 같은 오브젝트의 SidebarUIManager
   ✓ Custom UI        → 같은 오브젝트의 CustomUI
   ✓ Panel Prefab     → Prefabs/DataAdjustmentPanel
   ✓ Panel Container  → 자식 PanelContainer

4️⃣ [UI자동생성] 버튼 클릭 ✅
```

---

## 각 스크립트 세부 역할

### **SidebarUIManager** - Inspector 할당 불필요
- SidebarUISetup이 panelPrefab과 panelContainer를 전달
- CarManager의 차량 변경 이벤트를 감지해 패널 자동 갱신

### **CustomUI** - Inspector 할당 불필요
- CanvasGroup을 자동으로 찾아 UI 투명도 제어
- `SetUIVisible()` / `ToggleUI()` 메서드 제공

### **DataAdjustmentPanel** (프리팹) - Inspector 할당 불필요
```
DataAdjustmentPanel
├─ Title (TextMeshProUGUI)
└─ ItemContainer (VerticalLayoutGroup)
```

### **DataAdjustmentUIItem** (프리팹) - Inspector 할당 불필요
```
DataAdjustmentUIItem
├─ Label (TextMeshProUGUI)
├─ Slider
└─ Value (TextMeshProUGUI)
```

---

## ❓ FAQ

**Q: "Sidebar Manager를 할당하세요!" 오류가 납니다**
A: SidebarUI에 **SidebarUIManager 컴포넌트가 추가됐는지** 확인하세요. 추가 후 SidebarUISetup의 Sidebar Manager 필드에 드래그하면 됩니다.

**Q: 패널이 안 나타나요**
A: 순서대로 확인하세요:
- [ ] CarManager가 씬에 있고 CarDatabase가 할당됐나?
- [ ] SidebarUISetup의 4개 필드가 모두 할당됐나?
- [ ] Panel Container가 SidebarUI의 자식인가?

**Q: Play 모드에서 슬라이더 값이 0이에요**
A: **CarManager가 씬에 없거나 CarDatabase가 할당 안 됐을 가능성이 높습니다.**

**Q: CustomUI / SidebarUIManager는 뭘 할당하나요?**
A: Inspector에서 직접 할당하는 것 없음. SidebarUISetup이 런타임에 전달합니다.

**Q: ItemPrefab은 어디서 할당하나요?**
A: 할당 안 합니다. DataAdjustmentPanel이 자동으로 찾습니다.
