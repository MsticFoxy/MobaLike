using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{

    public ItemHolder holder { get; private set; }

    public bool SetHolder(ItemHolder holder, bool canBeStolen = false)
    {
        if(holder != null)
        {
            if (canBeStolen)
            {

            }
            else
            {
                if(this.holder != null)
                {
                    this.holder = holder;
                }
            }
        }
        else
        {
            OnRemovedFromHolder();
        }
        
        return false;
    }

    public void OnAddedToHolder()
    {

    }

    public void OnRemovedFromHolder()
    {

    }
}
