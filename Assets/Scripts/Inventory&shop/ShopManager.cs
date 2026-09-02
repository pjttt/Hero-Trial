using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("物品分类商品")]
    [SerializeField] private List<ShopItems> itemsShopItems;
    [Header("武器分类商品")]
    [SerializeField] private List<ShopItems> weaponShopItems;
    [Header("其它分类商品")]
    [SerializeField] private List<ShopItems> otherShopItems;

    [Header("商店UI面板【拖入UI面板，不要把本脚本挂在这个面板上！】")]
    public GameObject shopPanel;

    [Header("UI：商店格子数组，拖入场景所有ShopSlot")]
    [SerializeField] private ShopSlot[] shopSlots;

    private List<ShopItems> currentShowItems;

    [System.Serializable]
    public class ShopItems
    {
        public ItemSO itemSO;
        public int price;
    }

    private void Awake()
    {
        //单例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        //仅仅隐藏UI面板，不会关闭自己！本脚本在独立空物体ShopManagerObj
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        SwitchToItemsTab();
    }

    void Update()
    {
        // 商店面板打开状态，按ESC关闭商店
        if (shopPanel != null && shopPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
        }
    }

    #region 打开/关闭商店（给NPC、关闭按钮调用）
    //NPC交互调用：打开商店面板
    public void OpenShop()
    {
        if (shopPanel == null)
        {
            //Debug.LogError("ShopManager：没有赋值shopPanel！");
            return;
        }
        shopPanel.SetActive(true);
        SwitchToItemsTab();
        // 和背包共用标记，禁止玩家移动攻击
        GameManager.Instance.isBagOpen = true;
        Time.timeScale = 0f; //打开商店暂停游戏
    }

    //关闭按钮 / ESC调用，关闭商店
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        GameManager.Instance.isBagOpen = false;
        Time.timeScale = 1f; //关闭商店恢复
        // 清理拖拽幽灵，防止UI残留
        DragGhostManager.Instance?.DestroyActiveGhost();
    }
    #endregion

    // ==========分类切换按钮调用==========
    public void SwitchToItemsTab()
    {
        currentShowItems = itemsShopItems;
        RefreshShopUI();
    }

    public void SwitchToWeaponTab()
    {
        currentShowItems = weaponShopItems;
        RefreshShopUI();
    }

    public void SwitchToothersTab()
    {
        currentShowItems = otherShopItems;
        RefreshShopUI();
    }

    //刷新格子，把currentShowItems填充到shopSlots，复用原有逻辑
    public void RefreshShopUI()
    {
        if (currentShowItems == null)
            currentShowItems = new List<ShopItems>();
        // 循环填充商品
        for (int i = 0; i < currentShowItems.Count && i < shopSlots.Length; i++)
        {
            ShopItems shopItem = currentShowItems[i];
            shopSlots[i].Initialize(shopItem.itemSO, shopItem.price);
            shopSlots[i].gameObject.SetActive(true);
        }
        // 多余格子隐藏
        for (int i = currentShowItems.Count; i < shopSlots.Length; i++)
        {
            shopSlots[i].gameObject.SetActive(false);
        }
    }

    public void TryBuyItem(ItemSO itemSO, int price)
    {
        if (GameManager.Instance.playerCoin < price)
        {
            //Debug.Log("金币不足，无法购买！");
            return;
        }
        if (!InventoryController.Instance.CheckHasSpace(itemSO))
        {
            //Debug.Log("背包已满，无法购买！");
            return;
        }
        GameManager.Instance.playerCoin -= price;
        bool ok = InventoryController.Instance.TryPickupItem(itemSO, 1);
        if (ok)
        {
            //Debug.Log($"购买成功：{itemSO.itemName}，剩余金币：{GameManager.Instance.playerCoin}");
            GameManager.Instance.SaveToUserFile(); //拾取物品完成立刻整体保存（金币+背包）
        }
    }
}
