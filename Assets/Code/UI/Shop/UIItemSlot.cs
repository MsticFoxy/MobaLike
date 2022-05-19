using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIItemSlot : MonoBehaviour
{
    public Item item;
    public Image sprite;
    public Image spriteBackground;
    public Sprite backgroundImage;
    public TextMeshProUGUI priceText;
    public bool isOwned = true;
    public bool showPrice = false;
    public bool useUpgradePrice = false;

    //from to
    public Action<Item, Item> OnItemChanged;
    public ShopManager shopManager { get; set; }
    public bool inDrag { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        SetItem(item);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (item != null)
        {
            if (item.activeAbility != null)
            {
                sprite.fillAmount = GetAbilityFillAmount();
            }
            if (isOwned)
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1);
                spriteBackground.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1);
            }
            else
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.0f);
                spriteBackground.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.8f) * 0.5f;
            }
            priceText.enabled = showPrice;
            if (isOwned)
            {
                priceText.color = new Color(1, 1, 1, 0.4f);
            }
            else
            {
                priceText.color = Color.white;
            }
        }
    }

    public void SetInDrag(bool val)
    {
        inDrag = val;
        if(inDrag)
        {
            sprite.sprite = backgroundImage;
            spriteBackground.sprite = backgroundImage;
        }
        else
        {
            if (item != null)
            {
                sprite.sprite = item.sprite;
                spriteBackground.sprite = item.sprite;
            }
        }
    }

    public void SetItem(Item item)
    {
        Item prevItem = this.item;
        this.item = item;
        if (item == null)
        {
            sprite.sprite = backgroundImage;
            spriteBackground.sprite = backgroundImage;
        }
        else
        {
            sprite.sprite = item.sprite;
            spriteBackground.sprite = item.sprite;
        }
        sprite.fillAmount = GetAbilityFillAmount();
        if (item != null)
        {
            if (useUpgradePrice)
            {
                priceText.text = "" + item.GetInstancePrice();
            }
            else
            {
                priceText.text = "" + item.GetInstanceCollectivePrice();
            }
        }
        else
        {
            priceText.text = "";
        }
        //OnItemChanged?.Invoke(prevItem, item);
        if(OnItemChanged != null && isOwned)
        {
            OnItemChanged.Invoke(prevItem, item);
        }
    }

    protected float GetAbilityFillAmount()
    {
        if (item != null)
        {
            if (item.activeAbility != null)
            {
                return 1.0f - item.activeAbility.currentCooldown / item.activeAbility.GetCooldown();
            }
            else
            {
                return 1;
            }
        }
        return 1;
    }

    public void SwapItem(UIItemSlot other)
    {
        Item it = other.item;
        bool own = other.isOwned;
        bool stopSwap = false;
        if (isOwned)
        {
            other.SetItem(item);
        }
        else
        {
            if(shopManager.combinationItemSlots.Contains(other))
            {
                stopSwap = true;
            }
            if(!stopSwap)
            {
                other.SetItem(null);
            }
        }
        if (!stopSwap)
        {
            isOwned = other.isOwned;
            SetItem(it);
        }
    }
}
