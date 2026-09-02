using System;
using System.Collections.Generic;
using UnityEngine;

//单个槽位数据Model
[Serializable]
public class SlotModel
{
    public ItemSO item;
    public int count;
    public SlotModel()
    {
        item = null;
        count = 0;
    }
    public bool IsEmpty => item == null || count <= 0;
}

//序列化存档载体，用于Json存储
[Serializable]
public class SaveSlotData
{
    public string itemKey;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<SaveSlotData> bagSlots;
    public List<SaveSlotData> quickBarSlots;
}

// Model：只管理数据，完全不知道UI存在
public class InventoryModel
{
    //背包槽位数据
    public List<SlotModel> BagSlots { get; private set; }
    //快捷栏槽位数据
    public List<SlotModel> QuickBarSlots { get; private set; }

    public int BagSlotCount { get; }
    public int QuickBarSlotCount { get; }

    public InventoryModel(int bagSlotCount, int quickBarSlotCount)
    {
        BagSlotCount = bagSlotCount;
        QuickBarSlotCount = quickBarSlotCount;
        BagSlots = new List<SlotModel>();
        for (int i = 0; i < BagSlotCount; i++)
            BagSlots.Add(new SlotModel());
        QuickBarSlots = new List<SlotModel>();
        for (int i = 0; i < QuickBarSlotCount; i++)
            QuickBarSlots.Add(new SlotModel());
    }

    #region 业务逻辑
    // 拾取物品：优先快捷栏堆叠→快捷栏空位→背包堆叠→背包空位
    public bool TryAddItem(ItemSO item, int addCount)
    {
        if (item == null || addCount <= 0) return false;
        int remain = addCount;

        //=====第一步：优先【快捷栏】找相同物品堆叠 =====
        foreach (var slot in QuickBarSlots)
        {
            if (!slot.IsEmpty && slot.item == item)
            {
                slot.count += remain;
                return true;
            }
        }
        //=====第二步：快捷栏找空槽，直接放入快捷栏 =====
        foreach (var slot in QuickBarSlots)
        {
            if (slot.IsEmpty)
            {
                slot.item = item;
                slot.count = remain;
                return true;
            }
        }
        //=====第三步：快捷栏处理完毕，剩余再处理背包 =====
        //背包找同类堆叠
        foreach (var slot in BagSlots)
        {
            if (!slot.IsEmpty && slot.item == item)
            {
                slot.count += remain;
                return true;
            }
        }
        //背包找空槽
        foreach (var slot in BagSlots)
        {
            if (slot.IsEmpty)
            {
                slot.item = item;
                slot.count = remain;
                return true;
            }
        }
        //全部槽位都满
        return false;
    }

    //交换两个槽位数据（支持背包↔快捷栏互相交换）
    public void SwapSlot(SlotModel a, SlotModel b)
    {
        ItemSO tempItem = a.item;
        int tempCnt = a.count;
        a.item = b.item;
        a.count = b.count;
        b.item = tempItem;
        b.count = tempCnt;
    }

    //清空槽位
    public void ClearSlot(SlotModel slot)
    {
        slot.item = null;
        slot.count = 0;
    }

    //判断背包是否有空位放该物品（商店购买校验）
    public bool HasSpaceFor(ItemSO item)
    {
        //先查快捷栏：能否堆叠 / 是否有空槽
        foreach (var s in QuickBarSlots)
        {
            if (!s.IsEmpty && s.item == item) return true;
        }
        foreach (var s in QuickBarSlots)
        {
            if (s.IsEmpty) return true;
        }
        foreach (var s in BagSlots)
        {
            if (!s.IsEmpty && s.item == item) return true;
        }
        foreach (var s in BagSlots)
        {
            if (s.IsEmpty) return true;
        }
        return false;
    }

    // 消耗一个槽位内物品，count-1，如果扣完槽置空，返回是否消耗成功
    public bool ConsumeOneItem(SlotModel slot)
    {
        if (slot.IsEmpty) return false;
        slot.count--;
        if (slot.count <= 0)
        {
            ClearSlot(slot);
        }
        return true;
    }
    #endregion

    #region 存档序列化 Model只负责把数据转存档对象，不调用PlayerPrefs
    public InventorySaveData GetSaveData()
    {
        var save = new InventorySaveData();
        save.bagSlots = new List<SaveSlotData>();
        save.quickBarSlots = new List<SaveSlotData>();
        foreach (var s in BagSlots)
        {
            save.bagSlots.Add(new SaveSlotData
            {
                itemKey = s.IsEmpty ? "" : s.item.itemName,
                count = s.count
            });
        }
        foreach (var s in QuickBarSlots)
        {
            save.quickBarSlots.Add(new SaveSlotData
            {
                itemKey = s.IsEmpty ? "" : s.item.itemName,
                count = s.count
            });
        }
        return save;
    }

    public void LoadFromSave(InventorySaveData save, ItemSO[] allItems)
    {
        //重置全部为空
        foreach (var s in BagSlots) this.ClearSlot(s);
        foreach (var s in QuickBarSlots) this.ClearSlot(s);
        //读背包
        for (int i = 0; i < save.bagSlots.Count && i < BagSlots.Count; i++)
        {
            var sd = save.bagSlots[i];
            if (string.IsNullOrEmpty(sd.itemKey)) continue;
            var item = Array.Find(allItems, x => x.itemName == sd.itemKey);
            if (item != null)
            {
                BagSlots[i].item = item;
                BagSlots[i].count = sd.count;
            }
        }
        //读快捷栏
        for (int i = 0; i < save.quickBarSlots.Count && i < QuickBarSlots.Count; i++)
        {
            var sd = save.quickBarSlots[i];
            if (string.IsNullOrEmpty(sd.itemKey)) continue;
            var item = Array.Find(allItems, x => x.itemName == sd.itemKey);
            if (item != null)
            {
                QuickBarSlots[i].item = item;
                QuickBarSlots[i].count = sd.count;
            }
        }
    }
    #endregion
}
