using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SlotView : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public Image itemIcon;
    public TMP_Text quantityText;

    [HideInInspector] public SlotModel boundSlot;

    public void Render()
    {
        if (ReferenceEquals(null, itemIcon))
            return;
        if (boundSlot == null || boundSlot.IsEmpty)
        {
            itemIcon.gameObject.SetActive(false);
            quantityText.text = "";
            return;
        }
        itemIcon.gameObject.SetActive(true);
        itemIcon.sprite = boundSlot.item.icon;
        quantityText.text = boundSlot.count.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InventoryController.Instance == null) return;
        if (boundSlot == null) return;

        //===== 鼠标右键：执行食用食物 =====
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (!boundSlot.IsEmpty && boundSlot.item.isEdible)
            {
                InventoryController.Instance.TryEatItem(boundSlot);
                //右键只做食用，直接return，不走格子交换逻辑
                return;
            }
        }
        //===== 鼠标左键：原有逻辑，拿起/拖拽交换，完全保留原来行为 =====
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryController.Instance.OnSlotClick(boundSlot);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log($"[SlotView]开始拖拽 boundSlot={(boundSlot != null ? boundSlot.item : "空")}");
        if (InventoryController.Instance == null)
        {
            eventData.pointerDrag = null;
            return;
        }
        if (boundSlot == null || boundSlot.IsEmpty)
        {
            eventData.pointerDrag = null;
            return;
        }
        InventoryController.Instance.HoldingSlot = boundSlot;
        DragGhostManager.Instance.SpawnGhost(boundSlot.item.icon, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("[SlotView]OnDrag持续拖拽");
        DragGhostManager.Instance.SetGhostPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("[SlotView]结束拖拽");
        if (InventoryController.Instance == null) return;
        DragGhostManager.Instance.DestroyActiveGhost();
        if (InventoryController.Instance.HoldingSlot == null)
            return;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        SlotView target = null;
        foreach (var res in results)
        {
            target = res.gameObject.GetComponent<SlotView>();
            if (target != null) break;
        }

        if (target != null)
        {
            //Debug.Log($"[SlotView]拖拽目标找到:{target.name}");
            InventoryController.Instance.OnDragDrop(InventoryController.Instance.HoldingSlot, target.boundSlot);
        }
        else
        {
            //Debug.Log("[SlotView]没有命中任何格子，取消拖拽");
            InventoryController.Instance.ClearHolding();
        }
    }
}
