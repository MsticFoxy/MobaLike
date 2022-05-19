using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.Linq;
using UnityEditor;

public enum Tier
{
    Base,
    Tier_1,
    Tier_2,
    Tier_3,
    Tier_4,
    Tier_5,
    Tier_6,
    Tier_7,
    Tier_8,
    Tier_9,
    Tier_10
}

public enum StatIndicator
{
    Health,
    Mana,
    Range,
    Size,
    CritChance,
    CritDamage,
    CritResistence,
    AttackDamage,
    AttackSpeed,
    AbilityPower,
    AbilityHaste,
    Armor,
    ArmorPenetration,
    LifeSteal,
    HealPower,
    HealthRegeneration,
    ManaRegeneration,
    MovementSpeed,
    Tenacity,
    SlowResistence
}

[Serializable]
public struct VarianceInformation
{
    public float min;
    public float max;

    public static VarianceInformation operator +(VarianceInformation a, VarianceInformation b)
    {
        VarianceInformation ret = new VarianceInformation();
        ret.min = a.min + b.min;
        ret.max = a.max + b.max;
        return ret;
    }
}

[Serializable]
public struct BaseStatModificationInformation
{
    [HideInInspector]
    public string ownerString;
    [ConditionalField(nameof(ownerString), true, "")]
    public Item owner;
    public StatIndicator statIndicator;
    public float addition;
    [Tooltip("increases the stat by growth %")]
    public float growth;

    [ReadOnly]
    public VarianceInformation additionVariance;
    [ReadOnly]
    public VarianceInformation growthVariance;

    public static BaseStatModificationInformation operator +(BaseStatModificationInformation a, BaseStatModificationInformation b)
    {
        BaseStatModificationInformation ret = new BaseStatModificationInformation();
        ret.statIndicator = a.statIndicator;
        ret.addition = a.addition + b.addition;
        ret.growth = a.growth + b.growth;
        ret.additionVariance = a.additionVariance + b.additionVariance;
        ret.growthVariance = a.growthVariance + b.growthVariance;
        return ret;
    }
}

[Serializable]
public class ItemEffectChild : ItemEffect
{
    public string strength;
}


[CreateAssetMenu(fileName = "Item", menuName = "Items/Create Base Item", order = 0)]
public class Item : ScriptableObject
{
    public static float variance = 10;
    public static bool itemListIsDirty = true;
    private static List<Item> _items;
    public static List<Item> items 
    { 
        get
        {
            if(itemListIsDirty)
            {
                _items = GetListOfItems();
            }
            return _items;
        }
        private set
        {
            _items = value;
        }
    }


    [Foldout("Information")]
    public Sprite sprite;
    [Foldout("Information")]
    public string itemName;
    [Foldout("Information")]
    public string description;
    [Foldout("Information")]
    public Tier tier;

    [Foldout("Price")]
    public int price;
    [ReadOnly]
    [Foldout("Price")]
    public int collectivePrice;
    [ReadOnly]
    [Foldout("Price")]
    public VarianceInformation priceVariance;
    [ReadOnly]
    [Foldout("Price")]
    public VarianceInformation collectivePriceVariance;

    [Foldout("Modifications")]
    public List<BaseStatModificationInformation> baseStatModifications;
    [ReadOnly]
    [NonReorderable]
    [Foldout("Modifications")]
    public List<BaseStatModificationInformation> childStatModifications;
    [ReadOnly]
    [NonReorderable]
    [Foldout("Modifications")]
    public List<BaseStatModificationInformation> collectiveStatModifiations;

    [Foldout("Recipies")]
    public List<Item> recipe = new List<Item>();
    [ReadOnly]
    [NonReorderable]
    [Foldout("Recipies")]
    public List<Item> ingredientOf;

    [HideInInspector]
    public List<Item> previousRecipe;

    [Foldout("Effects")]
    public List<ItemEffect> effects;
    [Foldout("Effects")]
    [DisplayInspector]
    public Ability activeAbility;

    //[ReadOnly]
    [Foldout("Information")]
    public float instanceVariance;
    private float varianceDelta
    {
        get
        {
            return ((instanceVariance / variance) + 1.0f) / 2.0f;
        }
    }

