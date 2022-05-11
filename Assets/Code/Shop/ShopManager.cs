using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public Inventory inventory;
    public List<Item> ingredients;
    [ReadOnly]
    [NonReorderable]
    public List<Item> combinationItems;
    public Item combinationItem;

    public Task<List<Item>> shopCraftableItemCalculationTask;
    private bool shopCraftingFinished;
    public Task<Item> combinationItemCreationTask;
    private bool combinationItemFinished;

    public UIItemSlot combinationSlotPrefab;
    public List<UIItemSlot> combinationItemSlots;
    public RectTransform combinationRectTransform;
    public UIItemSlot combinationItemSlot;
    public Transform dragContainerRectTransform;
    public Transform combinationPossibilitiesTransform;
    public GameObject craftableItemPrefab;
    public List<Item> craftableItems;
    private int combiIndex;

    // Start is called before the first frame update
    void Start()
    {
        AddCombinationSlot();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRecipieChanged()
    {
        craftableItems = GetCraftableItems(combinationItems);
        Item combiItem = GetCombinationItem(combinationItems, combiIndex);
        combinationItemSlot.SetItem(combiItem);
        foreach(Transform t in combinationPossibilitiesTransform.GetComponentInChildren<Transform>())
        {
            Destroy(t.gameObject);
        }
        int id = 0;
        
        foreach (Item i in craftableItems)
        {
            GameObject go = Instantiate(craftableItemPrefab);
            UIShopCraftableItem sci = go.GetComponent<UIShopCraftableItem>();
            sci.SetItem(i);
            sci.index = id;
            id++;
            sci.OnItemSelected += (item, index) =>
            {
                Debug.Log("Other item selected");
                Item combiItem = GetCombinationItem(combinationItems, index);
                combiIndex = index;
                combinationItemSlot.SetItem(combiItem);
                if (combiItem != null)
                {
                    CompleteRecipie(combiItem);
                }
            };
            go.transform.SetParent(combinationPossibilitiesTransform);
        }
        CompleteRecipie(combiItem);
    }

    private void ClearNotOwnedItemSlots()
    {
        List<UIItemSlot> rem = new List<UIItemSlot>();
        foreach(UIItemSlot slot in combinationItemSlots)
        {
            if(slot != null)
            {
                if(!slot.isOwned)
                {
                    rem.Add(slot);
                }
            }
        }
        foreach(UIItemSlot slot in rem)
        {
            combinationItemSlots.Remove(slot);
            Destroy(slot.gameObject);
        }

    }

    public void CompleteRecipie(Item target)
    {
        combinationItemSlots.Last().gameObject.SetActive(true);
        ClearNotOwnedItemSlots();
        combinationItem = target;
        foreach (Item i in GetRemainingItems(target, combinationItems, -Item.variance))
        {
            UIItemSlot slot = combinationItemSlots.Last();
            slot.isOwned = false;
            slot.SetItem(i);
            //Debug.Log(slot);
            AddCombinationSlot(true);
        }
        if (combinationItemSlots.Count > 1)
        {
            combinationItemSlots.Last().gameObject.SetActive(false);
        }
    }

    private UIItemSlot AddCombinationSlot(bool isOwned = true)
    {
        
        GameObject go = Instantiate(combinationSlotPrefab.gameObject);
        UIItemSlot combinationSlot = go.GetComponent<UIItemSlot>();
        combinationSlot.shopManager = this;
        combinationSlot.isOwned = isOwned;
        if (go.GetComponent<ShopDragHandler>() != null)
        {
            go.GetComponent<ShopDragHandler>().dragContainerTransform = dragContainerRectTransform;
        }
        combinationSlot.OnItemChanged += (from, to) =>
        {
            if (from != null)
            {
                if (combinationSlot.isOwned)
                {
                    combinationItems.Remove(from);
                }

                if (to == null)
                {
                    RemoveCombinationSlot(combinationSlot);
                }
                if (combinationSlot.isOwned)
                {
                    OnRecipieChanged();
                }
            }
            if (to != null)
            {
                if (combinationSlot.isOwned)
                {
                    combinationItems.Add(to);
                }

                if (from == null)
                {
                    AddCombinationSlot();
                }
                if (combinationSlot.isOwned)
                {
                    OnRecipieChanged();
                }
            }
        };
        combinationItemSlots.Add(combinationSlot);
        go.transform.SetParent(combinationRectTransform);
        return combinationSlot;

    }

    private void RemoveCombinationSlot(UIItemSlot slot = null)
    {
        UIItemSlot sl;
        if (slot != null)
        {
            sl = slot;
        }
        else
        {
            sl = combinationItemSlots.Last();
        }
        combinationItemSlots.Remove(sl);
        Destroy(sl.gameObject);
    }

    private void OnValidate()
    {
        CalculateShop();
    }

    //InEditor call
    public void CalculateShop()
    {
        combinationItem = GetCombinationItem(ingredients);
        combinationItems = GetCraftableItems(ingredients);
    }

    public List<Item> GetCraftableItems(List<Item> craftingParts)
    {
        List<Item> ret = new List<Item>();

        List<Item> parentItems = new List<Item>();

        foreach (Item item in craftingParts)
        {
            if (item != null)
            {
                foreach (Item i in item.ingredientOf)
                {
                    if (!parentItems.Contains(i))
                    {
                        parentItems.Add(i);
                    }
                }
            }
        }

        foreach (Item item in parentItems)
        {
            if(item != null)
            {
                bool craftable = true;
                Dictionary<string, int> capacity = new Dictionary<string, int>();
                foreach(Item i in item.recipe)
                {
                    if(capacity.ContainsKey(i.GetItemStringCode()))
                    {
                        capacity[i.GetItemStringCode()]++;
                    }
                    else
                    {
                        capacity.Add(i.GetItemStringCode(), 1);
                    }
                }
                foreach(Item i in craftingParts)
                {
                    if(!capacity.ContainsKey(i.GetItemStringCode()))
                    {
                        craftable = false;
                        break;
                    }
                    else
                    {
                        capacity[i.GetItemStringCode()]--;
                        if(capacity[i.GetItemStringCode()] < 0)
                        {
                            craftable = false;
                            break;
                        }
                    }
                }
                if(craftable)
                {
                    ret.Add(item);
                }
            }
        }

        return ret;
    }

    public Item GetCombinationItem(List<Item> craftingParts, int index = 0)
    {
        float variance = 0;
        List<Item> retList = GetCraftableItems(craftingParts);
        if(retList.Count > 0)
        {
            index = Mathf.Min(index, retList.Count-1);
            float price = 0;
            foreach(Item i in craftingParts)
            {
                variance += i.instanceVariance * i.GetInstancePrice();
                price += i.GetInstancePrice();
            }
            variance = variance / price;
            Item ret = Instantiate(retList[index]);
            ret.instanceVariance = variance;
            return ret;
        }

        return null;
    }

    public List<Item> GetRemainingItems(Item outcome, List<Item> ingredients, float missingVaraince = 0)
    {
        
        List<Item> ret = new List<Item>();
        if (outcome != null)
        {
            List<Item> list = new List<Item>();
            foreach (Item i in ingredients)
            {
                list.Add(i);
            }

            foreach (Item ri in outcome.recipe)
            {
                if (ri != null)
                {
                    bool missing = true;
                    foreach (Item ing in list)
                    {
                        if (ing != null)
                        {
                            if (ing.GetItemStringCode() == ri.GetItemStringCode())
                            {
                                list.Remove(ing);
                                missing = false;
                                break;
                            }
                        }
                    }
                    if (missing)
                    {
                        Item missItem = Instantiate(ri);
                        missItem.instanceVariance = missingVaraince;
                        ret.Add(missItem);
                    }
                }
            }
        }
        return ret;
    }
}
