using UnityEngine;

public class QuickBarView : MonoBehaviour
{
    [Header("预制体内赋值9个SlotView")]
    public SlotView[] quickSlotViews;

    private void Start()
    {
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged += Refresh;
            Refresh();
        }
    }

    public void Refresh()
    {
        if (quickSlotViews == null || quickSlotViews.Length == 0)
        {
            //Debug.LogError($"QuickBarView Refresh失败！quickSlotViews数组是空！长度:{quickSlotViews?.Length}");
            return;
        }
        var slots = InventoryController.Instance.GetQuickBarSlots();
        for (int i = 0; i < quickSlotViews.Length; i++)
        {
            SlotView uiSlot = quickSlotViews[i];
            if (ReferenceEquals(null, uiSlot)) continue;
            if (i < slots.Count)
            {
                uiSlot.boundSlot = slots[i];
            }
            else
            {
                uiSlot.boundSlot = null;
            }
            uiSlot.Render();
        }
    }

    private void OnDestroy()
    {
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged -= Refresh;
        }
    }
}