    protected Dictionary<StatIndicator, List<StatModifier<float>>> modifiers = new Dictionary<StatIndicator, List<StatModifier<float>>>();
    protected Dictionary<StatIndicator, List<StatModifier<PoolValueFloat>>> poolMods = new Dictionary<StatIndicator, List<StatModifier<PoolValueFloat>>>();

    
    public Inventory inventory { get; private set; }

    public Item()
    {
        #if UNITY_EDITOR
        itemListIsDirty = true;
        #endif
    }

    protected List<BaseStatModificationInformation> GetCollectiveBaseStatModifications()
    {
        Dictionary<StatIndicator, BaseStatModificationInformation> mods = 
            new Dictionary<StatIndicator, BaseStatModificationInformation>();

        foreach(BaseStatModificationInformation bsm in baseStatModifications)
        {
            if (!mods.ContainsKey(bsm.statIndicator))
            {
                BaseStatModificationInformation b = new BaseStatModificationInformation();
                b += bsm;
                b.statIndicator = bsm.statIndicator;
                mods.Add(bsm.statIndicator, b);
            }
            else
            {
                mods[bsm.statIndicator] += bsm;
            }
        }
        foreach (BaseStatModificationInformation bsm in childStatModifications)
        {
            if (!mods.ContainsKey(bsm.statIndicator))
            {
                BaseStatModificationInformation b = new BaseStatModificationInformation();
                b += bsm;
                b.statIndicator = bsm.statIndicator;
                mods.Add(bsm.statIndicator, b);
            }
            else
            {
                mods[bsm.statIndicator] += bsm;
            }
        }

        List<BaseStatModificationInformation> ret = new List<BaseStatModificationInformation>();
        foreach(KeyValuePair<StatIndicator, BaseStatModificationInformation> pair in mods)
        {
            ret.Add(pair.Value);
        }
        return ret;
    }

