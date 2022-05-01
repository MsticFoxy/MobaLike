using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;

[RequireComponent(typeof(ChampionStats), typeof(CharacterController))]
public class Inventory : MonoBehaviour
{
    [Foldout("Initialization")]
    public List<Item> initialItems;

    [Foldout("Inventory Information")]
    public int maxItemCount = 10;
    [Foldout("Inventory Information")]
    [ReadOnly]
    public List<Item> items;
    
    public ChampionStats stats { get; private set; }
    protected CharacterController characterController;

    public Action<Item> OnItemCouldNotBeAdded;

    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<ChampionStats>();
        characterController = GetComponent<CharacterController>();
        foreach(Item itm in initialItems)
        {
            Item addition = Instantiate(itm);
            addition.SetInventory(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(stats.attackDamage.value);
    }

    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            item.OnRemovedFromInventory();
        }
    }

    public bool AddItem(Item item)
    {
        
        if (!items.Contains(item))
        {
            if (items.Count < maxItemCount)
            {
                items.Add(item);
                item.OnAddedToInventory();
                return true;
            }
            else
            {
                if(OnItemCouldNotBeAdded != null)
                {
                    OnItemCouldNotBeAdded.Invoke(item);
                }
            }
        }

        return false;
    }
}
