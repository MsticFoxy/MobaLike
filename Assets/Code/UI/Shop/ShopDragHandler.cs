using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIItemSlot))]
public class ShopDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
{
    public Image dragItem;
    private UIItemSlot slot;
    public Transform dragContainerTransform;
    private void Start()
    {
        slot = GetComponent<UIItemSlot>();
    }

    

    public void OnDrag(PointerEventData eventData)
    {
        if (slot.item != null && slot.isOwned)
        {
            dragItem.transform.position = Input.mousePosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot.item != null && slot.isOwned)
        {
            GameObject go = Instantiate(new GameObject());
            go.AddComponent<Image>();
            if(dragContainerTransform != null)
            {
                go.transform.SetParent(dragContainerTransform, false);
            }
            else
            {
                go.transform.SetParent(transform, false);
            }
            go.transform.position = Input.mousePosition;
            dragItem = go.GetComponent<Image>();
            dragItem.sprite = slot.item.sprite;
            slot.SetInDrag(true);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (slot != null && slot.isOwned)
        {
            slot.SetInDrag(false);
        }
        if (dragItem != null)
        {
            Destroy(dragItem.gameObject);
            dragItem = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if(eventData.pointerDrag != null)
        {
            if (eventData.pointerDrag.GetComponent<ShopDragHandler>() != null)
            {
                ShopDragHandler dragH = eventData.pointerDrag.GetComponent<ShopDragHandler>();
                if (dragH != null && dragH.dragItem != null)
                {
                    Destroy(dragH.dragItem.gameObject);
                }
            }
            if (eventData.pointerDrag.GetComponent<UIItemSlot>() != null)
            {
                UIItemSlot dragSlot = eventData.pointerDrag.GetComponent<UIItemSlot>();
                if (dragSlot.isOwned)
                {
                    slot.SwapItem(dragSlot);
                }
            }
        }
    }
}
