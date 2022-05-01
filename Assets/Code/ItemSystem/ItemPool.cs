using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ItemPool : MonoBehaviour
{
    public List<Item> itemPool;

    [ButtonMethod(0)]
    public void AddAllItemDependencies()
    {
        foreach(Item i in itemPool)
        {
            foreach(Item it in i.recipe)
            {
                if(!itemPool.Contains(it))
                {
                    itemPool.Add(it);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
