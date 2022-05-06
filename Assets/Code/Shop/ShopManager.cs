using MyBox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<Item> ingredients;
    [ReadOnly]
    [NonReorderable]
    public List<Item> combinationItems;
    public Item combinationItem;

    public Task<List<Item>> shopCraftableItemCalculationTask;
    private bool shopCraftingFinished;
    public Task<Item> combinationItemCreationTask;
    private bool combinationItemFinished;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ButtonMethod]
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
                Dictionary<int, int> capacity = new Dictionary<int, int>();
                foreach(Item i in item.recipe)
                {
                    if(capacity.ContainsKey(i.GetHashCode()))
                    {
                        capacity[i.GetHashCode()]++;
                    }
                    else
                    {
                        capacity.Add(i.GetHashCode(), 1);
                    }
                }
                foreach(Item i in craftingParts)
                {
                    if(!capacity.ContainsKey(i.GetHashCode()))
                    {
                        craftable = false;
                        break;
                    }
                    else
                    {
                        capacity[i.GetHashCode()]--;
                        if(capacity[i.GetHashCode()] < 0)
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
            index = Mathf.Max(index, retList.Count-1);
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

        List<Item> list = new List<Item>();
        foreach(Item i in ingredients)
        {
            list.Add(i);
        }

        foreach(Item ri in outcome.recipe)
        {
            if (ri != null)
            {
                bool missing = true;
                foreach (Item ing in list)
                {
                    if(ing != null)
                    {
                        if(ing.GetHashCode() == ri.GetHashCode())
                        {
                            list.Remove(ing);
                            missing = false;
                            break;
                        }
                    }
                }
                if(missing)
                {
                    Item missItem = Instantiate(ri);
                    missItem.instanceVariance = missingVaraince;
                    ret.Add(missItem);
                }
            }
        }

        return ret;
    }
}
