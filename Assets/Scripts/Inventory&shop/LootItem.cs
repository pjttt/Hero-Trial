using UnityEngine;

public class LootItem : MonoBehaviour
{
    public ItemSO itemData;
    public int count;
    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void SetLoot(ItemSO item, int num)
    {
        if (item == null) return;
        itemData = item;
        count = num;
        _sr.sprite = item.icon;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        //全部判空，防止空引用崩溃
        if (itemData == null)
        {
            Destroy(gameObject);
            return;
        }

        //金币逻辑
        if (itemData.itemName == "Coin")
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerCoin += count;
                GameManager.Instance.SaveToUserFile();
            }
            //金币拾取音效
            if (itemData.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(itemData.pickupSound, transform.position);
            }
            Destroy(gameObject);
            return;
        }

        //普通物品拾取
        if (InventoryController.Instance == null)
        {
            //Debug.LogWarning("InventoryController实例不存在，无法拾取");
            return;
        }
        bool addOk = InventoryController.Instance.TryPickupItem(itemData, count);
        if (addOk)
        {
            //拾取成功播放拾取音效
            if (itemData.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(itemData.pickupSound, transform.position);
            }
            Destroy(gameObject); //拾取成功才销毁
            GameManager.Instance?.SaveToUserFile(); //拾取普通道具立刻保存
        }
        else
        {
            //Debug.LogWarning($"背包已满，无法拾取 {itemData.itemName}");
            //背包满，**不销毁物体，掉落物留在地上**
        }
    }

    void Start()
    {
        Destroy(gameObject, 15f);
    }
}
