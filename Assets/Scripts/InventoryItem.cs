using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

[Serializable]

public class InventoryItem
{
    public ItemData itemData;
    public ItemSlot itemSlot;
    public int stackSize;


    public InventoryItem(ItemData item)
    {
        itemData = item;
        AddToStack();
    }

    public void AddToStack()
    {
        stackSize++;
    }

    public void RemoveFromStack()
    {
        stackSize--;
    }

    
    // Functia care se apeleaza cand se apasa pe item, scade stack size-ul si daca e 0, il sterge
    // Si apeleaza functia UseItem din itemData 
    public virtual void UseItem(ItemSlot itemSlot)
    {
        if(itemData.itemType == "Active")
            stackSize--;
        if (stackSize <= 0) itemSlot.RemoveItem();
        itemSlot.ItemStackSize.text = stackSize.ToString();
        itemData.UseItem();
    }
}
