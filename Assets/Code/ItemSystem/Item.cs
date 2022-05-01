using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
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
public struct BaseStatModificationInformation
{
    public StatIndicator statIndicator;
    public float addition;
    [Tooltip("increases the stat by growth %")]
    public float growth;
}

[CreateAssetMenu(fileName = "Item", menuName = "Items/Create Base Item", order = 1)]
public class Item : ScriptableObject
{
    public Sprite sprite;
    public string itemName;
    public string description;
    public Rarity rarity;
    public int price;
    [ReadOnly]
    public int collectivePrice;
    public List<BaseStatModificationInformation> baseStatModifications;
    protected Dictionary<StatIndicator, List<StatModifier<float>>> modifiers = new Dictionary<StatIndicator, List<StatModifier<float>>>();
    protected Dictionary<StatIndicator, List<StatModifier<PoolValueFloat>>> poolMods = new Dictionary<StatIndicator, List<StatModifier<PoolValueFloat>>>();

    public List<Item> recipe;
    public Inventory inventory { get; private set; }


    [ButtonMethod(0)]
    public void ShowCollectivePrice()
    {
        collectivePrice = GetPrice();
    }

    public int GetPrice(int maxIterationDepth = 7)
    {
        int componentCost = 0;
        if (maxIterationDepth > 0)
        {
            foreach (Item it in recipe)
            {
                componentCost += it.GetPrice(maxIterationDepth - 1);
            }
        }
        return price + componentCost;
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
        foreach(BaseStatModificationInformation bs in baseStatModifications)
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
}
