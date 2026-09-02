using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 玩家生命值管理：扣血、受伤闪烁、击退、死亡掉落背包、复活逻辑
public class PlayerHealthy : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth = 100;
    public Slider slider;
    public SpriteRenderer spriteRenderer;
    public Color hurtColor = Color.red;
    public float hurtTime = 0.1f;
    public Rigidbody2D rb;
    public float knockBackForce;
    private Color originalColor;

    [Header("玩家音效")]
    public AudioClip playerHurtSound;
    private AudioSource _audioSource;

    [Header("死亡复活UI设置")]
    public GameObject reviveCanvasPrefab;
    public int reviveHealth = 100;
    private GameObject _reviveCanvasInstance;

    [Header("掉落物设置")]
    public GameObject lootPrefab;

    [Header("出生点设置")]
    public Transform defaultSpawnPoint;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        if (defaultSpawnPoint != null)
        {
            transform.position = defaultSpawnPoint.position;
        }
        slider = GameObject.Find("HealthySlider").GetComponent<Slider>();
        currentHealth = GameManager.Instance.playerHealth;
        maxHealth = GameManager.Instance.playerMaxHealth;
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
    }

    //修改生命值，处理受伤击退、死亡判断，同步存档
    public void ChangeHealth(int amount, Vector2 hitDirection)
    {
        currentHealth += amount;
        if (amount < 0)
        {
            StartCoroutine(HurtFlash());
            KnockBack(hitDirection);
            if (playerHurtSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(playerHurtSound);
            }
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerHealth = currentHealth;
            GameManager.Instance.playerMaxHealth = maxHealth;
            GameManager.Instance.SaveToUserFile();
        }
        else
        {
            //Debug.LogError("GameManager.Instance 是空的");
        }
        if (slider != null)
        {
            slider.value = currentHealth;
        }
        if (currentHealth <= 0)
        {
            PlayerDie();
        }
    }

    //玩家死亡：掉落全部背包物品，弹出复活界面，暂停游戏
    void PlayerDie()
    {
        //Debug.Log("玩家死亡！");
        DropAllInventoryItems();
        StopAllCoroutines();
        gameObject.SetActive(false);
        Time.timeScale = 0f;
        if (reviveCanvasPrefab != null && _reviveCanvasInstance == null)
        {
            _reviveCanvasInstance = Instantiate(reviveCanvasPrefab);
            _reviveCanvasInstance.name = "ReviveCanvas_Instance";
            DontDestroyOnLoad(_reviveCanvasInstance);
            Button reviveBtn = _reviveCanvasInstance.GetComponentInChildren<Button>();
            if (reviveBtn != null)
            {
                reviveBtn.onClick.RemoveAllListeners();
                reviveBtn.onClick.AddListener(RevivePlayer);
            }
            else
            {
                //Debug.LogError("在ReviveCanvas预制体内找不到Button组件！检查层级");
            }
        }
    }

    //复活玩家，恢复血量，回到出生点，恢复游戏时间
    public void RevivePlayer()
    {
        DestroyRevivePopup();
        gameObject.SetActive(true);
        //修复受伤红色残留
        StopAllCoroutines();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        if (GameManager.Instance != null)
        {
            currentHealth = GameManager.Instance.playerMaxHealth;
            maxHealth = GameManager.Instance.playerMaxHealth;
            GameManager.Instance.playerHealth = currentHealth;
        }
        else
        {
            //兜底，如果GameManager为空，使用本地maxHealth
            currentHealth = maxHealth;
        }
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
        if (defaultSpawnPoint != null)
        {
            transform.position = defaultSpawnPoint.position;
        }
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
        Time.timeScale = 1f;
        GameManager.Instance.SaveToUserFile();
    }

    //死亡时把背包、快捷栏全部物品生成到地上
    void DropAllInventoryItems()
    {
        if (InventoryController.Instance == null) return;
        Vector3 playerPos = transform.position;
        var bagSlots = InventoryController.Instance.GetBagSlots();
        foreach (var slot in bagSlots)
        {
            if (!slot.IsEmpty)
            {
                SpawnLootOnGround(slot.item, slot.count, playerPos);
                InventoryController.Instance.ClearSlotExternal(slot);
            }
        }
        var quickSlots = InventoryController.Instance.GetQuickBarSlots();
        foreach (var slot in quickSlots)
        {
            if (!slot.IsEmpty)
            {
                SpawnLootOnGround(slot.item, slot.count, playerPos);
                InventoryController.Instance.ClearSlotExternal(slot);
            }
        }
        GameManager.Instance.SaveToUserFile();
    }

    //在玩家位置生成地上掉落物，带随机偏移
    void SpawnLootOnGround(ItemSO item, int count, Vector3 centerPos)
    {
        float offsetX = Random.Range(-1.2f, 1.2f);
        float offsetY = Random.Range(-1.2f, 1.2f);
        Vector3 spawnPos = centerPos + new Vector3(offsetX, offsetY, 0);
        if (lootPrefab == null)
        {
            //Debug.LogError("请把LootPrefab拖入PlayerHealthy组件lootPrefab字段");
            return;
        }
        GameObject lootGo = Instantiate(lootPrefab, spawnPos, Quaternion.identity);
        LootItem loot = lootGo.GetComponent<LootItem>();
        loot.SetLoot(item, count);
    }

    //销毁复活弹窗
    void DestroyRevivePopup()
    {
        if (_reviveCanvasInstance != null)
        {
            Destroy(_reviveCanvasInstance);
            _reviveCanvasInstance = null;
        }
    }

    private void OnDestroy()
    {
        DestroyRevivePopup();
    }

    //受伤闪烁颜色协程
    IEnumerator HurtFlash()
    {
        spriteRenderer.color = hurtColor;
        yield return new WaitForSeconds(hurtTime);
        spriteRenderer.color = originalColor;
    }

    //执行被击退效果
    void KnockBack(Vector2 direction)
    {
        if (rb == null)
            return;
        rb.velocity = Vector2.zero;
        rb.AddForce(
            direction * knockBackForce,
            ForceMode2D.Impulse
        );
    }
}
