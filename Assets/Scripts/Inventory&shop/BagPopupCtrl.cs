using UnityEngine;

public class BagPopupCtrl : MonoBehaviour
{
    [Header("背包弹窗面板")]
    public GameObject bagPanel;

    [HideInInspector] public bool bagIsOpen = false;

    //外部调用，切换背包开关（由InventoryController调用）
    public void ToggleBag()
    {
        bagIsOpen = !bagIsOpen;
        bagPanel.SetActive(bagIsOpen);
        GameManager.Instance.isBagOpen = bagIsOpen;
        if (bagIsOpen)
        {
            Time.timeScale = 0f; //打开背包，暂停游戏
            InventoryView view = bagPanel.GetComponentInChildren<InventoryView>();
            view?.Refresh();
        }
        else
        {
            Time.timeScale = 1f; //关闭背包，恢复游戏
            InventoryController.Instance?.ClearHolding();
            DragGhostManager.Instance?.DestroyActiveGhost();
        }
    }

    //外部调用：关闭背包
    public void CloseBag()
    {
        bagIsOpen = false;
        bagPanel.SetActive(false);
        GameManager.Instance.isBagOpen = false;
        Time.timeScale = 1f; //关闭恢复时间
        InventoryController.Instance?.ClearHolding();
        DragGhostManager.Instance?.DestroyActiveGhost();
    }
}