    private static List<Item> GetListOfItems()
    {
        List<Item> ret = new List<Item>();

        string[] guids = AssetDatabase.FindAssets("t:" + typeof(Item).Name);
        Item[] a = new Item[guids.Length];
        for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<Item>(path);
        }
        return a.ToList();
    }

    private List<Item> IngredientOf()
    {
        List<Item> ret = new List<Item>();

        foreach (Item it in items)
        {
            if (it != this)
            {
                if (it.recipe != null)
                {
                    if (it.recipe.Contains(this))
                    {
                        ret.Add(it);
                    }
                }
            }
        }
        return ret;
    }

    public void ShowPriceInformation()
    {
        collectivePrice = GetPrice();
        priceVariance = GetPriceVariance();
        collectivePriceVariance = GetCollectivePriceVariance();
    }

    public void ShowChildStats()
    {
        childStatModifications.Clear();
        foreach(Item it in recipe)
        {
            if (it != null)
            {
                foreach (BaseStatModificationInformation inf in it.collectiveStatModifiations)
                {
                    BaseStatModificationInformation mod = inf;
                    mod.ownerString = it.name;
                    mod.owner = it;
                    childStatModifications.Add(mod);
                }
            }
        }
    }

    public void ShowStatVariance()
    {
        for(int i = 0; i < baseStatModifications.Count; i++)
        {
            BaseStatModificationInformation mod = baseStatModifications[i];
            mod.additionVariance.min = baseStatModifications[i].addition * (1.0f - Item.variance * 0.01f);
            mod.additionVariance.max = baseStatModifications[i].addition * (1.0f + Item.variance * 0.01f);
            mod.growthVariance.min = baseStatModifications[i].growth * (1.0f - Item.variance * 0.01f);
            mod.growthVariance.max = baseStatModifications[i].growth * (1.0f + Item.variance * 0.01f);
            baseStatModifications.RemoveAt(i);
            baseStatModifications.Insert(i, mod);
        }
    }

    private void OnValidate()
    {
        ShowStatVariance();
        ShowPriceInformation();
        ShowChildStats();
        collectiveStatModifiations = GetCollectiveBaseStatModifications();
        ingredientOf = IngredientOf();
        foreach(Item it in previousRecipe)
        {
            it.ingredientOf = it.IngredientOf();
        }
        previousRecipe.Clear();
        foreach(Item it in recipe)
        {
            it.ingredientOf = it.IngredientOf();
            previousRecipe.Add(it);
        }

        foreach(ItemEffect ie in ItemEffect.globalItemEffecs)
        {
            if (ie != null)
            {
                if (ie.effectOf.Contains(this))
                {
                    ie.effectOf.Remove(this);
                }
            }
        }
        foreach (ItemEffect ie in effects)
        {
            if(ie != null)
            {
                if(!ie.effectOf.Contains(this))
                {
                    ie.effectOf.Add(this);
                }
            }
        }
    }

    public int GetPrice(int maxIterationDepth = 7)
    {
        int componentCost = 0;
        if (maxIterationDepth > 0)
        {
            foreach (Item it in recipe)
            {
                if (it != null)
                {
                    componentCost += it.GetPrice(maxIterationDepth - 1);
                }
            }
        }
        return price + componentCost;
    }
    
    public int GetInstancePrice()
    {
        return Mathf.RoundToInt(Mathf.Lerp(priceVariance.min, priceVariance.max, varianceDelta));
    }

    public int GetInstanceCollectivePrice()
    {
        return Mathf.RoundToInt(Mathf.Lerp(collectivePriceVariance.min, collectivePriceVariance.max, varianceDelta));
    }

    public VarianceInformation GetPriceVariance()
    {
        VarianceInformation priceVar = new VarianceInformation();
        priceVar.min += price * (1.0f - Item.variance * 0.01f);
        priceVar.max += price * (1.0f + Item.variance * 0.01f);
        return priceVar;
    }

    public VarianceInformation GetCollectivePriceVariance(int maxIterationDepth = 7)
    {
        VarianceInformation componentCost = new VarianceInformation();
        if (maxIterationDepth > 0)
        {
            foreach (Item it in recipe)
            {
                if (it != null)
                {
                    componentCost += it.GetCollectivePriceVariance(maxIterationDepth - 1);
                }
            }
        }
        return GetPriceVariance() + componentCost;
    }

    public bool SetInventory(Inventory inventory, bool canBeStolen = false)
    {
        if(inventory != null)
        {
            if (canBeStolen)
            {
                if(this.inventory != null)
                {
                    this.inventory.RemoveItem(this);
                }
                this.inventory = inventory;
                if(this.inventory.AddItem(this))
                {
                    this.inventory = null;
                }
            }
            else
            {
                if(this.inventory == null)
                {
                    this.inventory = inventory;
                    if (this.inventory.AddItem(this))
                    {
                        this.inventory = null;
                    }
                }
            }
        }
        else
        {
            if (this.inventory != null)
            {
                this.inventory.RemoveItem(this);
            }
            this.inventory = null;
        }
        
        return false;
    }

    protected StatValue<float> GetStatByStatIndicator(StatIndicator indicator)
    {
        switch(indicator)
        {
            case StatIndicator.Range: 
                return inventory.stats.range;
            case StatIndicator.Size: 
                return inventory.stats.size;
            case StatIndicator.CritChance:
                return inventory.stats.critChance;
            case StatIndicator.CritDamage:
                return inventory.stats.critDamage;
            case StatIndicator.CritResistence: 
                return inventory.stats.critResistence;
            case StatIndicator.AttackDamage:
                return inventory.stats.attackDamage;
            case StatIndicator.AttackSpeed:
                return inventory.stats.attackSpeed;
            case StatIndicator.AbilityPower:
                return inventory.stats.abilityPower;
            case StatIndicator.AbilityHaste:
                return inventory.stats.abilityHaste;
            case StatIndicator.Armor:
                return inventory.stats.armor;
            case StatIndicator.ArmorPenetration:
                return inventory.stats.armorPenetration;
            case StatIndicator.LifeSteal:
                return inventory.stats.lifeSteal;
            case StatIndicator.HealPower:
                return inventory.stats.healPower;
            case StatIndicator.HealthRegeneration:
                return inventory.stats.healthRegeneration;
            case StatIndicator.ManaRegeneration:
                return inventory.stats.manaRegeneration;
            case StatIndicator.MovementSpeed:
                return inventory.stats.movementSpeed;
            case StatIndicator.Tenacity:
                return inventory.stats.tenacity;
            case StatIndicator.SlowResistence:
                return inventory.stats.slowResistence;
            default:
                return null;
        }
    }

    private void AddToPoolMods(StatIndicator indicator, StatModifier<PoolValueFloat> val)
    {
        if(!poolMods.ContainsKey(indicator))
        {
            poolMods.Add(indicator, new List<StatModifier<PoolValueFloat>>());
        }
        poolMods[indicator].Add(val);
    }
    private void AddToMods(StatIndicator indicator, StatModifier<float> val)
    {
        if (!modifiers.ContainsKey(indicator))
        {
            modifiers.Add(indicator, new List<StatModifier<float>>());
        }
        modifiers[indicator].Add(val);
    }

    public virtual void OnAddedToInventory()
    {
        foreach(BaseStatModificationInformation bs in collectiveStatModifiations)
        {
            if(bs.statIndicator == StatIndicator.Health)
            {
                StatModifier<PoolValueFloat> add = new StatModifier<PoolValueFloat>((val) =>
                {
                    val.max += bs.addition;
                    return val;
                });
                StatModifier<PoolValueFloat> mult = new StatModifier<PoolValueFloat>((val) =>
                {
                    val.max *= (1.0f + (bs.growth * 0.01f));
                    return val;
                });
                inventory.stats.health.AddModifier(0, add);
                inventory.stats.health.AddModifier(1, mult);
                AddToPoolMods(bs.statIndicator, add);
                AddToPoolMods(bs.statIndicator, mult);
            }
            else if(bs.statIndicator == StatIndicator.Mana)
            {
                StatModifier<PoolValueFloat> add = new StatModifier<PoolValueFloat>((val) =>
                {
                    val.max += bs.addition;
                    return val;
                });
                StatModifier<PoolValueFloat> mult = new StatModifier<PoolValueFloat>((val) =>
                {
                    val.max *= (1.0f + (bs.growth * 0.01f));
                    return val;
                });
                inventory.stats.mana.AddModifier(0, add);
                inventory.stats.mana.AddModifier(1, mult);
                AddToPoolMods(bs.statIndicator, add);
                AddToPoolMods(bs.statIndicator, mult);
            }
            else
            {
                if (GetStatByStatIndicator(bs.statIndicator) != null)
                {
                    StatModifier<float> add = new StatModifier<float>((val) =>
                    {
                        return val + bs.addition;
                    });
                    StatModifier<float> mult = new StatModifier<float>((val) =>
                    {
                        return val * (1.0f + (bs.growth * 0.01f));
                    });
                    GetStatByStatIndicator(bs.statIndicator).AddModifier(0, add);
                    GetStatByStatIndicator(bs.statIndicator).AddModifier(1, mult);
                    AddToMods(bs.statIndicator, add);
                    AddToMods(bs.statIndicator, mult);
                    Debug.Log(bs.statIndicator + " -> " + add);
                }
            }
        }
        
    }

    public virtual void OnRemovedFromInventory()
    {
        foreach(KeyValuePair<StatIndicator, List<StatModifier<float>>> pair in modifiers)
        {
            foreach (StatModifier<float> mod in pair.Value)
            {
                GetStatByStatIndicator(pair.Key).RemoveModifier(mod);
            }
        }
        foreach (KeyValuePair<StatIndicator, List<StatModifier<PoolValueFloat>>> pair in poolMods)
        {
            foreach (StatModifier<PoolValueFloat> mod in pair.Value)
            {
                if (pair.Key == StatIndicator.Health)
                {
                    inventory.stats.health.RemoveModifier(mod);
                }
                else if (pair.Key == StatIndicator.Mana)
                {
                    inventory.stats.mana.RemoveModifier(mod);
                }
            }
        }
        modifiers.Clear();
        poolMods.Clear();
    }

    public string GetItemStringCode()
    {
        return "Item" + itemName + description + tier.ToString();
    }
}
