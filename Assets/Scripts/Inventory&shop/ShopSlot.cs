using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    public ItemSO itemSO;
    public TMP_Text itemNameText;
    public TMP_Text itemPriceText;
    public Image itemImage;

    private ShopManager _shopMgr;
    private int price;

    void Start()
    {
        // 游戏运行时自动找到场景里的ShopManager
        _shopMgr = FindObjectOfType<ShopManager>();
        if (_shopMgr == null)
        {
            //Debug.LogError("场景未放置ShopManager物体！");
        }
    }

    public void Initialize(ItemSO itemSO, int price)
    {
        this.itemSO = itemSO;
        this.price = price;
        itemNameText.text = itemSO.itemName;
        itemPriceText.text = price.ToString();
        itemImage.sprite = itemSO.icon;
    }

    public void OnBuyButtonClicked()
    {
        //Debug.Log("按钮被点击了！");
        if (_shopMgr == null || itemSO == null) return;
        _shopMgr.TryBuyItem(itemSO, price);
    }
}
