using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIShopCraftableItem : MonoBehaviour, IPointerDownHandler
{
    public Item item;
    public Action<Item, int> OnItemSelected;
    public Image sprite;
    public int index;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("MouseClick");
        if (OnItemSelected != null)
        {
            OnItemSelected.Invoke(item, index);
        }
    }

    public void SetItem(Item i)
    {
        if (i != null)
        {
            item = i;
            sprite.sprite = i.sprite;
        }
    }
}
