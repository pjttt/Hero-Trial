using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
    private static InventoryController _instance;
    public static InventoryController Instance
    {
        get
        {
            if (_isQuitting)
            {
                return null;
            }
            if (_instance == null)
            {
                _instance = FindObjectOfType<InventoryController>();
                if (_instance != null)
                {
                    //Debug.LogWarning("【Inventory】静态实例丢失，FindObjectOfType找回");
                }
                else
                {
                    //Debug.LogError("【Inventory】完全找不到InventoryController物体");
                }
            }
            return _instance;
        }
    }

    [Header("配置，和UI格子数量保持一致")]
    public int bagSlotTotal = 45;
    public int quickBarSlotTotal = 9;

    [Header("【弹窗背包UI预制体】从Project窗口拖入，不要Hierarchy")]
    public GameObject bagUIPrefab;
    private GameObject _currentBagUiInstance;

    [Header("快捷栏UI预制体，Project窗口拖入，不要Hierarchy！")]
    public GameObject quickBarUIPrefab;
    private GameObject _currentQuickBarInstance;

    private InventoryModel _model;
    public SlotModel HoldingSlot { get; set; }
    public event Action OnInventoryChanged;
    public static bool _isQuitting;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        _model = new InventoryModel(bagSlotTotal, quickBarSlotTotal);
        //❗不再在这里LoadInventory，由GameManager调用LoadFromFullSave
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_currentBagUiInstance != null)
        {
            Destroy(_currentBagUiInstance);
        }
        if (bagUIPrefab != null)
        {
            _currentBagUiInstance = Instantiate(bagUIPrefab);
            _currentBagUiInstance.name = "BagUI_Instance";
            DontDestroyOnLoad(_currentBagUiInstance);
            InventoryView view = _currentBagUiInstance.GetComponentInChildren<InventoryView>();
            view?.Refresh();
        }

        if (_currentQuickBarInstance != null)
        {
            Destroy(_currentQuickBarInstance);
        }
        if (quickBarUIPrefab != null)
        {
            _currentQuickBarInstance = Instantiate(quickBarUIPrefab);
            _currentQuickBarInstance.name = "QuickBar_Instance";
            DontDestroyOnLoad(_currentQuickBarInstance);
            QuickBarView qbView = _currentQuickBarInstance.GetComponentInChildren<QuickBarView>();
            qbView?.Refresh();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // 销毁由本脚本实例化出来的背包、快捷栏UI
        if (_currentBagUiInstance != null)
        {
            Destroy(_currentBagUiInstance);
            _currentBagUiInstance = null;
        }
        if (_currentQuickBarInstance != null)
        {
            Destroy(_currentQuickBarInstance);
            _currentQuickBarInstance = null;
        }
        if (_instance == this)
        {
            _instance = null; //清空静态单例引用
        }
    }

    #region 对外接口
    public bool TryPickupItem(ItemSO item, int count)
    {
        bool ret = _model.TryAddItem(item, count);
        if (ret) NotifyChanged();
        return ret;
    }

    public bool CheckHasSpace(ItemSO item)
    {
        return _model.HasSpaceFor(item);
    }

    public void OnSlotClick(SlotModel clickedSlot)
    {
        if (HoldingSlot == null)
        {
            if (!clickedSlot.IsEmpty)
            {
                HoldingSlot = clickedSlot;
            }
            NotifyChanged();
            GameManager.Instance?.SaveToUserFile(); //点击/拾取格子后立刻保存
            return;
        }

        SlotModel fromSlot = HoldingSlot;
        SlotModel toSlot = clickedSlot;

        if (fromSlot == toSlot)
        {
            HoldingSlot = null;
            NotifyChanged();
            GameManager.Instance?.SaveToUserFile();
            return;
        }
        if (toSlot.IsEmpty)
        {
            toSlot.item = fromSlot.item;
            toSlot.count = fromSlot.count;
            _model.ClearSlot(fromSlot);
            HoldingSlot = null;
            NotifyChanged();
            GameManager.Instance?.SaveToUserFile();
            return;
        }
        if (fromSlot.item == toSlot.item)
        {
            toSlot.count += fromSlot.count;
            _model.ClearSlot(fromSlot);
            HoldingSlot = null;
            NotifyChanged();
            GameManager.Instance?.SaveToUserFile();
            return;
        }
        _model.SwapSlot(fromSlot, toSlot);
        HoldingSlot = null;
        NotifyChanged();
        GameManager.Instance?.SaveToUserFile();
    }

    public void ClearHolding()
    {
        HoldingSlot = null;
        NotifyChanged();
    }

    // 尝试食用该槽位物品，返回是否食用成功
    public bool TryEatItem(SlotModel slot)
    {
        if (slot == null || slot.IsEmpty) return false;
        ItemSO item = slot.item;
        if (!item.isEdible) return false;

        // 获取玩家血量组件
        PlayerHealthy playerHp = FindObjectOfType<PlayerHealthy>();
        if (playerHp == null) return false;

        //血量已满不能吃
        if (playerHp.currentHealth >= playerHp.maxHealth)
        {
            //Debug.Log("血量已满，不需要食用");
            return false;
        }

        //执行扣物品
        bool consumeOk = _model.ConsumeOneItem(slot);
        if (!consumeOk) return false;

        //播放吃东西音效
        if (item.eatSound != null)
        {
            AudioSource.PlayClipAtPoint(item.eatSound, playerHp.transform.position);
        }
        //执行回血，amount传正数代表加血，hitDirection随便传zero（回血不需要击退）
        playerHp.ChangeHealth(item.healAmount, Vector2.zero);
        NotifyChanged();
        GameManager.Instance?.SaveToUserFile();
        return true;
    }

    // 外部：清空指定槽位（死亡掉落使用）
    public void ClearSlotExternal(SlotModel slot)
    {
        _model.ClearSlot(slot);
        NotifyChanged();
    }
    #endregion

    #region =====新接口【交给GameManager调用】=====
    public InventorySaveData GetInventorySaveData()
    {
        return _model.GetSaveData();
    }

    public void LoadFromFullSave(InventorySaveData save)
    {
        ItemSO[] allItems = Resources.LoadAll<ItemSO>("Items");
        _model.LoadFromSave(save, allItems);
        NotifyChanged();
    }

    public void ClearAllInventory()
    {
        foreach (var slot in _model.BagSlots) _model.ClearSlot(slot);
        foreach (var slot in _model.QuickBarSlots) _model.ClearSlot(slot);
        NotifyChanged();
    }
    #endregion

    #region 获取数据
    public List<SlotModel> GetBagSlots() => _model.BagSlots;
    public List<SlotModel> GetQuickBarSlots() => _model.QuickBarSlots;
    #endregion

    public void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            BagPopupCtrl bagCtrl = null;
            if (_currentBagUiInstance != null)
            {
                bagCtrl = _currentBagUiInstance.GetComponentInChildren<BagPopupCtrl>();
            }
            if (bagCtrl != null)
            {
                bagCtrl.ToggleBag();
            }
            else
            {
                //Debug.LogWarning("按P：找不到BagPopupCtrl，背包UI还未实例化完成");
            }
        }
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
        //退出保存统一交给GameManager，这里不再处理
    }

    public void OnDragDrop(SlotModel fromSlot, SlotModel toSlot)
    {
        if (fromSlot == null || toSlot == null) return;
        if (fromSlot == toSlot)
        {
            HoldingSlot = null;
            NotifyChanged();
            GameManager.Instance?.SaveToUserFile();
            return;
        }
        if (toSlot.IsEmpty)
        {
            toSlot.item = fromSlot.item;
            toSlot.count = fromSlot.count;
            _model.ClearSlot(fromSlot);
            HoldingSlot = null;
            NotifyChanged();
            GameManager.Instance?.SaveToUserFile();
            return;
        }
        if (fromSlot.item == toSlot.item)
        {
            toSlot.count += fromSlot.count;
            _model.ClearSlot(fromSlot);
            HoldingSlot = null;
            NotifyChanged();
            GameManager.Instance?.SaveToUserFile();
            return;
        }
        _model.SwapSlot(fromSlot, toSlot);
        HoldingSlot = null;
        NotifyChanged();
        GameManager.Instance?.SaveToUserFile();
    }
}
