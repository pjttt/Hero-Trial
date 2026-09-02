using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [Header("已经在场景摆好的全部背包格子，一共45个")]
    public SlotView[] slotViews;

    private void Awake()
    {
        // ❌不要在这里订阅！InventoryController.Instance此时可能为null
    }

    private void Start()
    {
        // ✅全部物体Awake执行完毕后再订阅事件，Instance已经初始化完成
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged += Refresh;
            // 游戏启动立刻渲染一次UI
            Refresh();
        }
        else
        {
            //Debug.LogError("InventoryView：找不到InventoryController！检查场景是否有InventorySystem物体", this);
        }
    }

    public void Refresh()
    {
        if (slotViews == null || slotViews.Length == 0) return;
        var bagSlots = InventoryController.Instance.GetBagSlots();
        //Debug.Log("-----开始刷新背包UI-----");
        for (int i = 0; i < slotViews.Length; i++)
        {
            SlotView uiSlot = slotViews[i];
            if (i >= bagSlots.Count)
            {
                uiSlot.boundSlot = null;
                uiSlot.Render();
                //Debug.Log($"UI格子[{i}] boundSlot = null");
                continue;
            }
            uiSlot.boundSlot = bagSlots[i];
            uiSlot.Render();
            //Debug.Log($"UI格子[{i}] 绑定Model槽位[{i}]  item={bagSlots[i].item?.itemName}  Model对象ID:{bagSlots[i].GetHashCode()}");
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
